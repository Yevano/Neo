using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neo.Runtime.Native.Modules {
    [NativeModule("std/ffi")]
    public static class FFILib {
        private static readonly HashSet<string> assemblies = new HashSet<string>();
        private static readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        [NativeValue("register")]
        public static NeoValue Register(NeoValue[] args) {
            foreach(var arg in args) {
                var name = arg.CheckString().Value;
                if (assemblies.Contains(name)) {
                    return NeoNil.NIL;
                }

                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == name);
                if (assembly == null) {
                    throw new NeoError($"assembly '{name}' cannot be resolved");
                }

                foreach (var type in assembly.GetTypes()) {
                    types[type.FullName] = type;
                }
            }

            return NeoNil.NIL;
        }

        [NativeValue("wrap")]
        public static NeoValue Wrap(NeoValue[] args) {
            if(args.Length != 1) throw new NeoError("wrap expects a single argument"); // @TODO @Untested
            return new FFITypeProxy(args[0]);
        }

        [NativeValue("unwrap")]
        public static NeoValue Unwrap(NeoValue[] args) {
            // @TODO Array bounds check
            if (args[0] is FFITypeProxy) {
                var result = ((FFITypeProxy)args[0]).Object;
                if (result is NeoValue) return (NeoValue)result;
            }
            if(args[0].IsNil || args[0] is NeoInt || args[0] is NeoFloat || args[0] is NeoString || args[0] is NeoBool || args[0].IsArray || args[0].IsObject) {
                return args[0];
            }
            throw new NeoError("attempt to unwrap a value that cannot be unwrapped");
        }

        [NativeValue("class")]
        public static NeoValue Class(NeoValue[] args) {
            // @TODO Array bounds check
            var clazz = args[0].CheckString().Value;
            return new FFITypeProxy(types[clazz]);
        }

        [NativeValue("new")]
        public static NeoValue New(NeoValue[] args) {
            // @TODO Array bounds check
            var clazz = args[0].CheckString().Value;
            var cls = types[clazz];
            if (cls == null) {
                throw new NeoError($"class '{clazz}' cannot be resolved");
            }

            var ctorArgs = new object[args.Length - 1];
            for (var i = 1; i < args.Length; i++) {
                ctorArgs[i - 1] = FromNeo(args[i]);
            }

            var ctorArgTypes = new Type[ctorArgs.Length];
            for (var i = 0; i < ctorArgTypes.Length; i++) {
                ctorArgTypes[i] = ctorArgs[i].GetType();
            }

            var ctor = cls.GetConstructor(ctorArgTypes);
            if (ctor == null) {
                throw new NeoError($"constructor not found for '{clazz}'");
            }

            var obj = ctor.Invoke(ctorArgs);
            return new FFITypeProxy(obj);
        }

        public static NeoValue ToNeo(object v) {
            if (v == null) {
                return NeoNil.NIL;
            } else if (v is Int32 i) {
                return NeoInt.ValueOf(i);
            } else if (v is Int64 l) {
                return NeoInt.ValueOf((int)l);
            } else if (v is Single f) {
                return NeoFloat.ValueOf(f);
            } else if (v is Double d) {
                return NeoFloat.ValueOf(d);
            } else if (v is Boolean b) {
                return NeoBool.ValueOf(b);
            } else if (v is String s) {
                return NeoString.ValueOf(s);
            } else if(v is object[] a) {
                // @TODO: is this where we want to do this? or do we just want an FFITypeProxy?
                var r = new NeoArray();
                foreach(object o in a) {
                    r.Insert(ToNeo(o));
                }
                return r;
            } else if(v is Dictionary<object, object> m) {
                throw new NotImplementedException();
            } else {
                return new FFITypeProxy(v);
            }
        }

        public static object FromNeo(NeoValue v) {
            if (v.IsNil) {
                return null;
            } else if (v.IsInt) {
                return v.CheckInt().Value;
            } else if (v.IsFloat) {
                return v.CheckFloat().Value;
            } else if (v.IsString) {
                return v.CheckString().Value;
            } else if (v.IsBool) {
                return v.CheckBool().Value;
            } else if (v.IsArray) {
                return v; // @TODO: convert to object[]?
            } else if (v.IsObject) {
                return v; // @TODO: convert to HashMap<object, object>?
            } else if (v is FFITypeProxy p) {
                return p.Object;
            } else {
                throw new NeoError(v.Type, -1);
            }
        }
    }

    internal sealed class FFITypeProxy : NeoValue {
        private readonly Dictionary<string, Func<NeoValue>> getters;
        private readonly Dictionary<string, Func<NeoValue>> staticGetters;
        private readonly Dictionary<string, Action<NeoValue>> setters;

        public FFITypeProxy(object obj) {
            Object = obj;

            getters = new Dictionary<string, Func<NeoValue>>();
            staticGetters = new Dictionary<string, Func<NeoValue>>();
            setters = new Dictionary<string, Action<NeoValue>>();

            Analyze();
        }

        public object Object { get; }

        private void Analyze() {
            Type type;
            if (Object is Type) {
                type = (Type)Object;
            } else {
                type = Object.GetType();
            }

            var propertyMethods = new HashSet<MethodInfo>();

            foreach (var prop in type.GetProperties()) {
                if (prop.GetMethod != null) {
                    if (prop.GetMethod.IsPublic) getters[prop.Name] = () => FFILib.ToNeo(prop.GetValue(Object));
                    propertyMethods.Add(prop.GetMethod);
                }

                if (prop.SetMethod != null) {
                    if (prop.SetMethod.IsPublic) setters[prop.Name] = (value) => prop.SetValue(Object, FFILib.FromNeo(value));
                    propertyMethods.Add(prop.SetMethod);
                }
            }

            foreach (var method in type.GetMethods()) {
                if (!method.IsPublic) continue;
                if (propertyMethods.Contains(method)) continue;

                if (method.IsStatic) {
                    if (staticGetters.ContainsKey(method.Name)) {
                        var proxy = (FFIStaticMethodProxy)staticGetters[method.Name]();
                        proxy.AddOverload(method);
                    } else {
                        var proxy = new FFIStaticMethodProxy(this, method);
                        staticGetters[method.Name] = () => proxy;
                    }
                } else {
                    if (getters.ContainsKey(method.Name)) {
                        var proxy = (FFIMethodProxy)getters[method.Name]();
                        proxy.AddOverload(method);
                    } else {
                        var proxy = new FFIMethodProxy(Object, method);
                        getters[method.Name] = () => proxy;
                    }
                }
            }

            foreach (var field in type.GetFields()) {
                if (!field.IsPublic) continue;

                getters[field.Name] = () => FFILib.ToNeo(field.GetValue(Object));
                setters[field.Name] = (value) => field.SetValue(Object, FFILib.FromNeo(value));
            }
        }

        public override NeoValue Get(NeoValue key) => this[key];

        public override void Set(NeoValue key, NeoValue value) => this[key] = value;

        public NeoValue this[NeoValue key] {
            get {
                var s = key.CheckString().Value;
                if (staticGetters.ContainsKey(s)) {
                    return staticGetters[s]();
                }
                if(getters.ContainsKey(s)) {
                    return getters[s]();
                }
                return NeoNil.NIL;
            }
            set {
                var s = key.CheckString().Value;
                if(setters.ContainsKey(s)) {
                    setters[s](value);
                } else {
                    throw new NeoError($"No such method '{key}'");
                }
            }
        }

        public override string Type => $"handle({Object.GetType()})";
    }

    internal sealed class FFIMethodProxy : NeoProcedure {
        private readonly object obj;
        private readonly List<MethodInfo> overloads;
        private readonly string name;

        public FFIMethodProxy(object obj, MethodInfo method) {
            this.obj = obj;
            overloads = new List<MethodInfo>() {
                method
            };
            name = method.Name;
        }

        internal void AddOverload(MethodInfo method) {
            overloads.Add(method);
        }

        public override NeoValue Call(NeoValue[] arguments) {
            var pargs = new List<NeoValue>();
            foreach (var arg in arguments) {
                if (arg is NeoSpreadValue va) {
                    for (var i = 0; i < va.Array.Count; i++) {
                        pargs.Add(va.Array[i]);
                    }
                } else {
                    pargs.Add(arg);
                }
            }

            var args = new object[pargs.Count];
            for (var i = 0; i < args.Length; i++) {
                args[i] = FFILib.FromNeo(pargs[i]);
            }

            MethodInfo method = null;
            foreach (var overload in overloads) {
                var parameters = overload.GetParameters();
                if (parameters.Length != args.Length) continue;

                var same = true;
                for (var i = 0; i < args.Length; i++) {
                    if (args[i] == null) {
                        continue;
                    }

                    if (!parameters[i].ParameterType.IsInstanceOfType(args[i])) {
                        same = false;
                        break;
                    }
                }
                if (!same) continue;

                method = overload;
                break;
            }

            if (method == null) {
                throw new NeoError($"FFILib method '{name}' not found");
            }

            if (method.ReturnType == typeof(void)) {
                method.Invoke(obj, args);
                return NeoNil.NIL;
            } else {
                return FFILib.ToNeo(method.Invoke(obj, args));
            }
        }

        public override string Name() => name;

        public override string ChunkName() => "<native code>";

        public override string Type => $"handle({name})";
    }

    internal sealed class FFIStaticMethodProxy : NeoProcedure {
        private readonly FFITypeProxy type;
        private readonly List<MethodInfo> overloads;
        private readonly string name;

        public FFIStaticMethodProxy(FFITypeProxy type, MethodInfo method) {
            this.type = type;
            overloads = new List<MethodInfo>() {
                method
            };
            name = method.Name;
        }

        internal void AddOverload(MethodInfo method) {
            overloads.Add(method);
        }

        public override NeoValue Call(NeoValue[] arguments) {
            var pargs = new List<NeoValue>();
            foreach (var arg in arguments) {
                if (arg is NeoSpreadValue va) {
                    for (var i = 0; i < va.Array.Count; i++) {
                        pargs.Add(va.Array[i]);
                    }
                } else {
                    pargs.Add(arg);
                }
            }

            var args = new object[pargs.Count];
            for (var i = 0; i < args.Length; i++) {
                args[i] = FFILib.FromNeo(pargs[i]);
            }

            MethodInfo method = null;
            foreach (var overload in overloads) {
                var parameters = overload.GetParameters();
                if (parameters.Length != args.Length) continue;

                var same = true;
                for (var i = 0; i < args.Length; i++) {
                    if (args[i] == null) {
                        continue;
                    }

                    if (parameters[i].ParameterType == typeof(double) && args[i] is int) {
                        args[i] = (double)(int)args[i];
                    }

                    if (!parameters[i].ParameterType.IsInstanceOfType(args[i])) {
                        same = false;
                        break;
                    }
                }
                if (!same) continue;

                method = overload;
                break;
            }

            if (method == null) {
                throw new NeoError($"FFILib method '{name}' not found");
            }

            if (method.ReturnType == typeof(void)) {
                method.Invoke(null, args);
                return NeoNil.NIL;
            } else {
                return FFILib.ToNeo(method.Invoke(null, args));
            }
        }

        public override string Name() => name;

        public override string ChunkName() => "<native code>";

        public override string Type => $"handle({name})";
    }
}