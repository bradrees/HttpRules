// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleLog.cs" company="">
//   
// </copyright>
// <summary>
//   The rule log.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;

    using Fiddler;

    #endregion

    /// <summary>
    /// The rule log.
    /// </summary>
    public class RuleLog
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="RuleLog"/> class.
        /// </summary>
        static RuleLog()
        {
            Current = new RuleLog();
        }

        #endregion

        #region Events

        /// <summary>
        /// The on rule logged.
        /// </summary>
        public event EventHandler<RuleEventArgs> OnRuleLogged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets Current.
        /// </summary>
        public static RuleLog Current { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add the specified rule.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <param name="session">The current session.</param>
        /// <param name="message">The message.</param>
        public void AddRule(Rule rule, Session session, string message)
        {
            if (rule != null && !rule.LogEnabled)
            {
                return;
            }

            if (this.OnRuleLogged != null)
            {
                this.OnRuleLogged(session, new RuleEventArgs(rule, session, message));
            }
        }

        #endregion
    }
}