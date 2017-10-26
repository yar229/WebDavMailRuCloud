namespace NWebDav.Server.Stores
{
    public interface IDiskStoreItem : IStoreItem
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}
