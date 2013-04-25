using System;
using System;
using System.Linq;
using System.Web;
using Triton.Controller;
using Triton.Controller.Publish;

namespace Triton.Controller.Publish {


/// <summary>
/// <c>IPublisherRule</c> to force use of dynamic of content if the
/// "dyn" request parameter is set, regardless of whether or not
/// the is published content for the request.
/// </summary>
///	<author>Scott Dyke</author>
///	<created>4/22/13</author>
public class DynamicPublisherRule : IPublisherRule
{
	private const string DYNAMIC_PARAM_NAME	= "dyn";


	#region IPublisherRule Members

	public string Name
	{
		get;
		set;
	}


	/// <summary>
	/// Determines if published content should be used.  For <c>DynamicPublisherRule</c>
	/// checks the dyn parameter and if set directs the publishing system to not
	/// use existing published content.
	/// </summary>
	/// <param name="context">The <c>TransitionContext</c> to determine if
	///		published content should be used for.</param>
	/// <returns><c>True</c> if published content should be used, <c>false</c> if not.</returns>
	public bool UsePublishedContent(
		TransitionContext context)
	{
		string dyn = context.Request[DYNAMIC_PARAM_NAME];

		return !((dyn != null) && ((dyn == "1") || (dyn.ToLower() == "true")));
	}


	/// <summary>
	/// Determines if the content generated to fulfill the given request
	/// should be published for use in fulfilling future requests.
	/// </summary>
	/// <param name="context">The <c>TransitionContext</c> to determine if
	///		content should be published for.</param>
	/// <returns><c>True</c> if the content should be published, <c>false</c> if not.</returns>
	public bool ShouldBePublished(
		TransitionContext context)
	{
		bool publish = true;
		string dyn = context.Request[DYNAMIC_PARAM_NAME];

		if (dyn != null) {
			publish = !((dyn == "1") || (dyn.ToLower() == "true"));
		}

		return publish;
	}

	#endregion
}
}
