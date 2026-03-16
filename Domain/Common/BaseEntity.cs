using System;

namespace Domain.Common
{
    /// <summary>
    /// Base Entity cho toàn bộ các thực thể trong Domain.
    /// Cung cấp thuộc tính Id kiểu Guid mặc định.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // public string? CreatedBy { get; set; }
        // public DateTime? UpdatedAt { get; set; }
        // public string? UpdatedBy { get; set; }
    }
}
