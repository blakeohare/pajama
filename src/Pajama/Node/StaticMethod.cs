using System.Collections.Generic;

namespace Pajama.Node
{
	internal class StaticMethod : Expression
	{
		public Class Class { get; private set; }
		public ClassMember Member { get; private set; }
		public string Name { get; private set; }
		public StaticMethod(Class cls, ClassMember member, string name, Token token)
			: base(token)
		{
			this.Class = cls;
			this.Member = member;
			this.Name = name;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			// resolved upon creation
		}
	}
}
