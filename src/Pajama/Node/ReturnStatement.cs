using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ReturnStatement : Executable
	{
		public Expression Expression { get; private set; }

		public ReturnStatement(Expression expression, Token token)
			: base(token)
		{
			this.Expression = expression;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			if (this.Expression != null)
			{
				if (member == null)
				{
					throw new ParserException(this.Token, "Cannot return a value from a constructor.");
				}

				if (member.Type == ZType.VOID) {
					throw new ParserException(this.Token, "Cannot return a value from a method that has a void return type.");
				}

				this.Expression.ResolveTypes(typeResolver, scope, typesByVariableUsage);

				this.Expression = this.Expression.EffectiveValue;

				if (!member.Type.CanBeCastedFrom(this.Expression.ResolvesTo, false))
				{
					throw new ParserException(this.Token, "This value is not compatible with the return type of this method.");
				}
			}
		}

		public static ReturnStatement Parse(Tokens tokens)
		{
			Token token = tokens.PopExpected("return");
			if (tokens.IsNext(";"))
			{
				return new ReturnStatement(null, token);
			}
			else
			{
				Expression expression = ExpressionParser.Parse(tokens);
				tokens.PopExpected(";");
				return new ReturnStatement(expression, token);
			}
		}
	}
}
