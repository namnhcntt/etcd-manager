using EtcdManager.API.Core;
using EtcdManager.API.Core.Attributes;
using EtcdManager.API.Core.Exceptions;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Auto register DI
builder.Services.Scan(scan =>
                scan.FromCallingAssembly()
                    .AddClasses()
                    .AsMatchingInterface()
                    .WithLifetime(ServiceLifetime.Scoped));

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new GlobalApiFilterAttribute());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // add jwt bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI3MjE0NmViOC01OTlkLTQwMDUtODVhNi0zZjc0MGNiNjdjYTEiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAiLCJzdWIiOiLjg4bjgrnjg4jnlKjjgqLjgq_jgrvjgrnjg4jjg7zjgq_jg7MiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAiLCJleHAiOiIxNjk3OTEyMTg3IiwiaWF0IjoiMTY2NzkxMjE4NyIsInNpZCI6IjcyMTQ2ZWI4LTU5OWQtNDAwNS04NWE2LTNmNzQwY2I2N2NhMSIsInRlbmFudElkIjoiYWMzMWI4ZWUtNjQ0ZC1mMjlmLTViNGEtMjdhNDI2YWJmNmQxIiwidXNlcklkIjoiYWMzMWI4ZWUtNjQ0ZC1mMjlmLTViNGEtMjdhNDI2YWJmNmQxIiwidXNlck5Db2RlIjoiYWMzMWI4ZWUtNjQ0ZC1mMjlmLTViNGEtMjdhNDI2YWJmNmQxIiwic3RhZmZLYm4iOiIxIiwia29qaW5LYm4iOiIxIiwic3lzdGVtS2FucmlGbGciOiIxIiwic2ltcGxlTmFtZSI6IlNpbXBsZU5hbWUiLCJlbWFpbCI6Ik1haWxBZGRyZXNzIiwidGltZVpvbmUiOiI5IiwidGltZURpZmZlcm5jZSI6IjkiLCJhcmVhIjoiODEiLCJsYW5nIjoiMCIsImxpY2Vuc2VMaXN0IjoiMCIsInN5c3RlbUNvZGUiOiIwIiwib3B0aW9uQ29kZSI6IjAiLCJlZGl0aW9uIjoiMCIsIkJpenNreUFjY2VzcyI6InN0cmluZyIsIkJpenNreVJlZnJlc2giOiJzdHJpbmcifQ.fGffN-XkYRxvd5xWhhclRKJwn6f6z7rqGnrS4IUmbow' ",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key")))
        };
    });
var connectionString = builder.Configuration.GetValue("ConnectionStrings:EtcdManager",
                                                    Path.Combine(builder.Environment.WebRootPath, "data", "etcd-manager.db"));
builder.Services.AddDbContext<EtcdManagerDataContext>(options =>
{
    options.UseSqlite($"DataSource={connectionString};");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

// seed data
using (var scope = app.Services.CreateScope())
{
    var liteDb = scope.ServiceProvider.GetRequiredService<EtcdManagerDataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<EtcdManagerDataContext>>();
    liteDb.SeedData(logger, builder.Environment);
}

app.Run();
