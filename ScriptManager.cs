using Microsoft.CSharp;
using NoQL.CEP.Adapters;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoQL.CEP
{
    public class ScriptManager
    {
        public string ScriptDirectory { get; set; }

        public Dictionary<string, IScriptTransformAdapter> Scripts { get; set; }

        public ScriptManager(string ScriptDirectory)
        {
            this.ScriptDirectory = ScriptDirectory;
            Scripts = new Dictionary<string, IScriptTransformAdapter>();
            LoadScripts();
        }

        internal Assembly Compile(string file)
        {
            var provider = new CSharpCodeProvider(
                new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
            ICodeCompiler compiler = provider.CreateCompiler();

            string Script = File.ReadAllText(file);

            var compilerparams = new CompilerParameters();

            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;

            IEnumerable<string> referencedAssemblies = GetType().Assembly.GetReferencedAssemblies().Select(a => a.Name);

            foreach (string referencedAssembly in referencedAssemblies)
            {
                compilerparams.ReferencedAssemblies.Add(referencedAssembly + ".dll");
            }

            compilerparams.ReferencedAssemblies.Add(GetType().Assembly.GetName().Name + ".dll");

            foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*.dll"))
            {
                compilerparams.ReferencedAssemblies.Add(f);
            }

            CompilerResults results =
                compiler.CompileAssemblyFromSource(compilerparams, Script);

            if (results.Errors.HasErrors)
            {
                var errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n",
                                        error.Line, error.Column, error.ErrorText);
                }
                Console.Write("-");
                throw new Exception(errors.ToString());
            }

            return results.CompiledAssembly;
        }

        public IScriptTransformAdapter<InputType, OutputType> GetScript<InputType, OutputType>(string name)
        {
            return Scripts[name] as IScriptTransformAdapter<InputType, OutputType>;
        }

        private void LoadScripts()
        {
            foreach (string file in Directory.GetFiles(ScriptDirectory, "*.cs"))
            {
                Console.WriteLine("Compiling Script File: " + file);
                Assembly asm = Compile(file);

                foreach (TypeInfo tt in asm.DefinedTypes)
                {
                    Type t = tt.AsType();
                    object instance = Activator.CreateInstance(t);
                    if (instance is IScriptTransformAdapter)
                    {
                        Scripts[t.Name] = instance as IScriptTransformAdapter;
                    }
                }
            }
        }
    }
}