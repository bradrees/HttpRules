// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IResponseAction.cs" company="">
//   
// </copyright>
// <summary>
//   The response action interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    using Fiddler;

    /// <summary>
    ///  The response action interface.
    /// </summary>
    public interface IResponseAction : IHttpAction
    {
        #region Public Methods

        /// <summary>
        /// Action to run once the response has been received.
        /// </summary>
        /// <param name="session">
        /// The fiddler session object.
        /// </param>
        /// <param name="rule">
        /// The current rule being executed.
        /// </param>
        /// <returns>
        /// false to terminate any further response actions, or true to continue.
        /// </returns>
        bool OnResponse(Session session, Rule rule);

        #endregion
    }
}