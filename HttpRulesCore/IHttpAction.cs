using System.Xml.Linq;

namespace HttpRulesCore
{
    public interface IHttpAction
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IHttpAction"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        string NodeName { get; }

        /// <summary>
        /// Deserializes the specified XML to create this rule.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns><c>true</c> on success.</returns>
        bool Deserialize(XElement xml);

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        XElement Serialize();
    }
}