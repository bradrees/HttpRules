using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HttpRulesCore.Actions
{
    /// <summary>
    /// Container for actions that shold only run on the request.
    /// </summary>
    public class RequestAction : RuleAction, IRequestAction
    {
        private IList<IRequestAction> _requestActions;

        public override string NodeName
        {
            get { return "request"; }
        }

        public override bool Deserialize(XElement xml)
        {
            IList<IHttpAction> actions;
            IList<IResponseAction> responseActions;
            return RuleParser.GetActions(new[] { xml }, out actions, out _requestActions, out responseActions);
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, from a in _requestActions select a.Serialize());
        }

        public bool BeforeRequest(Fiddler.Session session, Rule rule)
        {
            return _requestActions.All(action => action.BeforeRequest(session, rule));
        }
    }
}
