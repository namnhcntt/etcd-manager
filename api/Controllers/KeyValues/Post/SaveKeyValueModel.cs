namespace EtcdManager.API.Controllers.KeyValues.Post;

public class SaveKeyValueModel
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
