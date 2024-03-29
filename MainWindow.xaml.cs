﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Input;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices.ComTypes;
using RiftInstaller.Services;

namespace RiftInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MultiplayerInstallPage mpInstallPage;
        public OlderBuildInstallPage obInstallPage;
        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\");
            WebClient webclient = new();
            Services.CobaltManage.CreateModifiableCobaltConfiguration();
            if (File.Exists(Static.XenonSilicon))
            {
                File.Delete(Static.XenonSilicon);
            }
            if (File.Exists("Rift.Old.zip"))
            {
                File.Delete("Rift.Old.zip");
            }
            if (File.Exists("Rift.zip"))
            {
                File.Delete("Rift.zip");
            }
            if (File.Exists("Rift.MP.zip"))
            {
                File.Delete("Rift.MP.zip");
            }
            if (File.Exists("Fortnite.zip"))
            {
                File.Delete("Fortnite.zip");
            }
            mpInstallPage = new MultiplayerInstallPage();
            obInstallPage = new OlderBuildInstallPage();
            Services.Static.applicationFrame = this;
            webclient.DownloadFile(Services.Static.GalliumSilicon, Services.Static.XenonSilicon);
            string JSData = File.ReadAllText(Static.XenonSilicon);
            Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
            if (File.Exists(Static.XenonBase + "RiftClientVersion.json"))
            {
                string JSData2 = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\RiftClientVersion.json");
                Services.RICloud JSD2 = JsonConvert.DeserializeObject<Services.RICloud>(JSData2);
                if (JSD2.ClientVer == JSD.ClientVer)
                {
                    XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
                }
                else if (JSD2.ClientVer != JSD.ClientVer)
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("Rift Installer")
                        .AddText("I detected that your version of Rift is outdated, so I started an update. Your installations and settings save, so there's no need to worry about this.")
                        .SetToastDuration(ToastDuration.Short)
                        .Show();
                    RiftUpdate();
                    XenonEnums.xenonStatus = XenonEnums.XenonStatus.Installing;
                }
            }
            if (Directory.Exists("Rift"))
            {
                InstallButton.IsEnabled = false;
                InstallButton.Content = "Installed";
                XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
            }
        }

        private void DragBar(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            switch (XenonEnums.xenonStatus)
            {
                case XenonEnums.XenonStatus.Installing:
                    MessageBoxResult msb = MessageBox.Show("A download is in progress, and exiting will corrupt it. Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msb == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown(0xE);
                    }
                    break;
                case XenonEnums.XenonStatus.Initializing:
                    break;
                default:
                    File.Delete(Static.XenonSilicon);
                    Application.Current.Shutdown(0xE);
                    break;
            }
        }

        private void ChangeWindow(object sender, RoutedEventArgs e)
        {
            //Disabled as API for old versions of Rift launchers is gone.

            //pageHost.Visibility = Visibility.Visible;
            //pageHost.Content = obInstallPage;
        }

        private void MPButtonClick(object sender, RoutedEventArgs e)
        {
            pageHost.Visibility = Visibility.Visible;
            pageHost.Content = mpInstallPage;
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            string JSData = File.ReadAllText(Static.XenonSilicon);
            RICloud JSD = JsonConvert.DeserializeObject<RICloud>(JSData);
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
                string gzip = "Rift.zip";
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
        public void RiftUpdate()
        {
            XenonEnums.xenonStatus = XenonEnums.XenonStatus.Installing;
            Directory.Delete("./Rift", true);
            WebClient webclient = new();
            webclient.DownloadFile(Services.Static.GalliumSilicon, Static.XenonSilicon);
            string JSData = File.ReadAllText(Static.XenonSilicon);
            Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
            webclient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(UpdateRiftCompletedCallback);
            webclient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(RiftUpdateProgressChanged);
            webclient.DownloadFileAsync(new Uri(JSD.Latest), "Rift.zip");
            InstallButton.Content = "Installing...";
            InstallButton.IsEnabled = false;
        }
        public void RiftUpdateProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            Status.Text = "Status: Updating Rift - " + e.ProgressPercentage.ToString() + "%";
        }
        public void RiftDownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            Status.Text = "Status: Installing Rift - " + e.ProgressPercentage.ToString() + "%";
        }
        private void UpdateRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string JSData = File.ReadAllText(Static.XenonSilicon);
                Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
                string zn = "Rift.zip";
                ZipFile.ExtractToDirectory(zn, @"./Rift");
                File.Delete(zn);
                RICloud ClientVersionSerialize = new RICloud
                {
                    ClientVer = JSD.ClientVer
                };
                File.WriteAllText(Static.XenonBase + "RiftClientVersion.json", JsonConvert.SerializeObject(ClientVersionSerialize));
                Status.Text = "";
                InstallButton.Content = "Installed";
                InstallButton.IsEnabled = false;
                new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("Rift Installer")
                        .AddText("We've finished the update, you can use Rift normally again.")
                        .Show();
                XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
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
                string JSData = File.ReadAllText(Static.XenonSilicon);
                Services.RICloud JSD = JsonConvert.DeserializeObject<Services.RICloud>(JSData);
                string name = "dotnet.exe";
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\" + name).WaitForExit();
                WebClient wc = new();
                wc.DownloadFile("https://cdn.discordapp.com/attachments/972611854524354570/997363999995867246/rift.ico", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\Data\\Rift.ico");
                SetEnvironmentVariable();
                Status.Text = "Creating desktop icon for Rift";
                IShellLink link = (IShellLink)new ShellLink();
                link.SetDescription("Rift Launcher");
                link.SetPath(Environment.CurrentDirectory + "\\Rift\\FortniteLauncher.exe");
                link.SetWorkingDirectory(Environment.CurrentDirectory + "\\Rift");
                link.SetIconLocation(Static.XenonLithium + "Rift.ico", 0);
                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "Rift.lnk"), false);
                //horrid
                File.WriteAllText(Static.XenonBase + "RiftClientVersion.json", "{ \"ClientVer\":\"" + JSD.ClientVer + "\" }");
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

        private void MPButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Arrow.Margin = new Thickness(39, 256, 0, 0);
            mpDisabledNoticeL1.Content = "MP is built into";
            mpDisabledNoticeL2.Content = "the new launcher.";
            AnimationHandler.FadeIn(NotifyHost, 0.4);
        }

        private void MPButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationHandler.FadeOut(NotifyHost, 0.4);
        }

        private void ChangeWindowBTN_MouseEnter(object sender, MouseEventArgs e)
        {
            Arrow.Margin = new Thickness(14, 256, 0, 0);
            mpDisabledNoticeL1.Content = "Older build installs";
            mpDisabledNoticeL2.Content = "disabled for safety.";
            AnimationHandler.FadeIn(NotifyHost, 0.4);
        }

        private void ChangeWindowBTN_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationHandler.FadeOut(NotifyHost, 0.4);
        }
    }
}
