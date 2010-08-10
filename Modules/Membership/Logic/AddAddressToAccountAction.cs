using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Model;
using Triton.Logic.Support;
using Triton.CodeContracts;
using Triton.Support.Request;
using Triton.Location.Model;
using Triton.Membership.Model;

namespace Triton.Membership.Logic {


public class AddAddressToAccountAction : IAction
{

	public string AddressItemNameIn
	{
		get;
		set;
	}


	public string AccountItemNameIn
	{
		get;
		set;
	}


	public string AddressNameParamNameIn
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
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(AccountItemNameIn),
					"No item name given for the address in the AccountItemNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(AddressNameParamNameIn),
					"No item name given for the address in the AddressNameParamNameIn attribute.");

					//  get the address to add from the request
			PersistedAddress address = request.GetRequestItem<PersistedAddress>(AddressItemNameIn, true);
					//  get the account to add the address to from teh request
			Account account = request.GetRequestItem<Account>(AccountItemNameIn, true);
					//  get the name for the address from the request
			string addrName = request[AddressNameParamNameIn];

			if (account.Addresses == null) {
				account.Addresses = new Dictionary<string, PersistedAddress>();
			}
					//  add/set the address on the account
			if (account.Addresses.ContainsKey(addrName)) {
				account.Addresses[addrName] = address;
			} else {
				account.Addresses.Add(addrName, address);
			}

			retEvent = Events.Ok;
		} catch (Exception ex) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error("Error occured adding address to account.", ex);
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