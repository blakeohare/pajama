using System.Collections.Generic;

namespace Pajama.Node
{
	internal class VariableUsage : Expression
	{
		public string Name { get; private set; }

		public VariableUsage(string name, Token token)
			: base(token)
		{
			this.Name = name;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			for (int i = typesByVariableUsage.Count - 1; i >= 0; --i)
			{
				ZType type;
				if (typesByVariableUsage[i].TryGetValue(this.Name, out type))
				{
					this.ResolvesTo = type;
					break;
				}
			}

			if (this.Name == "this")
			{
				this.EffectiveValue = new ThisExpression(scope, this.Token);
				return;
			}

			if (this.ResolvesTo == null)
			{
				Class cls = typeResolver.GetClass(this.Name, scope);
				if (cls != null)
				{
					this.EffectiveValue = new StaticHost(this.Name, this.Token);
					this.EffectiveValue.ResolveTypes(typeResolver, scope, typesByVariableUsage);
					return;
				}

				throw new ParserException(this.Token, "Variable used before it was declared: " + this.Name);
			}
		}
	}
}
