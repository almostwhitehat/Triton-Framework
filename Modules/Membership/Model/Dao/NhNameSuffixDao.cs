using System;
using System.Collections.Generic;
using Triton.NHibernate.Model.Dao;

namespace Triton.Membership.Model.Dao
{
	public class NhNameSuffixDao : NHibernateBaseDao<NameSuffix>, INameSuffixDao
	{
		#region INameSuffixDao Members

		public NameSuffix Get(int id)
		{
			return base.Get(id);
		}


		public NameSuffix Get(string shortCode)
		{
			IList<NameSuffix> codes = base.Get(new NameSuffix
			                                      {
			                                      	ShortCode = shortCode
			                                      });

			if (codes.Count == 0) {
				throw new ApplicationException(string.Format("Could not find the NameSuffix by the ShortCode of: {0}.", shortCode));
			}

			return codes[0];
		}

		#endregion
	}
}