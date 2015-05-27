using Microsoft.CSharp;
using NoQL.CEP.NewExpressions;
using NoQL.CEP.Properties;
using NoQL.TempGUI.DesignStudio;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoQL.YoloPrompt
{
    namespace DashX.YoloLambda
    {
        public class YoloCompile
        {
            public static string CompiledExtensionFunctions = "";
            public const string REPLACE_TOKEN = @"@@RETURN@@";

            public static string EXPR_TEMPLATE;
            public static string FUNC_TEMPLATE;

            public static INewCEPExpression MakeExt(YoloItem item, out string compileErrors)
            {
                string emptystr = " ";
                if (item.Usings == null) item.Usings = emptystr;
                if (item.References == null) item.References = emptystr;
                string code = Resources.ExtensionTemplate;
                code = code.Replace("@@USINGS@@", item.Usings);
                code = code.Replace("@@EXTCLASS@@", item.Code);

                //string code = File.ReadAllText(TEMPLATE_PATH).Replace(REPLACE_TOKEN, lambda);

                var provider = new CSharpCodeProvider(
                    new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
                ICodeCompiler compiler = provider.CreateCompiler();

                var compilerparams = new CompilerParameters();

                compilerparams.GenerateExecutable = false;
                compilerparams.GenerateInMemory = true;

                IEnumerable<string> referencedAssemblies = typeof(YoloWrapper).Assembly.GetReferencedAssemblies().Select(a => a.Name);

                foreach (string referencedAssembly in referencedAssemblies)
                {
                    string name = referencedAssembly + ".dll";
                    if (name != "WindowsBase.dll")
                        compilerparams.ReferencedAssemblies.Add(name);
                }

                //compilerparams.ReferencedAssemblies.Add(typeof(YoloWrapper).Assembly.GetName().Name + ".exe");

                foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*.dll"))
                {
                    compilerparams.ReferencedAssemblies.Add(f);
                }
                string allcode = code;
                CompilerResults results = RuntimeCodeCompiler.CompileCode(code);

                if (results.Errors.HasErrors)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    foreach (CompilerError error in results.Errors)
                    {
                        errors.AppendFormat("Line {0},{1}\t: {2}\n",
                                            error.Line, error.Column, error.ErrorText);
                    }
                    Console.Write("-");
                    compileErrors = errors.ToString();
                    throw new Exception(errors.ToString());
                }
                else
                {
                    CompiledExtensionFunctions += "\n " + code;
                }

                //Assembly assm = results.CompiledAssembly;
                //INewCEPExpression expr2 = Yolo.Express();
                //foreach (Type t in assm.DefinedTypes)
                //{
                //    object wrapper = Activator.CreateInstance(t);
                //    expr2 = (INewCEPExpression)t.InvokeMember("Generate", BindingFlags.InvokeMethod, null, wrapper, null);
                //}

                //if (item.ItemType == YoloItemType.Component)
                //{
                //    Yolo.Manager.Register(expr2 as INewComponent, item.Name, 0);
                //}
                //if (item.ItemType == YoloItemType.Extension)
                //{
                //    Yolo.Expressions.AddExpression(item.Name);
                //}
                compileErrors = "";
                //return expr2;

                return null;
            }

            public static INewCEPExpression MakeExpr(YoloItem item, string expr, string usings, string references, out string compileErrors)
            {
                string emptystr = " ";
                if (usings == null) usings = emptystr;
                if (references == null) references = emptystr;
                string code = Resources.superTemplateExpr.Replace("@@RETURN@@", expr);
                code = code.Replace("@@USINGS@@", usings);
                code = code.Replace("@@REFS@@", references);
                //string code = File.ReadAllText(TEMPLATE_PATH).Replace(REPLACE_TOKEN, lambda);

                var provider = new CSharpCodeProvider(
                    new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
                ICodeCompiler compiler = provider.CreateCompiler();

                var compilerparams = new CompilerParameters();

                compilerparams.GenerateExecutable = false;
                compilerparams.GenerateInMemory = true;

                IEnumerable<string> referencedAssemblies = typeof(YoloWrapper).Assembly.GetReferencedAssemblies().Select(a => a.Name);

                foreach (string referencedAssembly in referencedAssemblies)
                {
                    string name = referencedAssembly + ".dll";
                    if (name != "WindowsBase.dll")
                        compilerparams.ReferencedAssemblies.Add(name);
                }

                //compilerparams.ReferencedAssemblies.Add(typeof(YoloWrapper).Assembly.GetName().Name + ".exe");

                foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*.dll"))
                {
                    compilerparams.ReferencedAssemblies.Add(f);
                }

                CompilerResults results = RuntimeCodeCompiler.CompileCode(code);
                //compiler.CompileAssemblyFromSource(compilerparams, code);

                if (results.Errors.HasErrors)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    foreach (CompilerError error in results.Errors)
                    {
                        errors.AppendFormat("Line {0},{1}\t: {2}\n",
                                            error.Line, error.Column, error.ErrorText);
                    }
                    Console.Write("-");
                    compileErrors = errors.ToString();
                    throw new Exception(errors.ToString());
                }
                compileErrors = "";

                Assembly assm = results.CompiledAssembly;
                INewCEPExpression expr2 = Yolo.Express();
                foreach (Type t in assm.DefinedTypes)
                {
                    object wrapper = Activator.CreateInstance(t);
                    expr2 = (INewCEPExpression)t.InvokeMember("Generate", BindingFlags.InvokeMethod, null, wrapper, null);
                }

                if (item.ItemType == YoloItemType.Component)
                {
                    Yolo.Manager.Register(item.Name, 0);
                    ((INewComponent)expr2).Attach(Yolo.Manager.Get(item.Name));
                }
                if (item.ItemType == YoloItemType.Extension)
                {
                    Yolo.Expressions.AddExpression(item.Name);
                }
                compileErrors = "";
                return expr2;

                return null;
            }

            public static INewCEPExpression MakeExpr(string expr, out string compileErrors)
            {
                Yolo.Express();
                //string code = Resource1.template;
                string code = Resources.templateExpr.Replace(REPLACE_TOKEN, expr);
                //string code = File.ReadAllText(TEMPLATE_PATH).Replace(REPLACE_TOKEN, lambda);

                var provider = new CSharpCodeProvider(
                    new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
                ICodeCompiler compiler = provider.CreateCompiler();

                var compilerparams = new CompilerParameters();

                compilerparams.GenerateExecutable = false;
                compilerparams.GenerateInMemory = true;

                IEnumerable<string> referencedAssemblies = typeof(YoloWrapper).Assembly.GetReferencedAssemblies().Select(a => a.Name);

                foreach (string referencedAssembly in referencedAssemblies)
                {
                    string name = referencedAssembly + ".dll";
                    if (name != "WindowsBase.dll")
                        compilerparams.ReferencedAssemblies.Add(name);
                }

                //compilerparams.ReferencedAssemblies.Add(typeof(YoloWrapper).Assembly.GetName().Name + ".exe");

                foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*.dll"))
                {
                    compilerparams.ReferencedAssemblies.Add(f);
                }

                CompilerResults results = RuntimeCodeCompiler.CompileCode(code);

                if (results.Errors.HasErrors)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    foreach (CompilerError error in results.Errors)
                    {
                        errors.AppendFormat("Line {0},{1}\t: {2}\n",
                                            error.Line, error.Column, error.ErrorText);
                    }
                    Console.Write("-");
                    compileErrors = errors.ToString();
                    throw new Exception(errors.ToString());
                }
                compileErrors = "";

                Assembly assm = results.CompiledAssembly;

                foreach (Type t in assm.DefinedTypes)
                {
                    object wrapper = Activator.CreateInstance(t);
                    return (INewCEPExpression)t.InvokeMember("Generate", BindingFlags.InvokeMethod, null, wrapper, null);
                }
                compileErrors = "";
                return null;
            }

            public static Func<object, object> MakeFunc(string lambda, out string compileErrors)
            {
                string code = FUNC_TEMPLATE.Replace(REPLACE_TOKEN, lambda);

                var provider = new CSharpCodeProvider(
                    new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
                ICodeCompiler compiler = provider.CreateCompiler();

                var compilerparams = new CompilerParameters();

                compilerparams.GenerateExecutable = false;
                compilerparams.GenerateInMemory = true;

                IEnumerable<string> referencedAssemblies = typeof(YoloWrapper).Assembly.GetReferencedAssemblies().Select(a => a.Name);

                foreach (string referencedAssembly in referencedAssemblies)
                {
                    compilerparams.ReferencedAssemblies.Add(referencedAssembly + ".dll");
                }

                //compilerparams.ReferencedAssemblies.Add(typeof(YoloWrapper).Assembly.GetName().Name + ".exe");

                foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*.dll"))
                {
                    compilerparams.ReferencedAssemblies.Add(f);
                }

                CompilerResults results =
                    compiler.CompileAssemblyFromSource(compilerparams, code);

                if (results.Errors.HasErrors)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    foreach (CompilerError error in results.Errors)
                    {
                        errors.AppendFormat("Line {0},{1}\t: {2}\n",
                                            error.Line, error.Column, error.ErrorText);
                    }
                    Console.Write("-");
                    compileErrors = errors.ToString();
                    throw new Exception(errors.ToString());
                }
                compileErrors = "";

                Assembly assm = results.CompiledAssembly;

                foreach (Type t in assm.DefinedTypes)
                {
                    object wrapper = Activator.CreateInstance(t);
                    return (Func<object, object>)t.InvokeMember("Generate", BindingFlags.InvokeMethod, null, wrapper, null);
                }
                return null;
            }

            #region Nested type: YoloWrapper

            public abstract class YoloWrapper
            {
                public abstract Func<object, object> Generate();
            }

            #endregion Nested type: YoloWrapper
        }
    }

    public static class RuntimeCodeCompiler
    {
        private static volatile Dictionary<string, Assembly> cache = new Dictionary<string, Assembly>();
        private static object syncRoot = new object();
        private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        static RuntimeCodeCompiler()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assembly assembly = null;
                assemblies.TryGetValue(e.Name, out assembly);
                return assembly;
            };
        }

        public static CompilerResults CompileCode(string code)
        {
            Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = false;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string location = assembly.Location;
                    if (!String.IsNullOrEmpty(location))
                    {
                        compilerparams.ReferencedAssemblies.Add(location);
                    }
                }
                catch (NotSupportedException)
                {
                    // this happens for dynamic assemblies, so just ignore it.
                }
            }

            CompilerResults results =
               compiler.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n",
                           error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                AppDomain.CurrentDomain.Load(results.CompiledAssembly.GetName());
                return results;
            }
        }

        public static Assembly CompileCodeOrGetFromCache(string code, string key)
        {
            bool exists = cache.ContainsKey(key);

            if (!exists)
            {
                lock (syncRoot)
                {
                    exists = cache.ContainsKey(key);

                    if (!exists)
                    {
                        cache.Add(key, CompileCode(code).CompiledAssembly);
                    }
                }
            }

            return cache[key];
        }
    }
}