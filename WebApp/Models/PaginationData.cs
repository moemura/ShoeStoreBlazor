namespace WebApp.Models
{
    public class PaginationData<T>
    {
        public IEnumerable<T>? Data { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }
        public int PageCount { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}
