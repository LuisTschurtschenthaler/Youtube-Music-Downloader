using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace Youtube_Music_Downloader {
    public partial class MainWindow : Window {

        private string defaultDownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Downloads\\";


        public MainWindow() {
            InitializeComponent();
            initDownloadFolder();
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

        private void UI_AddToDownload(object sender, RoutedEventArgs e) {

        }

        private void UI_SelectExtensionChanged(object sender, SelectionChangedEventArgs e) {

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

        private void UI_StartDownload(object sender, RoutedEventArgs e) {

        }

        private void UI_DownloadFolder_LostFocus(object sender, RoutedEventArgs e) {

        }
    }
}
