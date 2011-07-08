using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpRulesCore.Actions
{
    /// <summary>
    /// Routes request to a third party proxy (possibly an anonymiser).
    /// </summary>
    public class ProxyAction
    {
        public string ProxyFormat { get; set; }

        public bool BeforeReqest(Fiddler.Session session, Rule rule)
        {
            return true;
        }
    }
}
