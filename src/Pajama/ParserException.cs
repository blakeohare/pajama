using System;

namespace Pajama
{
	internal class ParserException : Exception
	{
		private static string FormatMessage(Token token, string message)
		{
			if (token == null)
			{
				message = "No token info in error message! This is a bug!\n" + message;
			}
			else
			{
				message = string.Join("",
					token.File,
					", Line: ",
					token.Line,
					", Column: ",
					token.Col,
					"\n",
					token.ErrorMessageCodeLine,
					"\n",
					token.ErrorMessagePointerLine,
					"\n",
					message);
			}

			return message;
		}

		public ParserException(Token token, string message)
			: base(ParserException.FormatMessage(token, message))
		{
		}
	}
}
