using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Fiddler;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace HttpRulesCore.Actions
{
    public class PythonAction : RuleAction, IRequestAction
    {
        private CompiledCode _code;
        private ScriptRuntime _runtime;

        #region IRequestAction Members

        public string Code { get; set; }

        public override string NodeName
        {
            get { return "python"; }
        }

        public override bool Deserialize(XElement xml)
        {
            this.Code = xml.Value;

            var options = new Dictionary<string, object>();
            options["LightweightScopes"] = true;
            var engine = Python.CreateEngine(options);

            _runtime = engine.Runtime;

            try
            {
                var source = engine.CreateScriptSourceFromString(this.Code, SourceCodeKind.AutoDetect);
                _code = source.Compile();
            }
            catch
            {
                // TODO: Log this
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serializes this instance so that it can be persisted after changes are made during operation.
        /// </summary>
        /// <returns>The XML for this instance.</returns>
        public override XElement Serialize()
        {
            return new XElement(this.NodeName, new XCData(this.Code));
        }

        public bool BeforeRequest(Session session, Rule rule)
        {
            var scope = _runtime.CreateScope();

            scope.SetVariable("session", session);
            scope.SetVariable("rule", rule);

            try
            {
                var result = _code.Execute<bool>(scope);
                RuleLog.Current.AddRule(rule, session, String.Format("Python Script ({0})", session.hostname));
                return result;
            }
            catch
            {
                RuleLog.Current.AddRule(rule, session, String.Format("Python Exception ({0})", session.hostname));
                return true;
            }
        }

        #endregion
    }
}