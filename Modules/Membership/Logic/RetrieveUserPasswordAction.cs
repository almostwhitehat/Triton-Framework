namespace web.security.membership.action
{

    using System;
    using System.Collections.Generic;
    using web.controller;
    using web.model;
    using web.security.membership.model;
    using web.security.membership.model.dao;
    using web.support.logging;

    /// <summary>
    /// Gets the account out of the request items and puts the password into the request params collection.
    /// </summary>
    public class RetrieveUserPasswordAction : web.controller.action.Action
    {

        protected const string EVENT_ERROR = "error";
        protected const string EVENT_SUCCESS = "ok";


        private string _requestitemname = "userpassword";
        private string _requestparamname = "user";

        
        private string _password = "";
        
        /// <summary>
        /// The name of the account in the request items.
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

                           
                            _password = account.Items[0].Password;

                            string decryptPwd = web.util.crypto.CryptoUtil.DecryptString(
                            _password,
                            MembershipConstants.ENCRYPTION_KEY,
                            MembershipConstants.INIT_VECTOR,
                            web.util.crypto.EncryptionAlgorithm.TripleDes);

                            _password = decryptPwd;

                            //context.Request.Items[_requestitemname] = _password;
                            context.Request[_requestitemname] = _password;

                            retEvent = EVENT_SUCCESS;
                        
                    }

                }

            }
            catch (Exception e)
            {
                Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("RetrieveUserPasswordAction", e);
            }

            return retEvent;
        }

        #endregion
    }
}