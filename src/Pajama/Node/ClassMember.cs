namespace Pajama.Node
{
	internal abstract class ClassMember
	{
		public string Name { get; private set; }
		public ZType Type { get; private set; }
		public Class Parent { get; private set; }
		public bool IsStatic { get; private set; }

		public ClassMember(string name, ZType type, Class parent, bool isStatic)
		{
			this.Name = name;
			this.Type = type;
			this.Parent = parent;
			this.IsStatic = isStatic;
		}

		public void ResolveType(TypeResolver typeResolver)
		{
			typeResolver.ResolveType(this.Parent, this.Type);
		}
	}
}
