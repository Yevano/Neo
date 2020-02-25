using System.Linq;

namespace Neo.Utils {
	public static class Arrays {
		public static T[] Concat<T>(params T[][] list) {
	        var result = new T[list.Sum(a => a.Length)];
	        int offset = 0;
	        for (int x = 0; x < list.Length; x++) {
	            list[x].CopyTo(result, offset);
	            offset += list[x].Length;
	        }
	        return result;
	    }
	}
}