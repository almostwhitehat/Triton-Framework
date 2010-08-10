using System;
using System.Configuration;

namespace Triton.Controller.Publish
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>Publisher</b> is a facade for the content publishing system.  All requests to the
	/// publishing systems are made via the <b>Publisher</b>.  
	/// </summary>
	///	<author>Scott Dyke</author>
	public class Publisher : IDisposable
	{
		private readonly string name;
		private ContentPublisher contentPublisher;


		protected Publisher(
			string name)
		{
			this.name = name;
		}


		/// <summary>
		/// Gets the name of the Publisher.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		#region IDisposable Members

		/// <summary>
		/// Frees the resources used by the Publisher.
		/// </summary>
		public void Dispose()
		{
			if (this.contentPublisher != null) {
				this.contentPublisher.Dispose();
				this.contentPublisher = null;
			}
		}

		#endregion

		/// <summary>
		/// Gets a <b>Publisher</b> with the given name, as identified in the configuration.
		/// </summary>
		/// <param name="name">The name of the Publisher to get.</param>
		/// <returns>A <b>Publisher</b> with the given name.</returns>
		public static Publisher GetPublisher(
			string name)
		{
			//  try to get the publisher from the manager
			Publisher publisher = PublishManager.GetPublishManager().GetPublisher(name);

			//  if no publisher exists for the given name, try creating a new one
			if (publisher == null) {
				publisher = MakePublisher(name);

				if (publisher != null) {
					PublishManager.GetPublishManager().AddPublisher(publisher);
				}
			}

			return publisher;
		}


		/// <summary>
		/// Generates a key for the publishing system based on the given context.
		/// </summary>
		/// <param name="context">The context to generate a key for.</param>
		/// <returns>A publish key for the given context.</returns>
		public string MakeKey(
			TransitionContext context)
		{
			return this.contentPublisher.GetPublishKey(context);
		}


		/// <summary>
		/// Determines if the content for the given publish key is published.
		/// </summary>
		/// <param name="key">The key to identify the publish content.</param>
		/// <returns><b>True</b> if there is published content for the given key,
		///			<b>false</b> if not.</returns>
		public bool IsPublished(
			string key)
		{
			PublishRecord pubRec = PublishManager.GetPublishManager().GetPublishRecord(key);

			bool isPublished = ((pubRec != null)
			                    && pubRec.PublishedState.Publish
			                    && !pubRec.Publishing
			                    && (pubRec.PublishedPath != null));

// TODO: call a delegate in the contentPublisher????
			//&& IsPublished(pubRec));

			return isPublished;
		}


		/// <summary>
		/// Determines if the published content for the given <b>PublishRecord</b> is expired.
		/// </summary>
		/// <param name="publishRecord">The <b>PublishRecord</b> to check the expiration for.</param>
		/// <returns><b>True</b> if the content is expired, <b>false</b> if not.</returns>
		public virtual bool IsExpired(
			PublishRecord publishRecord)
		{
			return this.contentPublisher.IsExpired(publishRecord);
		}


		/// <summary>
		/// Gets the PublishRecord for the given key.
		/// </summary>
		/// <param name="key">The key to get the PublishRecord for.</param>
		/// <returns>The PublishRecord for the given key, or null if the key is not found.</returns>
		public PublishRecord GetPublishRecord(
			string key)
		{
			return PublishManager.GetPublishManager().GetPublishRecord(key);
		}


		/// <summary>
		/// Gets the PublishRecord for the given key, creating a new PublishRecord if one does
		/// not already exist for the given key and addIfNotPresent is true.
		/// </summary>
		/// <param name="key">The key to get the PublishRecord for.</param>
		/// <param name="context">The context of the originating request.</param>
		/// <param name="addIfNotPresent">If <b>true</b> a  new PublishRecord is created if
		///			one is not found matching the given key.</param>
		/// <returns>The PublishRecord for the given key.</returns>
		public PublishRecord GetPublishRecord(
			string key,
			TransitionContext context,
			bool addIfNotPresent)
		{
			return PublishManager.GetPublishManager().GetPublishRecord(key, context, this.contentPublisher, addIfNotPresent);
		}


		/// <summary>
		/// Publishes the given content for the given context.
		/// </summary>
		/// <param name="content">The content to publish.</param>
		/// <param name="context">The context of the request.</param>
		/// <returns>A <b>string</b> containting the content to return to the calling client.</returns>
		public string Publish(
			object content,
			TransitionContext context)
		{
			return this.contentPublisher.Publish(content, context, this);
		}


		/// <summary>
		/// Factory method for constructing Publishers.
		/// </summary>
		/// <param name="name">The name of the publisher to make.</param>
		/// <returns>A <b>Publisher</b> of the named type.</returns>
		private static Publisher MakePublisher(
			string name)
		{
			Publisher publisher = null;

			PublishConfigSection config = ConfigurationManager.GetSection(
			                              	"controllerSettings/publishing") as PublishConfigSection;

			if (config.Publish) {
				//  make sure we found the publishing section and publisher definition
				//  for the request publisher
				if (config == null) {
					throw new ConfigurationErrorsException("No publishing section found in config.");
				}
				if (config.Publishers == null) {
					throw new ConfigurationErrorsException("No publishers identified in config.");
				}
				if (config.Publishers[name] == null) {
					throw new ConfigurationErrorsException("No publisher identified in config for: " + name);
				}

				//  instantiate the publisher
				publisher = new Publisher(name);

				//  get the handle (ContentPublisher) for the publisher
				string handlerClass = config.Publishers[name].Handler;

				if (handlerClass != null) {
					try {
						publisher.contentPublisher = (ContentPublisher) Activator.CreateInstance(Type.GetType(handlerClass));
					} catch (Exception) {}
				}
			}

			return publisher;
		}
	}
}