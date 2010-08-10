namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>PageState</b> is a state in the state machine representing an aspx page.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class PageState : BaseState, StartState, EndState, PublishableState
	{
		public PageState(
			long id,
			string name) : base(id, name) {}


		public PageState(
			long id,
			string name,
			string section,
			string site,
			string targetPage,
			bool publish,
			string keyExcludeParams) : base(id, name)
		{
			this.Section = section;
			this.Site = site;
			this.Page = targetPage;
			this.Publish = publish;
			this.PublishExcludeParams = keyExcludeParams;
		}


		public string Page
		{ 
			get; 
			internal set; 
		}


		#region PublishableState Members

		public bool Publish
		{
			get;
			internal set;
		}


		public string PublishExcludeParams 
		{ 
			get; 
			internal set; 
		}


		public string Site 
		{ 
			get; 
			internal set; 
		}


		public string Section 
		{ 
			get; 
			internal set; 
		}


		/// <summary>
		/// Gets the base file name the published content is to be saved to.
		/// </summary>
		public string BaseFileName
		{
			get { 
				return this.Page + ".html"; 
			}
		}

		#endregion
	}
}