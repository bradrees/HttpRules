// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BreakAction.cs" company="">
//   
// </copyright>
// <summary>
//   The break action, which prevents further rules from running.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore.Actions
{
    #region Using Directives

    using System;
    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// The break action, which prevents further rules from running.
    /// </summary>
    public class BreakAction : RuleAction, IRequestAction, IResponseAction
    {
        #region Properties

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get
            {
                return "break";
            }
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpAction

        /// <summary>
        /// Deserialize this object.
        /// </summary>
        /// <param name="xml">
        ///   The XML node.
        /// </param>
        /// <returns>
        /// This object.
        /// </returns>
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

        #endregion

        #region IRequestAction

        /// <summary>
        /// Prevents further rules from running.
        /// </summary>
        /// <param name="session">
        /// the current fiddler session.
        /// </param>
        /// <param name="rule">
        /// The current rule being executed.
        /// </param>
        /// <returns>
        /// false to terminate any further request actions.
        /// </returns>
        public bool BeforeRequest(Session session, Rule rule)
        {
            RuleLog.Current.AddRule(rule, session, String.Format("Break statement ({0})", session.hostname));
            return false;
        }

        #endregion

        #region IResponseAction

        /// <summary>
        /// Prevents further rules from running.
        /// </summary>
        /// <param name="session">
        /// The fiddler session object.
        /// </param>
        /// <param name="rule">
        /// The current rule being executed.
        /// </param>
        /// <returns>
        /// false to terminate any further response actions.
        /// </returns>
        public bool OnResponse(Session session, Rule rule)
        {
            return false;
        }

        #endregion

        #endregion
    }
}