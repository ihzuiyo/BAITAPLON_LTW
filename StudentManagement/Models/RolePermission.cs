namespace StudentManagement.Models
{
    public class RolePermission
    {
        // Khóa chính kép (Composite Primary Key)
        public int RoleId { get; set; }

        public int PermissionId { get; set; }

        // Navigation Properties (Tùy chọn, nhưng nên có để EF Core hoạt động tốt)
        public virtual Role Role { get; set; } = null!;

        public virtual Permission Permission { get; set; } = null!;
    }

}
