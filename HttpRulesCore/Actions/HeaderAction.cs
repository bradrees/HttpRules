using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Fiddler;
using HttpRulesCore.Actions.ValueActions;

namespace HttpRulesCore.Actions
{
    public class HeaderAction : RuleAction, IRequestAction
    {
        public virtual string Name { get; set; }

        public IEnumerable<ValueAction> Actions { get; set; }

        #region IRequestAction Members

        public override string NodeName
        {
            get { return "header"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.Name = xml.Attribute("name").ValueOrDefault();
            this.Actions = ParseValueAction(xml.Descendants());

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("name", this.Name), from a in this.Actions select a.Serialize());
        }

        public virtual bool BeforeRequest(Session session, Rule rule)
        {
            // Find the headers
            HTTPHeaderItem activeHeader = null;
            string value = String.Empty;
            foreach (HTTPHeaderItem header in session.oRequest.headers)
            {
                if (header.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase))
                {
                    value = header.Value;
                    activeHeader = header;
                }
            }

            if (activeHeader != null)
            {
                // Run all the sub actions.
                value = this.Actions.Aggregate(value, (current, action) => action.Run(session, current));

                if (String.IsNullOrEmpty(value))
                {
                    session.oRequest.headers.Remove(activeHeader);
                    RuleLog.Current.AddRule(rule, session,
                                            String.Format("Header ({0}) Removed ({1})", this.Name, session.hostname));
                }
                else
                {
                    // Set the new value.
                    activeHeader.Value = value;
                    RuleLog.Current.AddRule(rule, session,
                                            String.Format("Header ({0}) Updated ({1})", this.Name, session.hostname));
                }
            }

            return true;
        }

        #endregion
    }
}