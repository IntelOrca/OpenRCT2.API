namespace OpenRCT2.DB
{
    public class DBOptions
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsRegion { get; set; }
    }
}
