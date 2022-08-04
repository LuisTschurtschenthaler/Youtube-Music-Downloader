using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace Youtube_Music_Downloader {
    public partial class MainWindow : Window {

        private string defaultDownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Downloads\\";
        private DownloadManager downloadManager = new DownloadManager();


        public MainWindow() {
            InitializeComponent();
            initDownloadFolder();
            UI_Datagrid.ItemsSource = downloadManager.Downloads;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            UI_Stackpanel.Focus();
        }

        private void Window_Closed(object sender, CancelEventArgs e) {
        }


        private void initDownloadFolder() {
            if(Settings.Default.DownloadFolder == "")
                UI_DownloadFolder.Text = defaultDownloadFolder;
            else UI_DownloadFolder.Text = Settings.Default.DownloadFolder;
        }

        private async void UI_AddToDownload(object sender, RoutedEventArgs e) {
            string[] urls = UI_DownloadUrls.Text.Split(new string[] { "\n", "\r", "\r\n", " " }, StringSplitOptions.RemoveEmptyEntries);
            UI_DownloadUrls.Text = "";
            
            try {
                foreach(var url in urls)
                    await downloadManager.AddToDownload(url);

                UI_Datagrid.Items.Refresh();
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void UI_OpenCurrentDownloadFolder(object sender, RoutedEventArgs e) {
            Process.Start("explorer.exe", UI_DownloadFolder.Text);
        }

        private void UI_ResetDownloadFolder(object sender, RoutedEventArgs e) {
            UI_DownloadFolder.Text = defaultDownloadFolder;
            Settings.Default.DownloadFolder = defaultDownloadFolder;
            Settings.Default.Save();
        }

        private void UI_SelectNewDownloadFolder(object sender, RoutedEventArgs e) {
            using(var dialog = new CommonOpenFileDialog()) {
                dialog.IsFolderPicker = true;
                dialog.InitialDirectory = Settings.Default.DownloadFolder;

                var result = dialog.ShowDialog();
                if(result == CommonFileDialogResult.Ok) {
                    UI_DownloadFolder.Text = dialog.FileName;
                    Settings.Default.DownloadFolder = dialog.FileName;
                    Settings.Default.Save();
                }
            }
        }

        private void UI_ClearDownloadList(object sender, RoutedEventArgs e) {

        }

        private async void UI_StartDownload(object sender, RoutedEventArgs e) {
            if(UI_Datagrid.Items.Count == 0) {
                MessageBox.Show("There is nothing to download!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UI_Datagrid.Items.Refresh();
            UI_ButtonStartDownload.IsEnabled = false;

            List<Task> tasks = new List<Task>();
            foreach(var download in downloadManager.Downloads) {
                if(download.Status == Status.Finished || download.Status == Status.Downloading)
                    continue;

                string downloadFolder = Settings.Default.DownloadFolder + "\\";
                if(download.Subfolder != "")
                    downloadFolder += download.Subfolder + "\\";

                if(!Directory.Exists(downloadFolder))
                    Directory.CreateDirectory(downloadFolder);


                tasks.Add(Task.Factory.StartNew(async () => {
                    string fileName = $"{download.Artist.Trim()} - {download.Title.Trim()}";

                    foreach(var file in Directory.GetFiles(downloadFolder, "*.mp3", SearchOption.AllDirectories)) {
                        if(Path.GetFileName(file) == $"{fileName}.mp3") {
                            download.Status = Status.Error_File_Exists;
                            return;
                        }
                    }

                    await downloadManager.StartDownload(download, downloadFolder, fileName);
                }));
            }

            await Task.WhenAll(tasks);
            UI_ButtonStartDownload.IsEnabled = true;
        }

        private void UI_DownloadFolder_LostFocus(object sender, RoutedEventArgs e) {

        }
    }
}
