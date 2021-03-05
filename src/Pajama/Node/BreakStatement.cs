using System.Collections.Generic;

namespace Pajama.Node
{
	internal class BreakStatement : Executable
	{
		public BreakStatement(Token token) : base(token) { }

		public static BreakStatement Parse(Tokens tokens)
		{
			Token token = tokens.PopExpected("break");
			tokens.PopExpected(";");
			return new BreakStatement(token);
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			// nothing to do here.
		}
	}
}
