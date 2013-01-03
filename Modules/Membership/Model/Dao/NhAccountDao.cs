using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;

namespace Triton.Membership.Model.Dao
{
	#region History

	//   4/6/2011	SD	Added support for filtering by ModifiedDate and CreatedDate.

	#endregion

	public class NhAccountDao : NHibernateBaseDao<Account>, IAccountDao
	{
		#region IAccountDao Members

		public AccountFilter GetFilter()
		{
			return new AccountFilter();
		}


		public Account Get(Guid id)
		{
			return base.Get(id);
		}


		public SearchResult<Account> Find(
			AccountFilter filter)
		{
			List<Account> accounts = null;
			IList<Guid> accountIds = new List<Guid>();
			bool cont = true;

			int totalMatches = 0;



			//  geo code
			//  requires Longitude, Latitude, and Radius
			//  TODO: this need to be incorporated into the main query
			if (cont && !string.IsNullOrEmpty(filter.Latitude)
					&& !string.IsNullOrEmpty(filter.Longitude)
					&& !string.IsNullOrEmpty(filter.Radius)) {
				string radius = filter.Radius;
				ISQLQuery query = this.Session.CreateSQLQuery(
					string.Format(
						" SELECT aa.account_id," +
						" SQRT(POWER(addr.Latitude - '{0}', 2) + POWER(addr.Longitude - '{1}', 2)) * 62.1371192 AS DistanceFromAddress" +
						" FROM account_addresses aa INNER JOIN" +
						" addresses addr ON aa.address_id = addr.id" +
						" WHERE ABS(addr.Latitude - '{0}') < {2} AND ABS(addr.Longitude - '{1}') < {2}",
					//" ORDER BY DistanceFromAddress ",
						filter.Latitude, filter.Longitude, radius));

				IList temp = query.List();

				if (temp.Count > 0) {
					foreach (object[] obj in temp) {
						accountIds.Add(((Guid)obj[0]));
					}
				}
				else {
					//push default guid so we dont get all back
					accountIds.Add(Guid.Empty);
				}

				if ((accountIds != null) && (accountIds.Count > 0)) {
					filter.Ids = accountIds.ToArray();
					//  if no matches found on geocode, then nothing else matters
				}
				else {
					cont = false;
				}
			}

			if (cont && (filter.Ids != null) && (filter.Ids.Length == 1)) {
				Account account = this.Get(filter.Ids[0]);
				if (account != null) {
					accounts = new List<Account>();
					accounts.Add(account);
					totalMatches = 1;
				}

			}
			else if (cont) {
				ICriteria criteria = base.Session.CreateCriteria<Account>("account");


				// attributes
				if ((filter.Attributes != null) && (filter.Attributes.Count > 0)) {
					StringBuilder where = new StringBuilder();
					int attrCnt = 0;
					int distinctAttributeCount = 0;
					//  sort the attributes on the attribute code to group criteria for the same
					//  attribute together (for "and" vs "or")
					foreach (var attr in filter.Attributes.OrderBy(attribute => attribute.AttributeCode)) {

						//  if it's not the first attribute, determine if we need to "and" or "or" with the previous one
						if (++attrCnt > 1) {
							//  if there are multiple criteria for the current attribute...
							if (filter.Attributes.Count(a => a.AttributeCode == attr.AttributeCode) > 1) {
								//  get all the criteria (AttributeValue) for the attribute
								List<AccountFilter.AttributeFilter> vals =
										filter.Attributes.Where(a => a.AttributeCode == attr.AttributeCode).ToList();
								if (vals[vals.Count - 1] == attr) {
									where.Append(" and ");
									distinctAttributeCount++;
								}
								else {
									where.Append(" or ");
								}
							}
							else {
								where.Append(" or ");
								distinctAttributeCount++;
							}
						}

						//  add the criteria for the attribute to the where clause
						where.AppendFormat(" (typ.code = '{0}' and attr.value  {1} '{2}')",
								attr.AttributeCode, attr.Relation, attr.Value);
						//  add the values for the parameter placeholders to the collection to be included with the query
						//queryParameters.Add(new NhParameter { Type = "String", Name = attrNameParamName, Value = attr.Attribute });
						//queryParameters.Add(new NhParameter { Type = "String", Name = attrValueParamName, Value = attr.Value });
					}
					// must have at least one attribute filter
					distinctAttributeCount = distinctAttributeCount == 0 ? 1 : distinctAttributeCount;
					where.Append(" group by attr.account_id having COUNT(attr.account_id) = " + distinctAttributeCount);


					ISQLQuery query = this.Session.CreateSQLQuery(
							" select attr.account_id" +
							" from account_attributes attr " +
							" inner join attribute_types typ on attr.attribute_type_id = typ.id" +
							" where " + where.ToString());
					//" where (typ.code = '{0}') and attr.value {1} '{2}'",
					//filter.Attributes[0].AttributeCode, filter.Attributes[0].Relation, filter.Attributes[0].Value));

					//query.AddEntity(typeof (Guid));

					accountIds = query.List<Guid>();

					if ((accountIds != null) && (accountIds.Count > 0)) {
						//  if no other IDs to filter on, just use those founde matching attributes
						if (filter.Ids == null) {
							filter.Ids = accountIds.ToArray();
							//  if we already had IDs, intersect the two lists
						}
						else {
							filter.Ids = filter.Ids.Where(id => accountIds.Contains(id)).ToArray();
							//  if there no IDs left after intersection, force no matches
							if (filter.Ids.Length == 0) {
								criteria = criteria.Add(Expression.Sql("0=1"));
							}
						}

						//  if no matches found on attribute, force no matches
					}
					else {
						criteria = criteria.Add(Expression.Sql("0=1"));
					}
				}


				if ((filter.Ids != null) && (filter.Ids.Length > 0)) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if ((filter.Usernames != null) && (filter.Usernames.Length > 0)) {
					//figure out what the criteria is for searching in a list of values.
					criteria = criteria.CreateAlias("Usernames", "usernames")
						.Add(Expression.In("usernames.Value", filter.Usernames));

				}

				if (filter.RoleNames != null && filter.RoleNames.Length > 0) {
					criteria = criteria.CreateAlias("Roles", "roles")
						.Add(Expression.In("roles.Code", filter.RoleNames));
				}

				if (!string.IsNullOrEmpty(filter.Name)) {
					criteria = criteria.CreateAlias("Person", "person")
						.Add(Expression.Or(Expression.Like("person.Name.Last", filter.Name, MatchMode.Start),
										   Expression.Like("person.Name.First", filter.Name, MatchMode.Start)));
				}

				if (filter.ModifiedDate.HasValue) {
					criteria = criteria.Add(Expression.Ge("ModifiedDate", filter.ModifiedDate.Value));
				}

				if (filter.CreatedDate.HasValue) {
					criteria = criteria.Add(Expression.Ge("CreateDate", filter.CreatedDate.Value));
				}

				if (filter.Status != null) {
					criteria = criteria.CreateAlias("Status", "status")
						.Add(Expression.Eq("status.Id", filter.Status.Id));
				}

				accounts = SessionUtilities.GetItems<Account>(base.Session, criteria, filter, out totalMatches);
			}

			if (accounts == null) {
				accounts = new List<Account>();
			}

			SearchResult<Account> results = new SearchResult<Account>(
					accounts.ToArray(),
					filter,
					filter.Page ?? 1,
					filter.PageSize ?? accounts.Count,
					totalMatches);

			return results;
		}

		#endregion
	}
}