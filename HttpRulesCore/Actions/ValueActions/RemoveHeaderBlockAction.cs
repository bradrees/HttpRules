// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveHeaderBlockAction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes a block of text from the header.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore.Actions.ValueActions
{
    #region Using Directives

    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// Removes a block of text from the header.
    /// </summary>
    public class RemoveHeaderBlockAction : ValueAction
    {
        #region Properties

        /// <summary>
        /// Gets or sets the end of the block.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get
            {
                return "removeBlock";
            }
        }

        /// <summary>
        /// Gets or sets the start of the block.
        /// </summary>
        public string Start { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes a block of text from a header.
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
            var index = value.IndexOf(this.Start);

            while (index > -1)
            {
                var end = value.IndexOf(this.End, index);

                if (end == -1)
                {
                    end = value.Length;
                }
                else
                {
                    end++;
                }

                value = value.Remove(index, end - index);

                index = value.IndexOf(this.Start);
            }

            return value;
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
            this.Start = xml.Attribute("start").ValueOrDefault();
            this.End = xml.Attribute("end").ValueOrDefault();

            return this.Start.HasValue() && this.End.HasValue();
        }

        /// <summary>
        /// Serializes the <see cref="ValueAction"/> to an XML element.
        /// </summary>
        /// <returns>
        /// The XML that represents this <see cref="ValueAction"/>.
        /// </returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("start", this.Start), new XAttribute("end", this.End));
        }

        #endregion
    }
}