namespace Triton.Membership.Model
{
	public class Person
	{
		public virtual long? Id { get; set; }

		public virtual long? Version { get; set; }

		public virtual Name Name { get; set; }

		public virtual string Email { get; set; }

		public virtual string Phone { get; set; }
	}
}