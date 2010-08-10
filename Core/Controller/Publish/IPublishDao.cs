namespace Triton.Controller.Publish
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>IPublishDao</b> interface defines the methods used by the page publishing
	/// system to save and retrieve published page data to and from the persistent
	/// storage.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IPublishDao
	{
		/// <summary>
		/// Reads the published page data from the persistent store.
		/// </summary>
		/// <remarks>
		/// GetPublishInfo returns the published page data in an <c>XmlDocument</c> in the
		/// following format:
		/// <pre>
		///		<PublishedPages>
		///			<Page startState="" publishedState="" key="" event="" path="" lastPublished="" />
		///			...
		///		</PublishedPages>
		/// </pre>
		/// </remarks>
		/// <returns>An <b>XmlDocument</b> containing the published page data
		///			retrieved from the database.</returns>
		PublishedPageCollection GetPublishInfo();


		/// <summary>
		/// Saves the published page collection, represented as XML, to the 
		/// persistent store.
		/// </summary>
		/// <param name="publishInfo">An XML document containing the published page collection
		///			to save to the database.</param>
		void SavePublishInfo(PublishedPageCollection publishInfo);
	}
}