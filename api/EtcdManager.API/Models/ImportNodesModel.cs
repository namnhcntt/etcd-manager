namespace EtcdManager.API.Models
{
    public class ImportNodesModel
    {
        public KeyModel[] KeyModels { get; set; }
        public ConnectionModel Connection { get; set; }
    }
}