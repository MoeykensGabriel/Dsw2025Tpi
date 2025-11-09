using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public class ProductSummaryModel
    {
        public int Total { get; set; }
        public int Activos { get; set; }
        public int Inactivos { get; set; }
        public int BajoStock { get; set; }
    }
}
