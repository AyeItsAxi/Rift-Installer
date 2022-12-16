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
    public partial class OlderBuildInstallPage : System.Windows.Controls.Page
    {
        public OlderBuildInstallPage()
        {
            InitializeComponent();
            if (!Directory.Exists("Rift"))
            {
                VersionText.Visibility = Visibility.Hidden;
                Burger.Visibility = Visibility.Hidden;
                Info.Margin = new Thickness(0, 129, 0, 0);
                InstallButton.IsEnabled = false;
                InstallButton.Content = "Unavailable";
                Info.Text = "You need to install the current version of Rift to be able to use this feature. Go back to the main page and install Rift to use this feature.";
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
            if (Directory.Exists("Rift.Old"))
            {
                MessageBoxResult UserConfirm = MessageBox.Show("It seems you already have an old version of Rift installed. Installing a different old version will overrite the currently installed old version of Rift. Are you sure?", "Rift Installer: Notice", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (UserConfirm == MessageBoxResult.Yes)
                {
                    //if anybody knows a better way of doing this please open a pull request
                    Directory.Delete("Rift.Old", true);
                    if (Burger.SelectedItem == ttoz)
                    {
                        DownloadManager(JSD.ttoz);
                    }
                    if (Burger.SelectedItem == ttoo)
                    {
                        DownloadManager(JSD.ttoo);
                    }
                    if (Burger.SelectedItem == ttotw)
                    {
                        DownloadManager(JSD.ttotw);
                    }
                    if (Burger.SelectedItem == ttoth)
                    {
                        DownloadManager(JSD.ttoth);
                    }
                    if (Burger.SelectedItem == ttof)
                    {
                        DownloadManager(JSD.ttof);
                    }
                    if (Burger.SelectedItem == ttofv)
                    {
                        DownloadManager(JSD.ttofv);
                    }
                    if (Burger.SelectedItem == ttofvb)
                    {
                        DownloadManager(JSD.ttofvb);
                    }
                }
                else if (UserConfirm == MessageBoxResult.No)
                {

                }
            }
            else if (!Directory.Exists("Rift.Old"))
            {
                //if anybody knows a better way of doing this please open a pull request
                if (Burger.SelectedItem == ttoz)
                {
                    DownloadManager(JSD.ttoz);
                }
                if (Burger.SelectedItem == ttoo)
                {
                    DownloadManager(JSD.ttoo);
                }
                if (Burger.SelectedItem == ttotw)
                {
                    DownloadManager(JSD.ttotw);
                }
                if (Burger.SelectedItem == ttoth)
                {
                    DownloadManager(JSD.ttoth);
                }
                if (Burger.SelectedItem == ttof)
                {
                    DownloadManager(JSD.ttof);
                }
                if (Burger.SelectedItem == ttofv)
                {
                    DownloadManager(JSD.ttofv);
                }
                if (Burger.SelectedItem == ttofvb)
                {
                    DownloadManager(JSD.ttofvb);
                }
            }
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
                string gzip = "Rift.Old.zip";
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
                string gzip = "Rift.Old.zip";
                ZipFile.ExtractToDirectory(gzip, @"./Rift.Old");
                File.Delete(gzip);
                IShellLink link = (IShellLink)new ShellLink();
                link.SetDescription("Old Rift Launcher");
                link.SetPath(Environment.CurrentDirectory + "\\Rift.Old\\Rift.exe");
                link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift.Old");
                link.SetIconLocation(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico", 0);
                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "Old Rift.lnk"), false);
                Status.Text = "Old version installed! You can install a different old version but this will overrite the currently installed old version.";
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
