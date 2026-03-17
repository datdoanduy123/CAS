namespace Application.DTOs.Common
{
    public class MetaDataDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; } = 0;

        public int TotalPage => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    }
}
