using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Triton.Utilities.Db
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// This class contains useful functions for use with database access.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class DbUtilities
	{


		public static string GetStringValue(
			IDataReader rs,
			string fieldName,
			string defaultValue)
		{
			return GetStringValue(rs, fieldName) ?? defaultValue;
		}

	
		public static string GetStringValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);

			return rs.IsDBNull(col) ? null : rs.GetString(col);
		}


		public static string GetGuidValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);

			return rs.IsDBNull(col) ? null : rs.GetGuid(col).ToString();
		}


		public static int GetIntValue(
			IDataReader rs,
			string fieldName,
			int defaultValue)
		{
			return GetIntValue(rs, fieldName) ?? defaultValue;
		}


		public static int? GetIntValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);

			// GetInt32 only works when the field has a data type of "int" in SQL Server.
			// If the field is a "smallint", an InvalidCastException will be thrown and GetIn16
			// will be used as a result.
			// Question: what if it is a "tinyint"? Or this method will only be used for "int" and "smallint"?
			try {
				return rs.IsDBNull(col) ? (int?)null : (int)rs.GetInt32(col);
			} catch (InvalidCastException) {
				return rs.IsDBNull(col) ? (int?)null : (int)rs.GetInt16(col);
			}
		}


		public static long GetLongValue(
			IDataReader rs,
			string fieldName,
			long defaultValue)
		{
			return GetLongValue(rs, fieldName) ?? defaultValue;
		}


		public static long? GetLongValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);
			long? val = null;

			if (!rs.IsDBNull(col)) {
				try {
					val = rs.GetInt32(col);
				} catch (InvalidCastException) {
					val = (long)rs.GetInt64(col);
				}
			}

			return val;
		}


		public static double GetFloatValue(
			IDataReader rs,
			string fieldName,
			double defaultValue)
		{
			return GetFloatValue(rs, fieldName) ?? defaultValue;
		}


		public static double? GetFloatValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);

			return rs.IsDBNull(col) ? (double?)null : System.Convert.ToDouble(rs.GetDecimal(col));
		}


		public static decimal GetDecimalValue(
			IDataReader rs,
			string fieldName,
			decimal defaultValue)
		{
			return GetDecimalValue(rs, fieldName) ?? defaultValue;
		}


		public static decimal? GetDecimalValue(
			IDataReader rs,
			string fieldName)
		{
			int col = rs.GetOrdinal(fieldName);

			return rs.IsDBNull(col) ? (decimal?)null : System.Convert.ToDecimal(rs.GetDecimal(col));
		}


		public static bool GetBooleanValue(
			IDataReader rs,
			string fieldName,
			bool defaultValue)
		{
			return GetBooleanValue(rs, fieldName) ?? defaultValue;
		}


		public static bool? GetBooleanValue(
			IDataReader rs,
			string fieldName)
		{
			bool? result = null;
			int col = rs.GetOrdinal(fieldName);

			if (!rs.IsDBNull(col)) {
				result = rs.GetBoolean(col);
			}

			return result;
		}


		public static DateTime GetDateValue(
			IDataReader rs,
			string fieldName,
			DateTime defaultValue)
		{
			return GetDateValue(rs, fieldName) ?? defaultValue;
		}


		public static DateTime? GetDateValue(
			IDataReader rs,
			string fieldName)
		{
			DateTime? result = null;
			int col = rs.GetOrdinal(fieldName);

			if (!rs.IsDBNull(col)) {
				result = rs.GetDateTime(col);
			}

			return result;
		}


		public static void BuildStringInsert(
			string value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (!string.IsNullOrEmpty(value)) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value;
				param.DbType = DbType.String;
				param.Size = value.Length;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildLongInsert(
			long? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Int64;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildFloatInsert(
			float? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildDecimalInsert(
			decimal? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildIntInsert(
			int? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Int32;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildDateInsert(
			DateTime value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{
			BuildDateInsert((DateTime?)value, fieldName, fieldsSql, valuesSql, stmt);
		}


		public static void BuildDateInsert(
			DateTime? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.DateTime;
				stmt.Parameters.Add(param);
			}
		}


		public static void BuildBooleanInsert(
			Boolean? value,
			string fieldName,
			StringBuilder fieldsSql,
			StringBuilder valuesSql,
			IDbCommand stmt)
		{

			if (value.HasValue) {
				if (fieldsSql.Length > 0) {
					fieldsSql.Append(", ");
					valuesSql.Append(", ");
				}

				string paramName = "@" + fieldName.Replace(".", "_");
				fieldsSql.Append(fieldName);
				valuesSql.Append(paramName);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Boolean;
				stmt.Parameters.Add(param);
			}
		}


		public static string BuildStringUpdate(
			string value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value == null) {
				value = "";
			}

			string paramName = "@" + fieldName.Replace(".", "_");
			int len = value.Length;
			if (len == 0) {
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value;
				param.Size = 1;
				param.DbType = DbType.String;
				stmt.Parameters.Add(param);
			} else {
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value;
				param.Size = len;
				param.DbType = DbType.String;
				stmt.Parameters.Add(param);
			}

			retVal = fieldName + "=" + paramName + conj;

			return retVal;
		}


		public static string BuildBooleanUpdate(
			bool? value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Boolean;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildLongUpdate(
			long? value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Int64;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildFloatUpdate(
			float? value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildDecimalUpdate(
			decimal? value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildIntUpdate(
			int? value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = value.Value;
				param.DbType = DbType.Int32;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildDateUpdate(
			DateTime date,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			return BuildDateUpdate((DateTime?)date, fieldName, stmt, conj);
		}


		public static string BuildDateUpdate(
			DateTime? date,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (date != null) {
				string paramName = "@" + fieldName.Replace(".", "_");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = date;
				param.DbType = DbType.DateTime;
				stmt.Parameters.Add(param);
				retVal = fieldName + "=" + paramName + conj;
			}

			return retVal;
		}


		public static string BuildStringCondition(
			string value,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			string retVal = "";

			if (value != null) {
				//  if the value contains one or more "," assume it is a
				//  list of multiple values
				if (value.IndexOf(",") >= 0) {
					//  if a multiple value list contains "%", throw an error
					if (value.IndexOf("%") >= 0) {
						//throw new Exception("'%' can not be used with multiple " + fieldName + " values.");
					} else {
						//  build the where clause for multiple values search
						string quotedValues = value.Replace("'", "''");
						quotedValues = "'" + quotedValues.Replace(",", "','") + "'";
						retVal = " " + fieldName + " in (" + quotedValues + ")" + conj;
					}

					//  no "," in value, so it is a single userName
				} else {
					string paramName = "@" + fieldName.Replace(".", "_");
					if (value.IndexOf("%") >= 0) {
						retVal = " " + fieldName + " like " + paramName + " " + conj;
					} else {
						retVal = " " + fieldName + " = " + paramName + " " + conj;
					}
					//				retVal = " " + fieldName + " = " + paramName + " " + conj;
					if (value.Length == 0) {
						IDbDataParameter param = stmt.CreateParameter();
						param.ParameterName = paramName;
						param.Value = value;
						param.Size = 1;
						param.DbType = DbType.String;
						stmt.Parameters.Add(param);
					} else {
						IDbDataParameter param = stmt.CreateParameter();
						param.ParameterName = paramName;
						param.Value = value;
						param.Size = value.Length;
						param.DbType = DbType.String;
						stmt.Parameters.Add(param);
					}
				}
			}

			return retVal;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="fieldName"></param>
		/// <param name="stmt"></param>
		/// <param name="conj"></param>
		/// <returns></returns>
		public static string BuildDateCondition(
			DateTime start,
			DateTime end,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			return BuildDateCondition2(start, end, fieldName, stmt, conj);
		}


		/// <summary>
		/// Builds a SQL condition for use in a "where" clause for the specified
		/// date range on the specified field.  Either <c>start</c> or <c>end</c>
		/// may be null.
		/// </summary>
		/// <param name="start">The starting date of the date range to build the
		///			condition for.  If <c>null</c> only the ending date is used.</param>
		/// <param name="end">The ending date of the date range to build the
		///			condition for.  If <c>null</c> only the starting date is used.</param>
		/// <param name="fieldName">The name of the field the condition is to be
		///			built for.</param>
		/// <param name="stmt">The SqlCommand the condition is to be used with.</param>
		/// <param name="conj">The conjuntion, "and" or "or", to include in the condition.</param>
		/// <returns>A <c>string</c> containing the SQL condition for the given
		///			criteria.</returns>
		public static string BuildDateCondition2(
			DateTime? start,
			DateTime? end,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();
			string paramName = "@" + fieldName.Replace(".", "_");
			string internalConj = conj;

					//  if we have both a start and end date, the conjuction joining 
					//  the 2 date conditions must be "and" or the resulting condition
					//  is pretty much meaningless
			if ((start != null) && (end != null)) {
				internalConj = "and";
			}

					//  start date
			if (start != null) {
				retVal.Append(" " + fieldName + " >= " + paramName + "_start " + internalConj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName + "_start";
				param.Value = start;
				param.DbType = DbType.DateTime;
				stmt.Parameters.Add(param);
			}
					//  end date
			if (end != null) {
				retVal.Append(" " + fieldName + " <= " + paramName + "_end ");
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName + "_end";
				param.Value = end;
				param.DbType = DbType.DateTime;
				stmt.Parameters.Add(param);
				//retVal.Append(conj);
			}
			return retVal.ToString();
		}


		public static string BuildBooleanCondition(
			bool? boolVal,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();

			if (boolVal.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = boolVal.HasValue;
				param.DbType = DbType.Boolean;
				stmt.Parameters.Add(param);
			}

			return retVal.ToString();
		}


		public static string BuildFloatCondition(
			float? floatVal,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();

			if (floatVal.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = floatVal.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
			}

			return retVal.ToString();
		}


		public static string BuildDecimalCondition(
			decimal? decimalVal,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();

			if (decimalVal.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = decimalVal.Value;
				param.DbType = DbType.Decimal;
				stmt.Parameters.Add(param);
			}

			return retVal.ToString();
		}


		public static string BuildLongCondition(
			long? longVal,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();

			if (longVal.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = longVal;
				param.DbType = DbType.Int64;
				stmt.Parameters.Add(param);
			}

			return retVal.ToString();
		}


		public static string BuildIntCondition(
			int? intVal,
			string fieldName,
			IDbCommand stmt,
			string conj)
		{
			StringBuilder retVal = new StringBuilder();

			if (intVal.HasValue) {
				string paramName = "@" + fieldName.Replace(".", "_");
				retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
				IDbDataParameter param = stmt.CreateParameter();
				param.ParameterName = paramName;
				param.Value = intVal.Value;
				param.DbType = DbType.Int64;
				stmt.Parameters.Add(param);
			}

			return retVal.ToString();
		}


		//public static string BuildLongCondition(
		//    string longVals,
		//    string fieldName,
		//    IDbCommand stmt,
		//    string conj)
		//{
		//    StringBuilder retVal = new StringBuilder();
		//    string paramName = "@" + fieldName.Replace(".", "_");

		//    if (longVals != null) {
		//        retVal.Append(" " + fieldName + " in (" + paramName + ") " + conj);
		//        IDbDataParameter param = stmt.CreateParameter();
		//        param.ParameterName = paramName;
		//        param.Value = longVals;
		//        param.DbType = DbType.String;
		//        stmt.Parameters.Add(param);
		//    }

		//    return retVal.ToString();
		//}


		//public static string BuildIntCondition(
		//    string intVals,
		//    string fieldName,
		//    IDbCommand stmt,
		//    string conj)
		//{
		//    StringBuilder retVal = new StringBuilder();
		//    //		string			paramName = "@" + fieldName.Replace(".", "_");

		//    if (intVals != null) {
		//        string paramName = "@INTVALUES";
		//        retVal.Append(" " + fieldName + " in (" + paramName + ") " + conj);
		//        IDbDataParameter param = stmt.CreateParameter();
		//        param.ParameterName = paramName;
		//        param.Value = intVals;
		//        param.DbType = DbType.String;
		//        stmt.Parameters.Add(param);
		//    }

		//    return retVal.ToString();
		//}


		//public static string BuildIntCondition(
		//    int intVal,
		//    string fieldName,
		//    IDbCommand stmt,
		//    string conj)
		//{
		//    StringBuilder retVal = new StringBuilder();
		//    string paramName = "@" + fieldName.Replace(".", "_");

		//    if (intVal != Constants.NULL_INT) {
		//        retVal.Append(" " + fieldName + " = " + paramName + " " + conj);
		//        IDbDataParameter param = stmt.CreateParameter();
		//        param.ParameterName = paramName;
		//        param.Value = intVal;
		//        param.DbType = DbType.Int32;
		//        stmt.Parameters.Add(param);
		//    }

		//    return retVal.ToString();
		//}


		/// <summary>
		/// Closes the given database access objects.
		/// </summary>
		/// <param name="conn">The <c>IDbConnection</c> to close (null if none).</param>
		/// <param name="stmt">The <c>IDbCommand</c> to close (null if none).</param>
		/// <param name="dataReader">The <c>IDataReader</c> to close (null if none).</param>
		public static void Close(
			IDbConnection conn,
			IDbCommand stmt,
			IDataReader dataReader)
		{

			try {
				if (dataReader != null) {
					dataReader.Close();
				}
			} catch (Exception) { }

			//		try {
			//			if (stmt != null) {
			//				stmt.Close();
			//			}
			//		} catch (Exception e) {}

			try {
				if (conn != null) {
					conn.Close();
				}
			} catch (Exception) { }
		}

	}
}