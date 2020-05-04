using Galaxy.Core.Abstractions;

namespace Galaxy.Extension.FocusClient
{
    public class FocusClientOptions : ExtensionOptions
    {
        public string AppName { get; set; }

        public string FocusHost { get; set; }

        public string ContextPath { get; set; }
    }
}
