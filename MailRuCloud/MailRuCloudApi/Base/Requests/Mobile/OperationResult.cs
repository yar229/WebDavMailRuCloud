namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    internal enum Operation : byte
    {
        AddFile = 103
    }

    internal enum OperationResult: byte
    {
        Ok = 0,
        NotExists = 1,

        Dunno02 = 2,
        Dunno03 = 3,
        Dunno04 = 4,
        Dunno05 = 5,
        Dunno06 = 6,
        Dunno07 = 7,
        Dunno08 = 8,
        Dunno09 = 9,
        Dunno10 = 10,
        Dunno11 = 11,
        Dunno12 = 12,

        FailedB = 253,
        FailedA = 254
    }
}
