using System.Text.Json.Serialization;
using LiteDB;

namespace EtcdManager.API.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}