using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Fiddler;
using HttpRulesCore.UI;

namespace HttpRulesCore.Actions
{
    public class CacheAction : RuleAction, IRequestAction, IResponseAction
    {
        private HashSet<string> toCache = new HashSet<string>();

        [UI(Name = "Cache Storage Directory", XmlAttribute = "path", ControlType = ControlType.DirectoryPicker)]
        public string Path { get; set; }

        #region IRequestAction Members

        public override string NodeName
        {
            get { return "cache"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.Path = xml.Attribute("path").ValueOrDefault();

            if (!Directory.Exists(this.Path))
            {
                try
                {
                    Directory.CreateDirectory(this.Path);
                }
                catch
                {
                    this.Enabled = false;
                }
            }

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XElement("path", this.Path));
        }

        public bool BeforeRequest(Session session, Rule rule)
        {
            Console.WriteLine(String.Format("Request ({0}) cached due to the rule: {1}", session.hostname, rule.Name));

            // TODO: 
            // Cache-Control - Expires, MaxAge (private)
            // Age?
            // 
            var querystring = new Uri(session.fullUrl).Query;
            var startindex = querystring.IndexOf("&url=");
            var length = querystring.IndexOf("&ei", startindex) - startindex;
            var url = querystring.Substring(startindex, length);
            session.fullUrl = Uri.UnescapeDataString(url);

            return false;
        }

        #endregion

        #region IResponseAction Members

        public bool OnResponse(Session session, Rule rule)
        {
            return false;
        }

        #endregion
    }
}