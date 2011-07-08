using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    #region Using Directives

    

    #endregion

    /// <summary>
    ///   The ad block action.
    /// </summary>
    public class AdBlockAction : RuleAction, IRequestAction
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "AdBlockAction" /> class.
        /// </summary>
        public AdBlockAction()
        {
            this.Patterns = new List<UriPattern>();
            this.UpdateFrequencyHours = 72;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Backing field for the list of patterns.
        /// </summary>
        private UriMatchList _patternList;

        /// <summary>
        /// the patterns that are used by this rule.
        /// </summary>
        private IList<UriPattern> _patterns;

        /// <summary>
        ///   Gets or sets Patterns.
        /// </summary>
        public IList<UriPattern> Patterns
        {
            get { return _patterns; }
            set
            {
                _patterns = value;
                this._patternList = new UriMatchList(_patterns);
            }
        }

        /// <summary>
        ///   Gets or sets UpdateFrequencyHours.
        /// </summary>
        public int UpdateFrequencyHours { get; set; }

        /// <summary>
        ///   Gets or sets Url.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        ///   Gets NodeName.
        /// </summary>
        public override string NodeName
        {
            get { return "adblock"; }
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpAction

        /// <summary>
        ///   The setup method for this action.
        /// </summary>
        /// <param name = "xml">
        ///   The xml for the action.
        /// </param>
        /// <returns>
        ///   This object if successful, else null.
        /// </returns>
        public override bool Deserialize(XElement xml)
        {
            var url = xml.Attribute("url");
            if (url != null && Uri.IsWellFormedUriString(url.Value, UriKind.Absolute))
            {
                this.Url = new Uri(url.Value);
            }
            else
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.Url.AbsoluteUri))
            {
                return false;
            }

            return this.UpdatePatterns();
        }

        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XAttribute("url", this.Url.ToString()));
        }

        #endregion

        #region IRequestAction

        /// <summary>
        ///   Check to see if the request is allowed.
        /// </summary>
        /// <param name = "session">
        ///   The session.
        /// </param>
        /// <param name = "rule">
        ///   The rule that has this action.
        /// </param>
        /// <returns>
        ///   True to continue processing, or false if the request should be blocked.
        /// </returns>
        public bool BeforeRequest(Session session, Rule rule)
        {
            var requestProperties = new RequestProperties(session.fullUrl, session.oRequest.headers["Referer"],
                                                          session.oRequest.headers["Accept"]);

            UriPattern matchedPattern;
            var result = _patternList.Match(requestProperties, out matchedPattern);
            if (result == UriPatternMatchType.Match)
            {
                session.oRequest.FailSession(204, "Blocked By AdBlock", String.Empty);
                RuleLog.Current.AddRule(rule, session, String.Format("AdBlock ({0})", matchedPattern.Pattern));
                return false;
            }

            if (result == UriPatternMatchType.Exception)
            {
                RuleLog.Current.AddRule(rule, session, String.Format("AdBlock Except({0})", matchedPattern.Pattern));
            }

            return true;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        ///   The read stream.
        /// </summary>
        /// <param name = "stream">
        ///   The stream.
        /// </param>
        private void ReadStream(Stream stream)
        {
            var patterns = new List<UriPattern>();
            if (stream != null)
            {
                var reader = new StreamReader(stream);
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var pattern = new UriPattern(line);

                    patterns.Add(pattern);
                }
            }

            this.Patterns = patterns;
        }

        /// <summary>
        ///   Read all the AdBlock rules from either disk or the web.
        /// </summary>
        /// <returns>
        ///   True if the option was successful.
        /// </returns>
        private bool UpdatePatterns()
        {
            var filename = this.Url.AbsoluteUri.MD5Hash() + ".abp";
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            filename = Path.Combine(appData, "HttpRules", filename);
            var exists = File.Exists(filename);
            if (!exists ||
                (DateTime.UtcNow - new FileInfo(filename).LastWriteTimeUtc).TotalHours > this.UpdateFrequencyHours)
            {
                try
                {
                    var request = WebRequest.Create(this.Url);
                    var response = request.GetResponse();
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                var bytes = new byte[ms.Length];
                                ms.Seek(0, SeekOrigin.Begin);
                                ms.Read(bytes, 0, (int) ms.Length);
                                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                                File.WriteAllBytes(filename, bytes);
                            }
                        }
                    }
                }
                catch (WebException)
                {
                    // TODO: Log this
                }
                catch (IOException)
                {
                    // TODO: Log this
                }
            }

            try
            {
                if (filename != null)
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        this.ReadStream(stream);
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        #endregion

        /* * is wildcard
         * @@ is exception 
         * | is match at start/end of filter (regex ^ and $)
         * || match domain
         * ^ is wildcard for separator characters (or end). [~0-9a-zA-Z-.%]
         * ! is comment
         * $ is option
         * $~ is not option
         * domain option uses | as OR operator
         * /../ is a regular expression
         * 
         * ## is element hiding
         * domains can be separated by , (domain1.example,domain2.example,domain3.example##*.sponsor)
         * ~ can be applied to domains
         * # is element hiding (depricated)
         */
    }
}