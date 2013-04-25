using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Common.Logging;
using Triton.Utilities;

namespace Triton.Controller.Publish
{

	#region History

	// History:
	// 6/2/2009		KP Changed the logging to Common.logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <b>PublshedPageCollection</b> maintains a collection of <b>PublishRecord</b>s.
	/// </summary>
	/// <remarks>
	/// <b>PublshedPageCollection</b> is essentially a wrapper around a Hashtable that
	/// ensures type safety and provides some convenience methods/properties.
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class PublishedPageCollection
	{
		private readonly Hashtable pageHash = Hashtable.Synchronized(CollectionsUtil.CreateCaseInsensitiveHashtable());


		/// <summary>
		/// Constructs an empty <b>PublishedPageCollection</b>.
		/// </summary>
		internal PublishedPageCollection()
		{
		}


		/// <summary>
		/// Constructs a <b>PublishedPageCollection</b> from the given data in XML
		/// format.
		/// </summary>
		/// <remarks>
		/// The format of the pagesXml parameter is:
		/// <pre>
		///		<PublishedPages>
		///			<Page startState="" publishedState="" key="" event="" path="" lastPublished="" />
		///			...
		///		</PublishedPages>
		/// </pre>
		/// </remarks>
		/// <param name="pagesXml">The <b>XmlDocument</b> containing the data to
		///			populate the new <b>PublishedPageCollection</b> with.</param>
		public PublishedPageCollection(
			XmlDocument pagesXml)
		{
			if ((pagesXml != null) && (pagesXml.DocumentElement != null)) {
				XmlNodeList pages = pagesXml.DocumentElement.SelectNodes("Page");
				foreach (XmlNode pg in pages) {
					long startState = long.Parse(pg.Attributes["startState"].Value);
					long publishedState = long.Parse(pg.Attributes["publishedState"].Value);
					string key = pg.Attributes["key"].Value;
					string evnt = pg.Attributes["event"].Value;
					string path = pg.Attributes["path"].Value;
					string publisher = pg.Attributes["publisher"].Value;
					int hits = int.Parse(pg.Attributes["hits"].Value);
					DateTime? publishTime = DateTime.Parse(pg.Attributes["lastPublished"].Value);

					PublishRecord pageRec = new PublishRecord(
							key, startState, evnt, publishedState, path, publishTime, publisher);
					pageRec.HitCount = hits;

					Add(key, pageRec);
				}
			}
		}


		/// <summary>
		/// Gets the <b>PublishRecord</b> for the page with the given key.
		/// </summary>
		internal PublishRecord this[
			string key]
		{
			get {
				return (PublishRecord)this.pageHash[key];
			}
		}


		/// <summary>
		/// Gets the number of page records in the collection.
		/// </summary>
		internal int Count
		{
			get {
				return this.pageHash.Count;
			}
		}


		/// <summary>
		/// Get the list of keys of the collection.
		/// </summary>
		internal ICollection Keys
		{
			get {
				return this.pageHash.Keys;
			}
		}


		/// <summary>
		/// Adds the given <b>PublishRecord</b> to the collection with the specified
		/// key.
		/// </summary>
		/// <param name="key">The key to add the <b>PublishRecord</b> under.</param>
		/// <param name="pubRec">The <b>PublishRecord</b> to add to the collection.</param>
		internal void Add(
			string key,
			PublishRecord pubRec)
		{
			if (!pageHash.ContainsKey(key)) {
				pageHash.Add(key, pubRec);
			}
		}


		/// <summary>
		/// Removes the <b>PublishRecord</b> with the specified key from the collection.
		/// If no entry exists for the key, no action is taken.
		/// </summary>
		/// <param name="key">The key for the <b>PublishRecord</b> to remove.</param>
		internal void Remove(
			string key)
		{
			this.pageHash.Remove(key);
		}


		/// <summary>
		/// Removes all of the page records from the collection.
		/// </summary>
		internal void Clear()
		{
			this.pageHash.Clear();
		}


		/// <summary>
		/// Returns an IDictionaryEnumerator that can iterate through the 
		/// PublishedPageCollection.
		/// </summary>
		/// <returns>An IDictionaryEnumerator that can iterate through the 
		///			PublishedPageCollection.</returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return this.pageHash.GetEnumerator();
		}


		/// <summary>
		/// Builds an XML representation of the collection.
		/// </summary>
		/// <remarks>
		/// The format of the returned XML document is:
		/// <pre>
		///		<PublishedPages>
		///			<Page startState="" publishedState="" key="" event="" path="" lastPublished="" />
		///			...
		///		</PublishedPages>
		/// </pre>
		/// </remarks>
		/// <returns></returns>
		public XmlDocument ToXml()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement publishedPagesNode = doc.CreateElement("PublishedPages");
			doc.AppendChild(publishedPagesNode);

			foreach (PublishRecord pr in this.pageHash.Values) {
				//  don't include pages in the process of publishing (incomplete information)
				if (!pr.Publishing) {
					try {
						XmlElement pg = doc.CreateElement("Page");
						XmlAttribute attr = doc.CreateAttribute("startState");
						attr.Value = pr.StartState.Id.ToString();
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("publishedState");
						attr.Value = pr.PublishedState.Id.ToString();
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("event");
						attr.Value = pr.Event;
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("key");
						attr.Value = pr.Key;
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("path");
						attr.Value = pr.PublishedPath;
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("lastPublished");
						attr.Value = pr.LastPublished.ToString();
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("hits");
						attr.Value = pr.HitCount.ToString();
						pg.Attributes.Append(attr);

						attr = doc.CreateAttribute("publisher");
						attr.Value = pr.PublisherName;
						pg.Attributes.Append(attr);

						publishedPagesNode.AppendChild(pg);
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Error(
								errorMessage => errorMessage("PublishedPageCollection.ToXml [{0}] : {1}", pr.Key, e));
					}
				}
			}

			return doc;
		}
	}
}