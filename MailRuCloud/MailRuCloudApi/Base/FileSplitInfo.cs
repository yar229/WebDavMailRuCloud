namespace YaR.Clouds.Base
{
    public class FileSplitInfo
    {
        public bool IsHeader { get; set; }
        public bool IsPart => PartNumber > 0;
        public int PartNumber { get; set; }
    }
}