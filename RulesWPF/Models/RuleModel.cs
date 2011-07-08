// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleModel.cs" company="">
//   
// </copyright>
// <summary>
//   The rule model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RulesWPF.Models
{
    #region Using Directives

    using System.ComponentModel;

    using HttpRulesCore;

    #endregion

    /// <summary>
    /// The rule model.
    /// </summary>
    public class RuleModel : INotifyPropertyChanged
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleModel"/> class.
        /// </summary>
        /// <param name="rule">
        /// The rule that is model represents.
        /// </param>
        public RuleModel(Rule rule)
        {
            this.Rule = rule;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this rule is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.Rule.Enabled;
            }

            set
            {
                this.Rule.Enabled = value;
                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the rules logging is enabled.
        /// </summary>
        public bool LogEnabled
        {
            get
            {
                return this.Rule.LogEnabled;
            }

            set
            {
                this.Rule.LogEnabled = value;
                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("LogEnabled"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        public string Name
        {
            get
            {
                return this.Rule.Name;
            }

            set
            {
                this.Rule.Name = value;
                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Gets the rule this model represents.
        /// </summary>
        public Rule Rule { get; private set; }

        #endregion
    }
}