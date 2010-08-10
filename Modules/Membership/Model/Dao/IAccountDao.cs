using System;
using System.Collections;
using System.Collections.Generic;
using Triton.Model;

namespace Triton.Membership.Model.Dao
{
	public interface IAccountDao
	{
		AccountFilter GetFilter();
		Account Get(Guid id);
		IList<Account> Get(Account example);
		SearchResult<Account> Find(AccountFilter filter);
		void Save(Account account);
		void Delete(Account account);
		void Save(IList<Account> account);
	}
}