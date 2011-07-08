// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAction.cs" company="">
//   
// </copyright>
// <summary>
//   Value actions are used to modify values in the request, such as headers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore.Actions.ValueActions
{
    #region Using Directives

    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// Value actions are used to modify values in the request, such as headers.
    /// </summary>
    public abstract class ValueAction
    {
        #region Properties

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public abstract string NodeName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs the specified session.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The resulting value, after this rule had modified it.
        /// </returns>
        public abstract string Run(Session session, string value);

        /// <summary>
        /// Initializes the <see cref="ValueAction"/>.
        /// </summary>
        /// <param name="xml">
        /// The XML the defines the action.
        /// </param>
        /// <returns>
        /// The value action.
        /// </returns>
        public abstract bool Deserialize(XElement xml);

        /// <summary>
        /// Serializes the <see cref="ValueAction"/> to an XML element.
        /// </summary>
        /// <returns>The XML that represents this <see cref="ValueAction"/>.</returns>
        public abstract XElement Serialize();

        #endregion
    }
}