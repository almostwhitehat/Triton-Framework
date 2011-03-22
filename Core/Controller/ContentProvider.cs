using System;
using Triton.Controller.Publish;

namespace Triton.Controller {

#region History

// History:
//  3/21/11 SD	Moved ShouldBePublished from here to IContentPublisher.

#endregion

/// <summary>
/// Defines the contract that the framework implements to obtain content to return to
/// the client in response to an incoming request.
/// </summary>
/// <remarks>
/// The <b>ContentProvider</b> interface is implemented to provide the framework with
/// content for a specific type of request.
/// </remarks>
///	<author>Scott Dyke</author>
public interface ContentProvider : IDisposable
{
	/// <summary>
	/// Performs initialization tasks for the provider.
	/// </summary>
	/// <param name="context">The <b>TransitionContext</b> the provider is for.</param>
	void Init(
		TransitionContext context);


	/// <summary>
	/// Gets a <b>Publisher</b> for the provider.
	/// </summary>
	/// <returns>A <b>Publisher</b> for publishing content.</returns>
	Publisher GetPublisher();


	/// <summary>
	/// Gets the published content to fulfill the given context's request.
	/// </summary>
	/// <param name="context">The <b>TransitionContext</b> to get the published content for.</param>
	/// <returns>A <b>string</b> containing the content to be returned to the client.</returns>
	string GetPublishedContent(
		TransitionContext context);


	/// <summary>
	/// Renders the content to be returned to the client.
	/// </summary>
	/// <param name="context">The context of the request the content is being rendered for.</param>
	/// <returns>A <b>string</b> containing the content to be returned to the client.</returns>
	string RenderContent(
		TransitionContext context);


	/// <summary>
	/// Renders the content to be published.
	/// </summary>
	/// <param name="context">The context of the request the content is being rendered for.</param>
	/// <returns>A <b>string</b> containing the content to be published.</returns>
	string RenderPublishContent(
		TransitionContext context);
}
}