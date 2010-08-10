using System;

namespace Triton.Utilities
{

	#region History

	// History:

	#endregion

	///<summary>
	///</summary>
	/// <author>Garun Vagidov</author>
	public static class ConvertUtilities
	{
		public static float? GetFloatValue(string requestValue)
		{
			float converted;
			float? returned = null;

			if (float.TryParse(requestValue, out converted)) {
				returned = converted;
			}

			return returned;
		}


		public static DateTime? GetDateTimeValue(string requestValue)
		{
			DateTime converted;
			DateTime? returned = null;

			if (DateTime.TryParse(requestValue, out converted)) {
				returned = converted;
			}

			return returned;
		}


		public static int? GetIntValue(string requestValue)
		{
			int converted;
			int? returned = null;

			if (int.TryParse(requestValue, out converted)) {
				returned = converted;
			}

			return returned;
		}


		public static bool GetBooleanValue(string requestValue)
		{
			bool converted;
			bool returned = false;

			if (bool.TryParse(requestValue, out converted)) {
				returned = converted;
			}

			return returned;
		}


		public static string GetStringValue(string value)
		{
			string retValue = null;
			if (!string.IsNullOrEmpty(value)) {
				retValue = value;
			}
			return retValue;
		}
	}
}