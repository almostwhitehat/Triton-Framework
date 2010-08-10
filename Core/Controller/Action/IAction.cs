namespace Triton.Controller.Action
{

	#region History

	// History:
	// 08/12/2009	GV	Rename of BizAction back to Action

	#endregion

	/// <summary>
	/// The <c>Action</c> interface defines the API for actions used in the state
	/// transition engine.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IAction
	{
		/// <summary>
		/// Carries out the business actions performed by the <c>Action</c>.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> the action is being
		///			performed in.</param>
		/// <returns>A string containing the event resulting from the Action.</returns>
		string Execute(
			TransitionContext context);
	}
}