namespace StudentManagement.Models.ViewModels
{
    public class CreateStudentUserViewModel
    {
        // I. User Fields (Required for login/auth)
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; } // Sẽ được cố định là 3 (Student)

        // II. Student Fields (Required for personal/academic info)
        public string StudentCode { get; set; } = null!;
        public string FullName { get; set; } = null!; // Dùng chung cho cả User và Student
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int StatusId { get; set; }
    }
}