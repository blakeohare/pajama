using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ContinueStatement : Executable
	{
		public ContinueStatement(Token token) : base(token) { }

		public static ContinueStatement Parse(Tokens tokens)
		{
			Token token = tokens.PopExpected("continue");
			tokens.PopExpected(";");
			return new ContinueStatement(token);
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			// TODO: verify continues and breaks etc are actually in loops.
			// nothing here.
		}
	}
}
