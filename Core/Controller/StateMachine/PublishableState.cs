namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>PublishableState</b> interface defines a State that produces
	/// content that can be published.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface PublishableState : IState
	{
		/// <summary>
		/// Gets a boolean value indicating whether or not the state is set
		/// to publish its content.
		/// </summary>
		bool Publish { get; }


		/// <summary>
		/// Gets a comma-delimited list of parameter names to be excluded
		/// from the parameters that uniquely identify the content generated
		/// by the state.
		/// </summary>
		string PublishExcludeParams { get; }


		/// <summary>
		/// Gets the site code the state is for.
		/// </summary>
		string Site { get; }


		/// <summary>
		/// Gets the section the state is for.
		/// </summary>
		string Section { get; }


		/// <summary>
		/// Gets the base file name the published content is to be saved to.
		/// </summary>
		string BaseFileName { get; }


		/// <summary>
		/// Gets the content to be published by the paublisher.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> containing the context information
		///			about the request whose results are being published.</param>
//	string GetPublishContent(Triton.Controller.TransitionContext	context);
	}
}