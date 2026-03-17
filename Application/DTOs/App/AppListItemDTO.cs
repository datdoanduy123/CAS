using System;

namespace Application.DTOs.App
{
    public class AppListItemDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AppType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? RealmId { get; set; }
    }
}
