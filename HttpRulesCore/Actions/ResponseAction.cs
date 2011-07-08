using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    /// <summary>
    /// Container for actions that shold only run on the response.
    /// </summary>
    public class ResponseAction : RuleAction, IResponseAction
    {
        private IList<IResponseAction> _responseActions;

        public override string NodeName
        {
            get { return "response"; }
        }

        public override bool Deserialize(XElement xml)
        {
            IList<IHttpAction> actions;
            IList<IRequestAction> requestActions;
            return RuleParser.GetActions(new [] { xml }, out actions, out requestActions, out _responseActions);
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, from a in _responseActions select a.Serialize()); // Note this will only serialize rules that have response parts of the action
        }

        public bool OnResponse(Session session, Rule rule)
        {
            return _responseActions.All(action => action.OnResponse(session, rule));
        }
    }
}
