// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleEngine.cs" company="">
//   
// </copyright>
// <summary>
//   The rule engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Xml.Linq;

    using Fiddler;

    #endregion

    /// <summary>
    /// The rule engine.
    /// </summary>
    public class RuleEngine
    {
        #region Constants and Fields

        /// <summary>
        /// The path to the rules.
        /// </summary>
        private string _path;

        private int _sessionCount;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEngine"/> class.
        /// </summary>
        public RuleEngine()
        {
            FiddlerApplication.BeforeRequest += this.FiddlerApplicationBeforeRequest;
            FiddlerApplication.BeforeResponse += this.FiddlerApplicationBeforeResponse;
            FiddlerApplication.AfterSessionComplete += FiddlerApplicationAfterSessionComplete;
        }

        #endregion

        #region Events

        /// <summary>
        /// The response received.
        /// </summary>
        public event EventHandler<ResponseSummaryEventArgs> ResponseReceived;

        /// <summary>
        /// The rules updated.
        /// </summary>
        public event EventHandler RulesUpdated;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the request rules.
        /// </summary>
        public IEnumerable<Rule> RequestRules { get; set; }

        /// <summary>
        /// Gets or sets the response rules.
        /// </summary>
        public IEnumerable<Rule> ResponseRules { get; set; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        public IEnumerable<Rule> Rules { get; set; }

        /// <summary>
        /// Gets the number of sessions that are currently active.
        /// </summary>
        public int SessionCount
        {
            get
            {
                return this._sessionCount;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serialize the rules to an XML document.
        /// </summary>
        /// <returns>
        /// The XML document of the rules.
        /// </returns>
        public XDocument Serialize()
        {
            var doc = new XDocument();
            var rulesNode = new XElement("rules");
            doc.AddFirst(rulesNode);
            foreach (var rule in this.Rules)
            {
                rulesNode.Add(rule.Serialize());
            }

            return doc;
        }

        /// <summary>
        /// Shutdown the engine and save the rules.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                FiddlerApplication.Shutdown();
                this.Serialize().Save(this._path);
                var startedShutdown = DateTime.UtcNow;
                while (FiddlerApplication.isClosing && (DateTime.UtcNow - startedShutdown).TotalSeconds < 10)
                {
                    Thread.Sleep(250);
                }
            }
            catch
            {
            }
            finally
            {
                this._sessionCount = 0;
            }
        }

        /// <summary>
        /// Start the rules engine.
        /// </summary>
        /// <param name="path">
        /// The path to the rules.
        /// </param>
        public void Start(string path)
        {
            this._sessionCount = 0;
            this._path = path;
            this.Rules = Rule.Parse(this._path).ToList();
            this.Rules.ForEach(r => r.ResponseLog = this.ResponseReceived);

            this.RequestRules = (from r in this.Rules where r.RequestActions.Any() select r).ToList();
            this.ResponseRules = (from r in this.Rules where r.ResponseActions.Any() select r).ToList();

            FiddlerApplication.Startup(FindFreePort(), true, false);

            this.RulesUpdated.FireEvent(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds a free port to listen on.
        /// </summary>
        /// <returns>
        /// A port number that is available.
        /// </returns>
        private static int FindFreePort()
        {
            var port = 8181;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            foreach (var tcpi in
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().OrderBy(t => t.LocalEndPoint.Port))
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    port++;
                }
            }

            return port;
        }

        /// <summary>
        /// The fiddler application before request.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        private void FiddlerApplicationBeforeRequest(Session session)
        {
            Interlocked.Increment(ref this._sessionCount);

            var requestProperties = new RequestProperties(session.fullUrl, session.oRequest.headers["Referer"], session.oRequest.headers["Accept"]);

            if (requestProperties.Uri == null)
            { 
                // Malformed URI, let's terminate the request.
                session.oRequest.FailSession(400, "Bad Request (Malformed Uri)", String.Empty);
                return;
            }

            var filteredRules = from r in this.RequestRules where r.Enabled where r.Matches == null || UriPattern.Match(requestProperties, r.Matches) select r;

            foreach (var rule in filteredRules.Where(rule => rule.RequestActions.Any(action => action.Enabled && !action.BeforeRequest(session, rule))))
            {
                break;
            }
        }

        /// <summary>
        /// The fiddler application before response.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        private void FiddlerApplicationBeforeResponse(Session session)
        {
            var requestProperties = new RequestProperties(session.fullUrl, session.oRequest.headers["Referer"], session.oRequest.headers["Accept"]);

            var filteredRules = from r in this.ResponseRules where r.Enabled where r.Matches == null || UriPattern.Match(requestProperties, r.Matches) select r;

            (from rule in filteredRules let rule1 = rule where rule.ResponseActions.Any(action => action.Enabled && !action.OnResponse(session, rule1)) select rule).Any();

            if (this.ResponseReceived != null)
            {
                this.ResponseReceived(
                    session, new ResponseSummaryEventArgs { ResponseCode = session.responseCode, FullUrl = session.fullUrl, ResponseCodeText = session.oResponse.headers.HTTPResponseStatus, Referer = (session.oRequest["Referer"].HasValue() ? session.oRequest["Referer"] : "None") + ", Sessions: " + this.SessionCount, Length = session.ResponseBody.LongLength });
            }
        }

        /// <summary>
        /// The handler for when a session completes.
        /// </summary>
        /// <param name="oSession">The session parameter.</param>
        void FiddlerApplicationAfterSessionComplete(Session oSession)
        {
            Interlocked.Decrement(ref this._sessionCount);
        }

        #endregion
    }
}