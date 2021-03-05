using System;
using System.Collections.Generic;
using System.Linq;

namespace Pajama
{
	internal class Tokens
	{
		private Token[] tokens;
		private int index;
		private int length;

		public static readonly string EOF_VALUE = "\0EOF\0";

		public override string ToString()
		{
			int start = index - 5;
			int end = index + 6;

			if (start < 0) start = 0;
			if (end > this.length) end = this.length;

			string output = "";
			for (int i = start; i < end; ++i)
			{
				if (i == index) output += "[[[";
				output += this.tokens[i].Value;
				if (i == index) output += "]]]";
				output += " ";
			}
			return output;
		}

		public Tokens(IList<Token> tokens)
		{
			this.index = 0;
			this.tokens = tokens.ToArray();
			this.length = this.tokens.Length;
		}

		public Token Pop()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index++];
			}
			return null;
		}

		public Token Peek()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index];
			}
			return null;
		}

		public bool PopIfPresent(string value)
		{
			Token token = this.Peek();
			if (token != null)
			{
				if (token.Value == value)
				{
					this.index++;
					return true;
				}
			}
			return false;
		}

		public Token PopExpected(string value)
		{
			Token output = this.Peek();
			if (!this.PopIfPresent(value))
			{
				ThrowToken(this.Peek(), value);
			}
			return output;
		}

		public static void ThrowToken(Token token, string expected)
		{
			if (token != null)
			{
				string error = token.File + ", Line: " + token.Line + ", Column: " + token.Col + "\n";
				string codeLine = token.ErrorMessageCodeLine;
				string pointerLine = token.ErrorMessagePointerLine;

				error += codeLine + "\n" + pointerLine + "\n";

				error += "Unexpected token: " + token.Value;
				if (expected != null)
				{
					error += ", Expected: " + expected;
				}

				throw new Exception(error);
			}
			else
			{
				if (expected != null)
				{
					throw new Exception("Unexpected EOF. Expected: " + expected);
				}
				else
				{
					throw new Exception("Unexpected EOF.");
				}
			}
		}

		public bool HasMore
		{
			get
			{
				return this.index < this.length;
			}
		}

		public void Back()
		{
			--this.index;
		}

		public string PopAlphaNoNum(bool failSilently)
		{
			string value = this.PopAlpha(true);

			if (value != null)
			{
				if (Node.ExpressionParser.IsStringAllIntegers(value))
				{
					--this.index;
					value = null;
				}

				return value;
			}

			if (failSilently) return null;

			ThrowToken(this.Peek(), "identifier");

			return null;
		}

		public string PopAlpha(bool failSilently)
		{
			string value = this.PeekValue();
			if (value != null)
			{
				char c = value[0];
				if (Tokenizer.IsAlphaNums(c))
				{
					this.index++;
					return value;
				}
			}

			if (failSilently)
			{
				return null;
			}

			ThrowToken(this.Peek(), "identifier");
			return null;
		}

		public bool IsNext(string value, int lookahead)
		{
			return this.index + lookahead < this.length && this.tokens[this.index + lookahead].Value == value;
		}

		public bool IsNext(string value)
		{
			return this.index < this.length && this.tokens[this.index].Value == value;
		}

		public static void ThrowUnexpectedTokenException(Token token)
		{
			throw new Exception("Unexpected token: " + token == null ? "EOF" : token.Value);
		}

		public bool AreNext(string token1, string token2, string token3)
		{
			if (this.index + 2 < this.length &&
				this.tokens[this.index].Value == token1 &&
				this.tokens[this.index + 1].Value == token2 &&
				this.tokens[this.index + 2].Value == token3)
			{
				return true;
			}
			return false;
		}

		public bool AreNext(string token1, string token2, string token3, string token4)
		{
			if (this.index + 3 < this.length &&
				this.tokens[this.index].Value == token1 &&
				this.tokens[this.index + 1].Value == token2 &&
				this.tokens[this.index + 2].Value == token3 &&
				this.tokens[this.index + 3].Value == token4)
			{
				return true;
			}
			return false;
		}

		public bool AreNext(string token1, string token2)
		{
			if (this.IsNext(token1))
			{
				if (this.index + 1 < this.length)
				{
					if (this.tokens[this.index + 1].Value == token2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public string PeekValue()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index].Value;
			}
			return null;
		}

		/// <summary>
		/// Only works for non-string/non-alphanums characters
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public string PopIfAnyAreNext(string[] values)
		{
			for (int i = 0; i < values.Length; ++i)
			{
				if (this.index + values[i].Length < this.length)
				{
					// There is a bug. Things like += -= etc are counted when there is whitespace between them.
					// Oh. Well.      PUNT!
					bool match = true;
					for (int j = 0; j < values[i].Length; ++j)
					{
						if (this.tokens[this.index + j].Value[0] != values[i][j])
						{
							match = false;
							break;
						}
					}

					if (match)
					{
						this.index += values[i].Length;
						return values[i];
					}
				}
			}

			return null;
		}

		public string PopValue()
		{
			Token token = this.Pop();
			if (token == null)
			{
				return null;
			}
			return token.Value;
		}

		public int CurrentIndex { get { return this.index; } }
		public void RestoreIndex(int index) { this.index = index; }
	}
}
