using EtcdManager.API.Models;
using LiteDB;

namespace EtcdManager.API.Database
{
    public interface ILiteDatabaseContext
    {
        #region Connections
        ResponseModel<List<ConnectionModel>?> GetConnections(string userName);
        ResponseModel<string> CreateConnection(ConnectionModel connectionModel);
        ResponseModel<bool> UpdateConnection(Guid id, ConnectionModel connectionModel);
        ResponseModel<bool> DeleteConnection(Guid id);
        ResponseModel<List<ConnectionModel>> GetConnections();
        ResponseModel<bool> DeleteConnectionByName(string name);
        ResponseModel<ConnectionModel> GetConnectionByName(string name);

        #endregion
        #region Users
        ResponseModel<List<User>?> GetUsers();
        ResponseModel<User> CreateUser(User user);
        ResponseModel<User> DeleteUser(User user);
        ResponseModel<bool> GetByUserNameAndPassword(string userName, string password);
        ResponseModel<User> GetByUserName(string userName);
        ResponseModel<User> GetUserInfo();
        #endregion
        #region Authentication 
        ResponseModel<bool> TokenIsValid(string token);
        ResponseModel<string> Login(string userName, string password);
        ResponseModel<string> Logout();
        void SeedData();
        #endregion
    }

    public class LiteDatabaseContext : ILiteDatabaseContext
    {
        private readonly string _databasePath;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TABLE_USERS = "users";
        private const string TABLE_CONNECTIONS = "connections";
        private const string TABLE_AUTHENTICATION = "authentications";
        public LiteDatabaseContext(
            IWebHostEnvironment host,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _databasePath = Path.Combine(host.WebRootPath, "database", "etcd.db");
            this._host = host;
            this._httpContextAccessor = httpContextAccessor;
        }

        public void SeedData()
        {
            var databasePath = Path.Combine(_host.WebRootPath, "database");
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }
            // create root user
            var rootUser = new User
            {
                UserName = "root",
                Password = "root",
            };
            this.CreateUser(rootUser);
        }

