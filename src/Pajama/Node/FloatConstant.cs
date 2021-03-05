using System.Collections.Generic;

namespace Pajama.Node
{
	internal class FloatConstant : Expression
	{
		public double Value { get; private set; }

		public FloatConstant(double value, Token token)
			: base(token)
		{
			this.Value = value;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.ResolvesTo = ZType.FLOAT;
		}
	}
}
