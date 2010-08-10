namespace Triton.Membership.Model.Dao
{
	public interface IAccountStatusDao
	{
		AccountStatus Get(int id);


		AccountStatus Get(string code);


		void Save(AccountStatus status);


		void Delete(AccountStatus status);
	}
}