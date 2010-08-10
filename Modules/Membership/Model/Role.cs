namespace Triton.Membership.Model
{
	/// <summary>
	/// 
	/// </summary>
	public class Role
	{
		public virtual int? Id { get; set; }

		public virtual string Code { get; set; }

		public virtual string Description { get; set; }

		public virtual int? Version { get; set; }

		public virtual MemberContext Context { get; set; }
	}
}