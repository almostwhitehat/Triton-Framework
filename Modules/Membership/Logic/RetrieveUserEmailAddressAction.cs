namespace web.security.membership.action
{

    using System;
    using System.Collections.Generic;
    using web.controller;
    using web.model;
    using web.security.membership.model;
    using web.security.membership.model.dao;
    using web.support.logging;
    using web.error;

        /// <summary>
        /// Gets the email address out of the contact type that is provided in the EmailContactType Accessor
        /// </summary>
    public class RetrieveUserEmailAddressAction : web.controller.action.Action
    {

       

        protected const string EVENT_ERROR = "error";
        protected const string EVENT_SUCCESS = "ok";


        private string _requestitemname = "useremailaddress";
        private string _requestparamname = "user";
        private string _emailcontacttype = "EMAIL";
        private string _emailaddress = "";
        
        //private string _password = "";

        /// <summary>
        /// The name of the request item that contains the account
        /// </summary>
        public string RequestParamName { 
            get { return _requestparamname;  }
            set { _requestparamname = value; } 
        }

        public string RequestItemName
        {
            get { return _requestitemname; }
            set { _requestitemname = value; }
        }
        /// <summary>
        /// The contact type that the action will use to pull the email address.  It is initiaized to "EMAIL"
        /// </summary>
        public string EmailContactType
        {
            get { return _emailcontacttype; }
            set { _emailcontacttype = value; }
        }

        #region Action Members

        public string Execute(
            TransitionContext context)
        {
            string retEvent = EVENT_ERROR;

            try
            {

                if ((context.Request.Items[_requestparamname] != null) &&
                   (context.Request.Items[_requestparamname] is SearchResult<Account>))
                {
                    SearchResult<Account> account = (SearchResult<Account>)context.Request.Items[_requestparamname];

                    if (account.Items.Count == 1)
                    {
                        try
                        {
                            _emailaddress = account.Items[0].GetContact(_emailcontacttype)[0].Value;
                            context.Request["email"] = _emailaddress;
                            retEvent = EVENT_SUCCESS;
                        }
                        catch (Exception excep)
                        {
                            AddErrorToRequest(context, 40004);
                            return "error";
                        }

                    }
                    else
                    {
                        AddErrorToRequest(context, 40006);
                        return "error";
                    }

                }
                else
                {
                    AddErrorToRequest(context, 40006);
                    return "error";
                }

            }
            catch (Exception e)
            {
                Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("RetrieveUserPasswordAction", e);
            }

            return retEvent;
        }
        /// <summary>
        /// Adds the error the the errors request Item template
        /// </summary>
        /// <param name="context">Transition context</param>
        /// <param name="errorId">The id of the error in the error dictonary.</param>
        private void AddErrorToRequest(web.controller.TransitionContext context, long errorId)
        {

            if (context.Request.Items["errors"] == null)
            {
                context.Request.Items["errors"] = new ErrorList();
            }
            // create the error object
            try
            {
                ((ErrorList)context.Request.Items["errors"]).Add(DictionaryManager.GetDictionaryManager().GetDictionary(context.Site).getError(errorId));
            }
            catch (NoSuchErrorException ex)
            {
                ((ErrorList)context.Request.Items["errors"]).Add(new Error(1, "", "An error has occured.", Error.ErrorType.FATAL));
            }

        }

        #endregion
    }
}