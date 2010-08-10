using System;
using System.Collections.Generic;
using System.Text;
using web.model;
using System.Data.SqlClient;
using web.util;
using web.util.db;
using web.support.logging;

namespace web.security.membership.model.dao {


	public class MSAccountDAO : web.model.dao.MSDAOBase, AccountDAO
	{
		private	Logger	logger	= Logger.GetLogger("DaoLogger");


		/// <summary>
		/// 
		/// </summary>
		public override string Name
		{
			get {
				return "AccountDAO";
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public MSAccountDAO()
		{
			this.connectionType = "*";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connType"></param>
		public MSAccountDAO(
			string connType)
		{
			this.connectionType = connType;
		}


		/// <summary>
		/// Get an account by specifying a GUID of the account.
		/// </summary>
		/// <param name="accountId">Account Id to return.</param>
		/// <returns>Account</returns>
		public Account GetAccount(
			Guid accountId)
		{
			/* select *
			 *	from member_account 
			 *	where account_id = 'ACCOUNT ID'
			 */

			SqlConnection	conn	= null;
			SqlCommand		cmd		= null;
			SqlDataReader	dr		= null;
			Account			acc		= null;

			try {
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();
				
				

				string sqlCmd = "select account_id, password, account_status_id, create_date" +
						" from member_account (nolock) " +
						"where account_id = @accountId";

				cmd.CommandText	= sqlCmd;
				cmd.Connection	= conn;

				cmd.Parameters.AddWithValue("@accountId", accountId);
				conn.Open();

				dr = cmd.ExecuteReader();
				while (dr.Read()) {
					acc = new Account(new Guid(DbUtil.getGuidValue(dr, "account_id")), DbUtil.getStringValue(dr, "password"), DbUtil.getIntValue(dr, "account_status_id"),DbUtil.getDateValue(dr,"create_date"));
				}

			} catch (Exception e) {
				logger.Error("MSAccountDAO:GetAccount(Guid): ", e);
			}
			finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}
			
			return acc;
		}


		/// <summary>
		/// Get an account by specifying the username of the account.
		/// TODO: Username should be context specific.
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public Account GetAccount(
			string userName)
		{
			/*select * 
			 *	from member_account mac 
			 *		join member_account_username mau 
			 *			on mac.account_id = mau.account_id
			 *	where mau.username = 'USERNAME'
			*/
			SqlConnection	conn	= null;
			SqlCommand		cmd		= null;
			SqlDataReader	dr		= null;
			Account			acc		= null;

			try {
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();

				string sqlCmd = "select mac.account_id, mac.password, mac.account_status_id, mau.username, mac.create_date " +
						"from member_account mac (nolock) " +
							"join member_account_username mau " +
								"on mac.account_id = mau.account_id " +
						"where mau.username = @userName";

				cmd.Connection	= conn;
				cmd.CommandText = sqlCmd;

				cmd.Parameters.AddWithValue("@userName", userName);

				conn.Open();

				dr = cmd.ExecuteReader();
				while (dr.Read()) {
                    acc = new Account(new Guid(DbUtil.getGuidValue(dr, "account_id")), DbUtil.getStringValue(dr, "password"), DbUtil.getIntValue(dr, "account_status_id"), DbUtil.getDateValue(dr, "create_date"));
				}

			} catch (Exception e) {
				logger.Error("MSAccountDAO:GetAccount(string): ", e);
			} finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}

			return acc;

		}


		/// <summary>
		/// Returns an <c>AccountFilter</c> to use when calling FindAccounts.
		/// </summary>
		/// <returns>An <c>AccountFilter</c> to use when calling FindAccounts.</returns>
		public AccountFilter GetFilter()
		{
			return new AccountFilter();
		}


		/// <summary>
		/// Find Accounts using the Filter.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns>Search result of <c>Accounts</c>.</returns>
		public SearchResult<Account> FindAccounts(
			AccountFilter filter)
		{
			/* select *
			 *	from member_account mac
			 *			//get account based on username
			 *		join member_account_username mau
			 *			on mac.account_id = mau.account_id
			 *			
			 *			//get account based on attributes
			 *		join member_account_attribute maat
			 *			on mac.account_id = maat.account_id
			 * 
			 *			//get account based on contacts
			 *		join member_account_contact macc
			 *			on mac.account_id = macc.account_id
			 *		join member_contact mc
			 *			on macc.contact_id = mc.contact_id
			 * 
			 *			//get account based on role - this needs to be added if needed
			 *		join member_account_role mar
			 *			on mac.account_id = mar.account_id
			 *		join member_role mr
			 *			on mar.role_id = mr.role_id 
			 * 
			 * 			 //get account based on addresses - this needs to be added if needed
			 *		join member_account_address maad
			 *			on mac.account_id = maad.account_id
			 *		join member_address mad
			 *			on maad.address_id = mad.address_id
			 */
			
					//reasoning for a global try catch:
					//if the function fails at any moment we want to exit and get back to the callee
			SearchResult<Account>	searchResult	= null;
			SqlConnection			conn			= null;
			SqlCommand				cmd				= null;
			SqlDataReader			dr				= null;
			List<Account>			accountList	= new List<Account>();

            int totalMatches = 0;
            int numReturned = 0;

			try {
				List<string>	fields		= new List<string>();
				List<string>	joinTables	= new List<string>();
				List<string>	where		= new List<string>();
                string sortby = "";
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();
				
				fields.AddRange(new string[] { "mac.account_id", "mac.password", "mac.account_status_id, mac.create_date" });

				if (filter.AccountIds.Count > 0) {

					StringBuilder accounts = new StringBuilder();
					accounts.Append("mac.account_id in (");
					for(int i = 0; i < filter.AccountIds.Count; i++){
						string accIdParamName = "@accountId" + i;
						accounts.Append(accIdParamName + ",");
						cmd.Parameters.AddWithValue(accIdParamName, filter.AccountIds[i]);
					}

					//remove the trailing ,
					accounts.Length--;
					//append the end )
					accounts.Append(")");
					where.Add(accounts.ToString());
				}

				if (filter.StatusId != null) {
					//get the accounts by status id
					StringBuilder status = new StringBuilder();
					status.Append("mac.account_status_id = @statusId");
					cmd.Parameters.AddWithValue("@statusId", filter.StatusId);
					where.Add(status.ToString());
				}

				if (filter.UserNames.Count > 0) {
					//get the accounts by username
					joinTables.Add("join member_account_username mau on mac.account_id = mau.account_id");

					StringBuilder usernames = new StringBuilder();
					if ( filter.UserNames.Count == 1 )
                    {   
                        if (filter.ExactMatch )
                            where.Add(DbUtil.buildStringCondition(filter.UserNames[0], "mau.username", cmd, ""));
                        else
                            where.Add(DbUtil.buildStringCondition("%" + filter.UserNames[0] + "%", "mau.username", cmd, ""));
                    }
                    else
                    {
                        usernames.Append("mau.username in (");
					    for(int i = 0; i < filter.UserNames.Count; i++){
						    string userNameParamName = "@username" + i;
						    usernames.Append(userNameParamName + ",");
						    cmd.Parameters.AddWithValue(userNameParamName, filter.UserNames[i]);
					    }
					    //remove the trailing ,
					    usernames.Length--;
					    usernames.Append(")");
					    where.Add(usernames.ToString());
                    }



				}

				if (filter.AttributeInfo.Count > 0) {
					//get the accounts by attribute
					joinTables.Add("join member_account_attribute maat on mac.account_id = maat.account_id");

					StringBuilder attributes = new StringBuilder();

					for(int i = 0; i < filter.AttributeInfo.Count; i++){

						if (filter.AttributeInfo[i].TypeId == null) {
							filter.AttributeInfo[i].TypeId = SingletonBase<AccountAttributeType>.GetInstance()[filter.AttributeInfo[i].TypeCode].Id;
						}

						string attributeIdParamName = "@attributeId" + i;
						string attributeValueParamName = "@attributeValue" + i;

						attributes.Append("maat.account_attribute_type_id = " + attributeIdParamName);
						attributes.Append(" and maat.account_attribute_value = " + attributeValueParamName);

						cmd.Parameters.AddWithValue(attributeIdParamName, filter.AttributeInfo[i].TypeId);
						cmd.Parameters.AddWithValue(attributeValueParamName, filter.AttributeInfo[i].Value);
					}

					where.Add(attributes.ToString());
				}

				if (filter.ContactInfo.Count > 0) {
					//get the accounts by contact 
					joinTables.Add("join member_account_contact macc on mac.account_id = macc.account_id");
					joinTables.Add("join member_contact mc on macc.contact_id = mc.contact_id");

					StringBuilder contacts = new StringBuilder();

					for(int i = 0; i < filter.ContactInfo.Count; i++){

						if (filter.ContactInfo[i].TypeId == null) {
							filter.ContactInfo[i].TypeId = SingletonBase<ContactType>.GetInstance()[filter.ContactInfo[i].TypeCode].Id;
						}

						string contactIdParamName = "@contactId" + i;
						string contactValueParamName = "@contactValue" + i;

						contacts.Append("mc.contact_type_id = " + contactIdParamName);
						contacts.Append(" and mc.contact_value = " + contactValueParamName);

						cmd.Parameters.AddWithValue(contactIdParamName, filter.ContactInfo[i].TypeId);
						cmd.Parameters.AddWithValue(contactValueParamName, filter.ContactInfo[i].Value);
					}

					where.Add(contacts.ToString());
				}

				cmd.Connection = conn;

                //cmd.CommandText = "select " + BuildFields(fields) +
                //                " from member_account mac (nolock) " +
                //                BuildJoins(joinTables) +
                //                BuildWhere(where);

                string topFilter = string.Empty;
                if (filter.Page >= 0)
                {
                    topFilter = String.Format("top {0} ", (filter.Page + 1) * filter.PageSize);
                }

                if (filter.Sort != "")
                {
                    if (filter.SortAttributeId > 0)
                    {
                        if (!where.Contains("join member_account_attribute mats on mac.account_id = mats.account_id"))
                        {
                            joinTables.Add("join member_account_attribute mats on mac.account_id = mats.account_id");
                            where.Add(DbUtil.buildLongCondition(filter.SortAttributeId, "mats.account_attribute_type_id", cmd, ""));
                            sortby = string.Format("{0} {1}", "order by mats." + filter.Sort, filter.SortOrder == web.model.dao.DAOUtil.DBSortDirection.DESC ? " DESC" : "ASC");
                        }
                    }

                }

                cmd.CommandText = "select " + topFilter + BuildFields(fields) +
                                " from member_account mac (nolock) " +
                                BuildJoins(joinTables) +
                                BuildWhere(where) +
                                sortby; 

				cmd.Connection.Open();

				dr = cmd.ExecuteReader();

				while (dr.Read()) {
					Account account = new Account(new Guid(DbUtil.getGuidValue(dr, "account_id")), DbUtil.getStringValue(dr, "password"), DbUtil.getIntValue(dr, "account_status_id"),DbUtil.getDateValue(dr,"create_date"));
					accountList.Add(account);
                    numReturned++;
				}

                dr.Close();

                cmd.CommandText = "select count ( mac." + MembershipConstants.FIELD_ACCOUNT_ID + ") as " + MembershipConstants.FIELD_ACCOUNT_ID +
                                " from member_account mac (nolock) " +
                                BuildJoins(joinTables) +
                                BuildWhere(where);

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    totalMatches = DbUtil.getIntValue(dr, MembershipConstants.FIELD_ACCOUNT_ID);
                }

                if (filter.Page >= 0)
                {
                    accountList.RemoveRange(0, filter.Page * filter.PageSize);
                }

			} catch (Exception e) {
				logger.Error("MSAccountDAO:FindAccounts: ", e); 
			}
			finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}

