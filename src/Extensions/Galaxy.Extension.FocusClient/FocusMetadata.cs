using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Extension.FocusClient
{
    public class FocusMetadata
    {
        public string AppName { get; set; }

        public string Path { get; set; }

        public string PathDesc { get; set; }

        public string RpcType { get; set; }

        public string ServiceName { get; set; }

        public string MethodName { get; set; }

        public string ParameterTypes { get; set; }

        public string RpcExt { get; set; }

        public bool Enabled { get; set; }
    }
}
