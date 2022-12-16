using System;
using System.Collections.Generic;
using System.Text;

namespace RiftInstaller.Services
{
    internal class CobaltConfigClasses
    {
        public class Install
        {
            public string name { get; set; }
            public string path { get; set; }
            public object note { get; set; }
            public string id { get; set; }
        }
    }
}
