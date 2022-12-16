using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Background;

namespace RiftInstaller.Services
{
    class Static
    {
        //Gallium = Cloud data
        //Xenon = Rift Installer data
        //Cobalt = Rift data
        //Silicon = Strings
        //Lithium = Data
        public static MainWindow applicationFrame;
        public const string GalliumBase = "https://raw.githubusercontent.com/AyeItsAxi/rift-installer-strings/main/";
        public const string GalliumSilicon = GalliumBase + "strings.json";
        public static string XenonBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift Installer\\";
        public static string XenonSilicon = XenonBase + "strings.json";
        public static string XenonLithium = XenonBase + "Data\\";
        public static string CobaltBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Rift\\";
        public static string CobaltConfig = CobaltBase + "rift.conf";
        public static string ModifiedCobaltConfig = CobaltBase + "rift.riData.json";
    }
}
