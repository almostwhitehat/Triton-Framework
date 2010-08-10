using System;

namespace Triton.Model
{

	#region History

	// 10/26/2009	GV	Refactored to have a base class.	

	#endregion

	/// <summary>
	/// A parameterized SearchResult to return type-specific results from a search of a
	/// data source.
	/// </summary>
	/// <typeparam name="T">The <b>Type</b> of the objects to be returned in the SearchResult.</typeparam>
	///	<author>Scott Dyke</author>
	public class SearchResult<T> : SearchResultBase
	{
		/// <summary>
		/// A parameterized SearchResult to return type-specific 
		/// results from a search of a data source.
		/// </summary>
		/// <param name="items">Results from the search of the data source.</param>
		public SearchResult(
			T[] items) :
				this(items, null, 1, items.Length, items.Length) 
		{
		}


		/// <summary>
		/// A parameterized SearchResult to return type-specific 
		/// results from a search of a data source.
		/// </summary>
		/// <param name="items">Results from the search of the data source.</param>
		/// <param name="filter">The filter used to populate the search.</param>
		public SearchResult(
			T[] items,
			object filter) :
				this(items, filter, 1, items.Length, items.Length) 
		{
		}


		/// <summary>
		/// A parameterized SearchResult to return type-specific 
		/// results from a search of a data source.
		/// </summary>
		/// <param name="items">Results from the search of the data source.</param>
		/// <param name="filter">The filter used to populate the search.</param>
		/// <param name="page">The current zero-indexed page.</param>
		/// <param name="pageSize">The number of items returned for a page.</param>
		/// <param name="totalMatches">Total number of matches for the search.</param>
		public SearchResult(
			T[] items,
			object filter,
			int page,
			int pageSize,
			int totalMatches) :
				this(items, filter, page, pageSize, totalMatches, pageSize) 
		{
		}


		/// <summary>
		/// A parameterized SearchResult to return type-specific 
		/// results from a search of a data source.
		/// </summary>
		/// <param name="items">Results from the search of the data source.</param>
		/// <param name="filter">The filter used to populate the search.</param>
		/// <param name="page">The current zero-indexed page.</param>
		/// <param name="pageSize">The number of items returned for a page.</param>
		/// <param name="totalMatches">Total number of matches for the search.</param>
		/// <param name="firstPageSize">The number of items returned for the first page.</param>
		public SearchResult(
			T[] items,
			object filter,
			int page,
			int pageSize,
			int totalMatches,
			int firstPageSize)
		{
			this.Items = items;
			Filter = filter;
			Page = page;
			PageSize = pageSize;
			TotalMatches = totalMatches;
			FirstPageSize = firstPageSize;
			NumberReturned = items.Length;

			this.CalculatePaging();
		}


		/// <summary>
		/// Gets the items returned in as a result of the search.
		/// </summary>
		public T[] Items 
		{ 
			get; 
			private set; 
		}


		/// <summary>
		/// Calculates the index of the first and last record, the number of records remaining,
		/// and the total number of pages needed to display all records based on the given values
		/// for the data to be paged.
		/// </summary>
		private void CalculatePaging()
		{
			//  calculate the total number of pages required to display all of the records
			if (PageSize > 0) {
				TotalPages = (int)Math.Ceiling(Math.Max((TotalMatches - FirstPageSize) / (double)PageSize, 0) + 1);
			}
			
			//  calculate the index of the first record on the specified page
			FirstItemNumber = (Page == 1) ? 0 : (Page - 2) * PageSize + FirstPageSize;

			//  calculate the index of the last record on the specified page
			LastItemNumber = (Page == 1)
			                 	? Math.Min(TotalMatches, FirstPageSize) - 1
			                 	: Math.Min(TotalMatches, FirstPageSize + ((Page - 1) * PageSize)) - 1;

			//  calculate the number of records remaining
			NumberRemaining = Math.Max(0, TotalMatches - LastItemNumber - 1);
		}
	}
}