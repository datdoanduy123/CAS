namespace Application.DTOs.Realm
{
    public class RealmQueryDTO
    {
        public bool? IsActive { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}
