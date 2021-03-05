using System.Collections.Generic;

namespace Pajama.Node
{
	internal class IfStatement : Executable
	{
		public Expression Condition { get; private set; }
		public Executable[] TrueLines { get; private set; }
		public Executable[] FalseLines { get; private set; }

		public IfStatement(Expression condition, Executable[] trueLines, Executable[] falseLines, Token token)
			: base(token)
		{
			this.Condition = condition;
			this.TrueLines = trueLines;
			this.FalseLines = falseLines;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			this.Condition.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			typesByVariableUsage.Add(new Dictionary<string, ZType>());
			foreach (Executable line in this.TrueLines)
			{
				line.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
			}
			typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
			if (this.FalseLines != null)
			{
				typesByVariableUsage.Add(new Dictionary<string, ZType>());
				foreach (Executable line in this.FalseLines)
				{
					line.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
				}
				typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
			}
		}

		public static IfStatement Parse(Tokens tokens)
		{
			Token token = tokens.PopExpected("if");

			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");

			Executable[] trueLines = Executable.ParseBlock(tokens);
			Executable[] falseLines = Executable.EMPTY_BLOCK;
			if (tokens.PopIfPresent("else"))
			{
				falseLines = Executable.ParseBlock(tokens);
			}

			return new IfStatement(condition, trueLines, falseLines, token);
		}
	}
}
