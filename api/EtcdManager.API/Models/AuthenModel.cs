using LiteDB;

namespace EtcdManager.API.Models
{
    public class AuthenModel
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}