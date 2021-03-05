using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ExpressionAsExecutable : Executable
	{
		public Expression Expression { get; private set; }

		public ExpressionAsExecutable(Expression expression, Token token)
			: base(token)
		{
			this.Expression = expression;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			this.Expression.ResolveTypes(typeResolver, scope, typesByVariableUsage);
		}
	}
}
