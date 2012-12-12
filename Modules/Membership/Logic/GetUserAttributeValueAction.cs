using System;
using Common.Logging;
using Triton.CodeContracts;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Membership.Model;
using Triton.Membership.Support.Request;
using Triton.Model;

namespace Triton.Membership.Logic
{

    /// <summary>
    /// 
    /// </summary>
    public class GetUserAttributeValueAction : IAction
    {
        private readonly ILog logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the name of the results of the action used to identify this result in
        /// the Request.Items collection.
        /// </summary>
        public string AccountItemNameIn { get; set; }


        /// <summary>
        /// The Code name of the attribute Type to return the value for.
        /// </summary>
        public string AttributeTypeName { get; set; }

        /// <summary>
        /// Request ITem name for the attribute value to be placed into the Request.Items collection.
        /// </summary>
        public string AttributeValueItemNameOut { get; set; }

		/// <summary>
		/// Request Param name for the attribute value to be placed into the Request.Params collection.
		/// </summary>
		public string AttributeValueNameOut { get; set; }

        /// <summary>
        /// Default Contstructor.
        /// </summary>
        public GetUserAttributeValueAction()
        {
            this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
            this.AttributeTypeName = "";
            this.AttributeValueItemNameOut = "attribute_value";
	        this.AttributeValueNameOut = null;
        }

        #region IAction Members

        public string Execute(
            TransitionContext context)
        {
            string retEvent = Events.Error;

            string result = null;
            try {
                ActionContract.Requires<NullReferenceException>(context.Request.Items[this.AccountItemNameIn] != null, "Could not retrieve the account from the request to save.");
                ActionContract.Requires<TypeMismatchException>(context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> || context.Request.Items[this.AccountItemNameIn] is Account, "The account item was not of type SearchResult<Account> or Account.");
                ActionContract.Requires<ApplicationException>(context.Request.Items[this.AccountItemNameIn] is Account || (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length > 0), "The account items collection did not contain any items.");

                Account account = ((SearchResult<Account>)context.Request.Items[this.AccountItemNameIn]).Items[0];

                if (account.Attributes != null) {
                    result = account.GetAttributeValue(this.AttributeTypeName);
                }

                context.Request.Items[this.AttributeValueItemNameOut] = result;
				if (!string.IsNullOrEmpty(AttributeValueNameOut))
					context.Request[AttributeValueNameOut] = result;
                retEvent = Events.Ok;
            }
            catch (Exception exception) {
                this.logger.Error(
                    errorMessage => errorMessage("There was an error while saving an account."), exception);
            }

            return retEvent;
        }

        #endregion
    }
}
