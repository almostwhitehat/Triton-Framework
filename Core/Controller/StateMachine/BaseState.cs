using System.Collections;
using System.Collections.Specialized;
using Triton.Utilities;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	//  11/18/11	SD	Added support for Prerequisite.

	#endregion

	/// <summary>
	/// <b>BaseState</b> is the base, abstract implementation for states in the
	/// state machine used by the controller.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class BaseState : IState
	{
		protected NameValueCollection attributes;
		protected long id;
		protected string name;
		protected Hashtable transitions = new Hashtable();
		protected string type;
		protected StatePrerequisite[] prerequisite = null;


		public BaseState(
			long id,
			string name)
		{
			this.id = id;
			this.name = name;
		}


		public NameValueCollection Attributes
		{
			get {
				return this.attributes;
			}
		}


		#region State Members

		public string Type
		{
			get {
				return this.type;
			}
		}


		public string this[string key]
		{
			get {
				return ((this.attributes == null) ? null : this.attributes[key]);
			}
		}


		public long Id
		{
			get {
				return this.id;
			}
		}


		public string Name
		{
			get {
				return this.name;
			}
		}


		public bool HasPrerequisite
		{
			get {
				return ((prerequisite != null) && (prerequisite.Length > 0));
			}
		}


		public StatePrerequisite[] Prerequisite
		{
			get {
				return prerequisite;
			}
			internal set {
				prerequisite = value;
			}
		}


		public virtual Transition GetTransition(
			string name)
		{
			return (name == null) ? null : (Transition)this.transitions[name];
		}


		/// <summary>
		/// Carries out the actions to be performed by the state.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> the state is being
		///			executed in.</param>
		/// <returns>The event resulting from the execution of the state.</returns>
		public virtual string Execute(
			TransitionContext context)
		{
			return null;
		}

		#endregion


		/// <summary>
		/// For internal use only.  Do not use.
		/// </summary>
		/// <param name="stateType"></param>
		public void SetType(
			string stateType)
		{
			this.type = stateType;
		}


		public void AddTransition(
			Transition trans)
		{
			this.transitions.Add(trans.Name, trans);
		}


		public void AddAttribute(
			string name,
			string val)
		{
			if (this.attributes == null) {
				this.attributes = new NameValueCollection();
			}

			this.attributes.Add(name, val);
		}
	}
}