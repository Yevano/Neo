using System;

namespace Neo.Utils {
    public static class TypeSwitch {
        public class CaseInfo {
            public bool IsDefault { get; set; }
            public Type Target { get; set; }
            public Action Action { get; set; }
        }

        public static void Do<T>(params CaseInfo[] cases) {
            var type = typeof(T);
            foreach (var entry in cases) {
                if (entry.IsDefault || entry.Target.IsAssignableFrom(type)) {
                    entry.Action();
                    break;
                }
            }
        }

        public static CaseInfo Case<T>(Action action) {
            return new CaseInfo() {
                Action = action,
                Target = typeof(T)
            };
        }

        public static CaseInfo Default(Action action) {
            return new CaseInfo() {
                Action = action,
                IsDefault = true
            };
        }
    }
}