using System;
using System.Collections.Generic;
using Triton.Model;

namespace Triton.Location.Model.Dao
{
	/// <summary>
	/// Defines the contract of how PersistedAddress is retrieved, persisted and deleted.
	/// </summary>
	public interface IPersistedAddressDao
	{
		/// <summary>
		/// Gets the PersistedAddress by id.
		/// </summary>
		/// <param name="id">Id of the PersistedAddress to retrieve.</param>
		/// <returns>PersistedAddress object.</returns>
		PersistedAddress Get(long id);

		/// <summary>
		/// Gets the list of PersistedAddresss by example.
		/// </summary>
		/// <param name="example">Example object of PersistedAddress.</param>
		/// <returns>List of PersistedAddress matching the example.</returns>
		IList<PersistedAddress> Get(PersistedAddress example);
		
		/// <summary>
		/// Saves the PersistedAddress.
		/// </summary>
		/// <param name="persistedAddress">PersistedAddress object to save.</param>
		void Save(PersistedAddress persistedAddress);

		/// <summary>
		/// Saves list of PersistedAddress.
		/// </summary>
		/// <param name="persistedAddresses">List of PersistedAddresss to save.</param>
		void Save(IList<PersistedAddress> persistedAddresses);
		
		/// <summary>
		/// Deletes the PersistedAddress.
		/// </summary>
		/// <param name="persistedAddress">PersistedAddress object to delete.</param>
		void Delete(PersistedAddress persistedAddress);
		
		/// <summary>
		/// Get the filter.
		/// </summary>
		PersistedAddressFilter GetFilter();

		/// <summary>
		/// Find persistedAddresss by filter
		/// </summary>
		/// <param name="filter">Filter with the criteria to search</param>
		/// <returns>SearchResult of PersistedAddress</returns>
		SearchResult<PersistedAddress> Find(PersistedAddressFilter filter);
	}
}