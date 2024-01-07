using System.ComponentModel.DataAnnotations;

namespace ThuongMaiDT.ViewModels
{
    public class RegisterVM
    {
        [Display(Name ="Tên đăng nhập")]
        [Required(ErrorMessage = "*")]
        public string MaKh { get; set; }
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage ="Tối thiểu 6 kí tự")]
        [Required(ErrorMessage ="*")]
        public string MatKhau { get; set; }
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }
        public bool GioiTinhNam { get; set; } = true;
        public bool GioiTinhNu { get; set; } = false;
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Điện thoại")]
        [RegularExpression(@"0[9832]\d{8}", ErrorMessage = "Chưa đúng định dạng SDT")]
        public string DienThoai { get; set; }
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng ")]
        public string Email { get; set; }
        public string? Hinh { get; set; }
    }
}
