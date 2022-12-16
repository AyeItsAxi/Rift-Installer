using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiftInstaller.Services
{
    internal class CobaltManage
    {
        public static bool bCobaltExists()
        {
            if (File.Exists(Static.CobaltConfig))
            {
                return true;
            }
            return false;
        }
        public static bool bModifiableCobaldExists()
        {
            if (File.Exists(Static.ModifiedCobaltConfig))
            {
                return true;
            }
            return false;
        }
        public static void CreateModifiableCobaltConfiguration()
        {
            if (bCobaltExists())
            {
                File.Copy(Static.CobaltConfig, Static.ModifiedCobaltConfig, true);
            }
        }
        public static void AddInstallToCobaltConfigurationManagement(string installedPath)
        {
            if (bModifiableCobaldExists())
            {
                AddInstallPathToCobaltConfiguration(installedPath);
            }
        }
        public static void AddInstallPathToCobaltConfiguration(string installedPath)
        {
            JObject rss = JObject.Parse(File.ReadAllText(Static.ModifiedCobaltConfig));
            JArray installs = (JArray)rss["installs"];
            installs.Add(new JObject(
                new JProperty("name", "Rift Multiplayer (7.20)"),
                new JProperty("path", installedPath),
                new JProperty("note", "The version of Fortnite that is used for Rift Multiplayer. Automatically added by Rift Installer."),
                new JProperty("id", "7.20")
            ));
            SaveCobaltConfiguration(rss.ToString());
        }
        public static void SaveCobaltConfiguration(string IncomingFileDiff)
        {
            File.WriteAllText(Static.ModifiedCobaltConfig, IncomingFileDiff);
            File.Copy(Static.ModifiedCobaltConfig, Static.CobaltConfig, true);
        }
    }
}
