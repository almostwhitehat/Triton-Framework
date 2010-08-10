namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>EndState</b> interface must be "implemented" by any
	/// concrete state that causes processing within the state machine
	/// to end and the results be returned to the client.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface EndState : IState {}
}