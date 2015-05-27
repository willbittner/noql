using Microsoft.CSharp;
using NoQL.CEP.NewExpressions;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoQL.YoloLambda
{
    public class YoloMaker
    {
        public const string REPLACE_TOKEN = @"@@RETURN@@";
        public const string TEMPLATE_PATH = @"C:\CEPScripts\YoloMaker.cs.template";

        public static string EXPR_TEMPLATE;
        public static string FUNC_TEMPLATE;

        public static INewCEPExpression MakeExpr(string expr, out string compileErrors)
        {
            string code = EXPR_TEMPLATE.Replace(REPLACE_TOKEN, expr);
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