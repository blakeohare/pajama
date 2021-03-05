using System.Collections.Generic;

namespace Pajama.Node
{
	internal class NullConstant : Expression
	{
		public NullConstant(Token token) : base(token) { }

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			// uhhh....
		}
	}
}
