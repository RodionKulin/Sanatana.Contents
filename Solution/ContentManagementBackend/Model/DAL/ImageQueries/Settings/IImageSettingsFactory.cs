namespace ContentManagementBackend
{
    public interface IImageSettingsFactory
    {
        ImageSettings GetSettings(string name);
    }
}