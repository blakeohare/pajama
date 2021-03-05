using System.Collections.Generic;

namespace Pajama
{
	internal class Tokenizer
	{

		private Dictionary<string, string> files;

		public Tokenizer(Dictionary<string, string> files)
		{
			this.files = files;
		}

		public Token[] Tokenize()
		{
			List<Token> output = new List<Token>();

			foreach (string file in this.files.Keys)
			{
				string contents = this.files[file];
				List<Token> tokens = this.TokenizeFile(file, contents);
				if (tokens.Count > 0)
				{
					output.AddRange(tokens);
				}
			}
			string check = PrintTokens(output);

			return output.ToArray();
		}

		private string PrintTokens(List<Token> tokens)
		{
			string output = "";
			foreach (Token token in tokens)
			{
				output += " " + token.Value;
			}

			return output;
		}

		public static bool IsAlphaNums(char c)
		{
			return
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z') ||
				(c >= '0' && c <= '9');
		}

		private static readonly List<Token> tTokens = new List<Token>();
		private List<Token> TokenizeFile(string name, string contents)
		{
			tTokens.Clear();
			contents = contents.Replace("\r\n", "\n").Replace('\r', '\n');

			string[] lineValues = contents.Split('\n');

			int line = 1;
			int col = 0;
			char c;
			char stringType = '\0';
			char commentType = '\0';
			bool inWord = false;

			string currentToken = "";
			int currentTokenBegin = 0;

			List<int> linesBuilder = new List<int>();
			List<int> columnsBuilder = new List<int>();

			for (int i = 0; i < contents.Length; ++i)
			{
				c = contents[i];
				linesBuilder.Add(line);
				columnsBuilder.Add(++col);

				if (c == '\n')
				{
					col = -1;
					line++;
				}
			}

			int[] lines = linesBuilder.ToArray();
			int[] columns = columnsBuilder.ToArray();

			for (int i = 0; i < contents.Length; ++i)
			{
				c = contents[i];

				if (inWord)
				{
					if (IsAlphaNums(c))
					{
						currentToken += c;
					}
					else
					{
						--i;
						inWord = false;
						tTokens.Add(new Token(currentToken, name, lines[currentTokenBegin], columns[currentTokenBegin], lineValues));
						// start again with the in word flag unset
					}
				}
				else if (stringType != '\0')
				{
					if (c == stringType)
					{
						currentToken += c;
						tTokens.Add(new Token(currentToken, name, lines[currentTokenBegin], columns[currentTokenBegin], lineValues));
						stringType = '\0';
					}
					else
					{
						currentToken += c;
					}
				}
				else if (commentType != '\0')
				{
					if (commentType == '/') // double-slash comments
					{
						if (c == '\n')
						{
							commentType = '\0';
						}
					}
					else if (commentType == '*') // slash-star comments
					{
						if (c == '*')
						{
							if (i < contents.Length - 1 && contents[i + 1] == '/')
							{
								++i;
								commentType = '\0';
							}
						}
					}
				}
				else
				{
					// you are not currently in a state.
					if (c == ' ' || c == '\r' || c == '\n' || c == '\t')
					{
						// whitespace. skip.
					}
					else if (IsAlphaNums(c))
					{
						inWord = true;
						currentTokenBegin = i;
						currentToken = "" + c;
					}
					else if (c == '\'' || c == '"')
					{
						currentTokenBegin = i;
						currentToken = "" + c;
						stringType = c;
					}
					else if (c == '/' && i + 1 < contents.Length && (contents[i + 1] == '*' || contents[i + 1] == '/'))
					{
						commentType = contents[i + 1];
					}
					else
					{
						// special character. It's its own token.
						tTokens.Add(new Token("" + c, name, lines[i], columns[i], lineValues));
					}
				}
			}

			string lastLine = lineValues[lineValues.Length - 1];
			tTokens.Add(new Token(Tokens.EOF_VALUE, name, lineValues.Length, lastLine.TrimEnd().Length, lineValues));

			return tTokens;
		}
	}
}
