using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;
using Triton.Logic.Support;
using Triton.Utilities;

namespace Triton.Logic
{

	#region History

	// History:
	// 05/20/2009	KP	Changed the Logging to use Common.Logging
	// 08/12/2009	GV	Rename of BizAction to IAction
	// 09/20/2009	GV	Moved to Logic namespace
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	// 11/12/2009	GV	Added subclass of events, for use for returning events.

	#endregion

	/// <summary>
	/// <b>SetParameterAction</b> is an <c>Action</c> that sets the value of one or more request parameters.
	/// The name(s) and value(s) of the parameters to be set are defined in the corresponding state's
	/// <c>parameters</c> and/or <c>defaults</c> attributes.<p/>
	/// </summary>
	/// <remarks>
	/// The format of the <c>parameters</c> and <c>defaults</c> attributes is: <c>param1=val1|param2=val2...</c><br/>
	/// where<p/>
	///  <c>param1, param2</c>, etc. are the name(s) of the parameter(s) to set the values of, and<br/>
	///  <c>val1, val2</c>, etc. are the corresponding value(s) to be assigned to the parameter(s).<p/>
	/// The pipe (|) delimits different parameters.<p/>
	/// SetParameterAction also supports concatenating of values to a parameter's existing value, and
	/// dynamic parameter values.<p/>
	/// Concatenation of a value is signified using the "+=" operator instead of "=".<p/>
	/// A dynamic value is specified by placing the name of another request parameter within
	/// '[' and ']' delimters.  The value assigned to the parameter specified for SetParameterAction
	/// will be the value of the parameter whose name is included in the value section.<p/>
	/// <b>Examples:</b><p/>
	/// <c>param1=1</c> sets the parameter "param1" to the value "1".<br/>
	/// <c>param1=1|param2=abc</c> sets the parameter "param1" to the value "1" and parameter "param2"
	///			to the value "abc".<br/>
	///	<c>param1+=xyz</c> appends "xyz" to the current value of "param1".<br/>
	///	<c>param1+=xyz|param2+=[param1]*abc</c>, assuming "param1" had a value of "1" coming in, sets
	///			"param1" to "1xyz" and "param2" to "1xyz*abc"<br/>
	///	<p/>
	/// The <c>defaults</c> attribute acts like the <c>parameters</c> except it only set the parameter
	/// value(s) if there is not already a value for the parameter.
	/// <p/>
	///	<b>Notes:</b><p/>
	///	<list type="unordered">
	///	<item>A parameter may appear only once as parameter to set in a list of pipe-delimited parameters.</item>
	///	</list>
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class SetParameterAction : IAction
	{
		private const string DEFAULTS_ATTRIBUTE = "defaults";

		/// <summary>
		/// The regular expression used to parse parameter value into literal and dynamic
		/// components if it contains dynamic delimiters ([]).
		/// </summary>
		private const string PARAM_VALUE_REGEX = @"[^\[]+|\[[^\]]+\]";

		#region IAction Members

		public virtual string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				NameValueCollection paramList = ((ActionState)context.CurrentState).Parameters;
				NameValueCollection defaultList;
				MvcRequest req = context.Request;
				List<Param> parms = new List<Param>();

				//  add the parameters from the State's Parameters collection to the list of
				//  parameters to set in the Request
				if (paramList != null) {
					for (int i = 0; i < paramList.Count; i++) {
						parms.Add(new Param(paramList.GetKey(i), paramList.Get(i), false));
					}
				}

				//  if the State has attributes, and a value for the "defaults" attribute
				//  add the default parameters to the list of parameters to set in the Request
				if ((((ActionState)context.CurrentState).Attributes != null)
				    && (((ActionState)context.CurrentState).Attributes[DEFAULTS_ATTRIBUTE] != null)) {
					defaultList =
						StringUtilities.StringToCollection(((ActionState)context.CurrentState).Attributes[DEFAULTS_ATTRIBUTE], '|');

					for (int i = 0; i < defaultList.Count; i++) {
						parms.Add(new Param(defaultList.GetKey(i), defaultList.Get(i), true));
					}
				}

				for (int i = 0; i < parms.Count; i++) {
					Param parm = parms[i];
					//  get the name and value of the parameter to set
					string paramName = parm.Name;
					string paramVal = parm.Value;

// TODO: what if the value is null???
					//  set parameter only when the value is not null
					//  and either it's not a default we're setting or the current param value is null
					if ((paramVal != null) && (!parm.IsDefault || req[paramName] == null)) {
						//-----------------------------------------------------------
						//			Special Rules For Parameter Values
						//  1. If the name/value operator is "+=" it means concatenate
						//	   the given value to the existing value.
						//  2. If the value contains "[" and "]" it means the value in between
						//	   is the name of a request parameter whose value is to be used.
						//-----------------------------------------------------------

						//  set flag: by default do not concatinate parameter value
						bool concatinate = false;
						//  check if the parameter needs to be concatinated
						//  concatination format is "name+=value" and they are split on "="
						//  so if name ends with "+", it's a concatination
						if (paramName.EndsWith("+")) {
							//  set flag: parameter value needs to be concatinated
							concatinate = true;
							//  truncate the "+" from the name
							paramName = paramName.Remove(paramName.Length - 1, 1);
						}

						//  if the value contains '[' and ']', replace it with the value of the 
						//  parameter whose name is between the [ and ]
						if (paramVal.IndexOf('[') >= 0) {
							//  get the literal and dynamic pieces of the given value
							//  (dynamic pieces are those in [ ])
							MatchCollection valueParts = Regex.Matches(paramVal, PARAM_VALUE_REGEX, RegexOptions.None);

							paramVal = "";
							//  loop thru the literal and dynamic parts of the specified value
							foreach (Match valuePart in valueParts) {
								//  if it is a dynamic part, append the value of the named parameter
								if (valuePart.Value[0] == '[') {
									paramVal += req[valuePart.Value.Trim('[', ']')];

									//  if it is a literal part, just append the literal
								} else {
									paramVal += valuePart.Value;
								}
							}
						}

						//  to concatenate or not to concatenate
						if (concatinate && (req[paramName] != null)) {
							req[paramName] += paramVal;
						} else {
							req[paramName] = paramVal;
						}
					}
				}

				retEvent = Events.Ok;
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("SetParameterAction : Execute"), ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Ok
			{
				get { return EventNames.OK; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion

		#region Nested type: Param

		protected class Param
		{
			internal bool IsDefault;
			internal string Name;
			internal string Value;


			internal Param(
				string name,
				string value,
				bool isDefault)
			{
				this.Name = name;
				this.Value = value;
				this.IsDefault = isDefault;
			}
		}

		#endregion
	}
}