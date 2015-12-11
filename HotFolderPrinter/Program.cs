using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing;
using System.Collections.Generic;

namespace HotFolderPrinter {
    class Program {

        static ConcurrentQueue<Tuple<HotFolder, string>> fileQueue = new ConcurrentQueue<Tuple<HotFolder, string>>();

        static void Main(string[] args) {

            var folderList = new List<HotFolder>();

            folderList.Add(new HotFolder() {
                Path = @"C:\UUMCSanta\Incoming",
                OutputPath = @"C:\UUMCSanta\Printed",
                PrinterPool = new List<Printer>() {
                    new Printer() { PrinterName = "Adobe PDF", PaperSizeName = "4x6"}
                }
            });

            /****/

            foreach (var hotFolder in folderList) {

                #region Spin up the folder watcher
                FileSystemWatcher fsw = new FileSystemWatcher(hotFolder.Path, "*.jpg");
                fsw.Created += (sender, e) => {
                    fileQueue.Enqueue(new Tuple<HotFolder,string>(hotFolder,e.FullPath));
                };

                #endregion

                #region Verify Printer and Paper

                foreach (var printer in hotFolder.PrinterPool) {
                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = printer.PrinterName;
                    foreach (PaperSize item in pd.PrinterSettings.PaperSizes) {
                        Console.WriteLine("Size: {0} ({1}x{2})", item.PaperName, item.Width, item.Height);

                        if (item.PaperName == printer.PaperSizeName) {
                            printer.PaperSize = item;
                            Console.WriteLine("   ^--- Picked this one.");
                        }

                    }

                    if (printer.PaperSize == null)
                        throw new InvalidOperationException(String.Format("Could not find paper '{1}' size for printer '{0}'.", printer.PrinterName, printer.PaperSizeName));

                }

                #endregion

                fsw.EnableRaisingEvents = true;
            }



            while (true) {
                Tuple<HotFolder,string> fileToProcess;

                if (fileQueue.TryDequeue(out fileToProcess)) {


                    HotFolder hotFolder = fileToProcess.Item1;
                    string originalFileName = fileToProcess.Item2;
                    string fileName = originalFileName;

                    if (!string.IsNullOrEmpty(hotFolder.OutputPath)) {
                        if (!Directory.Exists(hotFolder.OutputPath)) {
                            Directory.CreateDirectory(hotFolder.OutputPath);
                        }

                        fileName = Path.Combine(hotFolder.OutputPath, Path.GetFileName(originalFileName));

                        if (File.Exists(fileName)) {
                            string withoutPath = Path.GetFileName(originalFileName);
                            string withoutExtension = Path.GetFileNameWithoutExtension(withoutPath);
                            string extension = Path.GetExtension(withoutPath);

                            int i = 1;
                            do {
                                fileName = Path.Combine(hotFolder.OutputPath, String.Format("{0}_{1}{2}", withoutExtension, i, extension));
                                i++;
                            } while (File.Exists(fileName));

                        }

                        File.Move(originalFileName, fileName);
                      
                    }

                    // Round-Robin... PrintCount increments with each job.
                    //     For a pool size of 3:
                    //       0 % 3 = 0
                    //       1 % 3 = 1
                    //       2 % 3 = 2
                    //       3 % 3 = 0...
                    int printerIndex = hotFolder.PrintCount % hotFolder.PrinterPool.Count;

                    Printer printer = hotFolder.PrinterPool[printerIndex];

                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = printer.PrinterName;
                    pd.DocumentName = Path.GetFileName(fileName);
                    pd.DefaultPageSettings.PaperSize = printer.PaperSize;
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrinterSettings.DefaultPageSettings.PaperSize = printer.PaperSize;

                    pd.PrintPage += (sender, e) => {
                        Image i = Image.FromFile(fileName);
                        e.Graphics.DrawImage(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
                    };
                    pd.Print();

                    hotFolder.PrintCount++;

                }


                Thread.Sleep(100);
            }

        }

    }
}
