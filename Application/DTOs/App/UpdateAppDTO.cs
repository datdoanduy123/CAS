using System;

namespace Application.DTOs.App
{
    public class UpdateAppDTO
    {
        public string? Name { get; set; }
        public string? AppSecret { get; set; }
        public string? RedirectUris { get; set; }
        public string? AllowedGrantTypes { get; set; }
        public string? LogoutUri { get; set; }
        public string? AppType { get; set; }
        public bool? IsActive { get; set; }
        public Guid? RealmId { get; set; }
    }
}
