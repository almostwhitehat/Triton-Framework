using System;
using Triton.Utilities;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	//   6/22/09 - SD -	Added ContentProvider property so that a ContentProvider can be specified
	//					on the transition.
	//				  -	Added overload of constructor to take the ContentProvider value for the
	//					ContentProvider property.

	#endregion

	/// <summary>
	/// Summary description for Transition.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class Transition
	{
		private readonly long fromStateId;
		private readonly string name;
		private readonly string[] publishKeyParams;
		private readonly long toStateId;


		public Transition(
			long fromStateId,
			long toStateId,
			string name,
			string publishKeyParams) : this(
					fromStateId,
					toStateId,
					name,
					publishKeyParams,
					null)
		{
		}


		public Transition(
			long fromStateId,
			long toStateId,
			string name,
			string publishKeyParams,
			string contentProvider)
		{
			this.fromStateId = fromStateId;
			this.toStateId = toStateId;
			this.name = name;
			this.ContentProvider = contentProvider;

			if (publishKeyParams != null) {
				this.publishKeyParams = publishKeyParams.Split(',');
				//  sort the params list in case they are in different 
				//  orders in different states
				Array.Sort(this.publishKeyParams);
			}
		}


		public long FromState
		{
			get {
				return this.fromStateId;
			}
		}


		public long ToState
		{
			get {
				return this.toStateId;
			}
		}


		public string Name
		{
			get {
				return this.name;
			}
		}


		public string[] PublishKeyParams
		{
			get {
				return this.publishKeyParams;
			}
		}


		/// <summary>
		/// Gets or sets the name of the <c>ContentProvider</c> used to render the
		/// results of the flow.
		/// </summary>
		/// <remarks>
		/// The <c>ContentProvider</c> name specified here is the name identified in 
		/// the <c>contentProviders</c> section of the config file.<para
		/// This is applicable only on transitions from a StartState.
		/// </remarks>
		public string ContentProvider
		{
			get;
			set;
		}


		/// <summary>
		/// Returns a copy of the Transition.
		/// </summary>
		/// <returns>A copy of the Transition.</returns>
		public Transition Clone()
		{
			return new Transition(this.fromStateId,
								  this.toStateId,
								  this.name,
								  (this.publishKeyParams == null) ? null : string.Join(",", this.publishKeyParams));
		}


		/// <summary>
		/// Returns a copy of the Transition with the FromState set to the given value.
		/// </summary>
		/// <returns>A copy of the Transition with the FromState set to the given value.</returns>
		public Transition Clone(
			long fromState)
		{
			return new Transition(fromState,
								  this.toStateId,
								  this.name,
								  (this.publishKeyParams == null) ? null : string.Join(",", this.publishKeyParams));
		}
	}
}