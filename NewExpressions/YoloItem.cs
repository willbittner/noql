using NoQL.CEP.NewExpressions;
using NoQL.YoloPrompt.DashX.YoloLambda;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NoQL.TempGUI.DesignStudio
{
    public enum YoloItemType
    {
        Extension = 0,
        Expression,
        Component,
        Framework
    }

    [Serializable()]
    public class YoloItem
    {
        private static int NextID;

        public static int GetNextID()
        {
            return Interlocked.Increment(ref NextID);
        }

        public List<YoloItem> ChildItems = new List<YoloItem>();

        public YoloItemType ItemType { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Usings { get; set; }

        public string References { get; set; }

        public int ID { get; set; }

        public bool LoadedFromCEP { get; set; }

        public bool IsCompiled { get; set; }

        public string CompileErrors { get; set; }

        public YoloItem(YoloItemType type)
        {
            ItemType = type;
            ID = GetNextID();
            LoadedFromCEP = false;
            IsCompiled = false;
        }

        public YoloItem()
        {
        }

        public void Compile()
        {
            if (IsCompiled || LoadedFromCEP) return;
            if (string.IsNullOrEmpty(Code)) return;
            string compileerrors = "";
            if (ItemType == YoloItemType.Extension)
            {
                YoloCompile.MakeExt(this, out compileerrors);
                Yolo.Expressions.AddExpression(Name);
            }
            else
            {
                YoloCompile.MakeExpr(this, Code, Usings, References, out compileerrors);
            }

            CompileErrors = compileerrors;
            if (compileerrors == " " || compileerrors == "")
            {
                IsCompiled = false;
            }
            else IsCompiled = true;
        }

        public void AddChild(YoloItem item)
        {
            ChildItems.Add(item);
        }

        public bool RemoveChild(YoloItem item)
        {
            return ChildItems.Remove(item);
        }

        public IEnumerable<YoloItem> GetAllChildren()
        {
            List<YoloItem> children = new List<YoloItem>();
            foreach (YoloItem item in ChildItems)
            {
                children.Add(item);
                children.AddRange(item.GetAllChildren());
            }
            return children;
        }
    }
}