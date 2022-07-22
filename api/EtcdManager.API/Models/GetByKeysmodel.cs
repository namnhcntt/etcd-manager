namespace EtcdManager.API.Models
{
    public class GetByKeysModel
    {
        public string[] Keys { get; set; }
        public ConnectionModel Connection { get; set; }
    }
}