namespace Triton.Logic.Support
{
	public class EventUtilities
	{
		/// <summary>
		/// Get the standard event names for the given count.
		/// Possible names are EventNames.ZERO, EventNames.ONE, EventNames.MULTIPLE.
		/// </summary>
		/// <param name="count">The search result count.</param>
		/// <returns>Event name for the count.</returns>
		public static string GetSearchResultEventName(int count)
		{
			string retEvent;
			switch (count) {
				case 0:
					retEvent = EventNames.ZERO;
					break;
				case 1:
					retEvent = EventNames.ONE;
					break;
				default:
					retEvent = EventNames.MULTIPLE;
					break;
			}

			return retEvent;
		}
	}
}