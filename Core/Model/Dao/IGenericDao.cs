using System.Data;
using System.Xml;

namespace Triton.Model.Dao
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>IGenericDao</b> interface defines the interface for a Dao that is not
	/// tied to a particular table or set of tables.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IGenericDao
	{
		/// <summary>
		/// A flag for setting indentity insert
		/// </summary>
		bool SetIdentity { get; set; }


		/// <summary>
		/// Retrieves data from one or multiple sources based on configuration.
		/// </summary>
		/// <param name="xml">string of xml-formatted data</param>
		/// <returns>DataSet</returns>
		/// <remarks>
		/// The xml format should be as follows:
		/// <Select name="" connectionType="">
		/// 	<Fields>
		///			prefix field names with their table name as tablename.fieldname
		/// 		<Field name=""/>
		///			...
		/// 	</Fields>
		/// 	<Tables>
		///			main table
		/// 	<Table name="content"/>
		///			joins
		/// 		<Table name="">
		/// 			<Join type="[JOIN|OUTER JOIN|etc.]">
		/// 				<Operator name="and">
		/// 					<Operator name="ea">
		///							if this is not a literal comparison, include the table name for the field
		/// 						<Field name="" literal="[true|false]">/// </Field>
		/// 					</Operator>
		/// 				</Operator>
		/// 			</Join>
		/// 		</Table>
		///			...
		/// 	</Tables>
		/// 	<Where>
		/// 		<Operator name="[and|or|not]">
		/// 			<Operator name="ea">
		///					if this is not a literal comparison, include the table name for the field
		/// 				<Field name="content.site_id" literal="">/// </Field>
		/// 			</Operator>
		///				...
		/// 		</Operator>
		/// 	</Where>
		/// 	<OrderBy>
		///			include the table name for the field's name attribute value
		/// 		<Field name="cma_content_status_report.[sortField]" direction="[ASC|DESC]"/>
		/// 	</OrderBy>
		/// </Select>
		/// </remarks>
		DataSet GetData(
			string xml);


		/// <summary>
		/// Retrieves data from numerous sources based on configuration.
		/// </summary>
		DataSet GetData(
			XmlNode node);


		DataSet GetData(
			string tableName,
			string[] fields);


		DataSet GetData(
			string tableName,
			string[] fields,
			string[] conditions);


		/// <summary>
		/// Saves data to the permanent store.  <b>SaveData</b> operates only on
		/// a single table -- it cannot save to multiple tables at one time.
		/// <b>SaveData</b> can either update existing data, or insert new data.
		/// </summary>
		/// <remarks>
		/// The XML given as input to <b>SaveData</b>is of the following format.
		/// For inserting new data:
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
		/// </pre>
		/// <p>For updating existing data:</p>
		/// <pre>
		/// <Update table="">
		///		<Fields>
		///			<Field name=""></Field>
		///			...
		///			<Null>
		///				<Field name="" />
		///			</Null>
		///		</Fields>
		///		<Where>
		///			...
		///		</Where>
		/// </Update>
		/// </pre>
		/// </remarks>
		/// <param name="xml">The XML definition of the data to save to the permanent
		///			store.</param>
		long SaveData(
			string xml);


		long SaveData(
			XmlDocument xmlDoc);
	}
}