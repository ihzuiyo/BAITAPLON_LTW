using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email ho?c tên ??ng nh?p là b?t bu?c")]
        [Display(Name = "Email/Tên ??ng nh?p")]
        public string UsernameOrEmail { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nh? ??ng nh?p")]
        public bool RememberMe { get; set; }
    }
}