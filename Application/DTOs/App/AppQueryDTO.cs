using System;

namespace Application.DTOs.App
{
    public class AppQueryDTO
    {
        public Guid? RealmId { get; set; }
        public bool? IsActive { get; set; }
    }
}
