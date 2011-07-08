// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="">
//   
// </copyright>
// <summary>
//   A rule that will be be run on a request or response.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using HttpRulesCore.Actions;

    #endregion

    /// <summary>
    /// A rule that will be be run on a request or response..
    /// </summary>
    public class Rule
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        public Rule()
        {
            this.Enabled = true;
            this.LogEnabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="ruleXml">
        /// The rule xml.
        /// </param>
        public Rule(XElement ruleXml)
            : this()
        {
            this.Name = (ruleXml.Attribute("name") ?? new XAttribute("name", "Unnamed Rule")).Value;
            this.RuleSource = ruleXml;

            // Read paths
            if (ruleXml.Descendants("match").Any())
            {
                this.Matches = (from host in ruleXml.Descendants("match") select new UriPattern(host.Value)).ToList();
            }
            else if (ruleXml.Descendants("matches").Any())
            {
                this.Matches = (from host in ruleXml.Descendants("matches").Descendants("match") select new UriPattern(host.Value)).ToList();
            }

            // Read actions
            if (ruleXml.Descendants("actions").Any())
            {
                var actionsXml = from a in ruleXml.Descendants("actions") select a;
                IList<IHttpAction> actions;
                IList<IRequestAction> requestActions;
                IList<IResponseAction> responseActions;
                this.Enabled = RuleParser.GetActions(actionsXml, out actions, out requestActions, out responseActions);
                this.Actions = actions;
                this.RequestActions = requestActions;
                this.ResponseActions = responseActions;
            }

            bool temp;
            this.Enabled = Boolean.TryParse((ruleXml.Attribute("enabled") ?? new XAttribute("enabled", "true")).Value, out temp) && temp;
            this.LogEnabled = Boolean.TryParse((ruleXml.Attribute("logEnabled") ?? new XAttribute("logEnabled", "true")).Value, out temp) && temp;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Actions.
        /// </summary>
        public IEnumerable<IHttpAction> Actions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether LogEnabled.
        /// </summary>
        public bool LogEnabled { get; set; }

        /// <summary>
        /// Gets or sets Matches.
        /// </summary>
        public IEnumerable<UriPattern> Matches { get; set; }

        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets RequestActions.
        /// </summary>
        public IEnumerable<IRequestAction> RequestActions { get; set; }

        /// <summary>
        /// Gets or sets ResponseActions.
        /// </summary>
        public IEnumerable<IResponseAction> ResponseActions { get; set; }

        /// <summary>
        /// Gets or sets RuleSource.
        /// </summary>
        public XElement RuleSource { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="fullPath">
        /// The full path.
        /// </param>
        /// <returns>
        /// The rules in an XML document.
        /// </returns>
        public static IEnumerable<Rule> Parse(string fullPath)
        {
            var doc = XDocument.Load(fullPath);

            return doc.Root.Descendants("rule").Select(ruleXml => new Rule(ruleXml)).ToArray();
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <returns>
        /// Serializes this rule to an XML element.
        /// </returns>
        public XElement Serialize()
        {
            var element = new XElement("rule");
            element.Add(new XAttribute("name", this.Name));
            element.Add(new XAttribute("enabled", this.Enabled));
            element.Add(new XAttribute("logEnabled", this.LogEnabled));

            if (this.Matches != null && this.Matches.Any())
            {
                var matches = new XElement("matches");
                element.Add(matches);
                foreach (var pattern in this.Matches)
                {
                    matches.Add(new XElement("match", pattern.Pattern));
                }
            }

            if (this.Actions != null && this.Actions.Any())
            {
                var actions = new XElement("actions");
                element.Add(actions);
                foreach (var action in this.Actions)
                {
                    actions.Add(action.Serialize());
                }
            }

            return element;
        }

        #endregion
    }
}