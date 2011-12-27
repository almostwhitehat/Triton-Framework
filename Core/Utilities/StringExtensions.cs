using System;
using System.Collections.Generic;
using Triton.Utilities.Reflection;

namespace Triton.Utilities
{
	#region History

	// History:
	//  11/23/10 - SD -	Added ToCamel extension method.

	#endregion

	///<summary>
	/// Helper string extensions.
	///</summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Converts a string containing a comma-delimited list of integer
		/// values to an array of <c>int</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <returns>Array of <c>int</c></returns>
		public static int[] ToIntArray(
			this string str)
		{
			return str.ToIntArray(null);
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of integer
		/// values to an array of <c>int</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <param name="treatAsEmpty">Character combination to ignore.</param>
		/// <returns>Array of <c>int</c></returns>
		public static int[] ToIntArray(
			this string str,
			string treatAsEmpty)
		{
			List<int> intVals = new List<int>();

			if (!string.IsNullOrEmpty(str)) {
				foreach (string val in str.Split(',')) {
					if (val != treatAsEmpty) {
						intVals.Add(int.Parse(val));
					}
				}
			}

			return intVals.ToArray();
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of integer
		/// values to an array of <c>long</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <returns>Array of <c>long</c></returns>
		public static long[] ToLongArray(
			this string str)
		{
			return str.ToLongArray(null);
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of integer
		/// values to an array of <c>long</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <param name="treatAsEmpty">Character combination to ignore.</param>
		/// <returns>Array of <c>long</c></returns>
		public static long[] ToLongArray(
			this string str,
			string treatAsEmpty)
		{
			List<long> longVals = new List<long>();

			if (!string.IsNullOrEmpty(str)) {
				foreach (string val in str.Split(',')) {
					if (val != treatAsEmpty) {
						longVals.Add(long.Parse(val));
					}
				}
			}

			return longVals.ToArray();
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of string
		/// values to an array of <c>string</c>.
		/// </summary>
		/// <param name="str">string to convert.</param>
		/// <returns>Array of <c>string</c>.</returns>
		public static string[] ToStringArray(
			this string str)
		{
			return str.ToStringArray(null);
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of string
		/// values to an array of <c>string</c>.
		/// </summary>
		/// <param name="str">string to convert.</param>
		/// <param name="treatAsEmpty">Character combination to ignore.</param>
		/// <returns>Array of <c>string</c>.</returns>
		public static string[] ToStringArray(
			this string str,
			string treatAsEmpty)
		{
			List<string> stringVals = new List<string>();

			if (!string.IsNullOrEmpty(str)) {
				foreach (string val in str.Split(',')) {
					if (val != treatAsEmpty) {
						stringVals.Add(val);
					}
				}
			}

			return stringVals.ToArray();
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of Guid
		/// values to an array of <c>Guid</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <returns>Array of <c>Guid</c></returns>
		public static Guid[] ToGuidArray(
			this string str)
		{
			return str.ToGuidArray(null);
		}


		/// <summary>
		/// Converts a string containing a comma-delimited list of Guid
		/// values to an array of <c>Guid</c>.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <param name="treatAsEmpty">Character combination to ignore.</param>
		/// <returns>Array of <c>Guid</c></returns>
		public static Guid[] ToGuidArray(
			this string str,
			string treatAsEmpty)
		{
			List<Guid> vals = new List<Guid>();

			if (!string.IsNullOrEmpty(str)) {
				foreach (string val in str.Split(',')) {
					if (val != treatAsEmpty) {
						vals.Add(new Guid(val));
					}
				}
			}

			return vals.ToArray();
		}


		/// <summary>
		/// Extension method to format a string as camel case -- with the first letter
		/// of each "word" upper case and the remaining letters lower case.
		/// </summary>
		/// <param name="inputStr">The string to convert to camel case.</param>
		/// <param name="delimiter">The delimiter to define the words in the string.</param>
		/// <returns>The given string converted to camel case.</returns>
		public static string ToCamel(
			this string inputStr,
			char delimiter)
		{
					//  separate the string into "words" based on the given delimiter
			string[] words = inputStr.ToLower().Split(delimiter);

					//  capitalize each word
			for (int k = words.Length - 1; k >= 0; k--) {
				words[k] = StringUtilities.Capitalize(words[k]);
			}

					//  put the capitalized words back together into a single string
			return string.Join(delimiter.ToString(), words);
		}

		/// <summary>
		/// This method will take the <b>String</b> provided and evaluate the Property/Const value.
		/// If no value can be discerned then the origional value is returned
		/// for instance "{Triton.Location.Support.Request.ParameterNames+PersistedAddress+Field.LINE1}"
		/// this will create a instance of the class 
		/// Triton.Location.Support.Request.ParameterNames.PersistedAddress.Field
		/// and evaluate the value for the constant field "LINE1"
		/// the + operators are needed when the class definition is internal to the parent class.
		/// so the namespace is "Triton.Location.Support.Request" and the class Parameternames has 
		/// an internal class PersistedAddress which has an internal class Field.
		/// Note: this is only usefull for constants or static Properties.
		/// </summary>
		/// <param name="request">MvcRequest</param>
		/// <param name="paramName">fully qualified class name</param>
		/// <returns>string representation</returns>
		public static string EvaluatePropertyValue(this String str)
		{
			object result = null;
			result = ReflectionUtilities.Evaluate(str.TrimStart('{', '[').TrimEnd(']', '}'));

			if (str.StartsWith("{[")) {
				result = string.Format("[{0}]", result);
			}

			return result != null ? result.ToString() : str;
		}
	}
}