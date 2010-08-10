using System;
using System.Collections.Generic;
using Triton.NHibernate.Model.Dao;

namespace Triton.Membership.Model.Dao
{
	public class NhMemberContextDao : NHibernateBaseDao<MemberContext>, IMemberContextDao
	{
		#region IMemberContextDao Members

		public MemberContext Get(int id)
		{
			return base.Get(id);
		}


		public MemberContext Get(string code)
		{
			IList<MemberContext> contexts = base.Get(new MemberContext {
			                            	Code = code
			                            });

			if (contexts.Count == 0) {
				throw new ApplicationException(string.Format("Could not find the MemberContext by the Code of: {0}.", code));
			}

			return (contexts.Count > 0) ? contexts[0] : null;
		}

		#endregion
	}
}