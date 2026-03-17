using System;

namespace Application.DTOs.User
{
    public class CreateUserResponseDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RealmId { get; set; }
    }
}
