using System.IO;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcPostedFile</b> is the interface for a client uploaded file within a request.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface MvcPostedFile
	{
		/// <summary>
		/// Gets the size in bytes of an uploaded file.
		/// </summary>
		long Length { get; }


		/// <summary>
		/// Gets the fully-qualified name of the file on the client's computer 
		/// (for example "C:\MyFiles\Test.txt").
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Gets the content type of a file sent by a client.
		/// </summary>
		string Type { get; }


		/// <summary>
		/// Gets a <c>Stream</c> object which points to an uploaded file to prepare 
		/// for reading the contents of the file.
		/// </summary>
		Stream InputStream { get; }


		/// <summary>
		/// Saves the contents of an uploaded file.
		/// </summary>
		/// <param name="fileName">The name of the saved file.</param>
		void SaveAs(string fileName);
	}
}