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


	public class CreateUserAction : web.controller.action.Action
	{
		private	const	string	EVENT_SUCCESS	= "success";
		private const	string	EVENT_ERROR		= "error";
        
		private			string	connectionType	= null;
        private         string  requestItemName = "";
        private         string  accountStatus   = "";    
		public string ConnectionType
		{
			get {
				return this.connectionType;
			}
			set {
				this.connectionType = value;
			}
		}
        public string AccountStatus {
            get { return accountStatus ;}
            set { accountStatus = value;}
        }

		#region Action Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
					Guid accountId = Guid.NewGuid();
					string username = context.Request[MembershipConstants.PARAM_USERNAME];
					string reqPassword = context.Request[MembershipConstants.PARAM_PASSWORD];

					string encryptPwd = CryptoUtil.EncryptString(
								reqPassword,
								MembershipConstants.ENCRYPTION_KEY,
								MembershipConstants.INIT_VECTOR,
								EncryptionAlgorithm.TripleDes);
				    string statusCode = accountStatus;
                    if ( context.Request[MembershipConstants.PARAM_STATUSCODE] != null ) {
                        statusCode = context.Request[MembershipConstants.PARAM_STATUSCODE];
                    }

					Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Status("SaveUserAction: encryped password: " + encryptPwd);

					try {
								//create the user
                        AccountDAO dao = AccountDAOFactory.GetAccountDAO(this.ConnectionType);
                        
						dao.CreateAccount(accountId, encryptPwd, username, statusCode);
					    retEvent = EVENT_SUCCESS;
					} catch (Exception e) {
						Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAction:Execute:AccountDAO", e);
					}
                    context.Request["uid"] = accountId.ToString();


			} catch (Exception e) {
				Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAction:Execute", e);
			}

			
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
