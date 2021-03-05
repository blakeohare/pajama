using System.Collections.Generic;

namespace Pajama.Node
{
	internal class StaticHost : Expression
	{
		public Class Class { get; private set; }
		public string FullName { get; private set; }

		public StaticHost(string fullName, Token token)
			: base(token)
		{
			this.FullName = fullName;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			Class cls = typeResolver.GetClass(this.FullName, scope);
			if (cls == null)
			{
				throw new ParserException(this.Token, "Unrecognized class");
			}

			this.Class = cls;

			this.ResolvesTo = new ZType("Class", new ZType[] { this.Class.Type });
		}
	}
}
