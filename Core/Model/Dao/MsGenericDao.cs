using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using Common.Logging;
using Triton.Utilities;
using Triton.Utilities.Db;

namespace Triton.Model.Dao
{

	#region History

	// History:
	//   6/5/2009	KP	Changed the logging to Common.Logging.
	//  9/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	//  5/10/2011	SD	Fix in LoadFieldInfo for reading maxLen and prec -- 
	//					DbUtilities.GetIntValue returns a nullable so doing a .Value directly
	//					threw an exception when the value was null.

	#endregion

	/// <summary>
	/// <c>MsGenericDao</c> implements the <c>IGenericDao</c> for a MS SQL Server database.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MsGenericDao : MsDaoBase, IGenericDao
	{
		// TODO: Should the name be the name of IGenericDao interface or that of the concrete class?
		private readonly Hashtable aliases = new Hashtable();
		private readonly Random randomGenerator = new Random();
		private SqlConnection conn;
		private Hashtable fieldInfo;
		private string name = "IGenericDao";
		private string tableName;


		public MsGenericDao(
			string connType)
		{
			base.connectionType = connType;
		}


		public override string Name
		{
			get { return this.name; }
		}

		#region IGenericDao Members

		public bool SetIdentity { set; get; }


		/// <summary>
		/// Retrieves data by passing the loaded xml string to the overloaded method.
		/// </summary>
		/// <param name="xml">string of xml formatted data.</param>
		/// <returns>DataSet</returns>
		public DataSet GetData(
			string xml)
		{
			DataSet data = null;

			try {
				// Load xml in an xml document and call the overload method.
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
				data = this.GetData(xmlDoc.DocumentElement);
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao : GetData"), ex);
			}

			return data;
		}


		/// <summary>
		/// Retrieves data based on the xml document passed in.
		/// </summary>
		/// <param name="doc">XmlDocument representing the query</param>
		/// <returns></returns>
		public DataSet GetData(
			XmlNode doc)
		{
			DataSet data = null;

			try {
				XmlNode selectNode = doc;
				XmlNodeList tableNodes = selectNode.SelectNodes("Tables/Table");
				StringBuilder sql = new StringBuilder();
				StringBuilder fields = new StringBuilder();
				StringBuilder joins = new StringBuilder();
				StringBuilder where = new StringBuilder();
				StringBuilder order = new StringBuilder();
				SqlCommand cmd = new SqlCommand();
				bool addDistinctClause = selectNode.SelectSingleNode("Fields").Attributes.GetNamedItem("Distinct") != null;
				SqlConnection conn = this.GetConnection();
				conn.Open();
				cmd.Connection = conn;

				// Add joins
				for (int i = 0; i < tableNodes.Count; i++) {
					XmlNode tableNode = tableNodes[i];

					this.tableName = tableNode.Attributes["name"].Value;
					this.aliases.Add(this.tableName, string.Format("alias{0}", i));

					// Add the FROM/JOIN clauses
					if (i == 0) {
						joins.Append(string.Format(" FROM {0} {1} ", this.tableName, this.aliases[this.tableName]));
					} else {
						// Clone this b/c we'll be updating values for temporary processing
						XmlNode joinNode = tableNode.SelectSingleNode("Join").Clone();

						// Add the table name to each field as it is only implicitly defined in the config
						foreach (XmlNode field in joinNode.SelectNodes("Operator//Field")) {
							field.Attributes["name"].Value = string.Format("{0}.{1}", this.tableName, field.Attributes["name"].Value);
						}

						joins.Append(string.Format(" {0} {1} {2} ON ",
												   joinNode.Attributes["type"].Value,
												   this.tableName,
												   this.aliases[this.tableName]));
						joins.Append(this.BuildConditions(joinNode, cmd, null));
					}
				}

				// Add each field
				foreach (XmlNode field in selectNode.SelectNodes("Fields/Field")) {
					fields.Append(string.Format("{0}, ", this.FormatFieldName(field.Attributes["name"].Value)));
				}

				// Remove the extra comma
				fields.Remove(fields.Length - 2, 2);

				// Add where conditions
				if (selectNode.SelectSingleNode("Where") != null && selectNode.SelectSingleNode("Where").ChildNodes.Count > 0) {
					where.Append(" WHERE ");
					where.Append(this.BuildConditions(selectNode.SelectSingleNode("Where"), cmd, null));
				}

				// Add the order by
				if (selectNode.SelectSingleNode("OrderBy") != null && selectNode.SelectSingleNode("OrderBy").ChildNodes.Count > 0) {
					order.Append(" ORDER BY ");

					// Loop all field for ordering
					foreach (XmlNode orderField in selectNode.SelectSingleNode("OrderBy").ChildNodes) {
						order.Append(string.Format("{0} {1}, ",
													this.FormatOrderBy(orderField),
												   orderField.Attributes["direction"].Value));
					}

					// Remove the extra comma
					order.Remove(order.Length - 2, 2);
				}

				// Formulate the sql
				sql.Append("SELECT ");
				if (addDistinctClause) sql.Append("DISTINCT ");
				sql.Append(fields);
				sql.Append(joins);
				sql.Append(where);
				sql.Append(order);
				sql.Append(";");

				cmd.CommandType = CommandType.Text;
				cmd.CommandText = sql.ToString();
				data = this.GetData(cmd, sql.ToString(), this.tableName);
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao.GetData()"), e);
			}

			return data;
		}


		public DataSet GetData(
			string tableName,
			string[] fields)
		{
			return this.GetData(tableName, fields, null);
		}


		/// <summary>
		/// This is just a temporary solution to meet a project deadline. 
		/// A more standard way of building queries will be implemented in the future.
		/// </summary>
		/// <param name="tableName"><c>string</c> - A table name (e.g. product)</param>
		/// <param name="fields"><c>string[]</c> - A list of table fields to be returned (e.g. product_id)</param>
		/// <param name="conditions"><c>conditions</c> - A list of conditions (e.g. product_number = 'C2119')</param>
		/// <returns></returns>
		public DataSet GetData(
			string tableName,
			string[] fields,
			string[] conditions)
		{
			// Build sql statement
			string sql = "";

			foreach (string s in fields) {
				sql += "[" + s + "], ";
			}

// TODO: this could potentially be vulnerable to sql injection!
			sql = "select " + sql.Substring(0, sql.Length - 2) +
				  " from " + tableName + " (nolock)";

			if (conditions != null) {
				bool isFirst = true;

				foreach (string cond in conditions) {
					if (isFirst) {
						sql += " where " + cond;
						isFirst = false;
					} else {
						sql += " and " + cond;
					}
				}
			}

			// Build db connection
			SqlConnection conn = this.GetConnection();
			conn.Open();

			// Build command object
			SqlCommand command = new SqlCommand(sql, conn);
			command.CommandType = CommandType.Text;

			return this.GetData(command, sql, tableName);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <pre>
		/// <Insert table="">
		///		<Row>
		///			<Fields>
		///				<Field name=""></Field>
		///				...
		///				<Null>
		///					<Field name="" />
		///				</Null>
		///			</Fields>
		///		</Row>
		///		...
		/// </Insert>
		/// 
		/// <Update table="">
		///		<Fields>
		///			<Field name=""></Field>
		///			...
		///			<Null>
		///				<Field name="" />
		///			</Null>
		///		</Fields>
		///		<Where>
		///			possible conditions take the form of:
		///			<Operator name="{operatorName}">
		///				<Field name=""></Field>
		///			</Operator>
		///			where the operatorName attribute is one of the following values:
		///					ea - equal
		///					lt - less than
		///					gt - greater than
		///					nea - not equal to
		///					lea - less than or equal to
		///					gea - greater than or equal to
		///					and - and
		///					or - or
		///					not - not
		///					bw - between (not supported currently)
		///					
		///					* and, or, and not operators can be nested
		///					* the Operator tag can also be replaced by And, Or, and Not tag
		///			eg.1
		///			<Operator name="and">
		///				<Operator name="ea">
		///					<Field name="f1">value1</Field>
		///				</Operator>
		///				<Operator name="gt"
		///					<Filed name="f2">value2</Filed>
		///				</Operator>	
		///				<Operator>
		///					<Null>
		///						<Filed name="f3"/>
		///					</Null>
		///				</Operator>
		///			</Operator>
		///			eg.2 grouping: good for <And></And>, <Or></Or>, <Not></Not>
		///			<And>
		///				<Operator name="ea">
		///					<Field name="f1">value1</Field>
		///				</Operator>
		///				<Operator name="gt"
		///					<Filed name="f2">value2</Filed>
		///				</Operator>	
		///				<Operator>
		///					<Null>
		///						<Filed name="f3"/>
		///					</Null>
		///				</Operator>
		///			</And>
		///		</Where>
		/// </Update>
		/// </pre>
		/// </remarks>
		public long SaveData(
			string xml)
		{
			long retVal = 0;

			try {
				// Load xml in an xml document and call the overload method.
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
				retVal = this.SaveData(xmlDoc);
			} catch (Exception ex) {
				// Log errors.
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao : SaveData"), ex);
			}

			return retVal;
		}


		public long SaveData(
			XmlDocument xmlDoc)
		{
			SqlConnection conn = null;
			SqlCommand cmd = null;
			StringBuilder sql = new StringBuilder();
			StringBuilder fields = new StringBuilder();
			StringBuilder values = new StringBuilder();
			string cmdName = null;
			long retVal = 0;

			try {
				conn = this.GetConnection();
				conn.Open();

				XmlNode cmdNode = xmlDoc.DocumentElement;

				// Set table name
				this.tableName = cmdNode.Attributes["table"].Value;
				this.aliases.Add(this.tableName, string.Format("{0}", "0"));
				cmdName = cmdNode.Name.ToLower();

				// If this is an insert request ...
				if (cmdName == "insert") {
					cmd = new SqlCommand();
					cmd.Connection = conn;

					XmlNodeList rowNodes = cmdNode.SelectNodes("Row");

					// If set indentity is true, turn on the set indentity_insert
					if (this.SetIdentity) {
						sql.Append(string.Format("if IDENT_CURRENT('{0}') IS NOT NULL\nBEGIN\nset identity_insert {0} on;", this.tableName));
					}

					// For each row ...
					foreach (XmlNode row in rowNodes) {
						sql.Append(string.Format("insert into {0} ", this.tableName));
						fields.Append("(");
						values.Append("values (");

//					XmlNode fieldsNode = row.SelectSingleNode("//Fields");
						Set fieldSet = this.GetFields(row.SelectSingleNode("//Fields"), cmd);

						// For each field returned: append field name and value
						foreach (string field in fieldSet) {
							fields.Append(string.Format("{0}, ", field));
							values.Append(string.Format("@{0}, ", field));
//						cmd.Parameters["@" + field].Value = 
//  !! need to add the param here
						}

						// Remove the extra comma and space
						fields.Remove(fields.Length - 2, 2);
						fields.Append(") ");
						values.Remove(values.Length - 2, 2);
						values.Append(") ");

						// Append the fields and values clauses
						sql.Append(fields + values.ToString() + ";select IDENT_CURRENT('" + this.tableName + "')");

						// Turn off the set indentity_insert
						if (this.SetIdentity) {
							sql.Append(string.Format("set identity_insert {0} off\nEND ", this.tableName));
						}

						// Set commandtext and execute command
						cmd.CommandText = sql.ToString();
						object ident = cmd.ExecuteScalar();

						if (ident is Decimal) {
							retVal = (long) ((Decimal) ident);
						}
					}
				} else if (cmdName == "update") {
					StringBuilder update = new StringBuilder();
					StringBuilder where = new StringBuilder();
					Set fieldSet = null;

					cmd = new SqlCommand();
					cmd.Connection = conn;
					fieldSet = this.GetFields(cmdNode.SelectSingleNode("Fields"), cmd);
					update.Append("SET ");

					// For each field returned: append field name and value
					foreach (string field in fieldSet) {
						update.Append(string.Format("{0} = @{0}, ", field));
					}

					// Remove the extra comma
					update.Remove(update.Length - 2, 2);

					// Add where conditions
					if (cmdNode.SelectSingleNode("Where").ChildNodes.Count > 0) {
						where.Append(" WHERE ");
						where.Append(this.BuildConditions(cmdNode.SelectSingleNode("Where"), cmd, null));
					}

					// Formulate the sql
					sql.Append(string.Format("UPDATE {0} ", cmdNode.Attributes["table"].Value));
					sql.Append(update);
					sql.Append(where);
					sql.Append(";");

					cmd.CommandText = sql.ToString();
					cmd.ExecuteNonQuery();
				}
			} catch (Exception ex) {
				// Log errors.
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao : SaveData"), ex);
			} finally {
				DbUtilities.Close(conn, null, null);
			}

			return retVal;
		}

		#endregion

		/// <summary>
		/// Performs the actual data retrieval for GetData() methods.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="cmd"></param>
		/// <returns></returns>
		private DataSet GetData(
			SqlCommand cmd,
			string sql,
			string tableName)
		{
			// Build data adapter object
			SqlDataAdapter adapter = new SqlDataAdapter();
			adapter.TableMappings.Add("Table", tableName);
			adapter.SelectCommand = cmd;

			// Build and fill dataset
			DataSet ds = new DataSet();
			adapter.Fill(ds);

			this.CloseConnection();

			return ds;
		}


		/// <summary>
		/// Processes a given node containing conditions for a where clause or join statement.
		/// </summary>
		/// <param name="conditions">node containing the list of conditions</param>
		/// <param name="cmd">sql command for parameter setting</param>
		/// <returns>StringBuilder representing a portion, or all of the formulated condition clause.</returns>
		private StringBuilder BuildConditions(
			XmlNode conditions,
			SqlCommand cmd,
			string joiningOperator)
		{
			StringBuilder conditionBuilder = new StringBuilder();
			int conditionCounter = 0;

			// Loop conditions
			foreach (XmlNode op in conditions.ChildNodes) {
				// Add the "and" or "or" operator to the where clause
				if (joiningOperator != null && conditionCounter > 0) {
					conditionBuilder.Append(string.Format(" {0} ", joiningOperator));
				}

				conditionBuilder.Append(this.CreateCondition(op, cmd));
				conditionCounter++;
			}

			return conditionBuilder;
		}


		/// <summary>
		/// Appends a condition to the where/join clause based on an xml mapping, along with adding the parameter to the command if necessar.
		/// </summary>
		/// <param name="op">node containing the condition</param>
		/// <param name="cmd">SqlCommand</param>
		/// <returns>StringBuilder representing condition</returns>
		private StringBuilder CreateCondition(
			XmlNode op,
			SqlCommand cmd)
		{
			StringBuilder condition = new StringBuilder();
			string formattedFieldName = null;
			// Determine operator name (necesary for backwards compatibility)
			string oper = (op.Name == "Operator") ? op.Attributes["name"].Value.ToLower() : op.Name.ToLower();
			string rightSide = null;

			// Set parameter info if we're not in a grouping node
			if ((op.Name != "Operator" && "or,and,not".IndexOf(oper) == -1)
					|| (op.Name == "Operator" && "or,and,not".IndexOf(oper) == -1)) {
				string fieldName = null;
				string fieldValue = null;
				string parameterName = null;
				string sourceColumn = null;

				if (op.SelectNodes("Null").Count > 0) {
					fieldName = op["Null"]["Field"].Attributes["name"].Value;
				} else {
					fieldName = op["Field"].Attributes["name"].Value;
					int lastPeriodPosition = fieldName.LastIndexOf(".");
					// no literal attribute defaults to mean literal="true"
					// false value indicates a table field is being used
					bool isLiteral = op["Field"].Attributes["literal"] == null
							|| "true".Equals(op["Field"].Attributes["literal"].Value.ToLower());

					fieldValue = op["Field"].InnerText;
					sourceColumn = (lastPeriodPosition == -1)
							? fieldName
							: fieldName.Substring(lastPeriodPosition + 1, fieldName.Length - lastPeriodPosition - 1);
					parameterName = string.Format("{0}{1}", sourceColumn, this.randomGenerator.Next(1, 1000));
						// Random number added for parameter uniqueness
					rightSide = (isLiteral) ? "@" + parameterName : this.FormatFieldName(fieldValue);

					// Create/add/set parameter if the field value is a literal
					if (isLiteral) {
						string[] fieldPieces = fieldName.Split('.');
						string newTable = (fieldPieces.Length > 1) ? fieldPieces[fieldPieces.Length - 2] : this.tableName;

						if (newTable != this.tableName) {
							this.fieldInfo = null;
						}

						// Reset the table name if necessary to coincide with the field for MakeParameter field lookup
						this.tableName = newTable;
						cmd.Parameters.Add(this.MakeParameter(parameterName, sourceColumn));
						cmd.Parameters["@" + parameterName].Value = fieldValue;
					}
				}

				formattedFieldName = this.FormatFieldName(fieldName);
			}

			switch (oper) {
				case "lt":
					condition.Append(string.Format("{0} < {1} ", formattedFieldName, rightSide));
					break;
				case "gt":
					condition.Append(string.Format("{0} > {1} ", formattedFieldName, rightSide));
					break;
				case "ea":
					if (op.SelectNodes("Null").Count > 0) {
						condition.Append(string.Format("{0} IS NULL ", formattedFieldName));
					} else {
						condition.Append(string.Format("{0} = {1} ", formattedFieldName, rightSide));
					}
					break;
				case "nea":
					if (op.SelectNodes("Null").Count > 0) {
						condition.Append(string.Format("{0} IS NOT NULL ", formattedFieldName));
					} else {
						condition.Append(string.Format("{0} != {1} ", formattedFieldName, rightSide));
					}
					break;
				case "lea":
					condition.Append(string.Format("{0} <= {1} ", formattedFieldName, rightSide));
					break;
				case "gea":
					condition.Append(string.Format("{0} >= {1} ", formattedFieldName, rightSide));
					break;
				case "null":
					condition.Append(string.Format("{0} IS NULL ", formattedFieldName));
					break;
				case "and":
					condition.Append("(");
					condition.Append(this.BuildConditions(op, cmd, "and"));
					condition.Append(")");
					break;
				case "or":
					condition.Append("(");
					condition.Append(this.BuildConditions(op, cmd, "or"));
					condition.Append(")");
					break;
				case "not":
					condition.Append("NOT(");
					condition.Append(this.BuildConditions(op, cmd, null));
					condition.Append(")");
					break;
			}

			return condition;
		}

		
		/// <summary>
		/// Formats a single part of an ORDER BY clause.  Adds support to order by expressions as well as simple fields.
		/// </summary>
		/// <param name="orderField"></param>
		/// <returns></returns>
		private string FormatOrderBy(XmlNode orderField)
		{
			return orderField.Name == "Expression" ? orderField.InnerText : this.FormatFieldName(orderField.Attributes["name"].Value);
		}


		/// <summary>
		/// Formats a field name so that correct aliases are used and brackets are inserted appropriately.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private string FormatFieldName(
			string field)
		{
			string[] fieldPieces = field.Split('.');

			// Replace the table name with an alias
			if (fieldPieces.Length > 1) {
				int tableNamePosition = fieldPieces.Length - 2;

				fieldPieces[tableNamePosition] = this.aliases[fieldPieces[tableNamePosition]].ToString();
			}

			// Join the array of field pieces back together with the ".", and enclose all pieces in brackets
			return string.Format("[{0}]", string.Join(".", fieldPieces).Replace(".", "].["));
		}


		private Set GetFields(
			XmlNode fieldsNode,
			SqlCommand cmd)
		{
			Set fields = new Set();

			try {
				Set currentFields = this.GetCommandParamNames(cmd);

				XmlNodeList fieldNodes = fieldsNode.SelectNodes("Field");
				foreach (XmlNode field in fieldNodes) {
					string name = field.Attributes["name"].Value;
					// TODO: ??  check for null ??
					//paramName = "@" + name;
					fields.Add(name);
				}

				Set missingFields = fields - currentFields;
				Set redundantFields = currentFields - fields;

				// Remove redundant fields from the sql command.
				foreach (string param in redundantFields) {
					cmd.Parameters.Remove(cmd.Parameters[param]);
				}

				// Add missing fields to the sql command.
				foreach (string param in missingFields) {
					cmd.Parameters.Add(this.MakeParameter(param));
				}

				// Set parameter values
				foreach (string param in fields) {
					string sqlParam = "@" + param;
					string stringValue = fieldsNode.SelectSingleNode("//Field[@name = '" + param + "']").InnerText;

					switch (cmd.Parameters[sqlParam].SqlDbType) {
							// TODO: Add code for other data types.
						case SqlDbType.Int:
							cmd.Parameters[sqlParam].Value = int.Parse(stringValue);
							break;

						default:
							cmd.Parameters[sqlParam].Value = stringValue;
							break;
					}
				}
			} catch (Exception ex) {
				// Log errors.
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao : GetFields"), ex);
			}

			return fields;
		}


		private Set GetCommandParamNames(
			SqlCommand cmd)
		{
			Set paramNames = new Set();
			string aParamName;

			IEnumerator enumerator = cmd.Parameters.GetEnumerator();
			while (enumerator.MoveNext()) {
				aParamName = ((SqlParameter) enumerator.Current).ParameterName;
				paramNames.Add(aParamName.Substring(1, aParamName.Length - 1));
			}

			return paramNames;
		}


		private SqlParameter MakeParameter(
			string paramName)
		{
			return this.MakeParameter(paramName, paramName);
		}


		private SqlParameter MakeParameter(
			string paramName,
			string paramSourceColumn)
		{
			FieldInfo theField = (FieldInfo) (this.GetFieldInfo()[paramSourceColumn.ToLower()]);

			return this.MakeParameter(paramName, paramSourceColumn, theField.Type, theField.MaxLength);
		}


		private SqlParameter MakeParameter(
			string paramName,
			string paramSourceColumn,
			SqlDbType paramType,
			int paramMaxLength)
		{
			SqlParameter param = new SqlParameter("@" + paramName, paramType);
			param.SourceColumn = paramSourceColumn;

			// Size CAN NOT be included in the SqlParameter constructor. 
			// An exception will occur if its value is set to null in database (e.g. numeric data types).
			if (paramMaxLength > 0) {
				param.Size = paramMaxLength;
			}

			return param;
		}


		/// <summary>
		/// Gets information about the fields of the specified table from the database.
		/// </summary>
		/// <returns>A <c>Hashtable</c> containing the meta data about the fields of the
		///			specified table, key on the field name.</returns>
		private Hashtable GetFieldInfo()
		{
			// Table name has to be set before this method is called the first time.
			if ((this.fieldInfo == null) && (this.tableName != null) && (this.tableName.Length > 0)) {
				this.LoadFieldInfo(this.tableName);
			}

			return this.fieldInfo;
		}


		/// <summary>
		/// Loads information about the fields of the specified table from the database.
		/// </summary>
		/// <param name="tableName">The name of the table to load the field information for.</param>
		private void LoadFieldInfo(
			string tableName)
		{
			SqlCommand cmd = null;
//		SqlConnection	conn	= null;
			SqlDataReader dr = null;

			try {
				cmd = new SqlCommand();
				cmd.Connection = this.GetConnection();

				cmd.CommandText = string.Format("select column_name, ordinal_position, column_default,"
												+
												" is_nullable, data_type, character_maximum_length, cast(numeric_precision as int) as numeric_precision"
												+ " from information_schema.columns where table_name = '{0}'",
												tableName);

				dr = cmd.ExecuteReader();

				this.fieldInfo = new Hashtable();

				while (dr.Read()) {
					string name = DbUtilities.GetStringValue(dr, "column_name");
					int pos = DbUtilities.GetIntValue(dr, "ordinal_position").Value;
//				object defaultVal	= DbUtilities.(dr, "column_default");
					bool nullable = (string.Compare("yes", DbUtilities.GetStringValue(dr, "is_nullable"), true) == 0);
					string type = DbUtilities.GetStringValue(dr, "data_type");
					int? maxLenNullable = DbUtilities.GetIntValue(dr, "character_maximum_length");
					int maxLen = maxLenNullable.HasValue ? maxLenNullable.Value : 0;
					int? precNullable = DbUtilities.GetIntValue(dr, "numeric_precision");
					int prec = precNullable.HasValue ? precNullable.Value : 0;

					this.fieldInfo.Add(name.ToLower(), new FieldInfo(pos, null, nullable, type, maxLen, prec));
				}
			} catch (Exception ex) {
				// Log errors
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("MsGenericDao : LoadFieldInfo"), ex);
			} finally {
				DbUtilities.Close(null, cmd, dr);
			}
		}


