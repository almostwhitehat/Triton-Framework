using System.IO;

namespace Triton.Support
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// IO contains static utility methods to support I/O operations.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class IO
	{
		/// <summary>
		/// Creates directory(s) in the given path, if they do not exist.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name="directoryPath">The path of the directory(s) to create.</param>
		public static void CreateDirectory(
			string directoryPath)
		{
			//  make sure separators are what's expected
			directoryPath = directoryPath.Replace(@"/", Path.DirectorySeparatorChar.ToString());
			// trim leading \ character
			directoryPath = directoryPath.TrimEnd(Path.DirectorySeparatorChar);

			Directory.CreateDirectory(directoryPath);
		}
	}
}