using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ThuongMaiDT.Helpers;
using ThuongMaiDT.Models;
using ThuongMaiDT.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using Mailjet.Client.Resources;
using System.Net.NetworkInformation;
using System.Collections.Specialized;
using System.Text;

namespace ThuongMaiDT.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IMapper _mapper;
        public KhachHangController(Hshop2023Context context, IMapper mapper)
        {
            db = context;
            _mapper = mapper; 
        }

        // Register
        #region Register
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DangKy(RegisterVM model, IFormFile Hinh)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var khachhang = _mapper.Map<KhachHang>(model);
                    khachhang.RandomKey = MyUtil.GenerateRandomKey();
                    khachhang.MatKhau = model.MatKhau.ToMd5Hash(khachhang.RandomKey);
                    khachhang.HieuLuc = true;
                    khachhang.VaiTro = 0;
                    if (Hinh != null)
                    {
                        khachhang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }
                    var check = db.KhachHangs.SingleOrDefault(kh=>kh.Email == model.Email);
                    if(check != null)
                    {
                        TempData["Message"] = "Email đã tồn tại";
                        return View();
                    }
                    db.Add(khachhang);
                    db.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            return View();
        }
        #endregion
        //Login
        #region Login
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model,  string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if(ModelState.IsValid)
            {
                var khachhang = db.KhachHangs.SingleOrDefault(p => p.MaKh == model.Username);
                HttpContext.Session.SetString("username", model.Username);
                if (khachhang == null)
                {
                    ModelState.AddModelError("loi", "Không có khách hàng này");
                }
                else
                {
                    if (!khachhang.HieuLuc)
                    {
                        ModelState.AddModelError("loi", "Tài khoản đã bị khóa");
                    }
                    else
                    {
                        if(khachhang.MatKhau != model.Password.ToMd5Hash(khachhang.RandomKey))
                        {
                            ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                        }
                        else
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, khachhang.Email),
                                new Claim(ClaimTypes.Name, khachhang.HoTen),
                                new Claim(MySetting.CLAIM_ID_KH, khachhang.MaKh),
                                new Claim(ClaimTypes.Role,"Customer")
                            };
                            var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                            await HttpContext.SignInAsync(claimsPrincipal);
                            if(Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }
            return View();
        }

        #endregion

        // Thông tin người dùng

        [Authorize]
        public IActionResult Profile()
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_ID_KH).Value;
            var kh = db.KhachHangs.SingleOrDefault(p => p.MaKh == customerId);
            if(kh != null)
            {
                ViewBag.KhachHang = kh;
            }
            return View();
        }

        // Đăng xuất

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            HttpContext.Session.Remove(MySetting.CART_KEY);
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        // Check email xem có tồn tại không

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        //public bool VerifyEmail(string emailVerify)
        //{
        //    using (WebClient webclient = new WebClient())
        //    {
        //        string url = "http://verify-email.org/";
        //        NameValueCollection formData = new NameValueCollection();
        //        formData["check"] = emailVerify;
        //        byte[] responseBytes = webclient.UploadValues(url, "POST", formData);
        //        string response = Encoding.ASCII.GetString(responseBytes);
        //        if (response.Contains("Result: Ok"))
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //}

        // Lấy mật khẩu

        [HttpGet]
        public IActionResult LayMatKhau()
        {
            return View();
        }
        [HttpPost]
        public IActionResult LayMatKhau(LayMatKhauVM model, string email)
        {
            email = HttpContext.Session.GetString("Email");
            var khachhang = db.KhachHangs.SingleOrDefault(p => p.Email == email);

            // Kiểm tra xem khách hàng có tồn tại và email trùng khớp hay không
            if (khachhang != null && string.Equals(khachhang.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                // Thực hiện các thao tác khác với đối tượng khachhang
                if (model.MatKhauMoi != model.ConfirmMK)
                {
                    TempData["Message"] = "Mật khẩu không trùng nhau";
                }
                else
                {
                    khachhang.MatKhau = model.MatKhauMoi;
                    db.Update(khachhang);
                    db.SaveChanges();
                    return Redirect("/");
                }
            }
            else
            {
                TempData["Message"] = "Email không tồn tại hoặc không trùng khớp";
            }

            return View("LayMatKhau");
        }

        // Quên mật khẩu

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> QuenMatKhau(LayMatKhauVM model,string email)
        {
            if (!IsValidEmail(email) )
            {
                // Xử lý lỗi, có thể redirect về trang nhập lại email hoặc hiển thị thông báo lỗi.
                TempData["Message"] = "Email không tồn tại";
                return View("QuenMatKhau");
            }
            //bool check = VerifyEmail(email);
            //if (check == false)
            //{
            //    TempData["Message"] = "Email không tồn tại";
            //    return View("QuenMatKhau");
            //}
            var khachhang = db.KhachHangs.SingleOrDefault(p => p.Email == email);

            if (khachhang != null)
            {
                HttpContext.Session.Set("Email",model.Email);
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("doducviet3012@gmail.com", "ebfwregutahnwhrj"),
                    EnableSsl = true,

                };
                var resetLink = Url.Action("LayMatKhau", "KhachHang", new { email }, Request.Scheme);

                // Tạo nội dung email với đường dẫn đặt lại mật khẩu
                var emailContent = $"Nhấp vào <a href=\"{resetLink}\">đây</a> để đặt lại mật khẩu:";
                var fromAddress = new MailAddress("doducviet3012@gmail.com", "Hshop");

                // Tạo địa chỉ email người nhận
                var toAddress = new MailAddress(email);

                // Tạo đối tượng MailMessage
                var mailMessage = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Đặt Lại Mật Khẩu",
                    Body = emailContent,
                    IsBodyHtml = true // Đặt true nếu bạn sử dụng HTML trong nội dung email
                };

                // Gửi email
                smtpClient.Send(mailMessage);
                TempData["Message"] = "Vui lòng check email của bạn";
                return View("QuenMatKhau");
            }
            TempData["Message"] = "Email không tồn tại";
            // Redirect hoặc hiển thị thông báo thành công.
            return View("QuenMatKhau");
        }
        // Đổi mật khẩu
        [HttpGet]
        public IActionResult DoiMK()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DoiMK(LayMatKhauVM model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString("username");
                var khachhang = db.KhachHangs.SingleOrDefault(p => p.MaKh == username);
                if(khachhang.MatKhau != model.MatKhauCu.ToMd5Hash(khachhang.RandomKey))
                {
                    TempData["Message"] = "Mật khẩu hiện tại không đúng!";
                }
                if(model.MatKhauMoi != model.ConfirmMK)
                {
                    TempData["Message"] = "Mật khẩu không trùng nhau!";
                    return View();
                }
                else
                {
                    khachhang.MatKhau = model.MatKhauMoi.ToMd5Hash(khachhang.RandomKey);
                    db.Update(khachhang);
                    db.SaveChanges();
                    return Redirect("/");
                }
            }
            return View();
        }
    }
}
