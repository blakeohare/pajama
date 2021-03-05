using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ConstructorCall : Expression
	{
		public ZType Class { get; private set; }
		public Expression[] Args { get; private set; }
		public Expression[] Keys { get; private set; }
		public Expression[] Values { get; private set; }
		public bool StartingValues { get; private set; }

		public ConstructorCall(ZType className, Expression[] args, Expression[] keysOrNull, Expression[] startingValues, Token token)
			: base(token)
		{
			this.Class = className;
			this.Args = args;
			this.Keys = keysOrNull;
			this.Values = startingValues;
			this.StartingValues = this.Values.Length > 0;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			typeResolver.ResolveType(scope, this.Class);
			this.ResolvesTo = this.Class;

			if (!this.ResolvesTo.Class.VariableLengthGenerics && this.ResolvesTo.Class.ConstructorArgs.Length != this.Args.Length)
			{
				throw new ParserException(this.Token, "Constructor has incorrect number of arguments.");
			}

			foreach (Expression expr in this.Args)
			{
				expr.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			}

			if (this.Keys != null)
			{
				foreach (Expression expr in this.Keys)
				{
					expr.ResolveTypes(typeResolver, scope, typesByVariableUsage);
				}
			}

			foreach (Expression expr in this.Values)
			{
				expr.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			}

			this.ResolvesTo = this.Class;
		}
	}
}
