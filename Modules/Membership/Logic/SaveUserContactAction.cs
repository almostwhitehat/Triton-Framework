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


	public class SaveUserContactAction : web.controller.action.Action
	{
		private	const	string	EVENT_SUCCESS	= "success";
		private const	string	EVENT_ERROR		= "error";
        
		//private const	string	PARAM_USERNAME	= "username";
		//private const	string	PARAM_PASSWORD	= "password";
		//private const	string	PARAM_FORMNAME	= "formname";

		private			string	connectionType	    = null;
        private         string  requestItemName     = "";
        private         string  userRequestItemName = "users";
		
        public string ConnectionType
		{
			get {
				return this.connectionType;
			}
			set {
				this.connectionType = value;
			}
		}

        public string UserRequestItemName
        {
            get
            {
                return userRequestItemName;
            }
            set
            {
                userRequestItemName = value;
            }
        }
		#region Action Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
                // TODO: should not be hard-coded to "users" -- has to match RequestItemName on GetUser action
                SearchResult<Account> acctResults = (SearchResult<Account>)context.Request.Items[userRequestItemName];

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


                //  get the contact type (name) and value from the request
                string[] contactType = context.Request[MembershipConstants.PARAM_CONTACT_TYPE].Split(',');
                string[] contactValue = context.Request[MembershipConstants.PARAM_CONTACT_VALUE].Split(',');

                AccountDAO dao = AccountDAOFactory.GetAccountDAO(this.ConnectionType);
                //  determine if the attribute specified in the request already exists in the account


                if (contactType.Length == contactValue.Length)
                {
                    for (int i = 0; i < contactType.Length; i++)
                    {

                        if (account.GetContactTypes().Contains(contactType[i]))
                        { // we have this type of contact already
                            int index = account.GetContact(contactType[i]).IndexOf(new MembershipAttribute(contactType[i], contactValue[i]));
                            if (index == -1)
                            {
                                // create the new contact
                                dao.InsertContact(account.Id, contactType[i], contactValue[i], 1);
                            }
                            else
                            {
                                if (account.GetContact(contactType[i])[index].Value != contactValue[i])
                                {
                                    account.GetContact(contactType[i])[index].Value = contactValue[i];
                                    dao.UpdateContact(account.Id, contactType[i], contactValue[i], 1);
                                }
                            }

                        }
                        else
                        { // this contact type does not exists. 
                            // consequently this contact cannot exist.
                            // so we create it.
                            account.AddContact(contactType[i], contactValue[i], 1);
                            dao.InsertContact(account.Id, contactType[i], contactValue[i], 1);
                        }

                    }
                    retEvent = EVENT_SUCCESS;
                }
                



                //else {
                //    // lets update the value if different
                //    int index = account.GetContact(contactType).IndexOf(new MembershipAttribute(contactType, contactValue));
                //    if ( index > 0 ) {
                //        if ( account.GetContact(contactType)[index].Value != contactValue ) {
                //            account.GetContact(contactType)[index].Value = contactValue;
                //            dao.UpdateContact(account.Id, contactType, contactValue, 1);
                //        }
                //    }
                //}


			} catch (Exception e) {
				Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserContactAction:Execute", e);
			}

			#region Future Support
			/*	Guid accountId = null;

			//first find out if the account_id is in the session
			if (System.Web.HttpContext.Current.Session[Constants.SESSION_USER_ACCOUNT] != null) {
				accountId = (Guid)System.Web.HttpContext.Current.Session[Constants.SESSION_USER_ACCOUNT];
			}
		

			//if yes get the account and start the update of it
			//if no start creating new account



			try {
				// first check if the account already exists.
				MSAccountDAO dao = new MSAccountDAO(this.ConnectionType);
				Account oldAccount = dao.GetAccount(accountid);
				Account newAccount = MakeNewAccount(context.Request);

				if (oldAccount == null) {
					// create the account with the dao.
					dao.CreateAccount(newAccount.Id, newAccount.Password, newAccount.UserName, 0);
					oldAccount = new Account(new Guid(), "", -1);
				} else {
					// check the username and the password for 
					if (oldAccount.UserName != newAccount.UserName) {
						// add a DAOAction to the dao to change the username

					}
					if (oldAccount.Password != newAccount.Password) {
						// add a DAOAction to the dao to change the password

					}
				}
				// process the account Attributes
				foreach (string sType in newAccount.GetAttributeTypes()) {
					foreach (MembershipAttribute mAttr in newAccount.GetContact(sType)) {
						// does this MembershipAttribute exist in the old account
						foreach (MembershipAttribute oattr in oldAccount.GetContact(sType)) {
							if (!oattr.Equals(mAttr)) {
								// we need to create this. add the command to the DAO
								dao.InsertAttribute(newAccount.Id, sType, mAttr.Value);
							} else {
								// we need to alter the existing Attribute;
								dao.UpdateAttribute(newAccount.Id, sType, mAttr.Value);
							}
						}
					}
				}
			} catch (Exception e) {
				Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAction", e);
			}
			*/
			#endregion

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


		#region Future Support

		/*private Account MakeNewAccount(
			FPRequest request)
		{
			Account account = new Account();


			//get id out of session
			//TODO: admin stuff here
			if (WebUtil.HasValue(PARAM_MEMBER_ID)) {
				account.Id = new Guid(request[PARAM_MEMBER_ID]);
			} else {
				account.Id = Guid.NewGuid();
			}



			foreach (XmlNode formParameter in GetFormParameters()) {
				string paramName = formParameter.Attributes["name"].Value;
				// if this parameter has been passed in then process it.
				//if ( request[paramName] != null )
				//{
				string value = request[paramName];
				// the mapping is a node in the parameter.
				XmlNode map = formParameter.SelectSingleNode("map");
				string to = map.Attributes["to"].Value;
				string? key = map.Attributes["key"].Value;
				string? property = map.Attributes["property"].Value;
				// now we need to know where this value belongs
				if (key != null) {
					// then this is either a generic attribute or a specific name/value pair.
					if (property != null) {
						// this is a specific property of the account
						// use reflection to call the proper method.
						//  if the attribute matches a property on the State set the property
						if (ReflectionUtil.HasProperty(account, property)) {
							PropertyInfo prop = ReflectionUtil.GetProperty(account, property);
							//  determine if the property is settable -- 
							//  if it is, set it, otherwise see if there is a "Set"
							//  method for the property and use that instead

							ReflectionUtil.GetPropertyValue(account, property);
							if (prop.CanWrite) {
								ReflectionUtil.SetPropertyValue(account, property, value);
							} else if (ReflectionUtil.HasMethod(account, "Set" + attrName, "".GetType())) {
								ReflectionUtil.CallMethod(property, "Set" + property, value);
							}

							//  otherwise, 
						} else if (ReflectionUtil.HasMethod(property, "AddAttribute", new Type[] { "".GetType(), "".GetType() })) {
							ReflectionUtil.CallMethod(property, "AddAttribute", property, value);
						}
					} else {
						// this is a generic property of the account
						// create an AccountAttributeType
						AccountAttributeType aaType = new AccountAttributeType();
						aaType.Code = key;
						aaType.Description = value;
						aaType.Id = -1;
						account.Attribute.Add(aaType);
					}
				} else {
					// this is a specific member if the account
					switch (to) {
						case "UserName":
							account.UserName = value;
							break;
						case "Password":
							account.Password = value;
							break;
					}
				}

			}


			return account;
		}
		private XmlNodeList GetFormParameters(string formname)
		{
			// pull account information for the form from the Config File.
			// form name is found in the formname request parameer
			XmlNodeList result = null; // new XmlNodeList();
			string formName = request[PARAM_FORMNAME];

			XmlDocument doc = new XmlDocument();

			string configPath = @"C:\Documents and Settings\cummings\My Documents\Google Talk Received Files\membership.xml"; // ConfigurationManager.AppSettings["formparameterdefinitions"];

			doc.Load(configPath);

			doc.LoadXml(doc.SelectSingleNode("//Form[@name=\"" + formName + "\"]").InnerXml);
			//doc.LoadXml(doc.SelectSingleNode("//Fields").InnerXml);
			result = doc.FirstChild.ChildNodes;
			//{
			//    try
			//    {
			//        result.Add(node.Attributes["name"].Value);
			//        XmlNode map = node.SelectSingleNode("map");
			//        if (map != null)
			//        {
			//            // "to attribute is required
			//            apps.Add("to=" + map.Attributes["to"].Value);
			//            // key attribute and propert attribute are optional
			//            if (map.Attributes["key"] != null) apps.Add("key=" + map.Attributes["key"].Value);
			//        }

			//    }
			//    catch { }
			//}


			//config.LoadXml(config.SelectSingleNode("form/" + formName).OuterXml);
		}*/

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
