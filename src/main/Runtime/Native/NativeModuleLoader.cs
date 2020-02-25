using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Neo.Utils;
using Neo.Runtime;
using Neo.Runtime.Internal;

namespace Neo.Runtime.Native {
	public static class NativeModuleLoader {
		private static readonly HashSet<string> nativeModulesFound = new HashSet<string>();
		private static readonly Dictionary<string, Type> nativeModulesByName = new Dictionary<string, Type>();

		private static int NativeProcedureCount = 0;

		public static void LoadNativeModules(Assembly assembly) {
			foreach(var type in assembly.GetTypes()) {
				var nativeModule = type.FindCustomAttribute<NativeModule>();
				if(nativeModule == null) continue;

				nativeModulesFound.Add(nativeModule.Name);
				nativeModulesByName[nativeModule.Name] = type;
			}
		}

		public static NativeModuleInfo LoadNativeModule(string name) {
			if(!nativeModulesFound.Contains(name)) return null;
			return LoadNativeModule(nativeModulesByName[name]);
		}

		private static NativeModuleInfo LoadNativeModule(Type type) {
			var nativeModule = type.FindCustomAttribute<NativeModule>();
			if(nativeModule == null) return null;

			if(!(type.IsAbstract && type.IsSealed)) {
				throw new Exception("Native Modules must be static!");				
			}

			var module = new NativeModuleInfo(nativeModule.Name, nativeModule.HasNeoSource);

			foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)) {
				var nativeValue = method.FindCustomAttribute<NativeValue>();
				if(nativeValue == null) continue;

				var procedure = GenerateProcedure(nativeValue.Name, method);
				module.Values.Add(new NativeValueInfo(nativeValue.Name, procedure));
			}

			return module;
		}

		private static NeoProcedure GenerateProcedure(string name, MethodInfo method) {
			var dm = new DynamicMethod($"native_procedure_{name}_{NativeProcedureCount++}", typeof(NeoValue), new [] { typeof(NeoValue[]) }, typeof(VM).Module);
			var il = dm.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Ret);

			return new NativeNeoProcedure((NeoProcedureFunc) dm.CreateDelegate(typeof(NeoProcedureFunc)));
		}
	}
}