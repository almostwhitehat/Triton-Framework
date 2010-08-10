namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>StopState</b> identifies the end of a series of state transitions for a request.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class StopState : BaseState, EndState, PublishableState
	{
		private string section;


		/// <summary>
		/// Constructs a new <b>StopState</b> with the specified id and name.
		/// </summary>
		/// <param name="id">The ID of the state.</param>
		/// <param name="name">The name of the state.</param>
		public StopState(
			long id,
			string name) : base(id, name) {}


		/// <summary>
		/// Constructs a new <b>StopState</b> with the specified values.
		/// </summary>
		/// <param name="id">The ID of the state.</param>
		/// <param name="name">The name of the state.</param>
		/// <param name="site">The site to which the state belongs.</param>
		/// <param name="publish">Boolean indicating whether or not the state should be published</param>
		/// <param name="keyExcludeParams">The names of any parameters to be excluded from the publish key.</param>
		public StopState(
			long id,
			string name,
			string site,
			bool publish,
			string keyExcludeParams) : base(id, name)
		{
			this.Site = site;
			this.Publish = publish;
			this.PublishExcludeParams = keyExcludeParams;
		}


		/// <summary>
		/// Gets the type of the state.
		/// </summary>
		//public override StateType Type
		//{
		//    get {
		//        return StateType.STOP;
		//    }
		//}

		#region PublishableState Members

		/// <summary>
		/// Gets a boolean value indicating whether or not the state is set
		/// to publish its content.
		/// </summary>
		public bool Publish { get; internal set; }


		/// <summary>
		/// Gets a comma-delimited list of parameter names to be excluded
		/// from the parameters that uniquely identify the content generated
		/// by the state.
		/// </summary>
		public string PublishExcludeParams { get; internal set; }


		/// <summary>
		/// Gets the site code the state is for.
		/// </summary>
		public string Site { get; internal set; }


		public string Section
		{
			get { return "XML"; //this.section;
			}
		}


		/// <summary>
		/// Gets the base file name the published content is to be saved to.
		/// </summary>
		public string BaseFileName
		{
			get { return ".xml"; }
		}

		#endregion
	}
}