namespace Application.DTOs.Role
{
    public class RoleQueryDTO
    {
        /// <summary>
        /// Lọc theo App: Nếu null => Hiện Realm Roles. Nếu có giá trị => Hiện Role của App đó.
        /// </summary>
        public Guid? AppId { get; set; }
    }
}