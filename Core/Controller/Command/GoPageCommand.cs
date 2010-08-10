using Triton.Controller.Request;

namespace Triton.Controller.Command
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Summary description for GetPageCommand.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class GoPageCommand : RedirectingCommand
	{
		protected override void OnExecute(
			MvcRequest request)
		{
//		PageTransitionManager ptm = PageTransitionManager.GetInstance();
//
//		PageState pg = ptm.GetStateByTargetPage(context.Request["p"], this.Section, this.Site);
		}
	}
}