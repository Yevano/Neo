using System;
using System.Reflection;

namespace Neo.Utils {
	public static class Extensions {
		public static T FindCustomAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute {
            var attribs = memberInfo.GetCustomAttributes(inherit);
            foreach (var attrib in attribs) {
                if (attrib.GetType() == typeof(T)) {
                    return (T)attrib;
                }
            }
            return null;
        }
	}
}