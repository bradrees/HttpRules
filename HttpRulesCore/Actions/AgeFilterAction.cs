using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    public class AgeFilterAction : RuleAction, IRequestAction, IResponseAction
    {
        /// <summary>
        ///   The name of the header that the server will response with age specific information.
        /// </summary>
        public const string RequestHeaderName = "X-User-Age";

        /// <summary>
        ///   The name of the header that is used to tell the server the appropriate age of the user.
        /// </summary>
        public const string ResponseHeaderName = "X-User-Age-Min";

        /// <summary>
        ///   The restrict content header.
        /// </summary>
        public const string RestrictKey = "Restrict";

        /// <summary>
        ///   The recommend content header.
        /// </summary>
        public const string RecommendKey = "Recommend";

        /// <summary>
        ///   The alternate content key constant.
        /// </summary>
        public const string AlternateContentKey = "Alt";

        /// <summary>
        ///   Internal backing field for the country code
        /// </summary>
        private string _countryCode;

        /// <summary>
        ///   Gets or sets the date of birth of the user. Note this will not be sent to the server.
        /// </summary>
        /// <value>The date of birth.</value>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        ///   Gets or sets the country code.
        /// </summary>
        /// <value>The country code.</value>
        public string CountryCode
        {
            get { return _countryCode; }
            set
            {
                try
                {
                    var info = new RegionInfo(value);
                    _countryCode = info.TwoLetterISORegionName;
                }
                catch (ArgumentException)
                {
                    // The code was not a valid country code, move along.
                }
            }
        }

        /// <summary>
        ///   Gets the age.
        /// </summary>
        /// <value>The age.</value>
        private int Age
        {
            get
            {
                var now = DateTime.Today;
                var age = now.Year - this.DateOfBirth.Year;
                if (this.DateOfBirth > now.AddYears(-age))
                {
                    age--;
                }

                return age;
            }
        }

        #region IRequestAction Members

        /// <summary>
        ///   Gets the name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        public override string NodeName
        {
            get { return "ageFilter"; }
        }

        /// <summary>
        ///   Initializes this rule.
        /// </summary>
        /// <param name = "xml">The XML for this node.</param>
        /// <returns>
        ///   The rule if the setup was successful, else null.
        /// </returns>
        public override bool Deserialize(XElement xml)
        {
            var dobAtt = xml.Attribute("dateOfBirth");
            DateTime dob;
            if (dobAtt != null && DateTime.TryParse(dobAtt.Value, out dob))
            {
                this.DateOfBirth = dob;
            }
            else
            {
                return false;
            }

            this.CountryCode = xml.Attribute("countryCode").ValueOrDefault();

            return true;
        }

        public override XElement Serialize()
        {
            return new XElement(
                this.NodeName,
                new XAttribute("countryCode", this.CountryCode),
                new XAttribute("dateOfBirth", this.DateOfBirth));
        }

        /// <summary>
        ///   The rule to run before the request is sent to the server.
        /// </summary>
        /// <param name = "session">the current fiddler session.</param>
        /// <param name = "rule">The current rule being executed.</param>
        /// <returns>
        ///   false to terminate any further request actions, true to continue.
        /// </returns>
        public bool BeforeRequest(Session session, Rule rule)
        {
            session.bBufferResponse = true;
            session.oRequest.headers[RequestHeaderName] = this.Age.ToString();
            return true;
        }

        #endregion

        #region IResponseAction Members

        /// <summary>
        ///   Action to run once the response has been received.
        /// </summary>
        /// <param name = "session">The fiddler session object.</param>
        /// <param name = "rule">The current rule being executed.</param>
        /// <returns>
        ///   false to terminate any further response actions, or true to continue.
        /// </returns>
        public bool OnResponse(Session session, Rule rule)
        {
            if (session.oResponse.headers[ResponseHeaderName].HasValue())
            {
                var headerElements = ParseResponseHeader(session.oResponse.headers[ResponseHeaderName]);
                var altUrl = GetAlternateContentUrl(headerElements, new Uri(session.fullUrl));
                if (this.IsBelowRestrictedAge(headerElements))
                {
                    if (altUrl.HasValue())
                    {
                        session.RedirectSession(altUrl, false);
                    }
                    else
                    {
                        session.oRequest.FailSession(403, "Restricted By Age", "Request blocked due to the rule: " + rule.Name);
                        return false;
                    }
                }
                else if (this.IsBelowRecommendedAge(headerElements))
                {
                    if (altUrl.HasValue())
                    {
                        session.RedirectSession(altUrl, false);
                    }
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        ///   Parses the response header.
        /// </summary>
        /// <param name = "header">The header.</param>
        /// <returns>A dictionary of the elements that make up the header.</returns>
        private static Dictionary<string, string> ParseResponseHeader(string header)
        {
            return (from kvp in header.Split(';')
                    let index = kvp.IndexOf('=')
                    where index > -1
                    let key = kvp.Substring(0, index)
                    let value = kvp.Substring(index + 1)
                    select new KeyValuePair<string, string>(key, value)).ToDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///   Determines whether the user is below restricted age specified by the header.
        /// </summary>
        /// <param name = "headerElements">The header elements.</param>
        /// <returns>
        ///   <c>true</c> if the user is below restricted age; otherwise, <c>false</c>.
        /// </returns>
        private bool IsBelowRestrictedAge(IDictionary<string, string> headerElements)
        {
            // Valid values can have the country code after the word 'Restricted', so check them first.
            return (this.CountryCode.HasValue() &&
                    CheckHeadersForAge(headerElements, RestrictKey + "-" + this.CountryCode)) ||
                   CheckHeadersForAge(headerElements, RestrictKey);
        }

        /// <summary>
        ///   Determines whether the user is below recommended age specified by the header.
        /// </summary>
        /// <param name = "headerElements">The header elements.</param>
        /// <returns>
        ///   <c>true</c> if the user is below recommended age; otherwise, <c>false</c>.
        /// </returns>
        private bool IsBelowRecommendedAge(IDictionary<string, string> headerElements)
        {
            // Valid values can have the country code after the word 'Recommended', so check them first.
            return (this.CountryCode.HasValue() &&
                    CheckHeadersForAge(headerElements, RecommendKey + "-" + this.CountryCode)) ||
                   CheckHeadersForAge(headerElements, RecommendKey);
        }

        /// <summary>
        ///   Gets the alternate content URL.
        /// </summary>
        /// <param name = "headerElements">The header elements.</param>
        /// <param name = "currentRequestUri">The current request URI.</param>
        /// <returns>The alternate content URL.</returns>
        private static string GetAlternateContentUrl(IDictionary<string, string> headerElements, Uri currentRequestUri)
        {
            var altString = headerElements[AlternateContentKey];
            if (!altString.HasValue())
            {
                return String.Empty;
            }

            var uri = new Uri(altString, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(currentRequestUri, uri);
            }

            return uri.ToString();
        }

        /// <summary>
        ///   Checks the headers to see if the user is below a certain age.
        /// </summary>
        /// <param name = "headerElements">The header elements.</param>
        /// <param name = "key">The key in the header to check.</param>
        /// <returns><c>true</c> if the user can view the content for the key, else <c>false</c>.</returns>
        private bool CheckHeadersForAge(IDictionary<string, string> headerElements, string key)
        {
            if (this.CountryCode.HasValue())
            {
                var countryValue =
                    headerElements[key];
                if (countryValue.HasValue())
                {
                    int minAge;
                    if (Int32.TryParse(countryValue, out minAge))
                    {
                        return minAge > this.Age;
                    }
                }
            }

            return false;
        }
    }
}