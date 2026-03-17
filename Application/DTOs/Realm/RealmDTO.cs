namespace Application.DTOs.Realm
{
    public class RealmDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}
