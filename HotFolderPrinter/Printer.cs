using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFolderPrinter {
    public class Printer {
        public string PrinterName { get; set; }
        public string PaperSizeName { get; set; }
        internal PaperSize PaperSize { get; set; }

    }
}
