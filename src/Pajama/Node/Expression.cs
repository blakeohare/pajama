using System.Collections.Generic;

namespace Pajama.Node
{
	internal abstract class Expression
	{
		public Token Token { get; private set; }
		private Expression effectiveValue = null;
		public Expression EffectiveValue
		{
			get { return this.effectiveValue ?? this; }
			set { this.effectiveValue = value; }
		}

		public Expression(Token token)
		{
			this.Token = token;
		}

		public abstract void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage);

		public ZType ResolvesTo { get; set; }
	}
}
