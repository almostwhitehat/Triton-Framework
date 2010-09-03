using System;
using System.Configuration;
using System.IO;
using Common.Logging;
using Triton.Controller.Publish;
using Triton.Controller.StateMachine;
using Triton.Support.Request;

namespace Triton.Controller {

#region History

// History:
// 06/02/2009	KP  Changed the logging to Common.logging.
// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
// 11/16/2009	GV	Added a check for not executing if the target page is null, added throw in GetPublishedContent
// 12/01/2009	GV	Changed the pageXmlFile string to come from CoreItemNames.
// 9/3/2010		SD	Added catch for ThreadAbortException when transferring to a published page to
//					ignore the ThreadAbortException caused by Transfer.

#endregion

/// <summary>
/// <b>HtmlContentProvider</b> is the default <b>ContentProvider</b> for providing
/// HTML content in response to incoming requests.
/// </summary>
///	<author>Scott Dyke</author>
public class HtmlContentProvider : ContentProvider
{
	protected TransitionContext context;
	//  ?? Do we want to keep a publisher associated with the
	//  Provider, or get one from the forthcoming pool each time?
	protected Publisher publisher;

	#region ContentProvider Members

	/// <summary>
	/// Initializes the provider.
	/// </summary>
	/// <param name="context">The <b>TransitionContext</b> the provider is for.</param>
	public virtual void Init(
		TransitionContext context)
	{
		this.context = context;
	}


	/// <summary>
	/// Gets a <b>Publisher</b> for the provider.
	/// </summary>
	/// <returns>A <b>Publisher</b> for publishing HTML content.</returns>
	public virtual Publisher GetPublisher()
	{
		return this.GetPublisher(this.context);
	}


	/// <summary>
	/// Gets the published content to fulfill the given context's request.
	/// </summary>
	/// <param name="context">The <b>TransitionContext</b> to get the published content for.</param>
	/// <returns>A <b>string</b> containing the content to be returned to the client.</returns>
	public virtual string GetPublishedContent(
		TransitionContext context)
	{
		Publisher publisher = GetPublisher(context);

		if (publisher != null) {
			PublishRecord pubRec = publisher.GetPublishRecord(context.PublishKey);

			if (pubRec == null) {
				throw new PublishException(context,  pubRec,
						string.Format("No PublishRecord found for {0}.", context.PublishKey));
			}

			try {
				pubRec.HitCount++;
				context.Request.Transfer(pubRec.PublishedPath);
			} catch (System.Threading.ThreadAbortException) {
				// ignore ThreadAbortException, caused by Transfer
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("Could not GetPublishedContent."), e);

						//  rethrow the error so that the application error handler can handle it
				throw new PublishException(context, pubRec,
						"Could not transfer the control to the published page.", e);
			}
		}

		return null;
	}


	/// <summary>
	/// Determines if the content fulfilling the given context's request should be published.
	/// </summary>
	/// <param name="context">The context of the request the content is for.</param>
	/// <returns><b>True</b> if the content should be published, <b>false</b> if not.</returns>
	public virtual bool ShouldBePublished(
		TransitionContext context)
	{
		PublishConfigSection config = ConfigurationManager.GetSection(
		                              	"controllerSettings/publishing") as PublishConfigSection;

		//  is the EndState a PublishableState and is publish set to true
		return (config.Publish && (context.EndState is PublishableState) && ((PublishableState)context.EndState).Publish);
	}


	/// <summary>
	/// Renders the HTML content to be returned to the client.
	/// </summary>
	/// <param name="context">The context of the request the content is being rendered for.</param>
	/// <returns>A <b>string</b> containing the content to be returned to the client.</returns>
	public virtual string RenderContent(
		TransitionContext context)
	{
		string content = null;
		PageFinder pageFinder = PageFinder.GetInstance();
		PageState target = (PageState)context.EndState;

		if (target == null) {
			throw new ApplicationException(
				string.Format("End state was not set, check the states configuration. Start state: {0}{1}. Start event: {2}.",
				              context.StartState.Id,
				              string.IsNullOrEmpty(context.StartState.Name) ? "" : " [" + context.StartState.Name + "]",
				              context.StartEvent));
		}

				//  find the aspx file for the target page
		PageFinder.FileRecord fileRec = pageFinder.FindPage(target.Page, target.Section, target.Site);

				//  find the xml file for the target page
		PageFinder.FileRecord xmlRec = pageFinder.FindXml(target.Page, target.Section, target.Site);

				//  put the XML file info into the request so the page can get it
		if (xmlRec != null) {
			context.Request.Items[CoreItemNames.PAGE_XML_FILE_RECORD] = xmlRec;
		}

		if (fileRec == null || string.IsNullOrEmpty(fileRec.fullPath)) {
			throw new FileNotFoundException("Could not find the page to Execute.");
		}

		try {
			int version;
			if (int.TryParse(fileRec.version, out version)) {
				context.Version = version;
			}

			context.Request.Version = fileRec.version;

			content = context.Request.Execute(fileRec.fullPath);
			//context.Request.Transfer(fileRec.fullPath);
		} catch (Exception e) {
			LogManager.GetCurrentClassLogger().Error(
				errorMessage => errorMessage("RenderContent: "), e);

			//rethrow the error so that the application error handler can handle it
			throw;
		}

		return content;
	}


	/// <summary>
	/// Renders the content to be published.
	/// </summary>
	/// <param name="context">The context of the request the content is being rendered for.</param>
	/// <returns>A <b>string</b> containing the content to be published.</returns>
	public virtual string RenderPublishContent(
		TransitionContext context)
	{
		return this.RenderContent(context);
	}


	/// <summary>
	/// Frees resources used by the <b>HtmlContentProvider</b>.
	/// </summary>
	public virtual void Dispose()
	{
		if (this.publisher != null) {
			this.publisher.Dispose();
			this.publisher = null;
		}
	}

	#endregion

	/// <summary>
	/// Gets a <b>Publisher</b> for the <b>HtmlContentProvider</b>.
	/// </summary>
	/// <param name="context">The context of the request the Publisher is for.</param>
	/// <returns>A <b>Publisher</b> for publishing HTML content.</returns>
	private Publisher GetPublisher(
		TransitionContext context)
	{
		if (this.publisher == null) {
			this.publisher = Publisher.GetPublisher("html");
		}

		return this.publisher;
	}
}
}