namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	//  11/18/11	SD	Added support for Prerequisite.

	#endregion

	/// <summary>
	/// <b>State</b> defines the interface for states in the state machine used by the
	/// controller to manager flow through the system.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IState
	{
		/// <summary>
		/// Indexer to get attributes about the State.
		/// </summary>
		/// <param name="key">The name of the attribute to get.</param>
		/// <returns>The value of the specified attribute.</returns>
		string this[string key]
		{
			get;
		}


		/// <summary>
		/// Gets the type of the state.
		/// </summary>
		string Type
		{
			get;
		}


		/// <summary>
		/// Gets the ID of the state.
		/// </summary>
		long Id
		{
			get;
		}


		/// <summary>
		/// Gets the name of the state.
		/// </summary>
		string Name
		{
			get;
		}


		/// <summary>
		/// Gets a flag indicating whether or no the state has prerequisites.
		/// </summary>
		bool HasPrerequisite
		{
			get;
		}


		/// <summary>
		/// Gets the prerequisite(s) for the state.
		/// </summary>
		StatePrerequisite[] Prerequisite
		{
			get;
		}


		/// <summary>
		/// Gets a <b>Transition</b> from the state with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Transition GetTransition(
			string name);


		/// <summary>
		/// Carries out the actions to be performed by the state.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> the state is being
		///			executed in.</param>
		/// <returns>The event resulting from the execution of the state.</returns>
		string Execute(
			TransitionContext context);
	}
}