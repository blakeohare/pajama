using System;
using System.Collections.Generic;

namespace Pajama.Node
{
	internal static class ExpressionParser
	{
		public static Expression Parse(Tokens tokens)
		{
			return ParseBitwise(tokens);
		}

		// Must be in order of longest to shortest, if lengths vary
		private static readonly string[] ADDITION_OP_LOOKUP = "+ -".Split(' ');
		private static readonly string[] MULTIPLICATION_OP_LOOKUP = "* / %".Split(' ');
		private static readonly string[] BOOLEAN_OP_LOOKUP = "&& ||".Split(' ');
		private static readonly string[] BITWISE_OP_LOOKUP = "& | ^".Split(' ');
		private static readonly string[] EQUALITY_OP_LOOKUP = "== !=".Split(' ');
		private static readonly string[] INEQUALITY_OP_LOOKUP = "<= >= < >".Split(' ');
		private static readonly string[] BITSHIFT_OP_LOOKUP = "<< >>".Split(' ');

		private static readonly string[] EXCLUDE_FOR_INEQUALITY = BITSHIFT_OP_LOOKUP;
		private static readonly string[] EXCLUDE_FOR_ADDITION = "+= -=".Split(' ');
		private static readonly string[] EXCLUDE_FOR_MULTIPLICATION = "*= /= %=".Split(' ');
		private static readonly string[] EXCLUDE_FOR_BITWISE = "&= |= ^=".Split(' ');
		private static readonly string[] EXCLUDE_FOR_BITSHIFT = "<<= >>=".Split(' ');

		private static readonly Func<Tokens, Expression> PARSE_BITWISE = new Func<Tokens, Expression>(tokens => ParseBitwise(tokens));
		private static readonly Func<Tokens, Expression> PARSE_BOOLEANS = new Func<Tokens, Expression>(tokens => ParseBooleans(tokens));
		private static readonly Func<Tokens, Expression> PARSE_EQUALITY = new Func<Tokens, Expression>(tokens => ParseEquality(tokens));
		private static readonly Func<Tokens, Expression> PARSE_INEQUALITY = new Func<Tokens, Expression>(tokens => ParseInequality(tokens));
		private static readonly Func<Tokens, Expression> PARSE_BITSHIFT = new Func<Tokens, Expression>(tokens => ParseBitshift(tokens));
		private static readonly Func<Tokens, Expression> PARSE_ADDITION = new Func<Tokens, Expression>(tokens => ParseAddition(tokens));
		private static readonly Func<Tokens, Expression> PARSE_MULTIPLICATION = new Func<Tokens, Expression>(tokens => ParseMultiplication(tokens));
		private static readonly Func<Tokens, Expression> PARSE_UNARY = new Func<Tokens, Expression>(tokens => ParseUnary(tokens));

		private static Expression ParseBitwise(Tokens tokens)
		{
			return ParseChain(tokens, BITWISE_OP_LOOKUP, PARSE_BOOLEANS, false);
		}

		private static Expression ParseBooleans(Tokens tokens)
		{
			return ParseChain(tokens, BOOLEAN_OP_LOOKUP, PARSE_EQUALITY, false);
		}

		private static Expression ParseEquality(Tokens tokens)
		{
			return ParseChain(tokens, EQUALITY_OP_LOOKUP, PARSE_INEQUALITY, false);
		}

		private static Expression ParseInequality(Tokens tokens)
		{
			return ParseChain(tokens, INEQUALITY_OP_LOOKUP, PARSE_BITSHIFT, true);
		}

		private static Expression ParseBitshift(Tokens tokens)
		{
			return ParseChain(tokens, BITSHIFT_OP_LOOKUP, PARSE_ADDITION, true);
		}

		private static Expression ParseAddition(Tokens tokens)
		{
			return ParseChain(tokens, ADDITION_OP_LOOKUP, PARSE_MULTIPLICATION, false);
		}

		private static Expression ParseMultiplication(Tokens tokens)
		{
			return ParseChain(tokens, MULTIPLICATION_OP_LOOKUP, PARSE_UNARY, false);
		}

		private static Expression ParseUnary(Tokens tokens)
		{
			Token token = tokens.Peek();
			if (tokens.PopIfPresent("-"))
			{
				Expression expression = ExpressionParser.ParseEntity(tokens);
				return new Negative(expression, token);
			}

			if (tokens.PopIfPresent("!"))
			{
				Expression expression = ExpressionParser.ParseEntity(tokens);
				return new BooleanNot(expression, token);
			}

			return ExpressionParser.ParseEntity(tokens);
		}

