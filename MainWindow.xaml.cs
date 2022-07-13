using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.IO.Compression;
using System.Threading.Tasks;

namespace RiftInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Directory.Exists("Rift-2.2.0.5-B"))
            {
                InstallButton.IsEnabled = false;
                InstallButton.Content = "Installed";
            }
        }

        private void DragBar(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            DownloadManager();
        }

        private void DownloadManager()
        {
            try
            {
                string gzip = "Rift-2.2.0.5-B.zip";
                WebClient webclient = new WebClient();
                webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadRiftCompletedCallback);
                webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(RiftDownloadProgressChanged);
                webclient.DownloadFileAsync(new Uri("https://cdn.discordapp.com/attachments/972611854524354570/972611918885953546/Rift-2.2.0.5-B.zip"), gzip);
                InstallButton.Content = "Installing...";
                InstallButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        public void RiftDownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            Status.Text = "Status: Installing Rift - " + e.ProgressPercentage.ToString() + "%";
        }
        private void DownloadRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string gzip = "Rift-2.2.0.5-B.zip";
                ZipFile.ExtractToDirectory(gzip, @"./Rift-2.2.0.5-B");
                File.Delete(gzip);
                DotNetDownload();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        private void DotNetDownload()
        {
            try
            {
                string gzip = "dotnet.exe";
                WebClient webclient = new WebClient();
                webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadDotNetCompletedCallback);
                webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DotNetDownloadProgressChanged);
                webclient.DownloadFileAsync(new Uri("https://download.visualstudio.microsoft.com/download/pr/962fa33f-e57c-4e8a-abc9-01882ff74e3d/23e11ee6c3da863fa1489f951aa7e75e/dotnet-sdk-3.1.417-win-x64.exe"), gzip);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        public void DotNetDownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            Status.Text = "Status: Downloading .Net - " + e.ProgressPercentage.ToString() + "%";
        }
        private void DownloadDotNetCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string name = "dotnet.exe";
                Process.Start(name);
                MessageBox.Show("Because of how installing .Net works, you will have to click ok on this message box when you have finished installing .Net", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                File.Delete(name);
                SetEnvironmentVariable();
                MessageBox.Show("Rift has been successfully installed to " + Environment.CurrentDirectory + "\\Rift-2.2.0.5-B", "Download finished!", MessageBoxButton.OK);
                Status.Text = "";
                InstallButton.Content = "Installed";
                InstallButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Finishing the download failed! Error message: " + ex.Message);
                this.Close();
            }
        }

        private void SetEnvironmentVariable()
        {
            Status.Text = "Checking if CPU fix is needed";
            if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENSSL_ia32cap")))
            {
                Status.Text = "CPU fix is needed, applying fix";
                Environment.SetEnvironmentVariable("OPENSSL_ia32cap", "~0x20000000", EnvironmentVariableTarget.Machine);
                Status.Text = "CPU fix applied";
            }
            Status.Text = "CPU fix is not needed, finishing up";
        }
    }
}
