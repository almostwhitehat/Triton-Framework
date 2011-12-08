using System;

namespace Triton.Controller.StateMachine {

#region History

// History:

#endregion

/// <summary>
/// Defines a prerequisite for a <c>State</c>.
/// </summary>
///	<author>Scott Dyke</author>
///	<created>11/18/11</created>
public class StatePrerequisite
{

	/// <summary>
	/// Gets or sets the (optional) name of prerequisite. 
	/// </summary>
	public string Name
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the start state ID of prerequisite. 
	/// </summary>
	public long StartStateId
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the starting event of prerequisite. 
	/// </summary>
	public string StartEvent
	{
		get;
		set;
	}
}
}
