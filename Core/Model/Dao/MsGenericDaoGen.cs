using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Triton.Utilities.Reflection;

namespace Triton.Model.Dao {

	#region History

	// 10/27/2009	GV	Fixed the search result return
	//   4/6/2011	SD	Updated GetData to use ReflectionUtilities rather than custom
	//					reflection implementation.

	#endregion

	/// <summary>
	/// Implements the <b>IGenericDao&lt;T&gt;</b> interface for MS SQL Server databases.
	/// </summary>
	/// <typeparam name="T">The <b>Type</b> of the object the Dao is to return.</typeparam>
	///	<author>Scott Dyke</author>
	public class MsGenericDao<T> : MsDaoBase, IGenericDao<T> where T : new()
	{
		private const string DAO_NAME = "GenericDAOGen";
		private const string PROPERTY_MAPPING_NAME = "object_attr";

		private readonly MsGenericDao baseDao;
		private SqlConnection conn;


		public MsGenericDao(
			string connType)
		{
			this.baseDao = new MsGenericDao(connType);
		}


		public override string Name
		{
			get { return DAO_NAME; }
		}

		#region IGenericDao<T> Members

		public SearchResult<T> GetData(
			string tableName,
			string[] fields)
		{
			return this.GetData(tableName, fields, null);
		}


		public SearchResult<T> GetData(
			string tableName,
			string[] fields,
			string[] conditions)
		{
// TODO: load field mappings

			return this.GetData(tableName, fields, conditions, null);
		}


