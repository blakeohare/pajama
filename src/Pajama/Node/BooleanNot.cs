using System.Collections.Generic;

namespace Pajama.Node
{
	internal class BooleanNot : Expression
	{
		public Expression Expression { get; private set; }

		public BooleanNot(Expression expression, Token token)
			: base(token)
		{
			this.Expression = expression;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.Expression.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			if (this.Expression.ResolvesTo != ZType.BOOL)
			{
				throw new ParserException(this.Token, "Value must resolve to a boolean.");
			}

			this.ResolvesTo = ZType.BOOL;
		}
	}
}
