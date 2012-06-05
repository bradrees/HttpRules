using System;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    public sealed class BlockHoursAction : RuleAction, IRequestAction
    {
        #region IRequestAction Members

        /// <summary>
        ///   Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get { return "blockhours"; }
        }

        public decimal StartHour { get; set; }

        public decimal EndHour { get; set; }

        /// <summary>
        /// Sets up any properties for this action.
        /// </summary>
        /// <param name = "xml">The XML.</param>
        /// <returns></returns>
        public override bool Deserialize(XElement xml)
        {
            try
            {
                this.StartHour = Decimal.Parse(xml.Attribute("starthour").ValueOrDefault());
                this.EndHour = Decimal.Parse(xml.Attribute("endhour").ValueOrDefault());
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("starthour", this.StartHour), new XAttribute("endhour", this.EndHour));
        }

        public bool BeforeRequest(Session session, Rule rule)
        {
            var now = DateTime.Now;
            var hour = now.Hour + now.Minute/60m;
            if (hour >= StartHour && hour < EndHour)
            {
                session.oRequest.FailSession(403, "Forbidden By Proxy", "Request blocked due to the rule: " + rule.Name);
                RuleLog.Current.AddRule(rule, session, String.Format("Block ({0})", session.hostname));
                return false;
            }

            return true;
        }

        #endregion
    }
}