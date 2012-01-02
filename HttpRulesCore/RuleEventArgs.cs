// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   The rule event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;

    using Fiddler;

    #endregion

    /// <summary>
    /// The rule event args.
    /// </summary>
    public class RuleEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEventArgs"/> class.
        /// </summary>
        /// <param name="rule">
        /// The rule that caused the event.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public RuleEventArgs(Rule rule, Session session, string message)
        {
            this.Rule = rule;
            this.Session = session;
            this.Message = message;
            this.Path = session.host + session.PathAndQuery;
            this.Referer = session.oRequest["Referer"] ?? "No Referer";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets Path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets Rule.
        /// </summary>
        public Rule Rule { get; set; }

        /// <summary>
        /// Gets or sets Session.
        /// </summary>
        public Session Session { get; set; }

        public string Referer { get; set; }

        #endregion
    }
}