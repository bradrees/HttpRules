using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HttpRulesCore.Actions
{
    public class RespondAction : RuleAction, IRequestAction
    {
        public override string NodeName
        {
            get { return "respond"; }
        }

        public override bool Deserialize(XElement xml)
        {
            // Does nothing, no setup required.          

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName);
        }

        public bool BeforeRequest(Fiddler.Session session, Rule rule)
        {
            // TODO
           
            return true;
        }
    }
}
