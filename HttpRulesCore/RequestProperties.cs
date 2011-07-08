// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestProperties.cs" company="">
//   
// </copyright>
// <summary>
//   The request properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;

    #endregion

    /// <summary>
    /// The request properties.
    /// </summary>
    public class RequestProperties
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestProperties"/> class.
        /// </summary>
        /// <param name="uri">
        /// The request URI.
        /// </param>
        /// <param name="referer">
        /// The referer.
        /// </param>
        /// <param name="acceptHeader">
        /// The accept header.
        /// </param>
        public RequestProperties(Uri uri, Uri referer, string acceptHeader)
        {
            this.Uri = uri;
            this.Referer = referer;
            this.Accept = acceptHeader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestProperties"/> class.
        /// </summary>
        /// <param name="uri">
        /// The request URI.
        /// </param>
        /// <param name="referer">
        /// The referer.
        /// </param>
        /// <param name="acceptHeader">
        /// The accept header.
        /// </param>
        public RequestProperties(string uri, string referer, string acceptHeader)
        {
            Uri uriOut;
            if (Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out uriOut))
            {
                this.Uri = uriOut;
            }

            Uri refererOut;
            if (Uri.TryCreate(referer, UriKind.RelativeOrAbsolute, out refererOut))
            {
                this.Referer = refererOut;
            }

            this.Accept = acceptHeader;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Accept.
        /// </summary>
        public string Accept { get; private set; }

        /// <summary>
        /// Gets Referer.
        /// </summary>
        public Uri Referer { get; private set; }

        /// <summary>
        /// Gets request URI.
        /// </summary>
        public Uri Uri { get; private set; }

        #endregion
    }
}