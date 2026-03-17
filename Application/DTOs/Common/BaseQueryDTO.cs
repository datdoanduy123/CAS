using System.Collections.Generic;

namespace Application.DTOs.Common
{
    /// <summary>
    /// Model nhận request từ client
    /// </summary>
    public class BaseQueryDTO
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BaseQueryDTO<T> : BaseQueryDTO
    {
        public T? Query { get; set; }
    }

    /// <summary>
    /// Model được convert từ BaseQueryDTO và làm đầu vào cho service
    /// </summary>
    public class QueryDTO : BaseRequestDTO
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 0 ? value : 20;
        }
        public int Skip => (Page - 1) * PageSize;
        public bool IsGetAll { get; set; } = false;
        public int Total { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class QueryDTO<T> : QueryDTO
    {
        public T? Query { get; set; }
    }
}
