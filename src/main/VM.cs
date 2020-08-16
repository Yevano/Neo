using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Neo.AST;
using Neo.Backend;
using Neo.Backend.Interp;
using Neo.Backend.JIT;
using Neo.Bytecode;
using Neo.Frontend.Lexer;
using Neo.Frontend.Parser;
using Neo.Runtime;
using Neo.Runtime.Internal;
using Neo.Runtime.Native;
using Neo.Runtime.Native.Modules;
using Neo.Utils;

namespace Neo {
    public sealed class VM {
        public static readonly SemanticVersion VERSION = new SemanticVersion(0, 3, 3);

        private string sourceRoot;
        private Dictionary<string, Scope> modules;
        private Dictionary<string, NeoChunk> loadedModules;

        private Stack<Frame> frames;

        private string importSearchPath;
        private Stack<string> importSearchPaths;

        private Scope baseLib;

        private string[] extraSourceSearchPaths;

        public VM() {
            Interpreter = new InterpreterBackend(this);
            JIT = new JITBackend(this);

            modules = new Dictionary<string, Scope>();
            loadedModules = new Dictionary<string, NeoChunk>();

            frames = new Stack<Frame>();
            importSearchPaths = new Stack<string>();

            NativeModuleLoader.LoadNativeModules(Assembly.GetExecutingAssembly());

            try {
                FFILib.Register(new [] { NeoString.ValueOf("mscorlib"), NeoString.ValueOf("System"), NeoString.ValueOf("neo") });

                baseLib = LoadSTDModule("std/base");

                PushFrame("base.neo", "___init", -1);
                baseLib.Get("___init").Call(new[] { new FFITypeProxy(this) });
                PopFrame();
            } catch(NeoError e) {
                PrintStackTrace(e);
                Environment.Exit(1);
            }
        }

        public JITBackend JIT { get; }

        public InterpreterBackend Interpreter { get; }

        private string SanatizePath(string rawPath) {
            switch (Path.DirectorySeparatorChar) {
                case '/':
                    return rawPath.Replace('\\', Path.DirectorySeparatorChar);
                case '\\':
                    return rawPath.Replace('/', Path.DirectorySeparatorChar);
            }
            throw new Exception(Path.DirectorySeparatorChar.ToString());
        }

        private Scope LoadNativeModule(NativeModuleInfo nativeModule) {
            var name = nativeModule.Name;

            Scope scope;
            if(nativeModule.HasNeoSource) {
                if(name.StartsWith("std")) { // @TODO @Hack StartsWith("std")
                    scope = LoadSTDSource(name);
                } else {
                    scope = LoadFile(name).Scope;
                }
            } else {
                scope = new Scope();
            }

            if (scope == null) throw new NeoError($"Native Import cannot be resolved: {name}");

            foreach(var value in nativeModule.Values) {
                scope.Declare(value.Name, VariableFlags.EXPORTED | VariableFlags.FINAL);
                scope.Set(value.Name, value.Value);
            }

            if(!modules.ContainsKey(name)) {
                modules[name] = scope;
            }

            return scope;
        }

