namespace Triton.Model
{
	/// <summary>
	/// Base Search Result. Defines base properties for the objects.
	/// </summary>
	/// <author>Garun Vagidov</author>
	public abstract class SearchResultBase
	{
		/// <summary>
		/// Gets the total number of items that matched the requested search.
		/// </summary>
		public long TotalMatches { get; protected set; }


		/// <summary>
		/// Gets the number of items returned.
		/// </summary>
		public long NumberReturned { get; protected set; }


		/// <summary>
		/// Gets the record number, relative to all matches, of the first record returned.
		/// </summary>
		public long FirstItemNumber { get; protected set; }


		/// <summary>
		/// Gets the record number, relative to all matches, of the last record returned.
		/// </summary>
		public long LastItemNumber { get; protected set; }


		/// <summary>
		/// Gets the number of items remaining from the search beyond the page
		/// of items included here.
		/// </summary>
		public long NumberRemaining { get; protected set; }


		/// <summary>
		/// Gets the filter applied to obtain the results returned.
		/// </summary>
		public object Filter { get; protected set; }


		/// <summary>
		/// Gets the number of the page of paged results.
		/// </summary>
		public int Page { get; protected set; }


		/// <summary>
		/// Gets the size of the current page of results.
		/// </summary>
		public int PageSize { get; protected set; }


		/// <summary>
		/// Gets the number of items on the first page if different than PageSize.
		/// </summary>
		public int FirstPageSize { get; protected set; }


		/// <summary>
		/// Gets the total number of pages required for all results.
		/// </summary>
		public int TotalPages { get; protected set; }
	}
}