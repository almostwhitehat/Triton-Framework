using System.IO;
using System.Web;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcHttpPostedFile</b> provides a way to access individual files that 
	/// have been uploaded by a client.  This is an Adapter class for HttpPostedFile.
	/// </summary>
	/// <remarks>
	/// The <c>MvcPostedFileCollection</c> class provides access to all the files uploaded 
	/// from a client as a file collection. <b>MvcHttpPostedFile</b> provides properties and 
	/// methods to get information on an individual file and to read and save the 
	/// file. Files are uploaded in MIME multipart/form-data format and are buffered 
	/// in server memory until explicitly saved to disk.
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class MvcHttpPostedFile : MvcPostedFile
	{
		/// <summary>
		/// Reference to the underlying HttpPostedFile object.
		/// </summary>
		private readonly HttpPostedFile baseFile;


		/// <summary>
		/// Constructs a new <b>MvcHttpPostedFile</b> adapter for the given <b>HttpPostedFile</b>.
		/// </summary>
		/// <param name="httpFile"></param>
		public MvcHttpPostedFile(
			HttpPostedFile httpFile)
		{
			this.baseFile = httpFile;
		}

		#region MvcPostedFile Members

		/// <summary>
		/// Gets the size in bytes of an uploaded file.
		/// </summary>
		public long Length
		{
			get { return this.baseFile.ContentLength; }
		}


		/// <summary>
		/// Gets the fully-qualified name of the file on the client's computer 
		/// (for example "C:\MyFiles\Test.txt").
		/// </summary>
		public string Name
		{
			get { return this.baseFile.FileName; }
		}


		/// <summary>
		/// Gets the MIME content type of a file sent by a client.
		/// </summary>
		public string Type
		{
			get { return this.baseFile.ContentType; }
		}


		/// <summary>
		/// Gets a <c>Stream</c> object which points to an uploaded file to prepare 
		/// for reading the contents of the file.
		/// </summary>
		public Stream InputStream
		{
			get { return this.baseFile.InputStream; }
		}


		/// <summary>
		/// Saves the contents of an uploaded file.
		/// </summary>
		/// <param name="fileName">The name of the saved file.</param>
		public void SaveAs(
			string fileName)
		{
			this.baseFile.SaveAs(fileName);
		}

		#endregion
	}
}