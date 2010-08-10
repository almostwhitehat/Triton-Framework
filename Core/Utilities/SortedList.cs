using System.Collections;

namespace Triton.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// SortedList maintains a list of objects that can be sorted.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class SortedList : BaseList
	{
		public void Sort()
		{
			theList.Sort();
		}


		public void Sort(
			IComparer comparer)
		{
			theList.Sort(comparer);
		}


		public void Sort(
			int index,
			int count,
			IComparer comparer)
		{
			theList.Sort(index, count, comparer);
		}
	}
}