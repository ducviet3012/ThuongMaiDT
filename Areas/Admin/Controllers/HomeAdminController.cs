using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using ThuongMaiDT.Models;
using X.PagedList;

namespace ThuongMaiDT.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    public class HomeAdminController : Controller
    {
        Hshop2023Context db = new Hshop2023Context();
        public IActionResult Index()
        {
            return View();
        }

        // Thông tin khách hàng 
        [Route("khachhang")]
        public IActionResult KhachHang(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;                                                           
            var khachhang = db.KhachHangs.AsNoTracking().OrderBy(p => p.MaKh);
            PagedList<KhachHang> lst = new PagedList<KhachHang>(khachhang, pageNumber,pageSize);
            return View(lst);
        }

        // Thông tin hóa đơn
        [Route("donhang")]
        public IActionResult DonHang(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var hoadon = db.HoaDons.AsNoTracking().OrderBy(p => p.MaHd);
            PagedList<HoaDon> lst = new PagedList<HoaDon>(hoadon, pageNumber, pageSize);
            return View(lst);
        }

        [Route("updatedonhang")]
        [HttpGet]
        public IActionResult UpdateDonHang(int mahd)
        {
            ViewBag.MaKh = new SelectList(db.KhachHangs.ToList(),"MaKh","MaKh");
            ViewBag.MaTrangThai = new SelectList(db.TrangThais.ToList(), "MaTrangThai","TenTrangThai");
            ViewBag.MaNv = new SelectList(db.NhanViens.ToList(), "MaNv","MaNv");
            var sp = db.HoaDons.Find(mahd);
            return View(sp);
        }

        [Route("updatedonhang")]
        [HttpPost]
        public IActionResult UpdateDonHang(HoaDon hoadon)
        {
            if(ModelState.IsValid)
            {
                if(hoadon.MaTrangThai == 2)
                {
                    var email = HttpContext.Session.GetString("Email");
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("doducviet3012@gmail.com", "ebfwregutahnwhrj"),
                        EnableSsl = true,

                    };

                    // Tạo nội dung email với đường dẫn đặt lại mật khẩu
                    var emailContent = $"Đơn hàng của bạn đang được giao";
                    var fromAddress = new MailAddress("doducviet3012@gmail.com", "Hshop");

                    // Tạo địa chỉ email người nhận
                    var toAddress = new MailAddress(email);

                    // Tạo đối tượng MailMessage
                    var mailMessage = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = "Đơn hàng",
                        Body = emailContent,
                        IsBodyHtml = true // Đặt true nếu bạn sử dụng HTML trong nội dung email
                    };

                    // Gửi email
                    smtpClient.Send(mailMessage);
                }
                db.Update(hoadon);
                db.SaveChanges();
                return RedirectToAction("Index","HomeAdmin");
            }
            return View();
        }
    }
}
