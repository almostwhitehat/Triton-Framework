using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Model;
using Triton.Logic.Support;
using Triton.CodeContracts;
using Triton.Support.Request;
using Triton.Location.Model;
using Triton.Location.Support.Request;

namespace Triton.Location.Logic {


public class PopulateAddressFromRequestAction : IAction
{

	public string AddressItemNameIn
	{
		get;
		set;
	}


	#region IAction Members

	public string Execute(
		TransitionContext context)
	{
		MvcRequest request = context.Request;
		string retEvent = Events.Error;

		try {
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(AddressItemNameIn),
					"No item name given for the address in the AddressItemNameIn attribute.");

			PersistedAddress address = request.GetRequestItem<PersistedAddress>(AddressItemNameIn, true);

			Deserialize.Populate(request, address);

			retEvent = Events.Ok;
		} catch (Exception ex) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error("Error occured populating the address.", ex);
		}

		return retEvent;
	}

	#endregion


	#region Nested type: Events

	public class Events
	{
		public static string Ok
		{
			get {
				return EventNames.OK;
			}
		}

		public static string Error
		{
			get {
				return EventNames.ERROR;
			}
		}
	}

	#endregion
}
}