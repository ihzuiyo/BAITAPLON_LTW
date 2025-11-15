namespace StudentManagement.Models.ViewModels
{
    public class CreateStudentViewModel
    {
        // User fields
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; } // Vai trò (mặc định là Student)

        // Student fields
        public string StudentCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int StatusId { get; set; }
    }
}
