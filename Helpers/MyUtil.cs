using System.Text;

namespace ThuongMaiDT.Helpers
{
    public class MyUtil
    {
        public static string UploadHinh(IFormFile Hinh , string folder)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, Hinh.FileName);
                using (var myfile = new FileStream(fullPath, FileMode.CreateNew))
                {
                    Hinh.CopyTo(myfile);
                }
                return Hinh.FileName;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public static string GenerateRandomKey(int length = 5)
        {
            var pattern = @"wjdfifskswnmscjskskskJNDJFKFSFKVMKDFMKK!@*%^&";
            var sb = new StringBuilder();
            var rd = new Random();
            for(int i=0;i<length; i++)
            {
                sb.Append(pattern[rd.Next(0,pattern.Length)]);
            }
            return sb.ToString();
        }
    }
}
