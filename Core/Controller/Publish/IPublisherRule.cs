using System;
using System.Collections.Generic;

namespace Triton.Controller.Publish {

#region History

// History:

#endregion

/// <summary>
/// Interface for defining a rule for the page publishing system.
/// A publisher rule allows extending of the rules for when published
/// content gets used to fulfill a request and when content should be
/// published for future use.
/// </summary>
///	<author>Scott Dyke</author>
///	<created>3/21/11</author>
public interface IPublisherRule
{

	/// <summary>
	/// Gets or sets the name of the <c>IPublisherRule</c>.
	/// </summary>
	string Name
	{
		get;
		set;
	}


	/// <summary>
	/// Determines if published content should be used to fulfill the
	/// request indicated by the given <c>TransitionContext</c>.
	/// </summary>
	/// <param name="context">The <c>TransitionContext</c> to determine if
	///		published content should be used for.</param>
	/// <returns><c>True</c> if published content should be used, <c>false</c> if not.</returns>
	bool UsePublishedContent(
		TransitionContext context);


	/// <summary>
	/// Determines if the content generated to fulfill the given request
	/// should be published for use in fulfilling future requests.
	/// </summary>
	/// <param name="context">The <c>TransitionContext</c> to determine if
	///		content should be published for.</param>
	/// <returns><c>True</c> if the content should be published, <c>false</c> if not.</returns>
	bool ShouldBePublished(
		TransitionContext context);
}
}
