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
                PrinterName = "Adobe PDF",
                PaperSizeName = "4x6"
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

                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = hotFolder.PrinterName;
                foreach (PaperSize item in pd.PrinterSettings.PaperSizes) {
                    Console.WriteLine("Size: {0} ({1}x{2})", item.PaperName, item.Width, item.Height);

                    if (item.PaperName == hotFolder.PaperSizeName) {
                        hotFolder.PaperSize = item;
                        Console.WriteLine("   ^--- Picked this one.");
                    }

                }

                if (hotFolder.PaperSize == null)
                    throw new InvalidOperationException(String.Format("Could not find paper '{1}' size for printer '{0}'.", hotFolder.PrinterName, hotFolder.PaperSizeName));

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
                        File.Move(originalFileName, fileName);
                      
                    }

                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = hotFolder.PrinterName;
                    pd.DocumentName = Path.GetFileName(fileName);
                    pd.DefaultPageSettings.PaperSize = hotFolder.PaperSize;
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrinterSettings.DefaultPageSettings.PaperSize = hotFolder.PaperSize;

                    pd.PrintPage += (sender, e) => {
                        Image i = Image.FromFile(fileName);
                        e.Graphics.DrawImage(i, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
                    };
                    pd.Print();

                }


                Thread.Sleep(100);
            }

        }

    }
}
