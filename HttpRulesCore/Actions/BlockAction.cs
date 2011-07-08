using System;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    public sealed class BlockAction : RuleAction, IRequestAction
    {
        #region IRequestAction Members

        /// <summary>
        ///   Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get { return "block"; }
        }

        /// <summary>
        /// Sets up any properties for this action.
        /// </summary>
        /// <param name = "xml">The XML.</param>
        /// <returns></returns>
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

        public bool BeforeRequest(Session session, Rule rule)
        {
            session.oRequest.FailSession(403, "Forbidden By Proxy", "Request blocked due to the rule: " + rule.Name);
            RuleLog.Current.AddRule(rule, session, String.Format("Block ({0})", session.hostname));
            return false;
        }

        #endregion
    }
}