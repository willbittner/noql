using NoQL.CEP.NewExpressions;
using System;
using System.Collections.Generic;

namespace NoQL.CoreYoloQL.StrategyDevGUI
{
    [Serializable]
    public class YoloQLFunction
    {
        public string Name { get; set; }

        public string Input { get; set; }

        private string BeginString { get; set; }

        private string EndString { get; set; }

        public bool IsCompiled { get; set; }

        private List<INewCEPExpression<object>> Expressions = new List<INewCEPExpression<object>>();

        public string Template
        {
            get { return BeginString + EndString; }
            set { }
        }

        public string Code
        {
            get
            {
                return BeginString + Input + EndString;
            }
            set { }
        }

        public YoloQLFunction(string name, string beginString)
        {
            IsCompiled = false;
            Name = name;
            BeginString = beginString;
            EndString = ")";
        }

        public void AddExpression(INewCEPExpression<object> expr)
        {
            Expressions.Add(expr);
        }

        public void DeleteExpression()
        {
            foreach (var expr in Expressions)
            {
                //foreach (var block in NodeAndEdgeKeepr.Graph.Vertices)
                //{
                //    block.RemoveChild(expr.HeadBlock);
                //    block.RemoveChild(expr.InputBlock);

                //}
            }
        }
    }

    [Serializable]
    public class RESTYoloQLFunctions
    {
        public static List<YoloQLFunction> Functions = new List<YoloQLFunction>();
    }

    public class RESTBackend
    {
    }
}