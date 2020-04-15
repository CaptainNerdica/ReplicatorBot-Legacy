using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ReplicatorBot
{
	public static class Extensions
	{
		public static bool Contains(this string s, IEnumerable<string> substrings)
		{
			foreach(string sub in substrings)
				if (s.Contains(sub))
					return true;
			return false;
		}
    }
}
