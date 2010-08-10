namespace Triton.Membership.Model
{
	public class NameSuffix
	{
		public virtual int? Id { get; set; }

		public virtual string LongCode { get; set; }

		public virtual string ShortCode { get; set; }

		public virtual int? Version { get; set; }
	}
}