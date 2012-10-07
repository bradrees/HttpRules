using System;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    public class AddHeaderAction : RuleAction, IRequestAction
    {
        public virtual string Name { get; set; }

        public virtual string Value { get; set; }

        #region IRequestAction Members

        public override string NodeName
        {
            get { return "addHeader"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.Name = xml.Attribute("name").ValueOrDefault();
            this.Value = xml.Attribute("value").ValueOrDefault();

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("name", this.Name), new XAttribute("value", this.Value));
        }

        public virtual bool BeforeRequest(Session session, Rule rule)
        {
            // Find any existing headers
            foreach (HTTPHeaderItem header in session.oRequest.headers)
            {
                if (header.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Header already present
                }
            }

            session.oRequest.headers.Add(this.Name, this.Value);
            
            return true;
        }

        #endregion
    }
}