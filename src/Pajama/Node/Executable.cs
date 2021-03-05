using System.Collections.Generic;

namespace Pajama.Node
{
	internal abstract class Executable
	{
		public static readonly Executable[] EMPTY_BLOCK = new Executable[0];

		public Token Token { get; private set; }

		public Executable(Token token)
		{
			this.Token = token;
		}

		public abstract void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member);

		public static Executable Parse(Tokens tokens, bool semicolonRequired)
		{
			string next = tokens.PeekValue();

			if (next == ";")
			{
				if (semicolonRequired)
				{
					tokens.Pop();
				}

				return new Noop(); // noop
			}

			switch (next)
			{
				case "if": return IfStatement.Parse(tokens);
				case "do":
				case "while":
				case "for": return LoopStatement.Parse(tokens);
				case "return": return ReturnStatement.Parse(tokens);
				case "break": return BreakStatement.Parse(tokens);
				case "continue": return ContinueStatement.Parse(tokens);
				default: break; // do nothing
			}

			Token token = tokens.Peek();
			ZType variableDeclarationType = ZType.ParseType(tokens, true, true);
			if (variableDeclarationType != null)
			{
				string variableName = tokens.PopAlpha(false);
				Expression amount = null;
				if (!tokens.PopIfPresent(";"))
				{
					Token opToken = tokens.Peek();
					string op = Executable.PopAssignmentOperator(tokens);
					amount = ExpressionParser.Parse(tokens);
					if (semicolonRequired)
					{
						tokens.PopExpected(";");
					}

					if (op != "=")
					{
						throw new ParserException(opToken, "Only assignment with '=' can be used with variable declarations.");
					}
				}

				if (amount == null)
				{
					amount = variableDeclarationType.GetDefaultValue(token);
				}

				return new VariableDeclaration(variableDeclarationType, variableName, amount, token);
			}
			else
			{
				Expression expression = ExpressionParser.Parse(tokens);
				Token opToken = tokens.Peek();
				string op = Executable.PopAssignmentOperator(tokens);
				if (op != null)
				{
					Expression root = expression;
					expression = ExpressionParser.Parse(tokens);

					if (semicolonRequired)
					{
						tokens.PopExpected(";");
					}

					return new AssignmentStatement(root, op, expression, opToken);
				}

				if (semicolonRequired)
				{
					tokens.PopExpected(";");
				}

				return new ExpressionAsExecutable(expression, opToken);
			}
		}

		private static string PopAssignmentOperator(Tokens tokens)
		{
			// = but exclude ==
			if (tokens.IsNext("=") && !tokens.AreNext("=", "="))
			{
				tokens.Pop();
				return "=";
			}

			// += -= *= /= %= &= |= ^= ~=
			foreach (char c in "+-*/%&|^~")
			{
				if (tokens.AreNext("" + c, "="))
				{
					tokens.Pop();
					tokens.Pop();
					return "" + c;
				}
			}

			// <<= >>= &&= ||=
			foreach (char c in "<>&|")
			{
				if (tokens.AreNext("" + c, "" + c, "="))
				{
					tokens.Pop();
					tokens.Pop();
					tokens.Pop();
					return "" + c + c;
				}
			}

			return null;
		}

		public static Executable[] ParseLines(Tokens tokens, bool openBracketPopped)
		{
			List<Executable> output = new List<Executable>();

			if (!openBracketPopped)
			{
				tokens.PopExpected("{");
			}


			while (!tokens.IsNext("}"))
			{
				Executable executable = Executable.Parse(tokens, true);
				output.Add(executable);
			}

			return output.ToArray();
		}

		public static Executable[] ParseBlock(Tokens tokens)
		{
			if (tokens.PopIfPresent("{"))
			{
				List<Executable> output = new List<Executable>();
				while (!tokens.IsNext("}"))
				{
					Executable line = Executable.Parse(tokens, true);
					output.Add(line);
				}

				tokens.PopExpected("}");
				return output.ToArray();
			}
			else
			{
				return new Executable[] { Executable.Parse(tokens, true) };
			}
		}
	}
}
