namespace EtcdManager.API.Models
{
    public class SaveKeyModel : KeyModel
    {
        public ConnectionModel Connection { get; set; }
        public bool IsInsert { get; set; }
    }
}