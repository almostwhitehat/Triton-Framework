namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>StartState</b> interface must be "implemented" by any concrete 
	/// state that can be the starting point of a series of tranistions/state processings.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface StartState : IState {}
}