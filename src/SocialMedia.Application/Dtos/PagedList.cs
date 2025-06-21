
namespace SocialMedia.Application.Dtos;

public class PagedList<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public bool HasNext => CurrentPage * PageSize < TotalCount;
    public bool HasPrevious => CurrentPage > 1;

    public PagedList(int totalCount, int pageSize, int currentPage, IEnumerable<T> data)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        Data = data;
    }

}
