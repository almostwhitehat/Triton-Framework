using System;
using System.Data.SqlClient;

namespace Triton.Utilities.Db
{
	public static class DbExtensions
	{
		/// <summary>
		/// Gets the command text and lists all the parameters and its values.
		/// </summary>
		/// <param name="command">The command to get the debug info.</param>
		/// <returns>string representation of the command contents.</returns>
		public static string GetDebugInfo(this SqlCommand command)
		{
			string retValue = Environment.NewLine + command.CommandText;

			if (command.Parameters.Count > 0) {
				retValue += Environment.NewLine;
				foreach (var parameter in command.Parameters) {
					SqlParameter param = (SqlParameter)parameter;
					retValue += param.ParameterName + " : " + param.Value + ";  ";
				}
			}

			return retValue;
		}
	}
}