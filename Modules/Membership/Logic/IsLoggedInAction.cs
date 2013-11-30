using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Model.Dao.Support;
using Triton.Membership.Support;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Membership.Logic
{
	/// <remarks>
	/// Returned events:<br/>
	/// <b>yes</b> - the account/user with the given username is logged in.<br/>
	/// <b>no</b> - the account/user with the given username is not logged in.<br/>
	/// </remarks>
	public class IsLoggedInAction : IAction
	{
		private const string AUTHENTICATION_TIME_SPAN_APP_SETTING_NAME = "authenticationtimespan";
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.No;

			try {
				//  try to get the account ID from the session
				// TODO: this should not access HttpContext directly!
				Account acct = HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] as Account;

				this.logger.Debug(debug => debug("Looking for Authentication cookie {0}.", MembershipConstants.COOKIE_USER_ACCOUNT));

				if (context.Request.GetCookie(MembershipConstants.COOKIE_USER_ACCOUNT) != null) {
					this.logger.Debug(debug => debug("Found Authentication cookie {0}.", MembershipConstants.COOKIE_USER_ACCOUNT));

					byte[] bytes = ASCIIEncoding.ASCII.GetBytes(MembershipConstants.INIT_VECTOR);
					MvcHttpCookie authcookie = (MvcHttpCookie)context.Request.GetCookie(MembershipConstants.COOKIE_USER_ACCOUNT);

					// decode the cookie value to include the dnsname and accountid
					// check to see if coockie has been expired
					// this does not work as the expires is never set properly
					this.logger.Debug(string.Format("Cookie found. Cookie expires: {0}", authcookie.Expires.ToShortTimeString()));

					if (acct == null) {
						this.logger.Info(info => info("Could not find the account in the session. Processing Cookie. Retrieving authentication information."));

						DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
						MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(authcookie.Value));
						CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);

						StreamReader reader = new StreamReader(cryptoStream);

						string decrypt = reader.ReadToEnd();
						string[] pairs = decrypt.Split('|');

						// make sure we have three items and the host is the second item.
						if ((pairs.Length == 3) && (pairs[1] == context.Request.IP)) {
							this.logger.Debug(debug => debug("Found User ID {0}.", pairs[2]));

							// get the user and store it in the session
							IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

							acct = dao.Get(new Guid(pairs[2]));

							if (acct != null) {
								this.logger.Debug(debug => debug("Retrieved the account from the id that was stored in the cookie."));

								// TODO: this should not access HttpContext directly!
								HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] = acct;
								
								retEvent = Events.Yes;
							} else {
								throw new ArgumentException("The search for the account by id from the database did not return any results.");
							}
						}
					}

					// get authentication timespan
					int hours = 1;
					if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[AUTHENTICATION_TIME_SPAN_APP_SETTING_NAME])) {
						int.TryParse(ConfigurationManager.AppSettings[AUTHENTICATION_TIME_SPAN_APP_SETTING_NAME], out hours);
					} else {
						this.logger.Debug(
							debug => debug("Using the default 1 hour expiration for the cookie. You can set {0} to increase or decrease the time in hours in web.config appSettings.", AUTHENTICATION_TIME_SPAN_APP_SETTING_NAME));
					}

					this.logger.Debug(debug => debug("Resetting the cookie expiration time and saving it."));
					authcookie.Expires = DateTime.Now.AddHours(hours);
					context.Request.SetResponseCookie(authcookie);
				}

				//  if we got the account from the session, set the return event accordingly
				if (acct != null) {
					retEvent = Events.Yes;

					//  if there is not a "uid" parameter in the request, add one for the current
					//  account (for subsequent GetUser actions)
					// this dont work if you want to save a new user.
					/*if (context.Request[ParameterNames.Account.ID] == null ||
						context.Request[ParameterNames.Account.ID] != acct.Id.ToString()) {

						context.Request[ParameterNames.Account.ID] = acct.Id.ToString();
					}*/
				}

			} catch (Exception ex) {
				this.logger.Error(error => error("Error in checking whether or not the user is logged in."), ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Yes
			{
				get { return EventNames.YES; }
			}

			public static string No
			{
				get { return EventNames.NO; }
			}
		}

		#endregion
	}
}