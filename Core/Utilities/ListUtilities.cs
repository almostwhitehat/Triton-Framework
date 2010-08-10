using System.Collections;
using System.Collections.Generic;

namespace Triton.Utilities
{
	public class ListUtilities
	{
		public static List<T> ConvertToGenericList<T>(IList listObjects)
		{
			List<T> convertedList = new List<T>(listObjects.Count);

			foreach (object listObject in listObjects) {
				convertedList.Add((T) listObject);
			}

			return convertedList;
		}
	}
}