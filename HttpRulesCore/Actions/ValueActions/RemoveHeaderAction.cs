// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveHeaderAction.cs" company="">
//   
// </copyright>
// <summary>
//   The remove header action.
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
    /// The remove header action.
    /// </summary>
    public class RemoveHeaderAction : ValueAction
    {
        #region Properties

        /// <summary>
        /// Gets NodeName.
        /// </summary>
        public override string NodeName
        {
            get
            {
                return "remove";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the value of the header.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>
        /// The resulting value, after this rule had modified it.
        /// </returns>
        public override string Run(Session session, string value)
        {
            return String.Empty;
        }

        /// <summary>
        /// Initializes the <see cref="ValueAction"/>.
        /// </summary>
        /// <param name="xml">The XML the defines the action.</param>
        /// <returns>The value action.</returns>
        public override bool Deserialize(XElement xml)
        {
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
            return new XElement(this.NodeName);
        }

        #endregion
    }
}