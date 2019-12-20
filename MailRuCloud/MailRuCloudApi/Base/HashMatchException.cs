using System;

namespace YaR.Clouds.Base
{
    public class HashMatchException : Exception
    {
        public string LocalHash { get; }
        public string RemoteHash { get; }

        public HashMatchException(string localHash, string remoteHash)
        {
            LocalHash = localHash;
            RemoteHash = remoteHash;
        }

        public override string Message => $"Local and remote hashes doen not match, local = {LocalHash}, remote = {RemoteHash}";
    }
}