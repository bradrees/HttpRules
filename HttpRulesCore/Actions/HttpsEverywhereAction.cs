using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    /// <summary>
    ///   HTTPS Everywhere: https://www.eff.org/https-everywhere
    /// </summary>
    public class HttpsEverywhereAction : RuleAction, IRequestAction
    {
        public string DirectoryPath { get; set; }

        public string OriginalPath { get; set; }

        public bool FakeHttps { get; set; }

        private IList<Ruleset> Rulesets { get; set; }

        #region IRequestAction Members

        public override string NodeName
        {
            get { return "httpseverywhere"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.FakeHttps = xml.Attribute("fakeHttps") != null && (bool) xml.Attribute("fakeHttps");
            var pathAtt = xml.Attribute("path");

            if (pathAtt == null)
            {
                return false;
            }

            this.OriginalPath = this.DirectoryPath = pathAtt.Value;

            if (!Regex.IsMatch(this.DirectoryPath, @"$[A-Za-z]:\\"))
            {
                this.DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.DirectoryPath);
            }

            this.DirectoryPath = Path.Combine(this.DirectoryPath, "default.rulesets");

            if (!File.Exists(this.DirectoryPath))
            {
                return false;
            }

            this.Rulesets = (from ruleset in XDocument.Load(this.DirectoryPath).Descendants("ruleset")
                             where ruleset.Attribute("default_off") == null
                             let matchRule = (string) ruleset.Attribute("match_rule")
                             select new Ruleset
                                        {
                                            Name = (string) ruleset.Attribute("name"),
                                            MatchRule =
                                                String.IsNullOrWhiteSpace(matchRule) ? null : new Regex(matchRule),
                                            Exclusions = (from ex in ruleset.Descendants("exclusion")
                                                          select new Regex((string) ex.Attribute("pattern"))).
                                                ToList(),
                                            Rule = (from r in ruleset.Descendants("rule")
                                                    select new RulesetRule
                                                               {
                                                                   From = new Regex((string) r.Attribute("from")),
                                                                   To = (string) r.Attribute("to")
                                                               }).ToList(),
                                            Targets = (from t in ruleset.Descendants("target")
                                                       select
                                                           new Regex((((string) t.Attribute("host"))).WildcardToRegex()))
                                                .ToList()
                                        }).ToList();
            

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("path", this.OriginalPath), new XAttribute("fakeHttps", this.FakeHttps));
        }

        public bool BeforeRequest(Session session, Rule rule)
        {
            // HTTPS Everywhere: https://www.eff.org/https-everywhere

            if (!session.isHTTPS && session.port != 443)
            {
                foreach (var ruleset in Rulesets)
                {
                    if (ruleset.MatchRule != null)
                    {
                        if (!ruleset.MatchRule.IsMatch(session.fullUrl))
                        {
                            continue;
                        }
                    }

                    if (ruleset.Exclusions.Any(ex => ex.IsMatch(session.fullUrl)))
                    {
                        continue;
                    }

                    if (ruleset.Targets.Any() && !ruleset.Targets.Any(t => t.IsMatch(session.fullUrl)))
                    {
                        continue;
                    }

                    foreach (var rulesetRule in ruleset.Rule)
                    {
                        if (!rulesetRule.From.IsMatch(session.fullUrl))
                        {
                            continue;
                        }

                        var secureUrl = rulesetRule.From.Replace(session.fullUrl, rulesetRule.To);

                        if (this.FakeHttps)
                        {
                            return !session.OverrideRequest(secureUrl);
                        }
                        else
                        {
                            session.RedirectSession(secureUrl, false);
                        }

                        RuleLog.Current.AddRule(
                            rule,
                            session,
                            String.Format("HttpsEverywhere: {0} ({1})", ruleset.Name, session.hostname));

                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Nested type: Ruleset

        private class Ruleset
        {
            public string Name { get; set; }
            public Regex MatchRule { get; set; }
            public IEnumerable<Regex> Exclusions { get; set; }
            public IEnumerable<Regex> Targets { get; set; }
            public IEnumerable<RulesetRule> Rule { get; set; }
        }

        #endregion

        #region Nested type: RulesetRule

        private class RulesetRule
        {
            public Regex From { get; set; }
            public string To { get; set; }
        }

        #endregion
    }
}