using System.ComponentModel.DataAnnotations;

namespace ThuongMaiDT.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage ="Tên đăng nhập không được để trống")]
        public string Username {  get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }
    }
}
