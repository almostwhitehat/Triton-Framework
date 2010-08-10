using System;
using System.Collections.Generic;
using Triton.NHibernate.Model.Dao;

namespace Triton.Membership.Model.Dao
{

	#region History

	//History:

	#endregion

	///<summary>
	///</summary>
	public class NhAccountStatusDao : NHibernateBaseDao<AccountStatus>, IAccountStatusDao
	{
		#region IAccountStatusDao Members

		public AccountStatus Get(int id)
		{
			return base.Get(id);
		}


		public AccountStatus Get(string code)
		{
			IList<AccountStatus> statuses = base.Get(new AccountStatus
			                                         {
			                                         	Code = code
			                                         });
			if (statuses.Count == 0) {
				throw new ApplicationException(string.Format("Could not find the AccountStatus by the Code of: {0}.", code));
			}

			return statuses[0];
		}

		#endregion
	}
}