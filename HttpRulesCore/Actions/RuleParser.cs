using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace HttpRulesCore.Actions
{
    public class RuleParser
    {
        private static readonly Dictionary<string, Type> RuleActionTypes;

        static RuleParser()
        {
            var ruleActionType = typeof(RuleAction);

            RuleActionTypes = (from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.IsSubclassOf(ruleActionType)
                                select t).ToDictionary(k => ((RuleAction)k.GetConstructor(new Type[] { }).Invoke(null)).NodeName, v => v);
        }

        public static bool GetActions(IEnumerable<XElement> actionsXml, out IList<IHttpAction> actions, out IList<IRequestAction> requestActions, out IList<IResponseAction> responseActions)
        {
            actions = new List<IHttpAction>();
            requestActions = new List<IRequestAction>();
            responseActions = new List<IResponseAction>();
            var actionsTemp = actions;
            Func<IList<IHttpAction>, IHttpAction, bool> addFunc = (a, ra) =>
                                                                     {
                                                                         a.Add(ra);
                                                                         return true;
                                                                     }; // hmm, this is just to keep the query below as a single statement, perhaps there is a better way? 
            foreach (var ruleAction in from actionXml in actionsXml.Elements()
                                       let name = actionXml.Name.ToString()
                                       where RuleActionTypes.ContainsKey(name)
                                       let actionType = RuleActionTypes[name]
                                       where actionType != null
                                       let constructor = actionType.GetConstructor(new Type[] {})
                                       where constructor != null
                                       let ruleAction = (IHttpAction) constructor.Invoke(new object[] {})
                                       where ruleAction != null && addFunc(actionsTemp, ruleAction)
                                       where ruleAction.Deserialize(actionXml)
                                       select ruleAction)
            {
                if (ruleAction is IRequestAction)
                {
                    requestActions.Add((IRequestAction)ruleAction);
                }

                if (ruleAction is IResponseAction)
                {
                    responseActions.Add((IResponseAction)ruleAction);
                }
            } 

            return true;
        }
    }
}
