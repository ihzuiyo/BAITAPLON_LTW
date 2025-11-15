namespace StudentManagement.Models.ViewModels
{
    public class CreateUserViewModel
    {
        // Thuộc tính khớp với Model User
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; } = "Active"; // Mặc định là Active

        // Thuộc tính chỉ dùng cho form
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int RoleId { get; set; }
    }
}
