using System.Collections;

namespace Triton.Support.Mail
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// An abstract class for mailer objects.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class Mailer
	{
		private readonly ArrayList to = new ArrayList();


		/// <summary>
		/// A list of recipient email addresses.
		/// </summary>
		public ArrayList To
		{
			get { return this.to; }
		}


		/// <summary>
		/// Defines the interface of the method that sends out emails.
		/// </summary>
		public abstract void Send();
	}
}