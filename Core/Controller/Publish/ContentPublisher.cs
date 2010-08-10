using System;

namespace Triton.Controller.Publish
{

	#region History

	// History:

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
	public interface ContentPublisher : IDisposable
	{
		/// <summary>
		/// Gets the name of the <b>ContentPublisher</b>.
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Publishes the given content.
		/// </summary>
		/// <param name="publishParam">An object containing the content to be published.
		///			The type of this object is determined by the associated <b>ContentProvider</b>.</param>
		/// <param name="context">The request context the content is being published for.</param>
		/// <param name="publisher">The <b>Publisher</b> that is calling this <b>ContentPublisher</b>.</param>
		/// <returns>A string containing the content to return to the client in the response.</returns>
		string Publish(
			object publishParam,
			TransitionContext context,
			Publisher publisher);


		/// <summary>
		/// Gets the unique publish key for the given context.
		/// </summary>
		/// <param name="context">The context to get a publish key for.</param>
		/// <returns>A published key unique to the request in the given context.</returns>
		string GetPublishKey(
			TransitionContext context);


		/// <summary>
		/// Determines if the published content identified by the given <b>PublishRecord</b>
		/// is expired.
		/// </summary>
		/// <param name="publishRecord">The PublishRecord to determine the expiration status of.</param>
		/// <returns><b>True</b> if the referenced content is expired, <b>false</b> if not.</returns>
		bool IsExpired(
			PublishRecord publishRecord);
	}
}