            //searchResult = new SearchResult<Account>(
            //    accountList, 
            //    filter, 
            //    accountList.Count, 
            //    accountList.Count, 
            //    0, 
            //    0, 
            //    accountList.Count, 
            //    1, 
            //    accountList.Count, 
            //    1);

           

            searchResult = new SearchResult<Account>(
                accountList,
                filter,
                totalMatches,
                numReturned,
                filter.Page,
                filter.PageSize,
                null);

			return searchResult;
		}

		/// <summary>
		/// Creates the where part of the sql statement.
		/// </summary>
		/// <param name="whereList"></param>
		/// <returns></returns>
		private string BuildWhere(
			List<string> whereList)
		{
			StringBuilder retValue = new StringBuilder();

			if (whereList.Count > 0) {
				retValue.Append(" where ");
				foreach (string where in whereList) {
					retValue.AppendFormat("({0}) and", where);
				}
					//remove the trailing "and"
				retValue.Length = retValue.Length - 3;
			}

			return retValue.ToString();
		}

		/// <summary>
		/// Creates the join part of the sql statement.
		/// </summary>
		/// <param name="joinTables"></param>
		/// <returns></returns>
		private string BuildJoins(
			List<string> joinTables)
		{
			string retValue = "";

			foreach (string join in joinTables) {
                switch (join) {
                    case "member_account_address_2_member_address" :
                        retValue += " join Member_account_address maa on maa.address_id = ma.address_id ";
                        break;
                    case "member_account_2_member_account_address" :
                        retValue += " join member_acount mac on maa.account_id = mac.account_id ";
                        break;
                    case "member_account_role_2_member_role":
                        retValue += " join member_account_role mar on mr.role_id = mar.role_id ";
                        break;
                    case "member_account_2_member_account_role":
                        retValue += " join member_account mac on mar.account_id = mac.account_id ";
                        break;
                    default :
                        retValue += join + " ";
                        break;
                }
			}

			return retValue;
		}

