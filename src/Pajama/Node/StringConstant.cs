using System.Collections.Generic;

namespace Pajama.Node
{
	internal class StringConstant : Expression
	{
		public string Value { get; private set; }

		public StringConstant(string value, Token token)
			: base(token)
		{
			// TODO: do I need to escape \'s here? I think so...
			this.Value = value.Substring(1, value.Length - 2);
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.ResolvesTo = ZType.STRING;
		}
	}
}
