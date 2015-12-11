using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing;

namespace HotFolderPrinter {
    class Program {

        static ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();

        static void Main(string[] args) {

            string printerName = "Adobe PDF";
            string paperSizeName = "4x6";

            FileSystemWatcher fsw = new FileSystemWatcher(@"C:\UUMCSanta\Incoming", "*.jpg");
            fsw.Created += Fsw_Created;
            fsw.EnableRaisingEvents = true;

            PaperSize paperSize = null;

            {

                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = printerName;
                foreach (PaperSize item in pd.PrinterSettings.PaperSizes) {
                    Console.WriteLine("Size: {0} ({1}x{2})", item.PaperName, item.Width, item.Height);

                    if (item.PaperName == paperSizeName) {
                        paperSize = item;
                        Console.WriteLine("   ^--- Picked this one.");
                    }

                }
            }

            while (true) {
                string fileToProcess;

                if (fileQueue.TryDequeue(out fileToProcess)) {

                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = printerName;
                    pd.DocumentName = Path.GetFileName(fileToProcess);
                    pd.DefaultPageSettings.PaperSize = paperSize;
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrinterSettings.DefaultPageSettings.PaperSize = paperSize;

                    pd.PrintPage += (sender, e) => {

                        float page_w = e.PageBounds.Width;
                        float page_h = e.PageBounds.Height;



                        Image i = Image.FromFile(fileToProcess);
                        e.Graphics.DrawImage(i, 0, 0, page_w, page_h);
                    };
                    pd.Print();


                }


                Thread.Sleep(100);
            }

        }


        private static void Fsw_Created(object sender, FileSystemEventArgs e) {
            Console.WriteLine("File created: {0}", e.Name);
            fileQueue.Enqueue(e.FullPath);
        }
    }
}
