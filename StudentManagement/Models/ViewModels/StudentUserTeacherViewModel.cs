namespace StudentManagement.Models.ViewModels
{
    public class StudentUserTeacherViewModel
    {
        // User Fields
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; } // <--- RẤT QUAN TRỌNG

        // Shared Fields
        public string FullName { get; set; } = null!;

        // Student Fields (Cần cho Student Role)
        public string StudentCode { get; set; }
        public int StatusId { get; set; }

        // Teacher Fields (Cần cho Teacher Role)
        public string TeacherCode { get; set; }
        public string Specialization { get; set; }
    }
}
