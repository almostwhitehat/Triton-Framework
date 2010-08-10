using System;
using Triton.Controller.StateMachine;
using Triton.Utilities;

namespace Triton.Controller.Publish
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>PublishRecord</b> contains the information about a published piece of content.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class PublishRecord
	{
		private readonly string evnt;
		private readonly string key;
		private readonly long? publishedStateId = null;
		private readonly string publisherName;

		/// <summary>
		/// The state ID of the start state.
		/// </summary>
		private readonly long? startStateId = null;


		internal PublishRecord(
			string key,
			long? startStateId,
			string evnt,
			long? publishedStateId,
			string publishedPath,
			DateTime? lastPublished,
			string publisherName)
		{
			this.key = key;
			this.startStateId = startStateId;
			this.evnt = evnt;
			this.publishedStateId = publishedStateId;
			this.PublishedPath = publishedPath;
			this.LastPublished = lastPublished;
			this.publisherName = publisherName;
		}


		public string Key
		{
			get { return this.key; }
		}


		public PageState StartState
		{
			get {
				return this.startStateId.HasValue
						? (PageState) StateManager.GetInstance().GetState(this.startStateId.Value)
						: null;
			}
		}


		public string Event
		{
			get { return this.evnt; }
		}


		public PublishableState PublishedState
		{
			get {
				return this.publishedStateId.HasValue
						? (PublishableState)StateManager.GetInstance().GetState(this.publishedStateId.Value)
						: null;
			}
		}


		public string PublisherName
		{
			get { return this.publisherName; }
		}


		public string PublishedPath { get; set; }


		public DateTime? LastPublished { get; set; }


		public bool Publishing { get; set; }


		public int HitCount { get; set; }
	}
}