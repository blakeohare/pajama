using System.Collections.Generic;

namespace Pajama.Node
{
	internal class BracketIndex : Expression
	{
		public Expression Root { get; private set; }
		public Expression Index { get; private set; }

		public BracketIndex(Expression root, Expression index, Token token)
			: base(token)
		{
			this.Root = root;
			this.Index = index;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.Root.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			this.Index.ResolveTypes(typeResolver, scope, typesByVariableUsage);

			ZType rootType = this.Root.ResolvesTo;
			ZType indexType = this.Index.ResolvesTo;

			if (rootType.RootType == "List")
			{
				if (indexType != ZType.INT)
				{
					throw new ParserException(this.Token, "List indices must be an integer.");
				}
				this.ResolvesTo = rootType.Generics[0];
			}
			else if (rootType.RootType == "Map")
			{
				// TODO: if you ever allow keys that aren't strings or integers, you'll need to fix this up.
				if (!indexType.CanBeCastedFrom(rootType.Generics[0], true))
				{
					throw new ParserException(this.Token, "Map index type is invalid.");
				}

				this.ResolvesTo = rootType.Generics[1];
			}
			else if (rootType == ZType.STRING)
			{
				if (indexType != ZType.INT)
				{
					throw new ParserException(this.Token, "String indices must be integers.");
				}
				this.ResolvesTo = ZType.STRING;
			}
			else
			{
				throw new ParserException(this.Token, "You cannot index into this type.");
			}
		}
	}
}
