// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="">
//   
// </copyright>
// <summary>
//   Extension methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Xml.Linq;

namespace HttpRulesCore
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    using Fiddler;

    #endregion

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        #region Public Methods

        /// <summary>
        /// Are two strings equal, case insensitive.
        /// </summary>
        /// <param name="s">
        /// The sources.
        /// </param>
        /// <param name="comparand">
        /// The comparand.
        /// </param>
        /// <returns>
        /// True if the string are the same (case insensitive).
        /// </returns>
        public static bool EqualsCIS(this string s, string comparand)
        {
            return s.Equals(comparand, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified string has value.
        /// </summary>
        /// <param name="s">The string to check for a value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified string has value; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValue(this string s)
        {
            return !String.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Gets the value of an attribute, or the default.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The value or an empty string.</returns>
        public static string ValueOrDefault(this XAttribute attribute)
        {
            return attribute != null ? attribute.Value : String.Empty;
        }

        public static Dictionary<TK, TV> ToDictionary<TK, TV>(this IEnumerable<KeyValuePair<TK, TV>> values, IEqualityComparer<TK> comparer = null)
        {
            var dict = comparer != null ? new Dictionary<TK, TV>(comparer) : new Dictionary<TK, TV>();
            values.ForEach(kvp => dict[kvp.Key] = kvp.Value);
            return dict;
        }

        /// <summary>
        /// Foreach loop, LINQ style.
        /// </summary>
        /// <typeparam name="T">
        /// The object type to action.
        /// </typeparam>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var obj in source)
            {
                action(obj);
            }
        }

        /// <summary>
        /// Fires the event.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="evt">The event.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data.</param>
        public static void FireEvent<T>(this EventHandler<T> evt, object sender, T e) where T : EventArgs
        {
            if (evt != null)
            {
                evt.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Fires the event.
        /// </summary> 
        /// <param name="evt">The event.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void FireEvent(this EventHandler evt, object sender, EventArgs e) 
        {
            if (evt != null)
            {
                evt.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Checks if the URI has a file extension.
        /// </summary>
        /// <param name="uri">
        /// The uri to check.
        /// </param>
        /// <param name="extensions">
        /// The extensions.
        /// </param>
        /// <returns>
        /// The has file extension.
        /// </returns>
        public static bool HasFileExtension(this Uri uri, params string[] extensions)
        {
            if (uri == null || !uri.OriginalString.HasValue())
            {
                return false;
            }

            var queryStart = uri.OriginalString.IndexOf('?');
            queryStart = queryStart == -1 ? uri.OriginalString.Length: queryStart;
            var pathEnd = uri.OriginalString.LastIndexOf('/', queryStart - 1, queryStart);
            pathEnd = pathEnd == -1 ? 0 : pathEnd;
            var fileName = uri.OriginalString.Substring(pathEnd, queryStart - pathEnd);
            if (!fileName.Any(c => c == '.'))
            {
                return false;
            }

            var extension = Path.GetExtension(Path.GetFileName(fileName)).TrimStart('.');
            return extensions.Any(e => e.EqualsCIS(extension));
        }

        /// <summary>
        /// Determines whether the specified URI has a value.
        /// </summary>
        /// <param name="uri">
        /// The URI to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified URI has a value; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValue(this Uri uri)
        {
            return !String.IsNullOrWhiteSpace(uri.OriginalString);
        }

        /// <summary>
        /// Determines whether a Uri is a domain or subdomain of a specified hostname.
        /// </summary>
        /// <param name="uri">
        /// The URI to check to see if it is a subdomain of the <see cref="hostName"/>.
        /// </param>
        /// <param name="hostName">
        /// The host name that may be the same as, or a parent domain of, the <see cref="uri"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the Uri is a domain or subdomain of a specified hostname; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDomainOrSubdomain(this Uri uri, string hostName)
        {
            var hostParts = hostName.Split('.').Reverse().ToArray();

            if (hostParts.Length < 2)
            {
                return uri.Host.Equals(hostName, StringComparison.OrdinalIgnoreCase);
            }

            var uriParts = uri.Host.Split('.').Reverse().ToArray();

            if (hostParts.Length > uriParts.Length)
            {
                return false;
            }

            return !hostParts.Where((t, i) => !String.Equals(hostParts[i], uriParts[i], StringComparison.OrdinalIgnoreCase)).Any();
        }

        /// <summary>
        /// Gets the MD5 hash of the string.
        /// </summary>
        /// <param name="s">
        /// The string to get the hash for.
        /// </param>
        /// <returns>
        /// An MD5 Hash of the string.
        /// </returns>
        public static string MD5Hash(this string s)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(s);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Overrides the request, issuing an internal request and then using that response as the response to the original request.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="url">
        /// The URL that will be requested.
        /// </param>
        /// <returns>
        /// True if the request succeeded.
        /// </returns>
        public static bool OverrideRequest(this Session session, string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = session.oRequest.headers.HTTPMethod;

            foreach (HTTPHeaderItem header in session.oRequest.headers)
            {
                switch (header.Name.ToLower())
                {
                    case "host":
                        break;
                    case "proxy-connection":
                        break;
                    case "if-modified-since":
                        DateTime modDate;
                        if (DateTime.TryParse(header.Value, out modDate))
                        {
                            request.IfModifiedSince = modDate;
                        }

                        break;
                    case "accept-encoding": // Currently this is not working
                        break;
                    case "user-agent":
                        request.UserAgent = header.Value;
                        break;
                    case "accept":
                        request.Accept = header.Value;
                        break;
                    case "referer":
                        request.Referer = header.Value;
                        break;
                    default:
                        request.Headers[header.Name] = header.Value;
                        break;
                }
            }

            if (session.requestBodyBytes.Length > 0)
            {
                request.GetRequestStream().Write(session.requestBodyBytes, 0, session.requestBodyBytes.Length);
            }

            HttpWebResponse response;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException webException)
            {
                response = webException.Response as HttpWebResponse;
            }

            Stream rs;
            if (response != null && (rs = response.GetResponseStream()) != null)
            {
                session.utilCreateResponseAndBypassServer();
                session.oResponse.headers = new HTTPResponseHeaders(CONFIG.oHeaderEncoding)
                    {
                       HTTPResponseCode = (int)response.StatusCode, HTTPResponseStatus = (int)response.StatusCode + " " + response.StatusDescription 
                    };

                foreach (var headerName in response.Headers.AllKeys)
                {
                    var headerValue = response.Headers[headerName];
                    session.oResponse.headers.Add(headerName, headerValue);
                }

                var sr = new StreamReader(rs);
                session.utilSetResponseBody(sr.ReadToEnd());
                return true;
            }

            return false;

            // duplicate the request but let the browser handle it. TODO: this should really return something to the client as we are trying to avoid making this request in the first place.
        }

        /// <summary>
        /// Redirects the session to a different URL.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="url">
        /// The URL to redirect to.
        /// </param>
        /// <param name="permanent">
        /// if set to <c>true</c> the redirect is a 301 Moved Permanently, else it is a 302 Found.
        /// </param>
        public static void RedirectSession(this Session session, string url, bool permanent)
        {
            session.utilCreateResponseAndBypassServer();

            session.oResponse.headers = new HTTPResponseHeaders(CONFIG.oHeaderEncoding);
            session.oResponse.headers.Add("Location", url);
            session.oResponse.headers.HTTPResponseStatus = permanent ? "301 Moved Permanently" : "302 Found";
            session.oResponse.headers.HTTPResponseCode = permanent ? 301 : 302;
            session.oResponse.headers.Add("Content-Length", "0");
        }

        /// <summary>
        /// Returns a 204 No Content response to the client, which will usually silently stop the request and stop further processing by the browser (i.e. safe abort).
        /// </summary>
        /// <param name="session">The session.</param>
        public static void NoContentSession(this Session session)
        {
            session.utilCreateResponseAndBypassServer();

            session.oResponse.headers = new HTTPResponseHeaders(CONFIG.oHeaderEncoding)
                                            {
                                                HTTPResponseStatus = "204 No Content",
                                                HTTPResponseCode = 204
                                            };
            session.oResponse.headers.Add("Content-Length", "0");
        }

        /// <summary>
        /// Splits the specified source on items that return true.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the the <see cref="IEnumerable{T}"/>
        /// </typeparam>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="splitFunc">
        /// The split function, where returning true will cause the list to split.
        /// </param>
        /// <returns>
        /// A list of source lists, split by a function. The elements that are the base for the split will not be included in the list.
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, Func<T, bool> splitFunc)
        {
            var temp = new List<T>();

            foreach (var item in source)
            {
                if (splitFunc(item))
                {
                    yield return temp;
                    temp = new List<T>();
                }
                else
                {
                    temp.Add(item);
                }
            }

            if (temp.Count > 0)
            {
                yield return temp;
            }
        }

        #endregion
    }
}