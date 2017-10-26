namespace NWebDav.Server.Stores
{
    public interface IDiskStoreCollection : IStoreCollection
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}
