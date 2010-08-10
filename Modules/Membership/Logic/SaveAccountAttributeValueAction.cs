using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Triton.CodeContracts;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Support;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Utilities;

namespace Triton.Membership.Logic
{
	public class SaveAccountAttributeValueAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public SaveAccountAttributeValueAction()
		{
			this.IsAttributeCsv = false;

			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.AccountItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.AttributeTypeItemNameIn = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;
		}


		public bool IsAttributeCsv { get; set; }

		public string AccountItemNameIn { get; set; }

		public string AttributeTypeItemNameIn { get; set; }

		public string AccountItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			
			string retEvent = Events.Error;

			try {

				ActionContract.Requires<NullReferenceException>(context.Request.Items[this.AccountItemNameIn] != null, "Could not retrieve the account from the request to modify.");

				ActionContract.Requires<TypeMismatchException>(context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> || context.Request.Items[this.AccountItemNameIn] is Account, "The account item was not of type SearchResult<Account> or Account.");

				ActionContract.Requires<ApplicationException>(context.Request.Items[this.AccountItemNameIn] is Account || (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length > 0), "The account items collection did not contain any items.");

				ActionContract.Requires<ApplicationException>(context.Request.Items[this.AccountItemNameIn] is Account || (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length > 0 && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length == 1), "The account items collection had too many items in it.");

				ActionContract.Requires<NullReferenceException>(context.Request.Items[this.AttributeTypeItemNameIn] != null, "Could not retrieve the attribute types from the request to modify.");

				ActionContract.Requires<TypeMismatchException>(context.Request.Items[this.AttributeTypeItemNameIn] is SearchResult<AttributeType>, "The attribute  was not of type SearchResult<AttributeType>.");

				ActionContract.Requires<ApplicationException>(context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn).Items.Length > 0, "Attribute collection did not contain more then one item.");
            
				SearchResult<AttributeType> attributes = context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn);

				Account account;

				if (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account>) {
					account = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items[0];
				} else {
					account = context.Request.GetItem<Account>(this.AccountItemNameIn);
				}

				if (!string.IsNullOrEmpty(context.Request[ParameterNames.Account.ATTRIBUTE_IS_CSV])) {
					if (context.Request[ParameterNames.Account.ATTRIBUTE_IS_CSV] == "true") {
						this.IsAttributeCsv = true;
					} else if (context.Request[ParameterNames.Account.ATTRIBUTE_IS_CSV] == "false") {
						this.IsAttributeCsv = false;
					}
				}

				string[] attributeNames = context.Request[ParameterNames.AttributeType.CODE].ToStringArray();
				string[] attributeValues = context.Request[ParameterNames.Account.ATTRIBUTE_VALUE].ToStringArray();

				//make sure that the number of names is more then or equal to the number of attribute values
				if (attributeNames.Length < attributeValues.Length) {
					throw new ArgumentException("The number of attribute names needs to more then or equal to the number of attribute values.");
				}

				//foreach name, either update or add the new attribute
				for (int i = 0; i < attributeNames.Length; i++) {
					string attributeName = attributeNames[i];

					if (string.IsNullOrEmpty(attributeName)) {
						this.logger.Warn(warn => warn("Attribute name at {0} is empty.", i));
						continue;
					}

					string attributeValue = string.Empty;

					if (attributeValues.Length - 1 >= i) {
						attributeValue = attributeValues[i];
					} else {
						this.logger.Warn(warn => warn("Attribute value for name {0} at position {1} is empty.", attributeName, i));
					}

					KeyValuePair<AttributeType, string> type = account.Attributes.FirstOrDefault(x => x.Key.Code == attributeName);

					//if the key was found then update the value
					if (type.Key != null) {
						string newValue = this.ProcessValue(type.Value, attributeValue);

						account.Attributes[type.Key] = newValue;
					} else {
						AttributeType newType = attributes.Items.FirstOrDefault(x => x.Code == attributeName);

						//find the new type is found in the search results, if missing log it
						if (newType != null) {
							account.Attributes[newType] = attributeValue;
						} else {
							this.logger.Warn(warn => warn(string.Format("Could not find the new attribute name [{0}] in the items found.", attributeName)));
							continue;
						}
					}
				}

				context.Request.Items[this.AccountItemNameOut] = account;

				retEvent = Events.Ok;
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}

		#endregion

		/// <summary>
		/// Updates the value of the attribute. Toggles the value if attribute is of type CSV.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		private string ProcessValue(string oldValue,
		                            string newValue)
		{
			string retValue = newValue;

			//Checks if the attribute value is a delineated value list. 
			if (!string.IsNullOrEmpty(newValue) && this.IsAttributeCsv) {
				List<string> oldValues = new List<string>();
				oldValues.AddRange(oldValue.Split(MembershipConstants.ATTRIBUTE_VALUE_DELINEATOR));

				//if the value is in the list, then remove it, else add it.
				if (oldValues.Contains(newValue)) {
					oldValues.Remove(newValue);
				} else {
					oldValues.Add(newValue);
				}

				retValue = this.CreateCsvValue(oldValues);
			}

			return retValue;
		}


		/// <summary>
		/// Compile the delineated value of the attribute
		/// </summary>
		/// <param name="oldValues"></param>
		/// <returns></returns>
		private string CreateCsvValue(List<string> oldValues)
		{
			StringBuilder retValue = new StringBuilder();

			foreach (string oldValue in oldValues) {
				retValue.Append(oldValue + MembershipConstants.ATTRIBUTE_VALUE_DELINEATOR);
			}

			if (retValue.Length > 0) {
				retValue.Remove(retValue.Length - 1, 1);
			}

			return retValue.ToString();
		}

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
	}
}