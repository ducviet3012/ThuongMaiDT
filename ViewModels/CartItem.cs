namespace ThuongMaiDT.ViewModels
{
    public class CartItem
    {
        public int Mahh { get; set; }
        public string Hinh { get; set; }
        public string Tenhh {  get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => DonGia * SoLuong;
    }
}
