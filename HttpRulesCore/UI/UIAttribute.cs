using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpRulesCore.UI
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class UIAttribute : Attribute
    {
        public string Name { get; set; } 

        public string XmlAttribute { get; set; }

        public ControlType ControlType { get; set; }
    }
}
