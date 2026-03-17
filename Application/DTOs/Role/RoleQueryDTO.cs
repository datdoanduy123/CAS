namespace Application.DTOs.Role
{
    public class RoleQueryDTO
    {
        public bool? IsActive { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}