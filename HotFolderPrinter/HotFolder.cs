using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFolderPrinter {
    public class HotFolder {
        public HotFolder() {
            this.PrinterPool = new List<Printer>();
        }
 
        public string Path { get; set; }

        public string OutputPath { get; set; }

        public IList<Printer> PrinterPool { get; set; }

        internal int PrintCount { get; set; }
    }
}
