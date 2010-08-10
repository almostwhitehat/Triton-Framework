namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>IStateMachineStatesDao</b> interface defines the contract used by the state machine
	/// to load states and related information from a database.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IStateMachineStatesDao
	{
		/// <summary>
		/// Loads the states and transitions from the data source.
		/// </summary>
		/// <returns>An array of <b>State</b>s that were loaded from the data source.</returns>
		StateInfo LoadStates();
	}

}