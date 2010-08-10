namespace Triton.Membership.Model
{
	public class AttributeType
	{
		public virtual int? Id { get; set; }

		public virtual string Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }

		public virtual float? Weight { get; set; }

		public virtual int? Version { get; set; }
	}
}