		/// <summary>
		/// Builds the fields of sql statement.
		/// </summary>
		/// <param name="fields"></param>
		/// <returns></returns>
		private string BuildFields(
			List<string> fields)
		{
			StringBuilder retValue = new StringBuilder();

			foreach (string field in fields) {
				retValue.Append(field + ",");
			}
			retValue.Length--;

			return retValue.ToString();
		}

		/// <summary>
		/// Returns the contact information for a particular user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Dictionary<string, List<MembershipAttribute>> GetContacts(
			Guid accountId)
		{
			Dictionary<string, List<MembershipAttribute>> contacts = new Dictionary<string,List<MembershipAttribute>>();
			
			SqlConnection	conn	= null;
			SqlCommand		cmd		= null;
			SqlDataReader	dr		= null;

			try {
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();

				string sqlText = "select mc.contact_id, mc.contact_type_id, mc.contact_value " +
									"from member_contact mc " +
										"join member_account_contact macc on macc.contact_id = mc.contact_id " +
									"where macc.account_id = @accountId";

				cmd.Parameters.AddWithValue("@accountId", accountId);
				cmd.CommandText = sqlText;
				cmd.Connection = conn;

				cmd.Connection.Open();

				dr = cmd.ExecuteReader();

				while (dr.Read()) {
							//read in info
					int		typeId			= DbUtil.getIntValue(dr, "contact_type_id");
					int		contactId		= DbUtil.getIntValue(dr, "contact_id");
					string	contactValue	= DbUtil.getStringValue(dr, "contact_value");
					string	contactTypeCode	= SingletonBase<ContactType>.GetInstance()[typeId].Code;
					
							//create the contact
					MembershipAttribute attr = new MembershipAttribute(contactId, contactTypeCode, typeId, contactValue);
					
							//check if the contact type already exists, if not create it
					if (contacts.ContainsKey(contactTypeCode)) {
								//if exists, add the attribute to the list
						contacts[contactTypeCode].Add(attr);
					} else {
						List<MembershipAttribute> attrList = new List<MembershipAttribute>();
						attrList.Add(attr);
						contacts.Add(contactTypeCode, attrList);
					}
				}

			} catch (Exception e) {
				logger.Error("MSAccountDAO:GetContacts: ", e);
			}
			finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}
			
			return contacts;
		}


        public List<string> GetUserName(
            Guid accountId)
        {
            List<string> usernames = new List<string>();

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                string sqlText = "select mau.account_id, mau.username " +
                                    "from member_account_username mau " +
                                    "where mau.account_id = @accountId";

                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.CommandText = sqlText;
                cmd.Connection = conn;
#if (DEBUG)
                logger.Status("SQL Query : " + cmd.CommandText);
#endif
                cmd.Connection.Open();

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    //read in info
                    usernames.Add( DbUtil.getStringValue(dr, "username"));
                }

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:GetUsernames: ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            return usernames;
        }

		/// <summary>
		/// Returns the attributes for a particular user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Dictionary<string,List<MembershipAttribute>> GetAttributes(
			Guid accountId)
		{
			Dictionary<string,List<MembershipAttribute>> attributes = new Dictionary<string,List<MembershipAttribute>>();
			
			SqlConnection	conn	= null;
			SqlCommand		cmd		= null;
			SqlDataReader	dr		= null;

			try {
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();

				string sqlText = "select maat.account_attribute_type_id, maat.account_attribute_value " +
									"from member_account_attribute maat " +
									"where maat.account_id = @accountId";

				cmd.Parameters.AddWithValue("@accountId", accountId);
				cmd.CommandText = sqlText;
				cmd.Connection = conn;

				cmd.Connection.Open();

				dr = cmd.ExecuteReader();

				while (dr.Read()) {
							//read in info
					int		typeId				= DbUtil.getIntValue(dr, "account_attribute_type_id");
					string	attributeValue		= DbUtil.getStringValue(dr, "account_attribute_value");
					string	attributeTypeCode	= SingletonBase<AccountAttributeType>.GetInstance()[typeId].Code;
					
							//create the attribute	
					MembershipAttribute attr = new MembershipAttribute(null, attributeTypeCode, typeId, attributeValue);
					//attr.TypeId = typeId;

							//check if the attribute type already exists, if not create it
					if (attributes.ContainsKey(attributeTypeCode)) {
								//if exists, add the attribute to the list
						attributes[attributeTypeCode].Add(attr);
					} else {
						List<MembershipAttribute> attrList = new List<MembershipAttribute>();
						attrList.Add(attr);
						attributes.Add(attributeTypeCode, attrList);
					}
				}

			} catch (Exception e) {
				logger.Error("MSAccountDAO:GetAttributes: ", e);
			} finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}
			
			return attributes;
		}

		/// <summary>
		/// Returns the Addresses for a particular user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Dictionary<string, Address> GetAddresses(
			Guid accountId)
		{
			Dictionary<string, Address> addresses = new Dictionary<string,Address>();

			SqlConnection	conn	= null;
			SqlCommand		cmd		= null;
			SqlDataReader	dr		= null;

			try {
				conn	= (SqlConnection)this.GetConnection();
				cmd		= new SqlCommand();

				string sqlText = "select maddr.address_id, maddr.address_line_1, maddr.address_line_2, maddr.address_line_3, " +
							"maddr.address_state, maddr.address_country, maddr.address_postalcode, maddr.address_city, macaddr.address_type_id " +
							"from member_address maddr " +
								"join member_account_address macaddr on maddr.address_id = macaddr.address_id " +
                            "where macaddr.account_id = @accountId";

				cmd.Parameters.AddWithValue("@accountId", accountId);
				cmd.CommandText = sqlText;
				cmd.Connection = conn;

				cmd.Connection.Open();

				dr = cmd.ExecuteReader();

				while (dr.Read()) {
							//read in info
					int		addressId			= DbUtil.getIntValue(dr, "address_id");
					string	line1				= DbUtil.getStringValue(dr, "address_line_1");
					string	line2				= DbUtil.getStringValue(dr, "address_line_2");
					string	line3				= DbUtil.getStringValue(dr, "address_line_3");
					string	state				= DbUtil.getStringValue(dr, "address_state");
					string	country				= DbUtil.getStringValue(dr, "address_country");
					string	postalCode			= DbUtil.getStringValue(dr, "address_postalcode");
					string	city				= DbUtil.getStringValue(dr, "address_city");
					int		addressTypeId		= DbUtil.getIntValue(dr, "address_type_id");	
					string	addressTypeCode		= SingletonBase<AddressType>.GetInstance()[addressTypeId].Code;
							//create new address
					Address addr = new Address(addressId, line1, line2, line3, state, city, postalCode, country);
					addresses.Add(addressTypeCode, addr);
				}

			} catch (Exception e) {
				logger.Error("MSAccountDAO:GetAddresses: ", e);
			} finally {
				DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
			}

			return addresses;
		}

		/// <summary>
		/// Updates the attribute for a user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="attributeCode"></param>
		/// <param name="newValue"></param>
		public void UpdateAttribute(
			Guid	accountId, 
			string	attributeCode, 
			string	newValue)
		{
			string sqlText = "update member_account_attribute " +
					"set account_attribute_value = @attrValue " + 
					"where account_id = @accountId and account_attribute_type_id = @typeId";

			DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
			DAOAction act = new DAOAction();
			act.Action = sqlText;

			act.AddParameter("@accountId", accountId);
			act.AddParameter("@attrValue", newValue);

						//retrieve the type id from the singleton
			int typeId = SingletonBase<AccountAttributeType>.GetInstance()[attributeCode].Id;

			act.AddParameter("@typeId", typeId);

			cmd.AddAction(act);

			try {
				cmd.ExecuteCommand();
			} catch(Exception e) {
				throw e;
			}
		}

		/// <summary>
		/// Deletes an attribute for a particular user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="attributeCode"></param>
		/// <param name="oldValue"></param>
		public void DeleteAttribute(
			Guid	accountId, 
			string	attributeCode, 
			string	oldValue)
		{

			string sqlText = "delete from member_account_attribute " +
							"where account_id = @accountId " + 
							"and account_attribute_type_id = @typeId " + 
							"and account_attribute_value = @oldValue";

			DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
			DAOAction act = new DAOAction();
			act.Action = sqlText;

			act.AddParameter("@accountId", accountId);
			act.AddParameter("@oldValue", oldValue);

						//retrieve the type id from the singleton
			int typeId = SingletonBase<AccountAttributeType>.GetInstance()[attributeCode].Id;

			act.AddParameter("@typeId", typeId);

			cmd.AddAction(act);

			try {
				cmd.ExecuteCommand();
			} catch(Exception e) {
				throw e;
			}
		}

        public void DeleteMultipleAttribute(
            Guid accountId,
            string attributeCode)
        {

            string sqlText = "delete from member_account_attribute " +
                            "where account_id = @accountId " +
                            "and account_attribute_type_id = @typeId";

            DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
            DAOAction act = new DAOAction();
            act.Action = sqlText;

            act.AddParameter("@accountId", accountId);
            //act.AddParameter("@oldValue", oldValue);

            //retrieve the type id from the singleton
            int typeId = SingletonBase<AccountAttributeType>.GetInstance()[attributeCode].Id;

            act.AddParameter("@typeId", typeId);

            cmd.AddAction(act);

            try
            {
                cmd.ExecuteCommand();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

		/// <summary>
		/// Insert an attribute for a user.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="attributeCode"></param>
		/// <param name="value"></param>
		public void InsertAttribute(
			Guid accountId, 
			string attributeCode, 
			string value)
		{
			
			string sqlText = "insert into member_account_attribute " +
						"(account_id, account_attribute_type_id, account_attribute_value) " +
						"values " +
						"(@accountId, @typeId, @value)";

			DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
			DAOAction act = new DAOAction();
			act.Action = sqlText;

			act.AddParameter("@accountId", accountId);
			act.AddParameter("@value", value);

						//retrieve the type id from the singleton
			int typeId = SingletonBase<AccountAttributeType>.GetInstance()[attributeCode].Id;

			act.AddParameter("@typeId", typeId);

			cmd.AddAction(act);

			try {
				cmd.ExecuteCommand();
			} catch(Exception e) {
				throw e;
			}
		}

		/// <summary>
		/// Creates an account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="password"></param>
		/// <param name="username"></param>
		/// <param name="statusCode"></param>
		public void CreateAccount(
			Guid accountId, 
			string password, 
			string username, 
			string statusCode)
		{
	
			DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
				
					//create the account in the member_account
			DAOAction act = new DAOAction();

			string sqlText = "insert into member_account " +
						"(account_id, password, account_status_id) " +
							"values " +
						"(@accountId, @password, @statusId)";

			act.Action = sqlText;

			act.AddParameter("@accountId", accountId);
			act.AddParameter("@password", password);

					//retrieve the type id from the singleton
			int statusId = SingletonBase<AccountStatus>.GetInstance()[statusCode].Id;

			act.AddParameter("@statusId", statusId);

			cmd.AddAction(act);

					//add the user name into the username table
			DAOAction act2 = new DAOAction();
			
			string sqlText2 = "insert into member_account_username " +
						"(account_id, username) " +
							"values " + 
						"(@accountId, @username)";

			act2.Action = sqlText2;

			act2.AddParameter("@accountId", accountId);
			act2.AddParameter("@username", username);

			cmd.AddAction(act2);

			try {
				cmd.ExecuteCommand();
			} catch(Exception e) {
				throw e;
			}
		}

		/// Basicaly the Command Pattern. There is a bit less abstraction.
		/// Right now can only handle fire and forget sql statements.
		/// It will not return or care about the execution. The only things that are trapped are errors.
	
		/// <summary>
		/// DAOAction acts a the placeholder for the sql command to be executed.
		/// </summary>	
		private class DAOAction
		{
			private string action = null;
			private Dictionary<string, object> parameters = new Dictionary<string,object>();

			/// <summary>
			/// Create a new DAOAction.
			/// </summary>
			public DAOAction() {}

			/// <summary>
			/// Set the sql statement.
			/// </summary>
			public string Action
			{
				get {
					return this.action;
				}
				set {
					this.action = value;
				}
			}

			/// <summary>
			/// Adds parameters for the sql statement.
			/// </summary>
			/// <param name="key"></param>
			/// <param name="value"></param>
			public void AddParameter(
				string key, 
				object value)
			{
				this.parameters.Add(key, value);
			}

			/// <summary>
			/// Gets a list of parameters.
			/// TODO: If ever refactored out, this should return ReadOnlyCollection<string>
			/// </summary>
			/// <returns></returns>
			public List<string> GetParameters()
			{
				return new List<string>(this.parameters.Keys);
			}

			/// <summary>
			/// Gets the value of the parameter.
			/// TODO: Maybe: create a indexer property, to easier navigate the Keys???
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public object GetParameterValue(string key)
			{
				return this.parameters[key]; 
			}

		}

		/// <summary>
		/// Stores the actions and executes them in the order recieved. 
		/// </summary>
		private class DAOCommand
		{
			private	Logger				logger	= Logger.GetLogger("DaoLogger");
			private	List<DAOAction>		actions	= new List<DAOAction>();
			private	SqlConnection		conn	= null;

			public DAOCommand(SqlConnection conn)
			{
				this.conn = conn;
			}

			/// <summary>
			/// Add a DAOAction to the execution.
			/// </summary>
			/// <param name="action"></param>
			public void AddAction(DAOAction action)
			{
				this.actions.Add(action);
			}

			/// <summary>
			/// Executes the action one after another. Keeping the transaction intact.
			/// </summary>
			public void ExecuteCommand()
			{
				SqlCommand		cmd		= null;
				SqlTransaction	trans	= null;
				
				try {
					conn.Open();
					trans = conn.BeginTransaction("UpdateAccount");

					try {
						foreach (DAOAction action in actions) {
							cmd = new SqlCommand();
							cmd.Connection = conn;
							cmd.Transaction = trans;

							cmd.CommandText = action.Action;

							foreach (string param in action.GetParameters()) {
								cmd.Parameters.AddWithValue(param, action.GetParameterValue(param));
							}
							
							cmd.ExecuteScalar();
						}

						trans.Commit();

					} catch (Exception ex) {
						//logger.Error("MSAccountDAO:DAOCommand:ExecuteCommand: ", e);
						trans.Rollback();
						throw ex;
					}
					
				} catch (Exception e) {
					logger.SevereError("MSAccountDAO:DAOCommand:ExecuteCommand: ", e);
					throw e;
				} finally {
					DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, null);
				}
			}
		}

        #region AccountDAO Members


        public Role GetRole(int Id)
        {
            /* select *
             *	from member_role 
             *	where roleid = 'ROLE ID'
             */

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            Role role = null;

            try
            {
                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();



                string sqlCmd = "select Role_id, Role_Code, Role_Description, Context_Id " +
                        "from member_role (nolock) " +
                        "where role_id = @roleId";

                cmd.CommandText = sqlCmd;
                cmd.Connection = conn;

                cmd.Parameters.AddWithValue("@roleId", Id);
                conn.Open();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    role = new Role(Id);
                    role.Code = DbUtil.getStringValue(dr,"Role_Code");
                    role.Description =DbUtil.getStringValue(dr,"Role_Description");
                    role.ContextId = DbUtil.getIntValue(dr,"Context_Id");
                }

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:GetRole(int): ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            return role;
        }

        public List<int> GetRoleIds(RoleFilter filter)
        {
            /* select Role_Id
             *	from member_role mro
             *			//get role based on username
             *		join member_account_username mau
             *			on mau.account_id = ??
             *      join member_account_role mar
             *          on mau.account_id = mar.account_id and
             *             mro.role_id = mar.role_id 
            */			


            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            //SearchResult<Account> searchResult = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            List<int> roleList = new List<int>();

            try
            {
                List<string> fields = new List<string>();
                List<string> joinTables = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                fields.AddRange(new string[] { "mr.role_id" });

                if (filter.RoleIds.Count > 0)
                {
                    StringBuilder roleids = new StringBuilder();
                    roleids.Append("mr.role_id in (");
                    for (int i = 0; i < filter.RoleIds.Count; i++)
                    {
                        string roleParamName = "@Role_Id" + i;
                        roleids.Append(roleParamName + ",");
                        cmd.Parameters.AddWithValue(roleParamName, filter.RoleIds[i]);
                    }
                    //remove the trailing ,
                    roleids.Length--;
                    roleids.Append(")");
                    where.Add(roleids.ToString());
                } else {

                if (filter.ContextIds.Count > 0)
                {
                    StringBuilder contextids = new StringBuilder();
                    contextids.Append("mr.context_id in (");
                    for (int i = 0; i < filter.ContextIds.Count; i++)
                    {
                        string contextParamName = "@Context_Id" + i;
                        contextids.Append(contextParamName + ",");
                        cmd.Parameters.AddWithValue(contextParamName, filter.ContextIds[i]);
                    }
                    //remove the trailing ,
                    contextids.Length--;
                    contextids.Append(")");
                    where.Add(contextids.ToString());
                }
                if (filter.Code != "")
                {
                    where.Add("mr.role_code like @rolecode");
                    cmd.Parameters.AddWithValue("@rolecode", "%" + filter.Code + "%");
                } 
                if (filter.Description != "")
                {
                    where.Add("mr.role_description like @roledescription");
                    cmd.Parameters.AddWithValue("@roledescription", "%" + filter.Description + "%");
                }
                if (filter.AccountId != Guid.Empty)
                {
                    //get the accounts by status id
                    joinTables.Add("join member_account_role mar on mr.role_id = mar.role_id");
                    joinTables.Add("join member_account mac on mar.account_id = mac.account_id");
                    //joinTables.Add("join member_role mro on mro.role_id = mar.role_id");
//                  join member_account_role mar on ma.account_id = mar.account_id  
//                  join member_role mr on mr.role_id = mar.role_id 
                    StringBuilder status = new StringBuilder();
                    status.Append("mac.account_id = @accountId");
                    cmd.Parameters.AddWithValue("@accountId", filter.AccountId);
                    where.Add(status.ToString());
                }
                }
                cmd.Connection = conn;

                cmd.CommandText = "select " + BuildFields(fields) +
                                " from member_role mr (nolock) " +
                                BuildJoins(joinTables) +
                                BuildWhere(where);

                cmd.Connection.Open();

#if (DEBUG)
                logger.Status("SQL : " + cmd.CommandText);
#endif


                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    //Role role = new Role(DbUtil.getIntValue(dr, "role_id"), DbUtil.getStringValue(dr, "role_code"), DbUtil.getStringValue(dr, "role_description"),DbUtil.getIntValue(dr,"context_id"));
                    roleList.Add(DbUtil.getIntValue(dr,"role_id"));
                }

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:GetRoleIds: ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            //searchResult = new SearchResult<Account>(
            //    accountList.ToArray(),
            //    filter,
            //    accountList.Count,
            //    accountList.Count,
            //    0,
            //    0,
            //    accountList.Count,
            //    1,
            //    accountList.Count,
            //    1);

            return roleList;
        }

        #endregion

        #region AccountDAO Members


        public SearchResult<Role> FindRoles(RoleFilter filter)
        {
            /* select Role_Id
             *	from member_role mro
             *			//get role based on username
             *		join member_account_username mau
             *			on mau.account_id = ??
             *      join member_account_role mar
             *          on mau.account_id = mar.account_id and
             *             mro.role_id = mar.role_id 
            */


            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SearchResult<Role> searchResult = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            List<Role> roleList = new List<Role>();

            try
            {
                List<string> fields = new List<string>();
                List<string> joinTables = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                fields.AddRange(new string[] { "mr.role_id, mr.role_code, mr.role_description, mr.context_id" });

                //if (filter.Context != "")
                //{

                //    StringBuilder accounts = new StringBuilder();
                //    accounts.Append("mr.context_id = @contextId");
                //    cmd.Parameters.AddWithValue("@contextId", SingletonBase<MemberContext>.GetInstance()[filter.Context].Id);
                //    where.Add(accounts.ToString());
                //}

                if (filter.RoleIds.Count > 0)
                {
                    StringBuilder roleids = new StringBuilder();
                    roleids.Append("mr.role_id in (");
                    for (int i = 0; i < filter.RoleIds.Count; i++)
                    {
                        string roleParamName = "@Role_Id" + i;
                        roleids.Append(roleParamName + ",");
                        cmd.Parameters.AddWithValue(roleParamName, filter.RoleIds[i]);
                    }
                    //remove the trailing ,
                    roleids.Length--;
                    roleids.Append(")");
                    where.Add(roleids.ToString());
                }

                if (filter.ContextIds.Count > 0)
                {
                    //get the accounts by username
                    //joinTables.Add("join member_account_username mau on mac.account_id = mau.account_id");

                    StringBuilder contextids = new StringBuilder();
                    contextids.Append("mr.context_id in (");

                    for (int i = 0; i < filter.ContextIds.Count; i++)
                    {
                        string contextParamName = "@Context_Id" + i;
                        contextids.Append(contextParamName + ",");
                        cmd.Parameters.AddWithValue(contextParamName, filter.ContextIds[i]);
                    }

                    //remove the trailing ,
                    contextids.Length--;
                    contextids.Append(")");
                    where.Add(contextids.ToString());
                }
                if (filter.AccountId != Guid.Empty)
                {
                    //get the accounts by status id
                    joinTables.Add("join member_account_role mar on mr.role_id = mar.role_id");
                    joinTables.Add("join member_account mac on mar.account_id = mac.account_id");
                    //joinTables.Add("join member_role mro on mro.role_id = mar.role_id");

                    StringBuilder status = new StringBuilder();
                    status.Append("mac.account_id = @accountId");
                    cmd.Parameters.AddWithValue("@accountId", filter.AccountId);
                    where.Add(status.ToString());
                }
                if (filter.Code != "")
                {
                    where.Add("mr.role_code like @rolecode");
                    cmd.Parameters.AddWithValue("@rolecode", "%" + filter.Code + "%");
                }
                if (filter.Description != "")
                {
                    where.Add("mr.role_description like @roledescription");
                    cmd.Parameters.AddWithValue("@roledescription", "%" + filter.Description + "%");
                }



                cmd.Connection = conn;

                cmd.CommandText = "select " + BuildFields(fields) +
                                " from member_role mr (nolock) " +
                                BuildJoins(joinTables) +
                                BuildWhere(where);

                cmd.Connection.Open();

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Role role = new Role(DbUtil.getIntValue(dr, "role_id"), DbUtil.getStringValue(dr, "role_code"), DbUtil.getStringValue(dr, "role_description"),DbUtil.getIntValue(dr,"context_id"));
                    roleList.Add(role);
                }

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:GetRoleIds: ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            searchResult = new SearchResult<Role>(
                roleList,
                filter,
                roleList.Count,
                roleList.Count,
                0,
                0,
                roleList.Count,
                1,
                roleList.Count,
                1);

            return searchResult;
        }

        #endregion

        #region AccountDAO Members


        public int InsertRole(Role role)
        {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> joinTables = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                DbUtil.buildStringInsert(role.Code, "Role_Code", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(role.Description, "Role_Description", fieldsSql, valuesSql, cmd);
                DbUtil.buildLongInsert(role.ContextId, "Context_Id", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Role (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + "); Select @@IDENTITY";

                cmd.Connection.Open();
#if (DEBUG)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // only update the result after the SQL has executed;
                    int.TryParse(dr[0].ToString(), out result);
                }
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertRole: ", e);
                throw new Exception("Unable to create Role.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result;

        }

        public int UpdateRole(Role role)
        {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
           
            try
            {
                StringBuilder fields = new StringBuilder();
                List<string> joinTables = new List<string>();
                List<string> where = new List<string>();
                string setString = string.Empty;

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                fields.Append(DbUtil.buildStringUpdate(role.Code, "Role_Code", cmd, ","));
                fields.Append(DbUtil.buildStringUpdate(role.Description, "Role_Description", cmd, ","));
                fields.Append(DbUtil.buildDateUpdate(DateTime.Now, "Update_Date", cmd, ","));
                fields.Append(DbUtil.buildLongUpdate(role.ContextId, "Context_Id", cmd, ""));

                where.Add(DbUtil.buildLongCondition(role.Id, "Role_Id", cmd, ""));

                cmd.Connection = conn;

                cmd.CommandText = "update member_role set " +
                                fields.ToString() +
                                BuildWhere(where);

                cmd.Connection.Open();
#if (DEBUG)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();

                // only update the result after the SQL has executed;
                result = role.Id;
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:UpdateRole: ", e);
                throw new Exception("Unable to update Role!");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="contactType"></param>
        /// <param name="value"></param>
        public void InsertContact(Guid accountId, string contactType, string value, int contextId){
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                //DbUtil.buildStringInsert(accountId, "CONTACT_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(SingletonBase<ContactType>.GetInstance()[contactType].Id.ToString(), "CONTACT_TYPE_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(value, "CONTACT_VALUE", fieldsSql, valuesSql, cmd);
                DbUtil.buildLongInsert(contextId, "CONTEXT_ID", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Contact (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + "); Select @@IDENTITY";

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // only update the result after the SQL has executed;
                    int.TryParse(dr[0].ToString(), out result);
                    // now we need to create the account_2_contact record
                    InsertAccount2Contact(accountId, result);
                    
                }
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertRole: ", e);
                throw new Exception("Unable to create Role.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            //return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="contactType"></param>
        /// <param name="value"></param>
        public void UpdateContact(Guid accountId, string contactType, string value, int contextId)
        {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                //DbUtil.buildStringInsert(accountId, "CONTACT_ID", fieldsSql, valuesSql, cmd);
                //DbUtil.buildStringUpdate(SingletonBase<ContactType>.GetInstance()[contactType].Id.ToString(), "CONTACT_TYPE_ID",cmd,",");
                fieldsSql.Append(DbUtil.buildStringUpdate(value, "CONTACT_VALUE", cmd,","));
                fieldsSql.Append(DbUtil.buildLongUpdate(contextId, "CONTEXT_ID", cmd,""));

                where.Add(DbUtil.buildStringCondition(accountId.ToString(), "CONTACT_ID", cmd,","));
                where.Add(DbUtil.buildStringCondition(SingletonBase<ContactType>.GetInstance()[contactType].Id.ToString(), "CONTACT_TYPE_ID", cmd, ""));

                cmd.Connection = conn;

                cmd.CommandText = "update Member_Contact " + fieldsSql.ToString() +
                                BuildWhere(where);

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // only update the result after the SQL has executed;
                    int.TryParse(dr[0].ToString(), out result);
                    // now we need to create the account_2_contact record

                }
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertRole: ", e);
                throw new Exception("Unable to create Role.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            //return result;
        }

        public void UpdateAccount(Guid accountId,string username, string password) {

           
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder usernameFieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();

                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();


               // fieldsSql.Append(DbUtil.buildStringUpdate(password, "password", cmd, " "));
                usernameFieldsSql.Append(DbUtil.buildStringUpdate(username, "username", cmd, " "));

                where.Add(DbUtil.buildStringCondition(accountId.ToString(), "account_id", cmd, ""));

                cmd.Parameters.AddWithValue("@password", password);

                cmd.Connection = conn;

                if (password != "") cmd.CommandText += "update Member_Account set password = @password " + BuildWhere(where) + "; ";
                cmd.CommandText += "update member_account_username set " + usernameFieldsSql.ToString() + BuildWhere(where);

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                  int.TryParse(dr[0].ToString(), out result);
                }



            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertRole: ", e);
                throw new Exception("Unable to create Role.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            //return result;
        
        }

        private bool InsertAccount2Contact( Guid accountId, int contactId) {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            bool result = false;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                DbUtil.buildStringInsert(contactId.ToString(), "CONTACT_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(accountId.ToString(), "ACCOUNT_ID", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Account_Contact (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + ")";

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                result = true;
            }
            catch (Exception e) {
                logger.Error("MSAccountDAO:InsertAccount2Contact: ", e);
                throw new Exception("Unable to create Contact.");
            }
            finally {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result;
        }

        public bool AddRoleToAccount(Account account, Role role) {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            bool result = false;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                DbUtil.buildStringInsert(role.Id.ToString(), "ROLE_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(account.Id.ToString(), "ACCOUNT_ID", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Account_Role (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + ")";

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                result = true;
            }
            catch (Exception e) {
                logger.Error("MSAccountDAO:InsertAccount2Role: ", e);
                throw new Exception("Unable to create Contact.");
            }
            finally {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result;
        }
        #endregion

        #region AccountDAO Members


        public int InsertAddress(Guid AccountId, string AddressType, Address address)
        {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int result = int.MinValue;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                DbUtil.buildStringInsert(address.Line1, MembershipConstants.FIELD_ADDRESS_LINE_1, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.Line2, MembershipConstants.FIELD_ADDRESS_LINE_2, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.Line3, MembershipConstants.FIELD_ADDRESS_LINE_3, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.City, MembershipConstants.FIELD_ADDRESS_CITY, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.State, MembershipConstants.FIELD_ADDRESS_STATE, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.Country, MembershipConstants.FIELD_ADDRESS_COUNTRY, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(address.PostalCode, MembershipConstants.FIELD_ADDRESS_POSTALCODE, fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(AccountId.ToString(), "Updated_By", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(AccountId.ToString(), "Created_By", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Address (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + "); Select @@IDENTITY";

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // only update the result after the SQL has executed;
                    int.TryParse(dr[0].ToString(), out result);
                    // now we need to create the account_2_address record
                    InsertAccount2Address(AccountId, SingletonBase<AddressType>.GetInstance()[AddressType].Id, result);
                    //address.Id = result;
                }
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertRole: ", e);
                throw new Exception("Unable to create Role.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result;
        }


        private bool InsertAccount2Address (Guid accountId,int addressTypeId, int addressId) {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            bool result = false;
            try
            {
                StringBuilder fieldsSql = new StringBuilder();
                StringBuilder valuesSql = new StringBuilder();

                List<string> fields = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                DbUtil.buildLongInsert(addressId, "ADDRESS_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildLongInsert(addressTypeId, "ADDRESS_TYPE_ID", fieldsSql, valuesSql, cmd);
                DbUtil.buildStringInsert(accountId.ToString(), "ACCOUNT_ID", fieldsSql, valuesSql, cmd);

                cmd.Connection = conn;

                cmd.CommandText = "insert into Member_Account_Address (" + fieldsSql.ToString() + ")" +
                                " values (" + valuesSql.ToString() + ")";

                cmd.Connection.Open();
#if (TRACE)
                logger.Status("SQL : " + cmd.CommandText);
#endif
                dr = cmd.ExecuteReader();
                result = true;
            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:InsertAccount2Address: ", e);
                throw new Exception("Unable to create Address.");
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            return result;
        }
        public bool UpdateAddress(Account account, string AddressType, Address address)
        {
            int result = int.MinValue;

            try 
            {
               //Address userAddress = account.GetAddress(AddressType);
               SqlConnection conn = null;
               SqlCommand cmd = null;
               SqlDataReader dr = null;
               

               StringBuilder fields = new StringBuilder();
               List<string> joinTables = new List<string>();
               List<string> where = new List<string>();
               string setString = string.Empty;

               conn = (SqlConnection)this.GetConnection();
               cmd = new SqlCommand();

               /// TODO : fix below
               fields.Append(DbUtil.buildStringUpdate(address.Line1, MembershipConstants.FIELD_ADDRESS_LINE_1, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.Line2, MembershipConstants.FIELD_ADDRESS_LINE_2, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.Line3, MembershipConstants.FIELD_ADDRESS_LINE_3, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.City, MembershipConstants.FIELD_ADDRESS_CITY, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.State, MembershipConstants.FIELD_ADDRESS_STATE, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.PostalCode, MembershipConstants.FIELD_ADDRESS_POSTALCODE, cmd, ","));
               fields.Append(DbUtil.buildStringUpdate(address.Country, MembershipConstants.FIELD_ADDRESS_COUNTRY, cmd, " "));

               where.Add(DbUtil.buildIntCondition(address.Id.ToString(), MembershipConstants.FIELD_ADDRESS_ID, cmd, ""));
                
               cmd.Connection = conn;

               cmd.CommandText = "update member_address set " +
                               fields.ToString() +
                               BuildWhere(where);

               cmd.Connection.Open();
#if (TRACE)
                    logger.Status("SQL : " + cmd.CommandText);
#endif
               dr = cmd.ExecuteReader();
               result = 0;
              
                
            }
            catch(Exception e){
            
            }

            return result != int.MinValue;
            //throw new Exception("The method or operation is not implemented.");
        }

        public void DeleteAddress(Guid AccountId, int AddressId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void DeleteContact(Guid AccountId, int ContactId)
        {

            string sqlText = " delete from member_account_contact " +
                           " where account_id = @accountId and contact_id = @contactId; " +
                           " delete from member_contact " +
                           " where contact_id = @contactId ";  
                           
            DAOCommand cmd = new DAOCommand((SqlConnection)this.GetConnection());
            DAOAction act = new DAOAction();
            act.Action = sqlText;

            act.AddParameter("@contactId", ContactId);
            act.AddParameter("@accountId", AccountId);
           
            cmd.AddAction(act);

            try
            {
                cmd.ExecuteCommand();
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        public AddressFilter GetAddressFilter()
        {
           return new AddressFilter();
        }

        public SearchResult<Address> FindAddress(AddressFilter filter)
        {
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SearchResult<Address> searchResult = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            List<Address> roleList = new List<Address>();

            try
            {
                List<string> fields = new List<string>();
                List<string> joinTables = new List<string>();
                List<string> where = new List<string>();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();

                fields.AddRange(new string[] { 
                    "ma." + MembershipConstants.FIELD_ADDRESS_ID,
                    MembershipConstants.FIELD_ADDRESS_LINE_1,
                    MembershipConstants.FIELD_ADDRESS_CITY,
                    MembershipConstants.FIELD_ADDRESS_COUNTRY,
                    MembershipConstants.FIELD_ADDRESS_LINE_2,
                    MembershipConstants.FIELD_ADDRESS_LINE_3,
                    MembershipConstants.FIELD_ADDRESS_POSTALCODE,
                    MembershipConstants.FIELD_ADDRESS_STATE
                    });

                if (filter.MemberaddressIds.Count > 0)
                {
                    StringBuilder roleids = new StringBuilder();
                    roleids.Append(MembershipConstants.FIELD_ADDRESS_ID + " in (");
                    for (int i = 0; i < filter.MemberaddressIds.Count; i++)
                    {
                        string roleParamName = "@" + MembershipConstants.FIELD_ADDRESS_ID + i;
                        roleids.Append(roleParamName + ",");
                        cmd.Parameters.AddWithValue(roleParamName, filter.MemberaddressIds[i]);
                    }
                    //remove the trailing ,
                    roleids.Length--;
                    roleids.Append(")");
                    where.Add(roleids.ToString());
                }
                

                if (filter.AccountId != Guid.Empty)
                {
                    //get the accounts by status id
                    //if ( ! joinTables.Contains("member_account_2_member_account_address"))
                    //    joinTables.Add("member_account_2_member_account_address");
                    if ( !joinTables.Contains("member_account_address_2_member_address"))
                        joinTables.Add("member_account_address_2_member_address");

                    StringBuilder status = new StringBuilder();
                    status.Append("maa.account_id = @accountId");
                    cmd.Parameters.AddWithValue("@accountId", filter.AccountId);
                    where.Add(status.ToString());
                }
                if (filter.MemberAddressTypeId > 0)
                {
                    if (!joinTables.Contains("member_account_address_2_member_address"))
                        joinTables.Add("member_account_address_2_member_address");
                    where.Add(DbUtil.buildLongCondition(filter.MemberAddressTypeId,MembershipConstants.FIELD_ADDRESS_TYPE_ID,cmd,""));
                    //"mr.role_code like @rolecode");
                    //cmd.Parameters.AddWithValue("@rolecode", "%" + filter.Code + "%");
                }



                cmd.Connection = conn;

                cmd.CommandText = "select " + BuildFields(fields) +
                                " from member_address ma (nolock) " +
                                BuildJoins(joinTables) +
                                BuildWhere(where);

                cmd.Connection.Open();

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Address address = new Address(DbUtil.getIntValue(dr, MembershipConstants.FIELD_ADDRESS_ID),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_LINE_1),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_LINE_2),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_LINE_3),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_STATE),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_CITY),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_POSTALCODE),
                        DbUtil.getStringValue(dr, MembershipConstants.FIELD_ADDRESS_COUNTRY));
                    roleList.Add(address);
                }

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:GetRoleIds: ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }

            searchResult = new SearchResult<Address>(
                roleList,
                filter,
                roleList.Count,
                roleList.Count,
                0,
                0,
                roleList.Count,
                1,
                roleList.Count,
                1);

            return searchResult;
        }

        #endregion

        #region AccountDAO Members


        public bool UpdateAccountStatus(Guid uid, AccountStatus newStatus)
        {
            int result = int.MinValue;
            //reasoning for a global try catch:
            //if the function fails at any moment we want to exit and get back to the callee
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                List<string> where = new List<string>();
                StringBuilder fields = new StringBuilder();

                conn = (SqlConnection)this.GetConnection();
                cmd = new SqlCommand();


                fields.Append(DbUtil.buildLongUpdate(newStatus.Id, "ACCOUNT_STATUS_ID", cmd,""));
                
                where.Add(DbUtil.buildStringCondition(uid.ToString(),"ACCOUNT_ID",cmd,""));

                
                cmd.Connection = conn;

                cmd.CommandText = "update member_account set " +
                                fields.ToString() + 
                                BuildWhere(where);

                cmd.Connection.Open();

                dr = cmd.ExecuteReader();

                result = 0;

            }
            catch (Exception e)
            {
                logger.Error("MSAccountDAO:UpdateAccountStatus: ", e);
            }
            finally
            {
                DbUtil.close((cmd == null) ? null : cmd.Connection, cmd, dr);
            }


            return result != int.MinValue;
        }

        #endregion
    }
}
