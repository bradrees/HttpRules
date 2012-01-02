// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseSummaryEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   The response summary event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;

    #endregion

    /// <summary>
    /// The response summary event args.
    /// </summary>
    public class ResponseSummaryEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets or sets FullUrl.
        /// </summary>
        public string FullUrl { get; set; }

        /// <summary>
        /// Gets or sets ResponseCode.
        /// </summary>
        public int ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets ResponseCodeText.
        /// </summary>
        public string ResponseCodeText { get; set; }

        /// <summary>
        /// Gets or sets the referer.
        /// </summary>
        /// <value>The referer.</value>
        public string Referer { get; set; }

        #endregion
    }
}