        private Scope LoadSTDSource(string path) {
            var name = $"{path.Substring(path.LastIndexOf('/') + 1)}.neo";
            var resourceName = path.Substring(path.IndexOf('/') + 1).Replace("/", ".");
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName)) {
                if(stream == null) {
                    return null;
                }

                using(var reader = new StreamReader(stream)) {
                    var result = LoadString(reader.ReadToEnd(), name);
                    loadedModules["std/" + name.Replace(".neo", "")] = result; // @TODO @Hack std/ .neo
                    return result.Scope;
                }
            }
        }

        private Scope LoadSTDModule(string rawPath) {
            var name = $"{rawPath.Substring(rawPath.LastIndexOf('/') + 1)}.neo";

            if(loadedModules.ContainsKey(name)) return loadedModules[name].Scope;

            var nativeModule = NativeModuleLoader.LoadNativeModule(rawPath);
            if(nativeModule == null) {
                return LoadSTDSource(rawPath);
            } else {
                return LoadNativeModule(nativeModule);
            }
        }

        internal void PushFrame(string chunkName, string procName, int callSiteLine) => frames.Push(new Frame(chunkName, procName, callSiteLine));

        internal void PopFrame() => frames.Pop();

        internal void Import(Scope parentScope, string rawPath, string alias = null) {
            object value = null;
            if (modules.ContainsKey(rawPath)) {
                value = modules[rawPath].Proxy;
            } else if(rawPath.StartsWith("std")) { // @TODO @Hack StartsWith("std")
                value = LoadSTDModule(rawPath);
            } else {
                var paths = new List<string>() {
                    importSearchPath,
                    sourceRoot,
                    AppDomain.CurrentDomain.BaseDirectory
                };
                paths.AddRange(extraSourceSearchPaths);

                var path = $"{SanatizePath(rawPath)}.neo";
                foreach (var tpath in paths) {
                    if (tpath != null) {
                        var rpath = Path.Combine(tpath, path);
                        if(File.Exists(rpath)) {
                            value = LoadFile(rpath);
                            break;
                        }
                    }
                }
            }

            if (value == null) throw new NeoError($"Import cannot be resolved: {rawPath}");

            if (alias == null) {
                if (value is NeoChunk c) parentScope.Import(c.Scope);
                else if(value is Scope s) parentScope.Import(s);
                else parentScope.Import(((ScopeProxy)value).Scope);
            } else {
                parentScope.Declare(alias, VariableFlags.FINAL);
                if(value is Scope s) parentScope.Set(alias, new ScopeProxy(s));
                else parentScope.Set(alias, (NeoValue)value);
            }
        }

        private NeoChunk BuildNeoChunk(Chunk c) {
            var chunk = new NeoChunk(c);

            foreach (var i in chunk.Chunk.Imports) Import(chunk.Scope, i.Path, i.Alias);

            foreach (var v in chunk.Chunk.Variables) chunk.Scope.Declare(v.Key, v.Value.Flags);

            foreach (var e in chunk.Chunk.Enums) {
            	var flags = VariableFlags.FINAL;
            	if(e.Value.Exported) flags |= VariableFlags.EXPORTED;
            	chunk.Scope.Declare(e.Value.Name, flags);
            }

            foreach (var proc in chunk.Chunk.Procedures) {
            	var flags = VariableFlags.FINAL;
            	if(proc.Value.Exported) flags |= VariableFlags.EXPORTED;
                chunk.Scope.Declare(proc.Key, flags);

                var upvalues = new UpValue[proc.Value.UpValues.Length];
                chunk.Scope.Set(proc.Key, new NeoBackendProcedure(this, Interpreter.Compile(chunk.Scope, chunk.Chunk, proc.Value, upvalues)));
                for (var i = 0; i < upvalues.Length; i++) {
                    upvalues[i] = new UpValue(chunk.Scope, proc.Value.UpValues[i]);
                }
            }

            if (baseLib != null) chunk.Scope.Import(baseLib);

            PushFrame(chunk.Chunk.Name, "__init", -1);
            Interpreter.Compile(chunk.Scope, chunk.Chunk, chunk.Chunk.Initializer, new UpValue[0]).Call(new NeoValue[0]);
            PopFrame();

            return chunk;
        }

        private NeoChunk Compile(ChunkNode ast) {
            var compiler = new BytecodeCompiler(ast);
            return BuildNeoChunk(compiler.Compile());
        }

        public NeoChunk LoadFile(string rawPath) {
            var path = SanatizePath(rawPath);

            if (!File.Exists(path)) throw new Exception($"file not found: {rawPath}");
            if (loadedModules.ContainsKey(path)) return loadedModules[path];

            importSearchPaths.Push(importSearchPath);
            importSearchPath = Directory.GetParent(path).FullName;

            var code = File.ReadAllText(path);
            var name = Path.GetFileName(path);
            var chunk = LoadString(code, name);

            chunk.Scope.Declare("__FILE__", VariableFlags.FINAL);
            chunk.Scope.Set("__FILE__", NeoString.ValueOf(path));

            loadedModules[path.Replace(".neo", "")] = chunk;

            importSearchPath = importSearchPaths.Pop();

            return chunk;
        }

        public NeoChunk LoadString(string code, string name) {
            var lexer = new Lexer(code);
            var parser = new Parser(name, lexer.Tokenize());
            return Compile(parser.ParseChunk());
        }

        public Frame[] GetStackTrace() => frames.ToArray();

        public void PrintStackTrace(NeoError error) {
            Console.WriteLine($"error: {error.Message}");

            var st = GetStackTrace();

            if(st.Length == 0) {
                Console.WriteLine($"    at {error.ChunkName}:{error.Line}");
                return;
            }

            var top = st[0];
            Console.WriteLine($"    at {top.ProcedureName}({top.ChunkName}:{error.Line})");

            for(var i = 1; i < st.Length; i++) {
                var prev = st[i - 1];
                var curr = st[i];
                Console.WriteLine($"    at {curr.ProcedureName}({curr.ChunkName}:{prev.CallSiteLine})");
            }
        }

        private void PrintStackTrace(StackOverflowException _e) {
            Console.WriteLine("error: stack overflow");
            
            var st = GetStackTrace();

            var top = st[0];
            Console.WriteLine($"    at {top.ProcedureName}({top.ChunkName}):???");

            for(var i = 1; i < st.Length; i++) {
                var prev = st[i - 1];
                var curr = st[i];
                Console.WriteLine($"    at {curr.ProcedureName}({curr.ChunkName}):{prev.CallSiteLine}");
            }
        }

        private void LoadEnvironment() {
            string extra = Environment.GetEnvironmentVariable("NEO_SEARCH_PATH");
            
            if(string.IsNullOrEmpty(extra)) {
                extraSourceSearchPaths = new string[0];
            } else {
                extraSourceSearchPaths = extra.Split(':');
                // TODO: Check these?
            }
        }

        public void Run(string file, string[] args) {
            try {
                LoadEnvironment();

                var path = Path.GetFullPath(file);
                sourceRoot = Directory.GetParent(path).FullName;
                
                var chunk = LoadFile(path);

                var mainv = chunk.Scope.Get("main");
                if (!mainv.IsProcedure) {
                    Console.WriteLine("No 'main' procedure found");
                    return;
                }
                var main = mainv.CheckProcedure();

                ((NeoBackendProcedure)main).JIT();

                var argsa = new NeoArray();
                foreach (var arg in args) argsa.Insert(NeoString.ValueOf(arg));

                PushFrame(chunk.Chunk.Name, main.Name(), -1);
                main.Call(new[] { argsa });
                PopFrame();
            } catch (NeoError e) {
                PrintStackTrace(e);
                Environment.Exit(1);
            } catch(StackOverflowException e) {
                PrintStackTrace(e);                
                Environment.Exit(1);
            }
        }
    }
}