using System.ComponentModel.DataAnnotations;

namespace Web_PCN.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nhập tài khoản")]
        public string Login { get; set; }          // <= dùng cho User Name

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }       // để bind checkbox
    }
}
