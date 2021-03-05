using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ThisExpression : Expression
	{
		public ThisExpression(Class resolvesTo, Token token)
			: base(token)
		{
			this.ResolvesTo = resolvesTo.Type;
			this.ResolvesTo.Class = resolvesTo;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			// Nothing to do. This is created during resolution phase.
		}
	}
}
