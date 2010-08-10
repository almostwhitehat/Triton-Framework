using Triton.Controller.Publish;

namespace Triton.Controller
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>SimpleContentProvider</b> is an empty <b>ContentProvider</b> that performs no
	/// formatting/rendering of results received in the context.
	/// </summary>
	///	<author>Scott Dyke</author>
	internal class SimpleContentProvider : ContentProvider
	{
		#region ContentProvider Members

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> the provider is for.</param>
		public void Init(
			TransitionContext context) {}


		/// <summary>
		/// Gets a <b>Publisher</b> for the provider.
		/// </summary>
		/// <returns>A <b>Publisher</b> for publishing HTML content.</returns>
		/// <remarks>
		/// Not yet implemented for SimpleContentProvider.
		/// </remarks>
		public Publisher GetPublisher()
		{
			return null;
		}


		/// <summary>
		/// Gets the published content to fulfill the given context's request.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> to get the published content for.</param>
		/// <returns>A <b>string</b> containing the content to be returned to the client.</returns>
		/// <remarks>
		/// Not yet implemented for SimpleContentProvider.
		/// </remarks>
		public string GetPublishedContent(
			TransitionContext context)
		{
			return null;
		}


		/// <summary>
		/// Determines if the content fulfilling the given context's request should be published.
		/// </summary>
		/// <param name="context">The context of the request the content is for.</param>
		/// <returns><b>True</b> if the content should be published, <b>false</b> if not.</returns>
		/// <remarks>
		/// Not yet implemented for SimpleContentProvider.
		/// </remarks>
		public bool ShouldBePublished(
			TransitionContext context)
		{
			return false;
		}


		/// <summary>
		/// For SimpleContentProvider, this does nothing.
		/// </summary>
		/// <param name="context">The context of the request the content is being rendered for.</param>
		/// <returns>null.</returns>
		public string RenderContent(
			TransitionContext context)
		{
			return null;
		}


		/// <summary>
		/// Renders the content to be published.
		/// </summary>
		/// <param name="context">The context of the request the content is being rendered for.</param>
		/// <returns>A <b>string</b> containing the content to be published.</returns>
		/// <remarks>
		/// Not yet implemented for SimpleContentProvider.
		/// </remarks>
		public string RenderPublishContent(
			TransitionContext context)
		{
			return null;
		}


		/// <summary>
		/// Frees resources used by the <b>HtmlContentProvider</b>.
		/// </summary>
		public void Dispose() {}

		#endregion
	}
}