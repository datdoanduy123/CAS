using System;

namespace Application.DTOs.App
{
    public class CreateAppDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string RedirectUris { get; set; } = string.Empty;
        public string AllowedGrantTypes { get; set; } = string.Empty;
        public string? LogoutUri { get; set; }
        public string AppType { get; set; } = "Confidential";
        public Guid? RealmId { get; set; }
    }
}
