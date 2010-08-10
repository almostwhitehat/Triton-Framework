using System.Collections.Specialized;
using Triton.Controller.Action;
using Triton.Controller.Format;
using Triton.Utilities;
using Triton.Utilities.Reflection;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	// 12/10/2009	GV	Changed the attribute setting to use Deserialize

	#endregion

	/// <summary>
	/// <b>ActionState</b> is a state in the state machine system the carries out
	/// some unit of business logic.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ActionState : BaseState
	{
		//  the name of the Action to carry out the state's action(s)
		private NameValueCollection stateParams;


		public ActionState(
			long id,
			string name) : base(id, name) {}


		public ActionState(
			long id,
			string name,
			string actionName,
			string formatterName,
			string parameters) : base(id, name)
		{
			this.Action = actionName;
			this.Formatter = formatterName;

			this.SetParameters(parameters);
		}


		public string Action { get; internal set; }


		public string Formatter { get; internal set; }


		public NameValueCollection Parameters
		{
			get { return this.stateParams; }
		}


		/// <summary>
		/// Sets the parameters that can be used by the <b>Action</b> when it is executed.
		/// </summary>
		/// <param name="parameters">A bar ("|") delimited list of parameters and values
		///			to be set.  For example: "id=123|name=test".</param>
		public void SetParameters(
			string parameters)
		{
			// TODO: Read the delimiter from a config file?
			if (parameters != null) {
				this.stateParams = StringUtilities.StringToCollection(parameters, '|');
			}
		}


		/// <summary>
		/// Builds the <c>Action</c> associated with the state and executes it.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> the state is being
		///			executed in.</param>
		/// <returns>The event resulting from the execution of the associated <c>Action</c>.</returns>
		public override string Execute(
			TransitionContext context)
		{
			string result = null;
			IAction action = ActionFactory.Make(this.Action);

			//  make sure the action was created
			if (action != null) {
				//  if we need a formatter, create it
				if ((action is FormatableAction) && (this.Formatter != null)) {
					((FormatableAction)action).Formatter = FormatterFactory.Make(this.Formatter, context.Request);
				}

				//  copy any attributes from the state to the action
				if (Attributes != null) {
					ReflectionUtilities.Deserialize(action, Attributes);
				}

				//  execute the action
				result = action.Execute(context);
			}

			return result;
		}
	}
}