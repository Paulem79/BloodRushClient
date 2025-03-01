using BloodRushClient;
using SevenZipExtractor;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BloodRushInstaller.utils
{
    public class ZipManager
    {
        public static void ExtractFile(string sourceArchive, string destination, Label label2, ProgressBar progressBar1, DownloadPopup downloadPopup)
        {
            using (ArchiveFile archiveFile = new ArchiveFile(sourceArchive, AppDomain.CurrentDomain.BaseDirectory + @"7zip/7z.dll"))
            {
                archiveFile.Extract(destination, true, (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 100;

                        double bytesIn = double.Parse(e.Completed.ToString());
                        double totalBytes = double.Parse(e.Total.ToString());
                        double percentage = bytesIn / totalBytes * 100;
                        label2.Content = percentage.ToString("N2") + "%";
                        progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
                    });
                });
            }
        }
    }
}
