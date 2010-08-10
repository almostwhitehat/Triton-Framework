using System.Xml;

namespace Triton.Model.Dao
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// This interface defines a parameterized Dao that can operate on any table/view
	/// and return a collection of the type specified.
	/// </summary>
	/// <typeparam name="T">The <b>Type</b> of the object the Dao is to return.</typeparam>
	///	<author>Scott Dyke</author>
	public interface IGenericDao<T> where T : new()
	{
		SearchResult<T> GetData(
			string tableName,
			string[] fields);


		SearchResult<T> GetData(
			string tableName,
			string[] fields,
			string[] conditions);


		SearchResult<T> GetData(
			string tableName,
			string[] fields,
			string[] conditions,
			XmlNode fieldMappings);


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


		/// <summary>
		/// Deletes records from the specified table.
		/// </summary>
		/// <param name="tableName">The name of the table to delete records from.</param>
		/// <param name="conditions">The condition(s) for which records to delete.  Conditions
		///			are of the form [field_name][operator][value].  For example: "id=3".
		///			If <b>conditions</b> is null all records are deleted.</param>
		/// <returns>The number of records deleted.</returns>
		int Delete(
			string tableName,
			string[] conditions);
	}
}