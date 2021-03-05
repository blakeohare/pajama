namespace Pajama.Node
{
	internal class Field : ClassMember
	{
		public Expression DefaultValue { get; set; }
		public Field(string name, ZType type, Expression defaultValue, Class parent, bool isStatic)
			: base(name, type, parent, isStatic)
		{
			this.DefaultValue = defaultValue;
		}
	}
}
