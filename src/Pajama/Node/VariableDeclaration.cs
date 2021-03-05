using System.Collections.Generic;

namespace Pajama.Node
{
	internal class VariableDeclaration : Executable
	{
		public ZType Type { get; private set; }
		public string Name { get; private set; }
		public Expression Value { get; private set; }

		public VariableDeclaration(ZType type, string name, Expression value, Token token)
			: base(token)
		{
			this.Type = type;
			this.Name = name;
			this.Value = value;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			this.Value.ResolveTypes(typeResolver, scope, typesByVariableUsage); // do this first to ensure user does not use the variable in its own declaration.
			typeResolver.ResolveType(scope, this.Type);

			ZType variableExistence = null;
			for (int i = typesByVariableUsage.Count - 1; i >= 0; --i)
			{
				if (typesByVariableUsage[i].TryGetValue(this.Name, out variableExistence))
				{
					throw new ParserException(this.Token, "A variable with the same name has already been declared: " + this.Name);
				}
			}

			typesByVariableUsage[typesByVariableUsage.Count - 1].Add(this.Name, this.Type);
		}
	}
}
