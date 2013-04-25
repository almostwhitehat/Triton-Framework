using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using Common.Logging;
using Triton.Configuration;

namespace Triton.Controller.Publish {

#region History

// History:
//  3/21/2011	SD	Added UsePublishedContent and ShouldBePublished methods to
//					encapsulate the existing checks along with support for the
//					new publisher rule checks.
//					In MakePublisher, added creation of asociated publisher rules.

#endregion

/// <summary>
/// <b>Publisher</b> is a facade for the content publishing system.  All requests to the
/// publishing systems are made via the <b>Publisher</b>.  
/// </summary>
///	<author>Scott Dyke</author>
public class Publisher : IDisposable
{
	private readonly string name;
	private IContentPublisher contentPublisher;


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
		get {
			return this.name;
		}
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
	/// Determines whether or not previously published content should be used
	/// to filfull the given request.
	/// </summary>
	/// <param name="key">The publish key of the request.</param>
	/// <param name="context">The <c>TransitionContext</c> to determine published
	///		content use for.</param>
	/// <returns><c>True</c> if published content should be used, <c>false</c> if not.</returns>
	public bool UsePublishedContent(
		string key,
		TransitionContext context)
	{
		bool usePublished = IsPublished(key);

				//  if the IsPublished check passed and the contentPublisher has rules
				//  check to see if any of the rules prevent use of the published content
		if (usePublished && contentPublisher.HasRules) {
			usePublished = !contentPublisher.Rules.Any(rule => !rule.UsePublishedContent(context));
		}

		return usePublished;
	}


	/// <summary>
	/// Determines if the content fulfilling the given context's request should be published.
	/// </summary>
	/// <param name="context">The context of the request the content is for.</param>
	/// <returns><b>True</b> if the content should be published, <b>false</b> if not.</returns>
	public bool ShouldBePublished(
		TransitionContext context)
	{
		bool publish = contentPublisher.ShouldBePublished(context);

		if (publish && contentPublisher.HasRules) {
			publish = !contentPublisher.Rules.Any(rule => !rule.ShouldBePublished(context));
		}

		return publish;
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
				TritonConfigurationSection.SectionName + "/publishing") as PublishConfigSection;

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

					//  get the handler (ContentPublisher) for the publisher
			string handlerClass = config.Publishers[name].Handler;

			if (handlerClass != null) {
				try {
					publisher.contentPublisher = (IContentPublisher)Activator.CreateInstance(Type.GetType(handlerClass));

							//  if there are IPublisherRules defined for the publisher, create the rules
					if ((config.Publishers[name].Rules != null) && (config.Publishers[name].Rules.Count > 0)) {
						foreach (PublisherRule ruleConfig in config.Publishers[name].Rules) {
							try {
								IPublisherRule rule = PublisherRuleFactory.Make(ruleConfig.Name, ruleConfig.Class);
								if (publisher.contentPublisher.Rules == null) {
									publisher.contentPublisher.Rules = new List<IPublisherRule>();
								}
								publisher.contentPublisher.Rules.Add(rule);
							} catch (Exception e) {
								LogManager.GetCurrentClassLogger().Error(msg => msg(string.Format(
										"Publisher : Error occurred creating the publisher rule {0} with class {1}.",
										ruleConfig.Name, ruleConfig.Class)), e);
							}
						}
					}
				} catch (Exception) {}
			}
		}

		return publisher;
	}
}
}