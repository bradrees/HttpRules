// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRequestAction.cs" company="">
//   
// </copyright>
// <summary>
//   The request action interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    using Fiddler;

    /// <summary>
    /// The request action interface.
    /// </summary>
    public interface IRequestAction : IHttpAction
    {
        #region Public Methods

        /// <summary>
        /// The rule to run before the request is sent to the server.
        /// </summary>
        /// <param name="session">
        /// the current fiddler session.
        /// </param>
        /// <param name="rule">
        /// The current rule being executed.
        /// </param>
        /// <returns>
        /// false to terminate any further request actions, true to continue.
        /// </returns>
        bool BeforeRequest(Session session, Rule rule);

        #endregion
    }
}