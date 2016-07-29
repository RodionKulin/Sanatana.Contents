namespace ContentManagementBackend
{
    public interface IContentEncryption
    {
        string Decrypt(string inputText);
        string Encrypt(string inputText);
    }
}