		/// <summary>
		/// Retrieves the data from the database based on the given criteria.
		/// </summary>
		/// <param name="tableName">The name of the table to retrieve data from.</param>
		/// <param name="fields">The names of the fields to retrieve data from.</param>
		/// <param name="conditions">Conditions to apply when retrieving the data.</param>
		/// <param name="fieldMappings">An XmlNode containing mappings from the table's
		///			fields to the target object's properties.  The format of the XML is:
		/// <pre>
		/// 	<fields>
		/// 		<field name="field_name" object_attr="PropertyName" [type="Type"] />
		///			...
		/// 	</fields>
		/// </pre>
		/// For example:
		/// <pre>
		/// 	<fields>
		/// 		<field name="item_code" object_attr="Code" type="long" />
		/// 		<field name="item_name" object_attr="Name" /> 
		/// 		<field name="item_description" object_attr="Name" /> 
		/// 	</fields>
		/// </pre>
		/// The optional <b>type</b> attribute can be used to specify the property type if
		/// the target object has overloaded properties with different types.
		/// </param>
		/// <returns></returns>
		// Notes:
		//  Since we need to maintain backward compatibility with the non-parameterized
		//  IGenericDao, we use that here to get the DataSet of data for the given
		//  criteria and just do the mapping to the return objects here.
		public SearchResult<T> GetData(
			string tableName,
			string[] fields,
			string[] conditions,
			XmlNode fieldMappings)
		{
			DataSet ds = this.baseDao.GetData(tableName, fields, conditions);
			DataTable dt = ds.Tables[tableName];
			List<T> resultList = new List<T>();

					//  loop through the rows of the table to build the object list
			for (int i = 0; i < dt.Rows.Count; i++) {
				DataRow dr = dt.Rows[i];

				T obj = new T();

				XmlNodeList fieldMappingsList = fieldMappings.SelectNodes("//field");

				foreach (XmlNode fieldNode in fieldMappingsList) {
					string fieldName = fieldNode.Attributes["name"].Value;
					string propertyName = fieldNode.Attributes["object_attr"].Value;
					string typeName = (fieldNode.Attributes["type"] == null) ? null : fieldNode.Attributes["type"].Value;

					bool hasProperty = false;
					if (typeName == null) {
						hasProperty = ReflectionUtilities.HasProperty(obj, propertyName);
					} else {
						hasProperty = ReflectionUtilities.HasProperty(obj, propertyName, Type.GetType(typeName));
					}

					if (hasProperty) {
						ReflectionUtilities.SetPropertyValue(obj, propertyName, dr[fieldName]);
					}
				}

				resultList.Add(obj);
			}

			object[] filter = new object[]{tableName, fields, conditions, fieldMappings};
			return new SearchResult<T>(
					resultList.ToArray(), 
					filter);
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
		///			<Operators>
		///				possible conditions take the form of:
		///				<Operator name="{operatorName}">
		///					<Field name=""></Field>
		///				</Operator>
		///				where the operatorName attribute is one of the following values:
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
		///			</Operators>
		///		</Where>
		/// </Update>
		/// </pre>
		/// </remarks>
		public long SaveData(
			string xml)
		{
			return this.baseDao.SaveData(xml);
		}


		public long SaveData(
			XmlDocument xmlDoc)
		{
			return this.baseDao.SaveData(xmlDoc);
		}


		/// <summary>
		/// Deletes records from the specified table.
		/// </summary>
		/// <param name="tableName">The name of the table to delete records from.</param>
		/// <param name="conditions">The condition(s) for which records to delete.  Conditions
		///			are of the form [field_name][operator][value].  For example: "id=3".
		///			If <b>conditions</b> is null all records are deleted.</param>
		/// <returns>The number of records deleted.</returns>
		public int Delete(
			string tableName,
			string[] conditions)
		{
			StringBuilder sql = new StringBuilder();

			sql.Append("delete from ");
			sql.Append(tableName);

			//  get the connection
			SqlConnection conn = this.GetConnection();
			conn.Open();
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;

			//  build the where clause
			sql.Append(this.BuildWhere(conditions, cmd));

			//  build the command
			cmd.CommandText = sql.ToString();
			cmd.CommandType = CommandType.Text;

			//  execute the command
			int cnt = cmd.ExecuteNonQuery();

			return cnt;
		}

		#endregion


		/// <summary>
		/// Builds the "where" clause for a sql query based on the given conditions and adds
		/// the corresponding parameters to the given <b>SqlCommand</b>.
		/// </summary>
		/// <remarks>
		/// The conditions are of the form [field_name][operator][value].  For example: "id=3",
		/// or "name is not null".
		/// </remarks>
		/// <param name="conditions">The conditions to build the where clause for.</param>
		/// <param name="cmd">The <b>SqlCommand</b> the parameters are added to.</param>
		/// <returns>The where clause for the given conditions.</returns>
		private string BuildWhere(
			string[] conditions,
			SqlCommand cmd)
		{
			StringBuilder where = new StringBuilder();

			//  if conditions were given, build the where clause
			if ((conditions != null) && (conditions.Length > 0)) {
				where.Append(" where ");

				for (int k = 0; k < conditions.Length; k++) {
					//  get the field name, operator, and value from the condition
					Match m = Regex.Match(conditions[k], "([^<>= ]*)( is |[ <>=]*)(.*)");

					//  make sure we have the correct number of matches
					if (m.Groups.Count < 3) {
						throw new ApplicationException(string.Format(
													   	"Invalid condition: '{0}'.", conditions[k]));
					}

					//  get the field/parameter name, operator, and value from the Regex
					string paramName = "@" + m.Groups[1].Value;
					string op = m.Groups[2].Value;
					string val = m.Groups[3].Value;

					//  if the operator is "is" we can't use parameters
					if (op.ToLower() == " is ") {
						paramName = val;
						val = null;
					} else {
						//  trim beginning end ending ' (single quote)
						if (val.StartsWith("'")) {
							val = val.Remove(0, 1);
						}
						if (val.EndsWith("'")) {
							val = val.Remove(val.Length - 1);
						}
					}
					//  add the condition to the sql statement
					where.Append(m.Groups[1].Value + op + paramName + " and ");
					//  add the value as a parameter
					if (val != null) {
						cmd.Parameters.AddWithValue(paramName, val);
					}
				}

				//  remove trailing " and "
				where.Length -= 5;
			}

			return where.ToString();
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


		private PropertyInfo GetProperty(
			Type objType,
			string propertyName,
			string typeName)
		{
			//  find properties of the object that are public instance properties,
			//  are writable, and match the given property name
			MemberInfo[] propList = objType.FindMembers(MemberTypes.Property,
					BindingFlags.Instance | BindingFlags.Public,
					this.FindMembersCanWriteDelegate,
					propertyName);

			//  if no property found, throw exception
			if (propList.Length == 0) {
				throw new ApplicationException(string.Format("No property '{0}' found on class '{1}'.",
						 propertyName,
						 objType.Name));

				//  if there is more than 1 property with the specified name,
				//  check for the specific type
			} else if (propList.Length > 1) {
				if (typeName != null) {
					propList = objType.FindMembers(MemberTypes.Property,
						   BindingFlags.Instance | BindingFlags.Public,
						   this.FindMembersCanWriteDelegate,
						   propertyName + "|" + typeName);
				}

				//  check again for the count (should be 1)
				if (propList.Length != 1) {
					throw new ApplicationException(string.Format("{2} properties '{0}' of type '{3}' found on class '{1}'.",
							 propertyName,
							 objType.Name,
							 (propList.Length == 0) ? "No" : "Multiple",
							 typeName));
				}
			}

			return (PropertyInfo) propList[0];
		}


		/// <summary>
		/// A <b>MemberFilter</b> delegate used by FindMembers to filter by
		/// member name and writable properties.
		/// </summary>
		/// <param name="memberInfo">The <b>MemberInfo</b> to filter.</param>
		/// <param name="searchObj">May be the name or name|type of the property to filter for.</param>
		/// <returns><b>True</b> the member currently being inspected matches the 
		///			filter criteria, <b>false</b> otherwise</returns>
		private bool FindMembersCanWriteDelegate(
			MemberInfo memberInfo,
			object searchObj)
		{
			string name = searchObj.ToString();
			string typeName = null;

			//  if the searchObj contains both a property name and type (name|type),
			//  get the individual components
			int pos = name.IndexOf('|');
			if (pos > 0) {
				typeName = name.Substring(pos = 1);
				name = name.Substring(0, pos);
			}

			//  compare the name of the property,
			//  make sure it is writable
			//  if a type was specified, match the type
			return ((memberInfo.Name.ToLower() == name.ToLower())
					&& ((PropertyInfo) memberInfo).CanWrite
					&& ((typeName == null) || (((PropertyInfo) memberInfo).PropertyType.Name == typeName)));
		}
	}
}