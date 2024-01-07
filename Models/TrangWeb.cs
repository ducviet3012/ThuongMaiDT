using System;
using System.Collections.Generic;

namespace ThuongMaiDT.Models
{
    public partial class TrangWeb
    {
        public TrangWeb()
        {
            PhanQuyens = new HashSet<PhanQuyen>();
        }

        public int MaTrang { get; set; }
        public string TenTrang { get; set; } = null!;
        public string Url { get; set; } = null!;

        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}
