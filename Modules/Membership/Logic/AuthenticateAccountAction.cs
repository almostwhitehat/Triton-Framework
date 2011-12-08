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

	#region History

	// History:

	#endregion

	public class AuthenticateAccountAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

        public string RequiredAccountStatus { get; set; }

		public AuthenticateAccountAction()
		{
			this.RequiredAccountStatus = "active";
		}


		
		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				IAccountDao dao = DaoFactory.GetDao<IAccountDao>();
				//  get the filter from the DAO
				AccountFilter filter = dao.GetFilter();

				filter.Fill(context.Request);

                // moved to filter extensions
				//filter.Status = DaoFactory.GetDao<IAccountStatusDao>().Get("active");

				//  get the account for the given username
				SearchResult<Account> result = dao.Find(filter);

				//  make sure we got exactly 1 matching account
				if (result.NumberReturned == 1) {
					string reqPwd = context.Request[ParameterNames.Account.PASSWORD];

					//  make sure we got a password param
					if (reqPwd != null) {
						//  encrypt the inputted password
						string encryptPwd = new EncryptionManager().EncryptField(reqPwd);

						Account acct = result.Items[0];
						if (acct.Status.Code == this.RequiredAccountStatus) {
							//  see if the encrypted input password matches the encrypted password from the DB
							if (encryptPwd == acct.Password) {
								retEvent = Events.Ok;

								// TODO: this should not access HttpContext directly!
								HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] = acct;

								try {
									this.logger.Debug(debug => debug("Looking for authentication cookie."));


									MvcCookie authcookie = context.Request.GetCookie(MembershipConstants.COOKIE_USER_ACCOUNT);

									// encrypt the cookie value - include the dnsname and accountid
									byte[] bytes = ASCIIEncoding.ASCII.GetBytes(MembershipConstants.INIT_VECTOR);
									DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
									MemoryStream memoryStream = new MemoryStream();
									CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);

									StreamWriter writer = new StreamWriter(cryptoStream);
									writer.Write(DateTime.Now + "|" + context.Request.IP + "|" + acct.Id);
									writer.Flush();
									cryptoStream.FlushFinalBlock();
									writer.Flush();


									// get authentication timespan
									int hours = 1;
									if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["authenticationtimespan"])) {
										int.TryParse(ConfigurationManager.AppSettings["authenticationtimespan"], out hours);
									}
									if (authcookie == null) {
										authcookie = new MvcCookie(MembershipConstants.COOKIE_USER_ACCOUNT);

										this.logger.Debug(debug => debug("Creating a new Authentication Cookie."));
									}
									authcookie.Value = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
									authcookie.Expires = DateTime.Now.AddHours(hours);

									this.logger.Debug(debug => debug("Saving Authentication Cookie."));
									context.Request.SetResponseCookie(authcookie);
								} catch (Exception e) {
									this.logger.Warn(warn => warn("Authentication Cookie could not be saved."), e);
								}
							}
						}
					}
				}
			} catch (Exception e) {
				this.logger.Error(
					error => error("AuthenticateAccountAction: username='{0}'.",
					               context.Request[ParameterNames.Account.USERNAME]),
					e);
			}

			return retEvent;
		}

		#endregion

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