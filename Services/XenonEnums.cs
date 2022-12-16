using System;
using System.Collections.Generic;
using System.Text;

namespace RiftInstaller.Services
{
    internal class XenonEnums
    {
        public enum XenonStatus
        {
            Idle,
            Installing,
            Initializing
        }
        public static XenonStatus xenonStatus = XenonStatus.Initializing;
    }
}
