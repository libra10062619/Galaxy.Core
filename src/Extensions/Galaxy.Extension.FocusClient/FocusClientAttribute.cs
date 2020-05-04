using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;

namespace Galaxy.Extension.FocusClient
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FocusClientAttribute : RouteAttribute
    {
        //public string Name;
        public string Description { get; set; }
        //public string Path { get; }
        public FocusClientAttribute(string template) : base(template)
        {
            //Path = path;
        }

        public string GetGetFocusPath()//=> Regex.Replace(Template, "\\{.[^\\}]*\\}", "**");
        {
            return Regex.Replace(Template, "\\{.[^\\}]*\\}", "**");
        }
    }
}
