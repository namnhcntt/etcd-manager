namespace EtcdManager.API.Controllers.KeyValues.Post
{
    public class RenameKeyModel
    {
        public string OldKey { get; set; } = null!;
        public string NewKey { get; set; } = null!;
    }
}
