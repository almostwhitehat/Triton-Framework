﻿using System;
using System.Collections.Generic;

namespace Triton.Utilities
{
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
	}
}