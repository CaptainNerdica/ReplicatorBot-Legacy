using System.Collections.Generic;

namespace ReplicatorBot
{
	public static class Extensions
	{
		public static bool Contains(this string s, IEnumerable<string> substrings)
		{
			foreach (string sub in substrings)
				if (s.Contains(sub))
					return true;
			return false;
		}
	}
}
