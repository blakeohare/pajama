using System.Collections.Generic;

namespace Pajama.Node
{
	internal class Negative : Expression
	{
		public Expression Expression { get; private set; }

		public Negative(Expression expression, Token token)
			: base(token)
		{
			this.Expression = expression;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.Expression.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			ZType type = this.Expression.ResolvesTo;
			if (type != ZType.INT && type != ZType.FLOAT)
			{
				throw new ParserException(this.Token, "Incompatible type: negative can only be applied to numbers.");
			}

			this.ResolvesTo = type;
		}
	}
}
