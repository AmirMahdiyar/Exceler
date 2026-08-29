using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Configuration
{
    public class ColumnStyle
    {
        public bool IsBold { get; set; } = false;
        public string? NumberFormat { get; set; }
        public Color? BackgroundColor { get; set; }
        public Color? FontColor { get; set; }
    }
}
