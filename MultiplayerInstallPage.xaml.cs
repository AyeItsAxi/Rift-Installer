using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Threading.Tasks;
using SevenZipExtractor;

namespace RiftInstaller
{
    /// <summary>
    /// Interaction logic for OlderBuildInstallPage.xaml
    /// </summary>
    public partial class MultiplayerInstallPage : Page
    {
        public MultiplayerInstallPage()
        {
            InitializeComponent();
            CheckRift();
        }

        //Allows for asynchronous detection of Rift install. If Rift is installed while the user is on the page, it automatically updates the content.
        private async void CheckRift()
        {
            for (; ; )
            {
                if (Directory.Exists("Rift"))
                {
                    Info.Margin = new Thickness(0, 0, 0, 0);
                    Info.Text = "Here, you can download the multiplayer version of Rift. You CAN NOT host multiplayer through Rift, this is just so you can play whenever Jake hosts Rift multiplayer. Just click install and we will automatically download the latest version and get you set up for multiplayer.";
                    InstallButton.IsEnabled = true;
                    InstallButton.Content = "Install";
                    break;
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }

        private void DragBar(object sender, MouseButtonEventArgs e)
        {
            Services.Static.applicationFrame.DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            string JSData = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\strings.json");
            Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
            DownloadManager(JSD.MPClient);
        }
        public void DownloadManager(string InstallURL)
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
                string gzip = "Rift.MP.zip";
                WebClient webclient = new();
                webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadRiftCompletedCallback);
                webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(RiftDownloadProgressChanged);
                webclient.DownloadFileAsync(new Uri(InstallURL), gzip);
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
        public void FortniteDownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            Status.Text = "Status: Installing Fortnite 7.20 - " + e.ProgressPercentage.ToString() + "%";
        }
        private void DownloadFortniteCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                var extractor = new ArchiveFile("Fortnite.zip");
                extractor.Extract("Fortnite 7.20");
                File.Delete("Fortnite.zip");
                Services.CobaltManage.AddInstallToCobaltConfigurationManagement(Environment.CurrentDirectory + "\\Fortnite 7.20");
                IShellLink link = (IShellLink)new ShellLink();
                link.SetDescription("Riftmas Launcher");
                link.SetPath(Environment.CurrentDirectory + "\\Rift.MP\\Rift.exe");
                link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift.MP");
                link.SetIconLocation(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico", 0);
                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "Rift Multiplayer.lnk"), false);
                Status.Text = "Rift Multiplayer installed!";
                InstallButton.IsEnabled = false;
                InstallButton.Content = "Installed";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        private void DownloadRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string gzip = "Rift.MP.zip";
                ZipFile.ExtractToDirectory(gzip, @"./Rift.MP");
                File.Delete(gzip);
                MessageBoxResult _ = MessageBox.Show("Rift Multiplayer has been installed! Do you already have Fortnite 7.20 installed?", "Rift Installer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (_ == MessageBoxResult.No)
                {
                    MessageBox.Show("The Rift Installer is now installing Fortnite 7.20. This can take up to 45GB of disk space, so please make sure you have enough space to install it." + Environment.NewLine + "Please note this download can take any amount of time, it depends upon how fast your internet connection is.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                    WebClient webclient = new();
                    webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFortniteCompletedCallback);
                    webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(FortniteDownloadProgressChanged);
                    webclient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1yy_4KYFfimphCVte9gxWo3fz7ThPYQ1e?alt=media&key=AIzaSyD3hsuSxEFnxZkgadbUSPt_iyx8qJ4lwWQ"), "Fortnite.zip");
                }
                else
                {
                    IShellLink link = (IShellLink)new ShellLink();
                    link.SetDescription("Riftmas Launcher");
                    link.SetPath(Environment.CurrentDirectory + "\\Rift.MP\\Rift.exe");
                    link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift.MP");
                    link.SetIconLocation(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico", 0);
                    IPersistFile file = (IPersistFile)link;
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    file.Save(Path.Combine(desktopPath, "Rift Multiplayer.lnk"), false);
                    Status.Text = "Rift Multiplayer installed!";
                    InstallButton.IsEnabled = false;
                    InstallButton.Content = "Installed";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void ColHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Services.Static.applicationFrame.pageHost.Visibility = Visibility.Hidden;
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
