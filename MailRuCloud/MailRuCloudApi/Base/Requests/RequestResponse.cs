namespace YaR.Clouds.Base.Requests
{
    public struct RequestResponse<T>
    {
        public bool Ok { get; set; }

        public string Description { get; set; }

        public T Result { get; set; }

        public long? ErrorCode { get; set; }
    }
}
