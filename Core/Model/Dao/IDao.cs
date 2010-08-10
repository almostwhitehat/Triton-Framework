using System.Data;

namespace Triton.Model.Dao
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>IDao</b> defines the interface for implementing a Dao (Data Access Object)
	/// for the system.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IDao
	{
		string Name { get; }


		string ConnectionType { get; }


		IDbConnection GetConnection();
	}
}