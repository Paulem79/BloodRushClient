using BloodRushInstaller.utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows;

namespace BloodRushClient
{
    /// <summary>
    /// Logique d'interaction pour DownloadPopup.xaml
    /// </summary>
    public partial class DownloadPopup : Window
    {
        public static readonly string baseDownloadURL =
            "https://github.com/Paulem79/BloodRush-Archives/raw/refs/heads/main/";
        public static PopupActionType currentActionType = PopupActionType.Download;

        private DateTime lastUpdate;
        private long lastBytes = 0;

        public DownloadPopup(PopupActionType actionType, GameLocation game)
        {
            Closing += DownloadPopup_FormClosing;

            currentActionType = actionType;
            InitializeComponent();
            if (actionType == PopupActionType.Download)
            {
                label1.Content = "Téléchargement des données du jeu...";
                informations.Content = "Vous pouvez faire autre chose en attendant";

                Thread thread = new Thread(() =>
                {
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadProgressChanged += (sender, e) => downloadSpeed(e.BytesReceived);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(HttpUtility.UrlPathEncode(baseDownloadURL + game.version + ".7z")), game.path);
                });
                thread.Start();
            }
            else if (actionType == PopupActionType.Extract)
            {
                label1.Content = "Extraction des données du jeu...";
                informations.Content = "Vous pouvez faire autre chose en attendant";

                //Directory.Delete(game.folderDest, true);

                Thread t1 = new Thread(() =>
                {
                    ZipManager.ExtractFile(game.path, game.folderDest + "/" + game.version, label2, progressBar1, this);
                });
                t1.Start();

                new Thread(() =>
                {
                    t1.Join();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        File.Delete(game.path);
                        currentActionType = PopupActionType.Play;
                        this.Close();
                    });
                }).Start();
            }

            void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    double bytesIn = double.Parse(e.BytesReceived.ToString());
                    double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    double percentage = bytesIn / totalBytes * 100;
                    int progression = int.Parse(Math.Truncate(percentage).ToString());
                    label2.Content = percentage.ToString("N2") + "%";
                    progressBar1.Value = progression;
                }));
            }

            void downloadSpeed(long bytes)
            {
                if (lastBytes == 0)
                {
                    lastUpdate = DateTime.Now;
                    lastBytes = bytes;
                }
                else
                {
                    var now = DateTime.Now;
                    var timeSpan = now - lastUpdate;
                    var bytesChange = bytes - lastBytes;

                    if (timeSpan.Seconds != 0)
                    {
                        var bytesPerSecond = bytesChange / timeSpan.Seconds;

                        lastBytes = bytes;
                        lastUpdate = now;

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            informations.Content = (bytesPerSecond / 1e+6).ToString("N2") + "Mb/s";
                        }));
                    }
                }
            }

            void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    label2.Content = "Terminé !";

                    currentActionType = PopupActionType.Play;
                    this.Close();
                    new DownloadPopup(PopupActionType.Extract, game).ShowDialog();
                }));
            }
        }

        private void DownloadPopup_FormClosing(object sender, CancelEventArgs e)
        {
            if (currentActionType == PopupActionType.Extract || currentActionType == PopupActionType.Download)
            {
                MessageBoxResult d =
                    MessageBox.Show("Fermer l'application pendant cet opération causera un problème, souhaitez-vous vraiment quitter ?", "Fermeture", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (d == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else if (d == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }

    public enum PopupActionType
    {
        None,
        Download,
        Extract,
        Play
    }
}
