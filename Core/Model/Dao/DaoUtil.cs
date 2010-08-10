namespace Triton.Model.Dao
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>DaoUtil</b> is a utility class for DAOs.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class DaoUtil
	{
		#region DBLogicalOperator enum

		/// <summary>
		/// <c>DBLogicalOperator</c> defines the logical operators that can be used in 
		/// filters and DAOs.
		/// </summary>
		public enum DBLogicalOperator
		{
			AND,
			OR
		}

		#endregion

		#region DBSortDirection enum

		/// <summary>
		/// Defines the direction of the sort to be used in the DAOs.
		/// </summary>
		public enum DBSortDirection
		{
			ASC,
			DESC
		}

		#endregion

		private DaoUtil() {}
	}
}