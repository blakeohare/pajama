using System;
using System.Collections.Generic;
using System.Linq;

namespace Pajama.Node
{
	internal class LoopStatement : Executable
	{
		public Executable[] Init { get; private set; }
		public Executable[] Body { get; private set; }
		public Executable[] Step { get; private set; }

		public Expression Condition { get; private set; }
		public bool ConditionAtBeginning { get; private set; }

		// Ugh. Hack.
		private string originalLoopType;


		public LoopStatement(Executable[] init, Executable[] body, Executable[] step, Expression condition, bool conditionAtBeginning, Token token, string originalType)
			: base(token)
		{
			this.originalLoopType = originalType;
			this.Init = init;
			this.Body = body;
			this.Step = step;
			this.Condition = condition;
			this.ConditionAtBeginning = conditionAtBeginning;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			switch (this.originalLoopType)
			{
				case "for":
					typesByVariableUsage.Add(new Dictionary<string, ZType>());
					foreach (Executable exec in this.Init)
					{
						exec.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
					}
					this.Condition.ResolveTypes(typeResolver, scope, typesByVariableUsage);
					foreach (Executable exec in this.Step)
					{
						exec.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
					}
					typesByVariableUsage.Add(new Dictionary<string, ZType>());
					foreach (Executable exec in this.Body)
					{
						exec.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
					}
					typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
					typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
					break;
				case "while":
					this.Condition.ResolveTypes(typeResolver, scope, typesByVariableUsage);
					typesByVariableUsage.Add(new Dictionary<string, ZType>());
					foreach (Executable exec in this.Body)
					{
						exec.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
					}
					typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
					break;
				case "dowhile":
					typesByVariableUsage.Add(new Dictionary<string, ZType>());
					foreach (Executable exec in this.Body)
					{
						exec.ResolveTypes(typeResolver, scope, typesByVariableUsage, member);
					}
					typesByVariableUsage.RemoveAt(typesByVariableUsage.Count - 1);
					this.Condition.ResolveTypes(typeResolver, scope, typesByVariableUsage);

					break;
				default:
					throw new Exception("This shouldn't happen.");
			}
		}

		public static LoopStatement Parse(Tokens tokens)
		{
			Token token = tokens.Peek();
			switch (tokens.PeekValue())
			{
				case "for": return ParseFor(token, tokens);
				case "do": return ParseDoWhile(token, tokens);
				case "while": return ParseWhile(token, tokens);
				default: throw new Exception("This shouldn't happen.");
			}
		}

		private static LoopStatement ParseDoWhile(Token first, Tokens tokens)
		{
			tokens.PopExpected("do");
			tokens.PopExpected("{");
			Executable[] body = Executable.ParseLines(tokens, true);
			tokens.PopExpected("while");
			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			tokens.PopExpected(";");
			return new LoopStatement(Executable.EMPTY_BLOCK, body, Executable.EMPTY_BLOCK, condition, false, first, "dowhile");
		}

		private static LoopStatement ParseWhile(Token first, Tokens tokens)
		{
			tokens.PopExpected("while");
			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			Executable[] body = Executable.ParseBlock(tokens);
			return new LoopStatement(Executable.EMPTY_BLOCK, body, Executable.EMPTY_BLOCK, condition, true, first, "while");
		}

		private static LoopStatement ParseFor(Token firstToken, Tokens tokens)
		{
			tokens.PopExpected("for");
			tokens.PopExpected("(");
			List<Executable> init = new List<Executable>();
			bool first = true;
			while (!tokens.IsNext(";"))
			{
				if (!first)
				{
					tokens.PopExpected(",");
				}
				first = false;

				init.Add(Executable.Parse(tokens, false));
			}

			tokens.PopExpected(";");

			Expression condition = null;
			if (!tokens.IsNext(";"))
			{
				condition = ExpressionParser.Parse(tokens);
			}
			else
			{
				condition = new BooleanConstant(true, null);
			}

			tokens.PopExpected(";");

			List<Executable> step = new List<Executable>();

			first = true;
			while (!tokens.IsNext(")"))
			{
				if (!first)
				{
					tokens.PopExpected(",");
				}
				first = false;
				step.Add(Executable.Parse(tokens, false));
			}

			tokens.PopExpected(")");

			Executable[] body = Executable.ParseBlock(tokens);

			return new LoopStatement(init.ToArray(), body.ToArray(), step.ToArray(), condition, true, firstToken, "for");
		}
	}
}