        #region Connections
        public ResponseModel<List<ConnectionModel>?> GetConnections(string userName)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                var connections = collection.Query().Where(x => x.PermissionUsers.Contains($",{userName},")).ToList();
                return ResponseModel<List<ConnectionModel>?>.ResponseWithData(connections);
            }
        }

        public ResponseModel<List<ConnectionModel>?> GetConnections()
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                var connections = collection.Query().ToList();
                return ResponseModel<List<ConnectionModel>?>.ResponseWithData(connections);
            }
        }

        public ResponseModel<ConnectionModel> GetConnectionByName(string name)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                var connection = collection.Query().Where(x => x.Name == name).FirstOrDefault();
                return ResponseModel<ConnectionModel>.ResponseWithData(connection);
            }
        }

        public ResponseModel<string> CreateConnection(ConnectionModel connectionModel)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                    var existConnection = collection.FindOne(x => x.Name == connectionModel.Name || x.Id == connectionModel.Id);
                    if (existConnection == null)
                    {
                        if (connectionModel.Id == Guid.Empty)
                        {
                            connectionModel.Id = Guid.NewGuid();
                        }
                        collection.Insert(connectionModel);
                    }
                    else
                    {
                        return ResponseModel<string>.ResponseWithError("Connection already exists");
                    }
                }
                return ResponseModel<string>.ResponseWithData("Connection created successfully");
            }
            catch (Exception ex)
            {
                return ResponseModel<string>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        public ResponseModel<bool> UpdateConnection(Guid id, ConnectionModel connectionModel)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                    var existConnection = collection.Find(x => x.Name == connectionModel.Name || x.Id == id);
                    if (existConnection == null)
                    {
                        return ResponseModel<bool>.ResponseWithError("Connection not found");
                    }
                    else
                    {
                        connectionModel.Id = id;
                        collection.Update(connectionModel);
                    }
                }
                return ResponseModel<bool>.ResponseWithData(true);
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        public ResponseModel<bool> DeleteConnectionByName(string name)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                    var existConnection = collection.Find(x => x.Name == name);
                    if (existConnection == null)
                    {
                        return ResponseModel<bool>.ResponseWithError("Connection not found");
                    }
                    else
                    {
                        collection.DeleteMany(x => x.Name == name);
                    }
                }
                return ResponseModel<bool>.ResponseWithData(true);
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        public ResponseModel<bool> DeleteConnection(Guid id)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<ConnectionModel>(TABLE_CONNECTIONS);
                    var existConnection = collection.Find(x => x.Id == id);
                    if (existConnection == null)
                    {
                        return ResponseModel<bool>.ResponseWithError("Connection not found");
                    }
                    else
                    {
                        collection.Delete(id);
                    }
                }
                return ResponseModel<bool>.ResponseWithData(true);
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
        #endregion
        #region User

        public ResponseModel<User> GetByUserName(string userName)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<User>(TABLE_USERS);
                var user = collection.FindOne(x => x.UserName == userName);
                return ResponseModel<User>.ResponseWithData(user);
            }
        }

        public ResponseModel<bool> GetByUserNameAndPassword(string userName, string password)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<User>(TABLE_USERS);
                var user = collection.FindOne(x => x.UserName == userName && x.Password == password);
                if (user != null)
                {
                    return ResponseModel<bool>.ResponseWithData(true);
                }
                return ResponseModel<bool>.ResponseWithData(false);
            }
        }
        public ResponseModel<List<User>?> GetUsers()
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var collection = db.GetCollection<User>(TABLE_USERS);
                var users = collection.FindAll().ToList();
                return ResponseModel<List<User>?>.ResponseWithData(users);
            }
        }

        public ResponseModel<User> CreateUser(User user)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<User>(TABLE_USERS);
                    var existUser = collection.FindOne(x => x.UserName == user.UserName);
                    if (existUser == null)
                    {
                        collection.Insert(user);
                    }
                    else
                    {
                        return ResponseModel<User>.ResponseWithError("User already exists");
                    }
                }
                return ResponseModel<User>.ResponseWithData(user);
            }
            catch (Exception ex)
            {
                return ResponseModel<User>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        public ResponseModel<User> DeleteUser(User user)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var collection = db.GetCollection<User>(TABLE_USERS);
                    var existUser = collection.FindOne(x => x.UserName == user.UserName);
                    if (existUser != null)
                    {
                        collection.Delete(existUser.Id);
                    }
                    else
                    {
                        return ResponseModel<User>.ResponseWithError("User not found");
                    }
                }
                return ResponseModel<User>.ResponseWithData(user);
            }
            catch (Exception ex)
            {
                return ResponseModel<User>.ResponseWithError("EXCEPTION", System.Net.HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
        #endregion

        #region Authentication
        public ResponseModel<string> Login(string userName, string password)
        {
            using (var db = new LiteDatabase(TABLE_AUTHENTICATION))
            {
                var collection = db.GetCollection<AuthenModel>(TABLE_AUTHENTICATION);
                var isAuthen = GetByUserNameAndPassword(userName, password);
                if (isAuthen.Data)
                {
                    var token = Guid.NewGuid().ToString();
                    collection.Insert(new AuthenModel
                    {
                        UserName = userName,
                        Token = token,
                        ExpiredAt = DateTime.Now.AddDays(1)
                    });
                    return ResponseModel<string>.ResponseWithData(token);
                }
                return ResponseModel<string>.ResponseWithError("User not found");
            }
        }

        public ResponseModel<string> Logout()
        {
            using (var db = new LiteDatabase(TABLE_AUTHENTICATION))
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
                var collection = db.GetCollection<AuthenModel>(TABLE_AUTHENTICATION);
                var authen = collection.FindOne(x => x.Token == token);
                if (authen != null)
                {
                    collection.Delete(authen.Id);
                    return ResponseModel<string>.ResponseWithData("Logout successfully");
                }
                return ResponseModel<string>.ResponseWithError("Token not found");
            }
        }

        public ResponseModel<bool> TokenIsValid(string token)
        {
            using (var db = new LiteDatabase(TABLE_AUTHENTICATION))
            {
                var collection = db.GetCollection<AuthenModel>(TABLE_AUTHENTICATION);
                var authen = collection.FindOne(x => x.Token == token);
                if (authen != null)
                {
                    if (authen.ExpiredAt > DateTime.Now)
                    {
                        return ResponseModel<bool>.ResponseWithData(true);
                    }
                    // delete token
                    Logout();
                }
                return ResponseModel<bool>.ResponseWithData(true);
            }
        }

        public ResponseModel<User> GetUserInfo()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            using (var db = new LiteDatabase(TABLE_AUTHENTICATION))
            {
                var collection = db.GetCollection<AuthenModel>(TABLE_AUTHENTICATION);
                var authen = collection.FindOne(x => x.Token == token);
                if (authen != null)
                {
                    var user = GetByUserName(authen.UserName);
                    user.Data.Password = null;
                    return ResponseModel<User>.ResponseWithData(user.Data);
                }
                return ResponseModel<User>.ResponseWithError("User not found");
            }
        }
        #endregion
    }
}