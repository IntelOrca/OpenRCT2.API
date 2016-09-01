namespace OpenRCT2.DB.Models
{
    public class User
    {
        public string Id { get; set; }
        public int OpenRCT2orgId { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
    }
}
