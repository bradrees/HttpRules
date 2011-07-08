using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Fiddler;
using System.IO;

namespace HttpRulesCore.Actions
{
    public class SaveFileAction : RuleAction, IRequestAction, IResponseAction
    {
        #region IRequestAction Members

        public string[] Extensions { get; private set; } 

        public override string NodeName
        {
            get { return "saveFile"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.Extensions = xml.Attribute("extensions").ValueOrDefault().Split(',');

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("extensions", String.Join(",", this.Extensions)));
        }

        public bool BeforeRequest(Session session, Rule rule)
        {
            Uri uriOut;
            if (Uri.TryCreate(session.fullUrl, UriKind.RelativeOrAbsolute, out uriOut))
            {
                if (uriOut.HasFileExtension(this.Extensions))
                {
                    session.bBufferResponse = true;
                }
            }

            return true;
        }

        #endregion

        public bool OnResponse(Session session, Rule rule)
        {
            Uri uriOut;
            if (Uri.TryCreate(session.fullUrl, UriKind.RelativeOrAbsolute, out uriOut))
            {
                if (uriOut.HasFileExtension(this.Extensions))
                {
                    // TODO: Config this directory
                    var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"\Downloads\");
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    session.SaveResponseBody(Path.Combine(directory, session.SuggestedFilename));
                }
            }

            return true;
        }
    }
}
