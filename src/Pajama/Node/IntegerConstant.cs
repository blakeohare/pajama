using System.Collections.Generic;

namespace Pajama.Node
{
	internal class IntegerConstant : Expression
	{
		public int Value { get; private set; }

		public IntegerConstant(int value, Token token)
			: base(token)
		{
			this.Value = value;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.ResolvesTo = ZType.INT;
		}
	}
}
