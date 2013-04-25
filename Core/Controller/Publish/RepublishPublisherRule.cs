using System;
using System;
using System.Linq;
using System.Web;
using Triton.Controller;
using Triton.Controller.Publish;

namespace Triton.Controller.Publish {


/// <summary>
/// <c>IPublisherRule</c> to force republish of content if the
/// "repub" request parameter is set.
/// </summary>
///	<author>Scott Dyke</author>
///	<created>4/16/13</author>
public class RepublishPublisherRule : IPublisherRule
{
	private const string REPUB_PARAM_NAME	= "repub";


	#region IPublisherRule Members

	public string Name
	{
		get;
		set;
	}


	/// <summary>
	/// Determines if published content should be used.  For <c>RepublishPublisherRule</c>
	/// checks the repub parameter and if set directs the publishing system to not
	/// use existing published content.
	/// </summary>
	/// <param name="context">The <c>TransitionContext</c> to determine if
	///		published content should be used for.</param>
	/// <returns><c>True</c> if published content should be used, <c>false</c> if not.</returns>
	public bool UsePublishedContent(
		TransitionContext context)
	{
		string repub = context.Request[REPUB_PARAM_NAME];

		return !((repub != null) && ((repub == "1") || (repub.ToLower() == "true")));
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
		string repub = context.Request[REPUB_PARAM_NAME];

		if (repub != null) {
			publish = ((repub == "1") || (repub.ToLower() == "true"));
		}

		return publish;
	}

	#endregion
}
}
