using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFolderPrinter {
    public class HotFolder {

        public string Path { get; set; }
        public string PrinterName { get; set; }
        public string PaperSizeName { get; set; }

        public string OutputPath { get; set; }


        internal PaperSize PaperSize { get; set; }
    }
}
