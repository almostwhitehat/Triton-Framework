using System.Collections.Specialized;
using System.Collections.Generic;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Provides access to and organizes files uploaded by a client.
	/// </summary>
	/// <remarks>
	/// Clients encode files and transmit them in the content body using multipart 
	/// MIME format with an HTTP Content-Type header of multipart/form-data. The
	/// system extracts the encoded file(s) from the content body into individual
	/// members of an MvcPostedFileCollection. Methods and properties of the MvcPostedFile 
	/// interface provide access to the contents and properties of each file.
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class MvcPostedFileCollection : Dictionary<string, MvcPostedFile>
	{

		//private  filesByName = new Dictionary<string, MvcPostedFile>();
		private List<MvcPostedFile> filesByIndex = new List<MvcPostedFile>();

		/// <summary>
		/// Indexer to get an individual <b>MvcPostedFile</b> object from the file 
		/// collection. This property is overloaded to allow retrieval of objects 
		/// by either name or numerical index. 
		/// </summary>
		public MvcPostedFile this[int index]
		{
			get { return (MvcPostedFile)filesByIndex[index]; }
		}


		/// <summary>
		/// Indexer to get an individual <b>MvcPostedFile</b> object from the file 
		/// collection. This property is overloaded to allow retrieval of objects 
		/// by either name or numerical index. 
		/// </summary>
		public MvcPostedFile this[string name]
		{
			get { return (MvcPostedFile)base[name]; }
		}


		/// <summary>
		/// Adds a file with the specified key to the collection.
		/// </summary>
		/// <param name="key">The key of the entry to add. The key can be a null reference.</param>
		/// <param name="file">The <b>MvcPostedFile</b> value of the entry to add. The value 
		/// can be a null reference.</param>
		internal void Add(
			string key,
			MvcPostedFile file)
		{
			base.Add(key, file);
			filesByIndex.Add(file);
		}
	}
}