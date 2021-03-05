using System.Collections.Generic;

namespace Pajama.Node
{
	internal class BooleanConstant : Expression
	{
		public bool Value { get; private set; }

		public BooleanConstant(bool value, Token token)
			: base(token)
		{
			this.Value = value;
		}
		
		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.ResolvesTo = ZType.BOOL;
		}
	}
}
