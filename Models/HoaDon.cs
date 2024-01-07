using System;
using System.Collections.Generic;

namespace ThuongMaiDT.Models
{
    public partial class HoaDon
    {
        public int MaHd { get; set; }
        public string MaKh { get; set; } = null!;
        public DateTime NgayDat { get; set; }
        public DateTime? NgayCan { get; set; }
        public DateTime? NgayGiao { get; set; }
        public string? HoTen { get; set; }
        public string DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string CachThanhToan { get; set; } 
        public string CachVanChuyen { get; set; } 
        public double PhiVanChuyen { get; set; }
        public int MaTrangThai { get; set; }
        public string? MaNv { get; set; }
        public string? GhiChu { get; set; }

        public virtual KhachHang MaKhNavigation { get; set; } 
        public virtual NhanVien? MaNvNavigation { get; set; }
        public virtual TrangThai MaTrangThaiNavigation { get; set; } 
    }
}
