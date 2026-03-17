namespace Application.DTOs.Role
{
    public class RoleUpsertDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}