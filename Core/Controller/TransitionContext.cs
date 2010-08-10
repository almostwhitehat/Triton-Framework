using Triton.Controller.Config;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;
using Triton.Utilities;

namespace Triton.Controller
{
	/// <summary>
	/// <b>TransitionContext</b> contains context information about the request and
	/// transition being processed by the state machine.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class TransitionContext
	{
		private readonly MvcRequest request;
		private readonly string startEvent;
		private readonly IState startState;
		private IState currentState;
		private string site;
		private int? version = null;


		/// <summary>
		/// Constructs a <c>TransitionContext</c> for the given starting state and
		/// site.
		/// </summary>
		/// <param name="startState">The starting state of the context.</param>
		/// <param name="startEvent">The start event of the context.</param>
		/// <param name="site">The site of the context.</param>
		/// <param name="request">The <c>MvcRequest</c>.</param>
		public TransitionContext(
			IState startState,
			string startEvent,
			string site,
			MvcRequest request)
		{
			this.startState = startState;
			this.currentState = startState;
			this.site = site;
			this.startEvent = startEvent;
			this.request = request;
			this.version = SitesConfig.GetInstance().GetSiteVersion(this.site);
		}


//	~TransitionContext()
//	{
//		Triton.Logging.Logger.GetLogger("ControllerLogger").Status("TransitionContext destructing");
//	}


		/// <summary>
		/// Gets the state that was the starting state of the context.
		/// </summary>
		public IState StartState
		{
			get { 
				return this.startState; 
			}
		}


		/// <summary>
		/// Gets the state that is the end PageState of the transition sequence.
		/// </summary>
		public IState EndState 
		{ 
			get; 
			set; 
		}


		/// <summary>
		/// Gets the state that is currently being processed.
		/// </summary>
		public IState CurrentState
		{
			get { 
				return this.currentState; 
			}
		}


		/// <summary>
		/// Gets the starting event for the context.
		/// </summary>
		internal string StartEvent
		{
			get { 
				return this.startEvent; 
			}
		}


		/// <summary>
		/// Gets the site code of the site the context is for.
		/// </summary>
		public string Site
		{
			get { 
				return this.site; 
			}
			set { 
				this.site = value; 
			}
		}


		/// <summary>
		/// Gets the version of the site the context is for.
		/// </summary>
		public int? Version
		{
			get { 
				return this.version; 
			}
			internal set { 
				this.version = value; 
			}
		}


		/// <summary>
		/// Gets or sets the publish key used by the publishing system.
		/// </summary>
		public string PublishKey 
		{ 
			get; 
			set; 
		}


		/// <summary>
		/// Gets the MvcRequest associated with the TransitionContext.
		/// </summary>
		public MvcRequest Request
		{
			get { 
				return this.request; 
			}
		}


		/// <summary>
		/// Sets the state that is currently being processed.
		/// </summary>
		/// <param name="curState">The state that is currently being processed.</param>
		internal void SetCurrentState(
			IState curState)
		{
			this.currentState = curState;
		}
	}
}