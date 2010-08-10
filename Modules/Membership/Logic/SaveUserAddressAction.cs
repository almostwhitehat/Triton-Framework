using System;
using System.Configuration;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using web.controller;
using web.controller.request;
using web.controller.util;
using web.model;
using web.security.membership;
using web.security.membership.model;
using web.security.membership.model.dao;
using web.util.reflection;
using web.util.crypto;
using web.support.logging;

namespace web.security.membership.action {

    /// <summary>
    /// 
    /// </summary>
    /// <Requirements>must have a get user action called before this action 
    /// can be called
    /// </Requirements>
	public class SaveUserAddressAction : web.controller.action.Action
	{
		private	const	string	EVENT_SUCCESS	= "ok";
		private const	string	EVENT_ERROR		= "error";
        
		private			string	connectionType	= null;
        private         string  requestItemName = "";
        private         string  requestUserName = "";
		public string ConnectionType
		{
			get {
				return this.connectionType;
			}
			set {
				this.connectionType = value;
			}
		}

        public string UserRequestItemName {
            get { return requestUserName;}
            set { requestUserName = value; }
        }
		#region Action Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
                // TODO: should not be hard-coded to "users" -- has to match RequestItemName on GetUser action
                SearchResult<Account> acctResults = (SearchResult<Account>)context.Request.Items[requestUserName];

                // TODO: what if > 1 account in list??

                if ((acctResults == null) || (acctResults.Items.Count == 0))
                {
                    throw new ApplicationException("No account found in Request.Items.");
                }

                Account account = acctResults.Items[0];

                if (account == null)
                {
                    throw new ApplicationException("No account found in SearchResult.");
                }
                // get the address type from the request.. 
                // if it is not defined then use the first one available from the singleton
                string addressType = context.Request[MembershipConstants.PARAM_ADDRESS_TYPE] != null ? context.Request[MembershipConstants.PARAM_ADDRESS_TYPE] : SingletonBase<AddressType>.GetInstance().ToList()[0].Code;
                AccountDAO dao = AccountDAOFactory.GetAccountDAO(this.ConnectionType);
                AddressFilter filter = dao.GetAddressFilter();

                //determine if the address already exists
                filter.Fill(context.Request);
                SearchResult<Address> sr = dao.FindAddress(filter);
                Address addr = new Address(int.MinValue,"","","","","","","");
                if (sr.Items.Count == 1 ) 
                    addr = sr.Items[sr.Items.Count-1];

                if ( filter.AddressCity != "" ) {
                    addr.City = filter.AddressCity;
                }
                if ( filter.AddressPostalCode != "" ) {
                    addr.PostalCode = filter.AddressPostalCode;
                }
                if ( filter.AddressLine1 != "" ) {
                    addr.Line1 = filter.AddressLine1;
                }
                if (filter.AddressLine2 != "")
                {
                    addr.Line2 = filter.AddressLine2;
                }
                if (filter.AddressLine3 != "")
                {
                    addr.Line3 = filter.AddressLine3;
                }
                if( filter.AddressCountry != "" ) {
                    addr.Country = filter.AddressCountry;
                }
                if ( filter.AddressState != "" ) {
                    addr.State = filter.AddressState;
                }
                if (addr.Id > 0 ) {
                    // found an address we need to update
                    // TODO implement the update
                    dao.UpdateAddress(account, addressType, addr);
                }
                else {
                    // we have a new one.
                    dao.InsertAddress(account.Id, addressType, addr);
                }
                retEvent = EVENT_SUCCESS;
			} catch (Exception e) {
				Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserContactAction:Execute", e);
			}


			//  TODO: GET THIS OUT OF HERE!!
			//  This is REALLY ugly -- used to get the last event fired back to the client
			//  for ajax requests
			try {
				if (context.Request is web.controller.request.WebXmlRequest) {
					web.controller.format.Formatter formatter = web.controller.format.FormatterFactory.Make("PrimitiveValueFormatter", context.Request);
					
					if (formatter != null) {
						context.Request.Items["endevent"] = formatter.Format(retEvent);
					}
				}
			} catch (Exception) {}
			
			return retEvent;
		}



		#endregion

        #region Action Members

        public string RequestItemName
        {
            get
            {
                return requestItemName;
            }
            set
            {
                requestItemName = value;
            }
        }

        #endregion
    }
}
