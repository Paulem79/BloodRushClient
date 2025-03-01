using BloodRushInstaller.utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Configuration;
using System.Collections.Specialized;
using static System.Windows.Forms.AxHost;
using System.Reflection;

namespace BloodRushClient
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string steamDemoFolderPath;

        public static string indexUrl = "https://raw.githubusercontent.com/Paulem79/BloodRush-Archives/refs/heads/main/index.txt";
        public static List<GameLocation> versions = new List<GameLocation>();

        public MainWindow()
        {
            InitializeComponent();

            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"files");

            Configuration configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = configManager.AppSettings.Settings;

            steamDemoFolderPath = confCollection["steamPath"].Value;
            steamPath.Text = steamDemoFolderPath;

            var result = new HttpClient().GetStringAsync(indexUrl).Result;
            string[] lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                string fileName = parts[0];
                string name = Utils.RemoveStr(fileName, ".7z");
                versions.Add(new GameLocation(AppDomain.CurrentDomain.BaseDirectory + @"files/" + fileName, name, false));
            }

            string steamFolderPath = GetSteamDemo();
            if (steamFolderPath != null)
            {
                versions.Add(new GameLocation(steamFolderPath, "Demo", true));
            }

            comboBox1.ItemsSource = versions.ConvertAll(version => version.version).ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner une version", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GameLocation selectedVersion = versions.Find(v =>
            {
                return v.version == comboBox1.SelectedValue.ToString();
            });

            string relPath = "/" + selectedVersion.version;

            if (selectedVersion.demo)
            {
                relPath = "";

                selectedVersion.path = GetSteamDemo();
                selectedVersion.folderDest = GetSteamDemo();
            }

            string launchPath = selectedVersion.folderDest + relPath + "/Hellscape.exe";

            if (!File.Exists(launchPath))
            {
                launchPath = selectedVersion.folderDest + relPath + "/BloodRush.exe";
            }

            if (!File.Exists(launchPath) && !selectedVersion.demo)
            {
                new DownloadPopup(PopupActionType.Download, selectedVersion).ShowDialog();
                button1_Click(sender, e);
                return;
            }

            if(!File.Exists(launchPath))
            {
                MessageBox.Show("Impossible de trouver les fichiers du jeu pour la version " + selectedVersion.version + " !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProcessStartInfo pro = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = launchPath
            };
            Console.WriteLine("Lancement de " + pro.FileName);
            Process.Start(pro);
        }

        private void browseFolderSteam_Click(object sender, EventArgs e)
        {
            var steamBrowseDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (GetSteamDemo() != null)
            {
                steamBrowseDialog.InitialDirectory = GetSteamDemo();
            }

            if (steamBrowseDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                steamPath.Text = steamBrowseDialog.FileName;
                steamDemoFolderPath = GetSteamDemo();
            }
        }

        private string GetSteamDemo()
        {
            string directory;
            if (steamPath.Text == "")
            {
                directory = Utils.ProgramFilesx86() + @"\Steam\steamapps\common\Blood Rush Demo";
            }
            else
            {
                directory = steamPath.Text;
            }

            if (!Directory.Exists(directory))
            {
                return null;
            }

            Configuration configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = configManager.AppSettings.Settings;

            confCollection["steamPath"].Value = directory;

            configManager.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configManager.AppSettings.SectionInformation.Name);

            return directory;
        }

        private void znsMods_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://gamejolt.com/games/br_mods/976386");
        }
    }
}