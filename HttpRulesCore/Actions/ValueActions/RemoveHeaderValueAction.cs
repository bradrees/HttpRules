// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveHeaderValueAction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes a header value from a delimited header.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore.Actions.ValueActions
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// Removes a header value from a delimited header.
    /// </summary>
    public class RemoveHeaderValueAction : ValueAction
    {
        #region Properties

        /// <summary>
        /// Gets or sets the delimiter string.
        /// </summary>
        /// <value>The delimiter.</value>
        public string Delimiter { get; set; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get
            {
                return "removeValue";
            }
        }

        /// <summary>
        /// Gets or sets the format of the header.
        /// </summary>
        public HeaderValueFormat Type { get; set; }

        /// <summary>
        /// Gets or sets the value to remove.
        /// </summary>
        public string Value { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the value from the header.
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
        public override string Run(Session session, string value)
        {
            if (this.Type == HeaderValueFormat.Delimited)
            {
                var values = value.Split(new[] { this.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

                values = values.Where(v => !v.Trim().Equals(this.Value)).ToArray();

                return String.Join(this.Delimiter, values);
            }
            else
            {
                return value.Replace(this.Value, String.Empty);
            }
        }

        /// <summary>
        /// Initializes the <see cref="ValueAction"/>.
        /// </summary>
        /// <param name="xml">
        /// The XML the defines the action.
        /// </param>
        /// <returns>
        /// The value action.
        /// </returns>
        public override bool Deserialize(XElement xml)
        {
            if (xml.Attribute("delimiter") != null)
            {
                this.Delimiter = xml.Attribute("delimiter").ValueOrDefault();
            }

            HeaderValueFormat parsed;
            if (xml.Attribute("type") != null && Enum.TryParse(xml.Attribute("type").ValueOrDefault(), true, out parsed))
            {
                this.Type = parsed;
            }

            this.Value = xml.Value;

            return this.Value.HasValue();
        }

        /// <summary>
        /// Serializes the <see cref="ValueAction"/> to an XML element.
        /// </summary>
        /// <returns>
        /// The XML that represents this <see cref="ValueAction"/>.
        /// </returns>
        public override XElement Serialize()
        {
            var elem = new XElement(this.NodeName, new XAttribute("type", this.Type));
            
            if (this.Delimiter.HasValue())
            {
                elem.Add(new XAttribute("delimiter", this.Delimiter));
            }

            elem.Value = this.Value;

            return elem;
        }

        #endregion
    }
}