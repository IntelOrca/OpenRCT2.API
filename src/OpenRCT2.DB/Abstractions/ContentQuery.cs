namespace OpenRCT2.DB.Abstractions
{
    public class ContentQuery
    {
        public string CurrentUserId { get; set; }
        public string OwnerId { get; set; }
        public ContentSortKind SortBy { get; set; }
    }
}
