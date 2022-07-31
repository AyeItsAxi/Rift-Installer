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

namespace RiftInstaller
{
    /// <summary>
    /// Interaction logic for OlderBuildInstallPage.xaml
    /// </summary>
    public partial class MultiplayerInstallPage : Window
    {
        public MultiplayerInstallPage()
        {
            InitializeComponent();
            if (!Directory.Exists("Rift"))
            {
                Info.Margin = new Thickness(0, 129, 0, 0);
                InstallButton.IsEnabled = false;
                InstallButton.Content = "Unavailable";
                Info.Text = "You need to install the current version of Rift to be able to use this feature. Go back to the main page and install Rift to use this feature.";
            }
        }

        private void DragBar(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            string JSData = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\strings.json");
            Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
            DownloadManager(JSD.Latest);
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
        private void DownloadRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string JSData = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\strings.json");
                Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
                string gzip = "Rift.MP.zip";
                ZipFile.ExtractToDirectory(gzip, @"./Rift.MP");
                File.Delete(gzip);
                File.Delete(@"./Rift.MP/Yosemite.dll");
                WebClient wc = new();
                wc.DownloadFile(JSD.MPClient, @"./Rift.MP/Yosemite.dll");
                IShellLink link = (IShellLink)new ShellLink();
                link.SetDescription("Rift Multiplayer Launcher");
                link.SetPath(Environment.CurrentDirectory + "\\Rift.MP\\Rift.exe");
                link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift.MP");
                link.SetIconLocation(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico", 0);
                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "Rift Multiplayer.lnk"), false);
                Status.Text = "Rift Multiplayer installed!";
                InstallButton.IsEnabled = true;
                InstallButton.Content = "Install";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void ColHomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow MW = new();
            this.Hide();
            MW.Show();
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
