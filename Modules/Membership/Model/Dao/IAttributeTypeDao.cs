using System.Collections.Generic;
using Triton.Model;

namespace Triton.Membership.Model.Dao
{
	/// <summary>
	/// Defines the contract of how AttributeType is retrieved, persisted and deleted.
	/// </summary>
	public interface IAttributeTypeDao
	{
		/// <summary>
		/// Gets the AttributeType by id.
		/// </summary>
		/// <param name="id">Id of the AttributeType to retrieve.</param>
		/// <returns>AttibuteType object.</returns>
		AttributeType Get(int id);

		/// <summary>
		/// Gets the list of attribute types by example.
		/// </summary>
		/// <param name="example">Example object of AttributeType.</param>
		/// <returns>List of AttributeType matching the example.</returns>
		IList<AttributeType> Get(AttributeType example);
		
		/// <summary>
		/// Saves the AttributeType.
		/// </summary>
		/// <param name="type">AttributeType object to save.</param>
		void Save(AttributeType type);

		/// <summary>
		/// Saves list of AttributeTypes.
		/// </summary>
		/// <param name="types">List of AttributeTypes to save.</param>
		void Save(IList<AttributeType> types);
		
		/// <summary>
		/// Deletes the AttributeType.
		/// </summary>
		/// <param name="type">AttributeType object to delete.</param>
		void Delete(AttributeType type);
		
		/// <summary>
		/// Gets the AttributeType by the code value.
		/// </summary>
		/// <param name="code">Code value of the AttributeType.</param>
		/// <returns>AttributeType object associated with the code.</returns>
		AttributeType Get(string code);


		/// <summary>
		/// Get the filter.
		/// </summary>
		AttributeTypeFilter GetFilter();

		/// <summary>
		/// Find attribute types by filter
		/// </summary>
		/// <param name="filter">Filter with the criteria to search</param>
		/// <returns>SearchResult of AttributeTypes</returns>
		SearchResult<AttributeType> Find(AttributeTypeFilter filter);


	}
}