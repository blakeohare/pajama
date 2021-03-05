using System.Collections.Generic;

namespace Pajama.Node
{
	internal class Noop : Executable
	{
		public Noop() : base(null) { }

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			// nothing to do.
		}
	}
}
