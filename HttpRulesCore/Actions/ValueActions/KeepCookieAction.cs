// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepCookieAction.cs" company="">
//   
// </copyright>
// <summary>
//   The keep action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore.Actions.ValueActions
{
    #region Using Directives

    using System;
    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// The keep action.
    /// </summary>
    public class KeepAction : ValueAction
    {
        #region Properties

        /// <summary>
        /// Gets or sets how long before a cookie expires.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Gets NodeName.
        /// </summary>
        public override string NodeName
        {
            get
            {
                return "keep";
            }
        }

        /// <summary>
        /// Gets or sets the URL path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets timeout.
        /// </summary>
        public int Timeout { get; set; }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Runs this acton, which keeps a cookie for a specified time.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="value">The value to modify.</param>
        /// <returns>
        /// The resulting value, after this rule had modified it.
        /// </returns>
        public override string Run(Session session, string value)
        {
            if (session.PathAndQuery.StartsWith(this.Path, StringComparison.OrdinalIgnoreCase))
            {
                this.Expires = DateTime.UtcNow.AddSeconds(this.Timeout);
            }

            if (DateTime.UtcNow > this.Expires)
            {
                return String.Empty;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Initializes the <see cref="ValueAction"/>.
        /// </summary>
        /// <param name="xml">The XML the defines the action.</param>
        /// <returns>The value action.</returns>
        public override bool Deserialize(XElement xml)
        {
            this.Path = xml.Attribute("path").Value;
            this.Timeout = Int32.Parse(xml.Attribute("timeout").Value);

            return true;
        }

        /// <summary>
        /// Serializes the <see cref="ValueAction"/> to an XML element.
        /// </summary>
        /// <returns>
        /// The XML that represents this <see cref="ValueAction"/>.
        /// </returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("path", this.Path), new XAttribute("timeout", this.Timeout));
        }

        #endregion
    }
}