using System.Collections.Generic;
using Pajama.Node;

namespace Pajama
{
	internal class Parser
	{
		private Tokens tokens;
		private Class[] classes;

		public Class[] Classes { get { return this.classes; } }

		public Parser(Tokens tokens)
		{
			this.tokens = tokens;
			this.classes = null;
		}

		public void Parse()
		{
			List<Class> classes = new List<Class>();
			List<Interface> interfaces = new List<Interface>();

			while (this.tokens.HasMore)
			{
				while (tokens.PopIfPresent(Tokens.EOF_VALUE)) { }

				if (!tokens.HasMore)
				{
					break;
				}

				Token maybeStaticToken = tokens.Peek();
				bool isStatic = tokens.PopIfPresent("static");

				if (tokens.IsNext("class"))
				{
					Class cls = Node.Class.Parse(null, this.tokens, isStatic);

					classes.Add(cls);
				}
				else if (tokens.IsNext("interface"))
				{
					if (isStatic)
					{
						throw new ParserException(maybeStaticToken, "Interfaces cannot be declared as static.");
					}
					interfaces.Add(Node.Interface.Parse(this.tokens));
				}
				else
				{
					Tokens.ThrowUnexpectedTokenException(this.tokens.Peek());
				}
			}
			this.classes = classes.ToArray();
		}
	}
}
