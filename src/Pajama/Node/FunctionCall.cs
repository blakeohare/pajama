using System;
using System.Collections.Generic;

namespace Pajama.Node
{
	internal class FunctionCall : Expression
	{
		public Expression Root { get; private set; }
		public Expression[] Args { get; private set; }

		public FunctionCall(Expression root, Expression[] args, Token token)
			: base(token)
		{
			this.Root = root;
			this.Args = args;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.Root.ResolveTypes(typeResolver, scope, typesByVariableUsage);

			ZType functionType = this.Root.ResolvesTo;
			if (functionType.Generics.Length != this.Args.Length + 1)
			{
				throw new ParserException(this.Token, "Wrong number of args.");
			}

			for (int i = 0; i < this.Args.Length; ++i)
			{
				Expression arg = this.Args[i];
				arg.ResolveTypes(typeResolver, scope, typesByVariableUsage);
				ZType genericType = functionType.Generics[i];
				if (genericType.RootType.Length == 1)
				{
					throw new Exception("Function generics were not resolved.");
				}
				if (!genericType.CanBeCastedFrom(arg.ResolvesTo, false))
				{
					throw new ParserException(arg.Token, "Function argument is of wrong type.");
				}
			}

			this.ResolvesTo = functionType.Generics[functionType.Generics.Length - 1];
		}
	}
}
