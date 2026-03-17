using System;

namespace Application.DTOs.Common
{
    public class BaseRequestDTO
    {
        // Sử dụng Guid để khớp với thực thể User trong dự án
        public Guid ActionBy { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class BaseRequestDTO<T> : BaseRequestDTO
    {
        public T Request { get; set; } = default!;
    }
}
