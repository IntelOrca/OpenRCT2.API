namespace OpenRCT2.API
{
    public class ApiConfig
    {
        public string BaseUrl { get; set; }
        public string Bind { get; set; }
        public string ReCaptchaSecret { get; set; }
        public string PasswordServerSalt { get; set; }
        public string AuthTokenSecret { get; set; }
    }
}
