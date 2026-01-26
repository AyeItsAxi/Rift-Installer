using System;
using System.IO;
using System.Net.Http;
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
using System.Threading.Tasks;

namespace RiftInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string RIFT_FOLDER = "Rift";
        private const string RIFT_ZIP = "Rift.zip";
        private const string RIFT_EXE = "Rift.exe";
        private const string RIFT_VERSION_FILE = "RiftClientVersion.json";
        private const int TOAST_CONVERSATION_ID = 9813;

        private static readonly string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Rift Installer", "Data");

        private static readonly string BasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Rift Installer");

        private static readonly HttpClient httpClient = new HttpClient();

        public MultiplayerInstallPage mpInstallPage;
        public OlderBuildInstallPage obInstallPage;

        public MainWindow()
        {
            InitializeComponent();
            _ = InitializeApplicationAsync();
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                Directory.CreateDirectory(DataPath);
                Services.CobaltManage.CreateModifiableCobaltConfiguration();

                CleanupTempFiles();

                mpInstallPage = new MultiplayerInstallPage();
                obInstallPage = new OlderBuildInstallPage();
                Services.Static.applicationFrame = this;

                await DownloadAndCheckVersionAsync();

                if (Directory.Exists(RIFT_FOLDER))
                {
                    SetInstallButtonState(false, "Installed");
                    XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CleanupTempFiles()
        {
            var tempFiles = new[] { Static.XenonSilicon, "Rift.Old.zip", RIFT_ZIP, "Rift.MP.zip", "Fortnite.zip" };
            foreach (var file in tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private async Task DownloadAndCheckVersionAsync()
        {
            var versionData = await DownloadStringAsync(Services.Static.GalliumSilicon);
            await File.WriteAllTextAsync(Static.XenonSilicon, versionData);

            var cloudData = JsonConvert.DeserializeObject<Services.RICloud>(versionData);
            var versionFile = Path.Combine(BasePath, RIFT_VERSION_FILE);

            if (File.Exists(versionFile))
            {
                var localVersionData = await File.ReadAllTextAsync(versionFile);
                var localCloudData = JsonConvert.DeserializeObject<Services.RICloud>(localVersionData);

                if (localCloudData?.ClientVer != cloudData?.ClientVer)
                {
                    ShowToast("Rift Installer",
                        "I detected that your version of Rift is outdated, so I started an update. Your installations and settings save, so there's no need to worry about this.",
                        ToastDuration.Short);
                    await RiftUpdateAsync();
                    XenonEnums.xenonStatus = XenonEnums.XenonStatus.Installing;
                }
                else
                {
                    XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
                }
            }
        }

        private async Task<string> DownloadStringAsync(string url)
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private void ShowToast(string title, string message, ToastDuration duration = ToastDuration.Short)
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", TOAST_CONVERSATION_ID)
                .AddText(title)
                .AddText(message)
                .SetToastDuration(duration)
                .Show();
        }

        private void SetInstallButtonState(bool enabled, string content)
        {
            InstallButton.IsEnabled = enabled;
            InstallButton.Content = content;
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
                    var result = MessageBox.Show(
                        "A download is in progress, and exiting will corrupt it. Are you sure?",
                        "Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        Cleanup();
                        Application.Current.Shutdown(0xE);
                    }
                    break;
                case XenonEnums.XenonStatus.Initializing:
                    break;
                default:
                    Cleanup();
                    Application.Current.Shutdown(0xE);
                    break;
            }
        }

        private void Cleanup()
        {
            if (File.Exists(Static.XenonSilicon))
            {
                File.Delete(Static.XenonSilicon);
            }
        }

        private void ChangeWindow(object sender, RoutedEventArgs e)
        {
            // disabled caus i forgot idk bro its been 4 years since i worked on this project
        }

        private void MPButtonClick(object sender, RoutedEventArgs e)
        {
            pageHost.Visibility = Visibility.Visible;
            pageHost.Content = mpInstallPage;
        }

        private async void Install(object sender, RoutedEventArgs e)
        {
            try
            {
                var versionData = await File.ReadAllTextAsync(Static.XenonSilicon);
                var cloudData = JsonConvert.DeserializeObject<RICloud>(versionData);

                ShowToast("Rift Installer", $"Starting installation from: {cloudData.Latest}");
                await DownloadManagerAsync(cloudData.Latest);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DownloadManagerAsync(string installURL)
        {
            try
            {
                await AddWindowsDefenderExclusionAsync();

                SetInstallButtonState(false, "Installing...");

                using (var webClient = new System.Net.WebClient())
                {
                    webClient.DownloadFileCompleted += DownloadRiftCompletedCallback;
                    webClient.DownloadProgressChanged += RiftDownloadProgressChanged;
                    await webClient.DownloadFileTaskAsync(new Uri(installURL), RIFT_ZIP);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetInstallButtonState(true, "Install");
            }
        }

        private async Task AddWindowsDefenderExclusionAsync()
        {
            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = $"-Command Add-MpPreference -ExclusionPath '{Environment.CurrentDirectory}'",
                    CreateNoWindow = true,
                };

                await Task.Run(() =>
                {
                    using (var process = Process.Start(start))
                    {
                        process?.WaitForExit();
                    }
                });
            }
            catch
            {
                // couldnt add the exclusion ??
            }
        }

        public async Task RiftUpdateAsync()
        {
            try
            {
                XenonEnums.xenonStatus = XenonEnums.XenonStatus.Installing;

                if (Directory.Exists(RIFT_FOLDER))
                {
                    Directory.Delete(RIFT_FOLDER, true);
                }

                var versionData = await DownloadStringAsync(Services.Static.GalliumSilicon);
                await File.WriteAllTextAsync(Static.XenonSilicon, versionData);
                var cloudData = JsonConvert.DeserializeObject<Services.RICloud>(versionData);

                SetInstallButtonState(false, "Installing...");

                using (var webClient = new System.Net.WebClient())
                {
                    webClient.DownloadFileCompleted += UpdateRiftCompletedCallback;
                    webClient.DownloadProgressChanged += RiftUpdateProgressChanged;
                    await webClient.DownloadFileTaskAsync(new Uri(cloudData.Latest), RIFT_ZIP);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
            }
        }

        public void RiftUpdateProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Status.Text = $"Status: Updating Rift - {e.ProgressPercentage}%";
        }

        public void RiftDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Status.Text = $"Status: Installing Rift - {e.ProgressPercentage}%";
        }

        private async void UpdateRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }

                await ExtractAndFinalizeUpdateAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update completion error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExtractAndFinalizeUpdateAsync()
        {
            var versionData = await File.ReadAllTextAsync(Static.XenonSilicon);
            var cloudData = JsonConvert.DeserializeObject<Services.RICloud>(versionData);

            ZipFile.ExtractToDirectory(RIFT_ZIP, RIFT_FOLDER);
            File.Delete(RIFT_ZIP);

            var clientVersion = new RICloud { ClientVer = cloudData.ClientVer };
            var versionFile = Path.Combine(BasePath, RIFT_VERSION_FILE);
            await File.WriteAllTextAsync(versionFile, JsonConvert.SerializeObject(clientVersion));

            Status.Text = "";
            SetInstallButtonState(false, "Installed");
            ShowToast("Rift Installer", "We've finished the update, you can use Rift normally again.");
            XenonEnums.xenonStatus = XenonEnums.XenonStatus.Idle;
        }

        private async void DownloadRiftCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }

                ZipFile.ExtractToDirectory(RIFT_ZIP, RIFT_FOLDER);
                File.Delete(RIFT_ZIP);
                await FinalizeRiftInstallAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation completion error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FinalizeRiftInstallAsync()
        {
            try
            {
                var versionData = await File.ReadAllTextAsync(Static.XenonSilicon);
                var cloudData = JsonConvert.DeserializeObject<Services.RICloud>(versionData);

                SetEnvironmentVariable();

                Status.Text = "Creating desktop icon for Rift";
                CreateDesktopShortcut();

                var versionFile = Path.Combine(BasePath, RIFT_VERSION_FILE);
                await File.WriteAllTextAsync(versionFile, JsonConvert.SerializeObject(new RICloud { ClientVer = cloudData.ClientVer }));

                Status.Text = "";
                SetInstallButtonState(false, "Installed");
                ShowToast("Rift Installer",
                    "Rift has been successfully installed and an icon to start Rift has been added to your desktop! Enjoy!",
                    ToastDuration.Long);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Finalization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CreateDesktopShortcut()
        {
            var link = (IShellLink)new ShellLink();
            link.SetDescription("Rift Launcher");
            link.SetPath(Path.Combine(Environment.CurrentDirectory, RIFT_FOLDER, RIFT_EXE));
            link.SetWorkingDirectory(Path.Combine(Environment.CurrentDirectory, RIFT_FOLDER));
            link.SetIconLocation(System.Reflection.Assembly.GetExecutingAssembly().Location, 0);

            var file = (IPersistFile)link;
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            file.Save(Path.Combine(desktopPath, "Rift.lnk"), false);
        }

        private void SetEnvironmentVariable()
        {
            Status.Text = "Checking if CPU fix is needed";
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENSSL_ia32cap")))
            {
                Status.Text = "CPU fix is needed, applying fix";
                try
                {
                    Environment.SetEnvironmentVariable("OPENSSL_ia32cap", "~0x20000000", EnvironmentVariableTarget.Machine);
                    Status.Text = "CPU fix applied";
                }
                catch
                {
                    Status.Text = "CPU fix failed (may require admin rights)";
                }
            }
            else
            {
                Status.Text = "CPU fix is not needed, finishing up";
            }
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink { }

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