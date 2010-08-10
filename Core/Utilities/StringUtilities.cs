using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Triton.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// This class contains utility functions that are useful for strings.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class StringUtilities
	{
		private StringUtilities() {}


		/// <summary>
		/// This method does case-insensitive replaces.
		/// </summary>
		/// <param name="input"><c>string</c> The string to modify</param>
		/// <param name="pattern"><c>string</c> The pattern to be replaced</param>
		/// <param name="replacement"><c>string</c> The replacement string</param>
		/// <returns><c>string</c> The modified string</returns>
		public static string ReplaceCaseInsensitive(
			string input,
			string pattern,
			string replacement)
		{
			return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
		}


		/// <summary>
		/// This method removes html tags from the input string.
		/// </summary>
		/// <param name="input"><c>string</c> The string to modify</param>
		/// <returns><c>string</c> The modified string</returns>
		/// Pattern 1: <(.|\n)+?> (Doesn't work for an expression like this: "5 < 8 and 9 > 3")
		/// Pattern 2: </?\w+((\s+\w+(\s*=\s*(?:"(.|\n)*?"|'(.|\n)*?'|[^'">\s]+))?)+\s*|\s*)/?>
		public static string RemoveHtmlTags(
			string input)
		{
			string pattern = @"</?\w+((\s+\w+(\s*=\s*(?:" + "\"" + @"(.|\n)*?" + "\"" + @"|'(.|\n)*?'|[^'" + "\""
			                 + @">\s]+))?)+\s*|\s*)/?>";

			// Remove html tags from the input using regular expressions
			return Regex.Replace(input, pattern, "");
		}


		/// <summary>
		/// Capitalizes the first letter of the given string.
		/// </summary>
		/// <param name="str">The string to capitalize.</param>
		/// <returns>The given string with the first letter capitalized.</returns>
		public static string Capitalize(
			string str)
		{
			string retStr = null;

			if (str != null) {
				retStr = str[0].ToString().ToUpper() + str.Substring(1);
			}

			return retStr;
		}


		/// <summary>
		/// This method split string by a string delimiter
		/// </summary>
		/// <param name="input"><c>string</c> The string to do split on</param>
		/// <param name="delimiter"><c>string</c> The string delimiter to do split upon</param>
		/// <returns><c>string array</c> The parses after split</returns>
		public static string[] Split(
			string input,
			string delimiter)
		{
			//the maximum number of parses will get should be 
			//of the length of the original input
			string[] result = new string[input.Length];
			int i = 0;
			int j = 0;

			if (delimiter.Length > 0) {
				while (input.LastIndexOf(delimiter) >= 0) {
					int index = input.LastIndexOf(delimiter);
					result[i] = input.Substring(index + delimiter.Length);
					input = input.Substring(0, index);
					i++;
				}
				result[i] = input;
			} else {
				result[i] = input;
			}

			string[] output = new string[i + 1];

			while (i >= 0) {
				output[j] = result[i];
				i--;
				j++;
			}

			return output;
		}


		/// <summary>
		/// Builds a name/value pair collection from a string of name/value pairs.
		/// </summary>
		/// <remarks>
		/// The input string to create the collection from is of the form:
		/// name1=value1[delim]name2=value2[delim]name3=value3...
		/// </remarks>
		/// <param name="theString">The string to generate the name/value pair collection from.</param>
		/// <param name="delim">The delimiter separating the name/value pairs in the input string.</param>
		/// <returns>A <b>NameValueCollection</b> containing the name/value pairs defined in the
		///			input string.</returns>
		public static NameValueCollection StringToCollection(
			string theString,
			char delim)
		{
			NameValueCollection collection = new NameValueCollection();

			if (theString != null) {
				string[] paramList = theString.Split(delim);

				foreach (string param in paramList) {
					int pos = param.IndexOf("=");
					if (pos >= 0) {
						collection[param.Substring(0, pos)] = param.Substring(pos + 1);
					} else {
						collection[param] = null;
					}
				}
			}

			return collection;
		}
	}
}