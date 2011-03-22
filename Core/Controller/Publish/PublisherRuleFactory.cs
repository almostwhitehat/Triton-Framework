using System;
using System.Collections.Generic;

namespace Triton.Controller.Publish {

#region History

// History:

#endregion

/// <summary>
/// Factory class for making <c>IPublisherRule</c>s.
/// </summary>
///	<author>Scott Dyke</author>
///	<created>3/21/11</author>
public class PublisherRuleFactory
{


	/// <summary>
	/// Creates the <c>IPublisherRule</c> for the given name and type.
	/// </summary>
	/// <param name="ruleName">The name of the rule.</param>
	/// <param name="typeClass">The type of the class.</param>
	/// <returns></returns>
	public static IPublisherRule Make(
		string ruleName,
		string typeClass)
	{
		IPublisherRule rule;
		Type type = null;

		try {
			type = Type.GetType(typeClass);

			if (type == null) {
				throw new TypeInitializationException(typeClass, null);
			}

			rule = (IPublisherRule)Activator.CreateInstance(type);
			rule.Name = ruleName;

		} catch (Exception) {
			// TODO: log it
			throw;
		}

		return rule;
	}
}
}
