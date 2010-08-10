using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Support;
using Triton.Membership.Support.Request;
using Triton.Model.Dao;
using Triton.Web.View;

namespace Triton.Membership.View.Web
{
	public static class WebPageExtensions
	{
		public static Account GetAuthenticatedUser(this WebPage page)
		{
			Account result = null;

			if ((page.Request.Items[ItemNames.Account.AUTHENTICATED_ACCOUNT] != null) &&
			    (page.Request.Items[ItemNames.Account.AUTHENTICATED_ACCOUNT] is Account)) {
				result = page.Request.Items[ItemNames.Account.AUTHENTICATED_ACCOUNT] as Account;
			} else {
				if (page.Session[MembershipConstants.SESSION_USER_ACCOUNT] != null) {

					//temporary fix before the real implementation will be put in place
					//reason, account in the session will be a nhibernate account, which could have missing ISession
					//which would throw errors.
					result = page.Session[MembershipConstants.SESSION_USER_ACCOUNT] as Account;

					IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

					result = dao.Get(result.Id.Value);
				}
			}
			return result;
		}
	}
}