		/// <summary>
		/// Gets a connection to the database.
		/// </summary>
		/// <returns>An <c>SqlConnection</c> for use in communicating with the
		///			database defined when this Dao was instantiated</returns>
		private new SqlConnection GetConnection()
		{
			if (this.conn == null) {
				this.conn = (SqlConnection) base.GetConnection();
			}

			return this.conn;
		}


		/// <summary>
		/// Closes the database connection for the instance.
		/// </summary>
		private void CloseConnection()
		{
			if (this.conn != null) {
				this.conn.Close();
				this.conn = null;
			}
		}

		#region Nested type: FieldInfo

		private class FieldInfo
		{
			private readonly bool isNullable;
			private readonly int maxLength;
			private readonly int numericPrecision;
			private readonly int position;
			private readonly SqlDbType sqlType;
			private readonly string typeName;
			private object defaultValue;


			public FieldInfo(
				int pos,
				// TODO: Make this a boolean value?
				object defValue,
				bool nullable,
				string tName,
				int maxLen,
				int numPrecision)
			{
				this.position = pos;
				this.defaultValue = defValue;
				this.isNullable = nullable;
				this.typeName = tName;
				this.maxLength = maxLen;
				this.numericPrecision = numPrecision;

				try {
					this.sqlType = this.GetSqlType(this.typeName);
				} catch (Exception ex) {
					// Log errors.
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("MsGenericDao.FieldInfo : Constructor"), ex);
				}
			}


			public int Position
			{
				get { return this.position; }
			}


			public object IsNullable
			{
				get { return this.isNullable; }
			}


			public SqlDbType Type
			{
				get { return this.sqlType; }
			}


			public int MaxLength
			{
				get { return this.maxLength; }
			}


			public int NumericPrecision
			{
				get { return this.numericPrecision; }
			}


			private SqlDbType GetSqlType(
				string name)
			{
				// TODO: What if the input value doesn't match any of the enum values?
//			SqlDbType returnValue;
//
//			try {
				return (SqlDbType) (Enum.Parse(typeof (SqlDbType), name, true));
//			} catch (Exception ex) {
//				// TODO: Log error
//			}
//
//			return returnValue;
			}
		}

		#endregion
	}
}