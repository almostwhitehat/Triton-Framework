using Triton.Controller.Format;

namespace Triton.Controller.Action
{

	#region History

	// History:
	// 08/12/2009	GV	Rename of BizAction back to Action

	#endregion

	/// <summary>
	/// <b>FormatableAction</b> is an abstract class that implements support for
	/// associating a <b>Formatter</b> with an Action.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class FormatableAction : IAction
	{
		/// <summary>
		/// Gets or sets the <b>Formatter</b> to be used to format the results
		/// of executing the action.
		/// </summary>
		public Formatter Formatter { get; set; }

		#region Action Members

		public abstract string Execute(TransitionContext context);

		#endregion
	}
}