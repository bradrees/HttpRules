// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleAction.cs" company="">
//   
// </copyright>
// <summary>
//   The rule action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    using Actions.ValueActions;

    #endregion

    /// <summary>
    /// The rule action.
    /// </summary>
    public abstract class RuleAction : IHttpAction
    {
        #region Constants and Fields

        /// <summary>
        /// A store of the value actions.
        /// </summary>
        private static readonly Dictionary<string, Type> ValueActionTypes;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="RuleAction"/> class.
        /// </summary>
        static RuleAction()
        {
            var valueActionType = typeof(ValueAction);

            ValueActionTypes =
                (from t in Assembly.GetExecutingAssembly().GetTypes() where t.IsSubclassOf(valueActionType) select t).ToDictionary(
                    k =>
                        {
                            var constructorInfo = k.GetConstructor(new Type[] { });
                            return constructorInfo != null ? ((ValueAction)constructorInfo.Invoke(null)).NodeName : null;
                        }, v => v);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAction"/> class.
        /// </summary>
        protected RuleAction()
        {
            this.Enabled = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RuleAction"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public abstract string NodeName { get; }

        #endregion

        #region Implemented Interfaces

        #region IHttpAction

        /// <summary>
        /// Initializes this rule.
        /// </summary>
        /// <param name="xml">
        /// The XML for this node.
        /// </param>
        /// <returns>
        /// The rule if the setup was successful, else null.
        /// </returns>
        public abstract bool Deserialize(XElement xml);

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public abstract XElement Serialize();

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Parses the value actions.
        /// </summary>
        /// <param name="valueActions">
        /// The value actions XML.
        /// </param>
        /// <returns>
        /// A list of value actions.
        /// </returns>
        protected static IEnumerable<ValueAction> ParseValueAction(IEnumerable<XElement> valueActions)
        {
            var result = new List<ValueAction>();

            foreach (var valueActionXml in valueActions)
            {
                var name = valueActionXml.Name.ToString();
                if (!ValueActionTypes.ContainsKey(name)) continue;
                var constructorInfo = ValueActionTypes[name].GetConstructor(new Type[] { });
                if (constructorInfo == null) continue;
                var valueAction = (ValueAction)constructorInfo.Invoke(null);
                valueAction.Deserialize(valueActionXml);
                result.Add(valueAction);
            }

            return result;
        }

        #endregion
    }
}