		private static Expression ParseEntity(Tokens tokens)
		{
			Token token;
			string next = tokens.PeekValue();
			switch (next)
			{
				case "true":
					token = tokens.Pop();
					return new BooleanConstant(true, token);
				case "false":
					token = tokens.Pop();
					return new BooleanConstant(false, token);
				case "null":
					token = tokens.Pop();
					return new NullConstant(token);
				default: break;
			}

			if (IsStringAllIntegers(next))
			{
				return ExpressionParser.ParseNumber(tokens);
			}

			Expression root = null;
			Token constructorCallToken = null;
			if (next == "new")
			{
				tokens.Pop();
				
				ZType className = ZType.ParseType(tokens, false, false);
				
				List<Expression> args = new List<Expression>();
				constructorCallToken = tokens.PopExpected("(");
				if (!tokens.IsNext(")"))
				{
					args.Add(ExpressionParser.Parse(tokens));
					while (tokens.PopIfPresent(","))
					{
						args.Add(ExpressionParser.Parse(tokens));
					}
				}
				tokens.PopExpected(")");

				List<Expression> keys = null;
				List<Expression> values = new List<Expression>();

				if (tokens.PopIfPresent("{") && !tokens.PopIfPresent("}"))
				{
					Expression first = ExpressionParser.Parse(tokens);
					if (tokens.PopIfPresent(":"))
					{
						// Dictionary values
						keys = new List<Expression>() { first };
						values.Add(ExpressionParser.Parse(tokens));
						while (tokens.PopIfPresent(","))
						{
							if (!tokens.IsNext("}"))
							{
								keys.Add(ExpressionParser.Parse(tokens));
								tokens.PopExpected(":");
								values.Add(ExpressionParser.Parse(tokens));
							}
						}
					}
					else
					{
						// List values
						values.Add(first);
						while (tokens.PopIfPresent(","))
						{
							if (!tokens.IsNext("}"))
							{
								values.Add(ExpressionParser.Parse(tokens));
							}
						}
					}

					tokens.PopExpected("}");
				};

				root = new ConstructorCall(className, args.ToArray(), keys == null ? null : keys.ToArray(), values.ToArray(), constructorCallToken);
			}
			else if (next[0] == '"' || next[0] == '\'')
			{
				Token stringToken = tokens.Pop();
				root = new StringConstant(next, stringToken);
			}
			else if (next == "(")
			{
				tokens.Pop();
				root = ExpressionParser.Parse(tokens);
				tokens.PopExpected(")");
			}
			else
			{
				Token idToken = tokens.Peek();
				string identifierName = tokens.PopAlpha(true);
				if (identifierName != null)
				{
					root = new VariableUsage(identifierName, idToken);
				}
				else
				{
					throw new Exception("Unexpected token: " + next);
				}
			}

			return ExpressionParser.ParseSuffixChain(root, tokens);
		}

		private static Expression ParseNumber(Tokens tokens)
		{
			// TODO: 0xHEXVALUE

			tokens.Back();
			Token token = tokens.Peek();
			tokens.Pop();

			string numberRoot = tokens.PopValue();
			if (tokens.PopIfPresent("."))
			{
				string value = tokens.PeekValue();
				if (IsStringAllIntegers(value))
				{
					tokens.Pop();
					string next = tokens.PeekValue();
					if (next.ToLower() == "f")
					{
						tokens.Pop();
					}

					return new FloatConstant(double.Parse(numberRoot + "." + value), token);
				}
				else
				{
					Tokens.ThrowUnexpectedTokenException(tokens.Peek());
					return null;
				}
			}
			else if (tokens.PopIfPresent("f") || tokens.PopIfPresent("F"))
			{
				return new FloatConstant(int.Parse(numberRoot), token);
			}
			else
			{
				return new IntegerConstant(int.Parse(numberRoot), token);
			}
		}

		private static Expression ParseSuffixChain(Expression root, Tokens tokens)
		{
			bool anyFound = true;
			while (anyFound)
			{
				anyFound = true;
				Token token = tokens.Peek();
				if (tokens.PopIfPresent("."))
				{
					string step = tokens.PopAlpha(false);
					root = new DotStep(root, step, token);
				}
				else if (tokens.PopIfPresent("["))
				{
					Expression index = ExpressionParser.Parse(tokens);
					tokens.PopExpected("]");
					root = new BracketIndex(root, index, token);
				}
				else if (tokens.PopIfPresent("("))
				{
					List<Expression> args = new List<Expression>();
					bool first = true;
					while (!tokens.IsNext(")"))
					{
						if (!first)
						{
							tokens.PopExpected(",");
						}
						else
						{
							first = false;
						}

						args.Add(ExpressionParser.Parse(tokens));
					}
					tokens.PopExpected(")");

					root = new FunctionCall(root, args.ToArray(), token);
				}
				else
				{
					anyFound = false;
				}
			}
				return root;
		}

		public static bool IsStringAllIntegers(string value)
		{
			char c;
			for (int i = value.Length - 1; i >= 0; --i)
			{
				c = value[i];
				if (c < '0' || c > '9')
				{
					return false;
				}
			}
			return true;
		}

		private static Expression ParseChain(Tokens tokens, string[] opLookup, Func<Tokens, Expression> nextLevelParser, bool onlyOne)
		{
			Expression first = nextLevelParser(tokens);
			Token opToken = tokens.Peek();
			string next = tokens.PopIfAnyAreNext(opLookup);
			if (next != null)
			{
				List<Expression> expressions = new List<Expression>() { first };
				List<string> ops = new List<string>();
				List<Token> opTokens = new List<Token>();
				do
				{
					ops.Add(next);
					opTokens.Add(opToken);
					expressions.Add(nextLevelParser(tokens));
					if (onlyOne)
					{
						break;
					}
					opToken = tokens.Peek();
					next = tokens.PopIfAnyAreNext(opLookup);

				} while (next != null);

				return new OpChain(expressions.ToArray(), ops.ToArray(), opTokens.ToArray());
			}

			return first;
		}

		
	}
}
