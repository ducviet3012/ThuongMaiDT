using AutoMapper;
using ThuongMaiDT.Models;
using ThuongMaiDT.ViewModels;

namespace ThuongMaiDT.Helpers
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile() {
            CreateMap<RegisterVM, KhachHang>();
            //.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen)).ReverseMap();
        }
    }
}
