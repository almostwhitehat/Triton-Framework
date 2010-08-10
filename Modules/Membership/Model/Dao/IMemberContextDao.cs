using System.Collections.Generic;

namespace Triton.Membership.Model.Dao
{
	public interface IMemberContextDao
	{
		MemberContext Get(int id);
		IList<MemberContext> Get(MemberContext example);
		void Save(MemberContext context);
		void Delete(MemberContext context);
		MemberContext Get(string code);
	}
}