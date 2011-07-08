using System;
using System.Linq;
using Fiddler;

namespace HttpRulesCore.Actions
{
    public class CookieAction : HeaderAction
    {
        public override string NodeName
        {
            get { return "cookie"; }
        }

        public override bool BeforeRequest(Session session, Rule rule)
        {
            // Find the headers
            HTTPHeaderItem activeHeader = null;
            foreach (HTTPHeaderItem header in session.oRequest.headers)
            {
                if (header.Name.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    activeHeader = header;
                }
            }

            if (activeHeader != null)
            {
                var cookies = activeHeader.Value.Split(new[] {"; ", ";"}, StringSplitOptions.RemoveEmptyEntries);
                var updated = false;
                for (var i = 0; i < cookies.Length; i++)
                {
                    var cookie = cookies[i];
                    if (cookie.StartsWith(this.Name + "=", StringComparison.OrdinalIgnoreCase)) // Ignore case?
                    {
                        var value = cookie.Substring(this.Name.Length + 1);

                        // Run all the sub actions.
                        foreach (var action in this.Actions)
                        {
                            updated = true;
                            value = action.Run(session, value);
                        }

                        if (String.IsNullOrEmpty(value))
                        {
                            cookies[i] = String.Empty;
                        }
                        else
                        {
                            cookies[i] = cookie.Substring(0, this.Name.Length) + "=" + value;
                        }
                    }
                }

                if (updated)
                {
                    // Set the new value.
                    activeHeader.Value = String.Join("; ", cookies.Where(c => !String.IsNullOrEmpty(c)));
                    RuleLog.Current.AddRule(rule, session, String.Format("Cookies Updated ({0})", session.hostname));
                }
            }

            return true;
        }
    }
}