using Microsoft.AspNetCore.Mvc;
using ThuongMaiDT.Models;
using ThuongMaiDT.ViewModels;
using ThuongMaiDT.Helpers;
using Microsoft.AspNetCore.Authorization;
using ThuongMaiDT.Services;
using System.Net.Mail;
using System.Net;
using RazorEngine;
using RazorEngine.Templating;
using System.Net.Mime;

namespace ThuongMaiDT.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayService;
        public CartController(Hshop2023Context context, IVnPayService vnPayService)
        {
            db = context;
            _vnPayService = vnPayService;
        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new
            List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p => p.Mahh == id);
            if (item == null)
            {
                var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hanghoa == null)
                {
                    TempData["Message"] = "Không tìm thấy hàng hóa";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    Mahh = hanghoa.MaHh,
                    Tenhh = hanghoa.TenHh,
                    DonGia = hanghoa.DonGia ?? 0,
                    Hinh = hanghoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                giohang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, giohang);
            //var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_ID_KH).Value;
            //var khachhang = db.KhachHangs.SingleOrDefault(p => p.MaKh == customerId);
            //if (khachhang != null)
            //{
            //    var gioHang = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);

            //    if (gioHang != null && gioHang.Any())
            //    {
            //        return RedirectToAction("Index");
            //    }
            //}

            return RedirectToAction("Index");
        }
        public IActionResult GiamSL(int id, int quantity = 1)
        {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p => p.Mahh == id);
            if (item != null)
            {
                // Tăng số lượng sản phẩm
                item.SoLuong -= quantity;

                if (item.SoLuong <= 0)
                {
                    giohang.Remove(item);
                }
                HttpContext.Session.Set(MySetting.CART_KEY, giohang);
            }
            return RedirectToAction("Index");
        }
        public IActionResult TangSL(int id, int quantity = 1)
        {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p => p.Mahh == id);
            if (item != null)
            {
                // Tăng số lượng sản phẩm
                item.SoLuong += quantity;

                if (item.SoLuong <= 0)
                {
                    giohang.Remove(item);
                }
                HttpContext.Session.Set(MySetting.CART_KEY, giohang);
            }
            return RedirectToAction("Index");
        }
        public IActionResult RemoveCart(int id)
        {
            var giohang = Cart;
            var item = giohang.SingleOrDefault(p => p.Mahh == id);
            if (item != null)
            {
                giohang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, giohang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult CheckOut()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }
            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult CheckOut(CheckOutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_ID_KH).Value;
                var khachhang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachhang = db.KhachHangs.SingleOrDefault(p => p.MaKh == customerId);
                }
                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachhang.HoTen,
                    DiaChi = model.DiaChi ?? khachhang.DiaChi,
                    SoDienThoai = model.DienThoai ?? khachhang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = payment == "Thanh toán VNPay" ? "VNPay" : "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = payment == "Thanh toán VNPay" ? 1 : 0,
                    GhiChu = model.GhiChu
                };
                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(hoadon);
                    db.SaveChanges();
                    var cthd = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthd.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.Mahh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(cthd);
                    db.SaveChanges();
                    //HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("doducviet3012@gmail.com", "ebfwregutahnwhrj"),
                        EnableSsl = true,

                    };

                    // Tạo nội dung email với đường dẫn đặt lại mật khẩu
                    double tongtien = 0;
                    var strSanPham = "<table border='1'>";
                    strSanPham += "<thead><tr><th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead>";
                    strSanPham += "<tbody>";

                    foreach (var item in Cart)
                    {
                        strSanPham += "<tr>";
                        strSanPham += $"<td>{item.Tenhh}</td>";
                        strSanPham += $"<td>{item.SoLuong}</td>";
                        strSanPham += $"<td>{item.DonGia}</td>";
                        strSanPham += $"<td>{item.ThanhTien}</td>";
                        strSanPham += "</tr>";
                        tongtien += (double)item.ThanhTien;
                    }

                    strSanPham += "</tbody></table>";
                    strSanPham += $"<p><strong>Tổng tiền: {tongtien}</strong></p>";
                    var strThongTinKhachHang = $@"
                        <p>Họ tên khách hàng: {khachhang.HoTen}</p>
                        <p>Địa chỉ: {khachhang.DiaChi}</p>
                        <p>Số điện thoại: {khachhang.DienThoai}</p>
                        ";
                    var fullContent = $@"
                         <h2>Thông tin đơn hàng</h2>
                              {strSanPham}
                         <h2>Thông tin khách hàng</h2>
                              {strThongTinKhachHang}
                         ";
                    // Đọc template từ file
                    //var templatePath = "_LayoutMail.cshtml"; // Đường dẫn đến template HTML
                    //var templateContent = System.IO.File.ReadAllText(templatePath);

                    // Render template với dữ liệu Cart
                    //var strSanPham = Engine.Razor.RunCompile(templateContent, "templateKey", typeof(IEnumerable<CartItem>), Cart);
                    var fromAddress = new MailAddress("doducviet3012@gmail.com", "Hshop");

                    // Tạo địa chỉ email người nhận
                    var toAddress = new MailAddress(khachhang.Email);

                    // Tạo đối tượng MailMessage
                    var mailMessage = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = "Đơn hàng được đặt thành công",
                        Body = fullContent,
                        IsBodyHtml = true // Đặt true nếu bạn sử dụng HTML trong nội dung email
                    };

                    // Gửi email
                    //var htmlView = AlternateView.CreateAlternateViewFromString(strSanPham, new ContentType("text/html"));
                    //mailMessage.AlternateViews.Add(htmlView);
                    smtpClient.Send(mailMessage);
                    TempData["Message"] = "Vui lòng check email của bạn";
                    if (payment == "Thanh toán VNPay")
                    {
                        var vnPayModel = new VnPaymentRequestModel
                        {
                            Amount = Cart.Sum(p => p.ThanhTien),
                            CreateDate = DateTime.Now,
                            Desscription = $"{model.HoTen}{model.DienThoai}",
                            FullName = model.HoTen,
                            OrderId = new Random().Next(1000, 10000)
                        };
                        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                    }


                    return View("Success");
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                }
            }
            return View(Cart);
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay:{response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }
            TempData["Message"] = $"Thanh toán VNP thành công";
            return View("Success");
        }
    }
}
