using System;
using System.Collections.Generic;
using Triton.Controller.Request;
using Triton.Membership.Support.Request;
using Triton.Model.Dao;
using Triton.Model.Dao.Support;
using Triton.Utilities;

namespace Triton.Membership.Model.Dao.Support
{

	#region History

	//   4/6/2011	SD	Added support for ModifiedDate and CreatedDate for AccountFilter.

	#endregion

	public static class FilterExtensions
	{
		/// <summary>
		/// List of valid relations for use with account attribute criteria for AccountFilter.Fill.
		/// </summary>
		private const string ACCOUNT_ATTRIBUTE_RELATIONS = "eq,ne,lt,gt,le,ge,lk";


		public static void Fill(
			this AccountFilter filter,
			MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.Account.ID])) {
				filter.Ids = request[ParameterNames.Account.ID].ToGuidArray();
			}
			else if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.ID])) {
				filter.Ids = request[ParameterNames.Account.Filter.ID].ToGuidArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.USERNAME])) {
				filter.Usernames = request[ParameterNames.Account.Filter.USERNAME].ToStringArray();
			}
			else if (!string.IsNullOrEmpty(request[ParameterNames.Account.USERNAME])) {
				filter.Usernames = request[ParameterNames.Account.USERNAME].ToStringArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.ROLE])) {
				filter.RoleNames = request[ParameterNames.Account.Filter.ROLE].ToStringArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.PERSONNAME])) {
				filter.Name = request[ParameterNames.Account.Filter.PERSONNAME];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.MODIFIED_DATE])) {
				DateTime date;
				if (DateTime.TryParse(request[ParameterNames.Account.Filter.MODIFIED_DATE], out date)) {
					filter.ModifiedDate = date;
				}
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.CREATED_DATE])) {
				DateTime date;
				if (DateTime.TryParse(request[ParameterNames.Account.Filter.CREATED_DATE], out date)) {
					filter.CreatedDate = date;
				}
			}

			//  attributes
			// TODO: should add support for specifying null -- "[null]"?
			//  find any attribute parameters in the params collection
			//  attribute parameter names are of the form: filter_account_attribute_[relation]_[attributeCode]
			string[] attrParams = Array.FindAll<string>(request.Params.AllKeys,
					key => (key != null) && key.StartsWith(ParameterNames.Account.Filter.ATTRIBUTE_PREFIX));

			//  if we found attribute parameters, process them
			if ((attrParams != null) && (attrParams.Length > 0)) {
				List<AccountFilter.AttributeFilter> attributeCriteria = new List<AccountFilter.AttributeFilter>();
				string[] relations = ACCOUNT_ATTRIBUTE_RELATIONS.Split(',');

				foreach (string paramName in attrParams) {
					//  skip it if there is no value
					if (!string.IsNullOrEmpty(request[paramName])) {
						int prefixLen = ParameterNames.Account.Filter.ATTRIBUTE_PREFIX.Length + 3;
						//  get the relation
						string relation = paramName.Substring(ParameterNames.Account.Filter.ATTRIBUTE_PREFIX.Length, 2).ToLower();

						AccountFilter.AttributeFilter val = new AccountFilter.AttributeFilter {
							AttributeCode = paramName.Remove(0, prefixLen),
							Value = request[paramName],
							Relation = TranslateRelation(relation)
						};

						attributeCriteria.Add(val);
					}
				}

				filter.Attributes = attributeCriteria;
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.LATITUDE])) {
				filter.Latitude = request[ParameterNames.Account.Filter.LATITUDE];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.LONGITUDE])) {
				filter.Longitude = request[ParameterNames.Account.Filter.LONGITUDE];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.RADIUS])) {
				filter.Radius = request[ParameterNames.Account.Filter.RADIUS];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.STATUS])) {
				string status = request[ParameterNames.Account.Filter.STATUS].ToString();

				filter.Status = DaoFactory.GetDao<IAccountStatusDao>().Get(status);
			}
			else {
				filter.Status = DaoFactory.GetDao<IAccountStatusDao>().Get("active");
			}

			((BaseFilter)filter).Fill(request);
		}


		public static void Fill(
			this RoleFilter filter,
			MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.Role.ID])) {
				filter.Ids = request[ParameterNames.Role.ID].ToIntArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Role.CODE])) {
				filter.Codes = request[ParameterNames.Role.CODE].ToStringArray();
			}

			((BaseFilter)filter).Fill(request);
		}


		public static void Fill(
			this AttributeTypeFilter filter,
			MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.AttributeType.ID])) {
				filter.Ids = request[ParameterNames.AttributeType.ID].ToIntArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.AttributeType.CODE])) {
				filter.Codes = request[ParameterNames.AttributeType.CODE].ToStringArray();
			}

			((BaseFilter)filter).Fill(request);
		}


		private static string TranslateRelation(
			string relationCode)
		{
			string relation;

			switch (relationCode) {
				case "ne":
					relation = "!=";
					break;

				case "lt":
					relation = "<";
					break;

				case "gt":
					relation = ">";
					break;

				case "le":
					relation = "<=";
					break;

				case "ge":
					relation = ">=";
					break;

				case "lk":
					relation = "like";
					break;

				default:
					relation = "=";
					break;
			}

			return relation;
		}
	}
}