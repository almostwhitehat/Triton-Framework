﻿using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.CodeContracts;
using Triton.Model;
using Triton.Location.Model;
using Triton.Location.Support.Request;

namespace Triton.Location.Logic {


public class CreateAddressAction : IAction
{

	public CreateAddressAction()
	{
		this.AddressItemNameOut = ItemNames.PersistedAddress.DEFAULT_SEARCH_RESULT_PERSISTED_ADDRESS;
	}

	public string AddressItemNameOut
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
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(AddressItemNameOut),
					"No item name given for the address in the AddressItemNameOut attribute.");

			PersistedAddress address = new PersistedAddress();

			request.Items[AddressItemNameOut] = address;

			retEvent = Events.Ok;
		} catch (Exception ex) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error("Error occured in Execute.", ex);
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
