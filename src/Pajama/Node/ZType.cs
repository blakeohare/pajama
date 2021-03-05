using System.Collections.Generic;

namespace Pajama.Node
{
	internal class ZType
	{
		private static readonly ZType[] NO_GENERICS = new ZType[0];
		public static readonly ZType VOID = new ZType("void", NO_GENERICS) { IsResolved = true, IsPrimitive = true };
		public static readonly ZType INT = new ZType("int", NO_GENERICS) { IsResolved = true, IsPrimitive = true };
		public static readonly ZType STRING = new ZType("string", NO_GENERICS) { IsResolved = true, IsPrimitive = true };
		public static readonly ZType BOOL = new ZType("bool", NO_GENERICS) { IsResolved = true, IsPrimitive = true };
		public static readonly ZType FLOAT = new ZType("float", NO_GENERICS) { IsResolved = true, IsPrimitive = true };

		public Token Token { get; private set; }
		public Class Class { get; set; }
		public string RootType { get; private set; }
		public ZType[] Generics { get; private set; }
		public bool IsResolved { get; set; }
		public bool IsPrimitive { get; private set; }
		public bool IsVariableLength { get; set; }

		public ZType(string typeName, ZType[] generics)
		{
			this.RootType = typeName;
			this.Generics = generics;
			this.IsVariableLength = generics == null;
			this.IsResolved = false;
		}
		/// <summary>
		/// Exact will not check base classes on root type.
		/// List&lt;int&gt; is IList&lt;int&gt;? --> exact: no, not exact: yes
		/// </summary>
		public bool CanBeCastedFrom(ZType otherType, bool exact)
		{
			if (this == VOID || otherType == VOID) return false;

			if (otherType == null)
			{
				if (this == STRING || !this.IsPrimitive)
				{
					return true;
				}
				return false;
			}

			if (this.IsPrimitive != otherType.IsPrimitive)
			{
				return false;
			}

			if (this.IsPrimitive)
			{
				if (this != otherType)
				{
					if (!exact && this == FLOAT && otherType == INT)
					{
						return true;
					}
					return false;
				}
				return true;
			}

			if (otherType.Generics.Length != this.Generics.Length)
			{
				return false;
			}

			for (int i = 0; i < this.Generics.Length; ++i)
			{
				if (!this.Generics[i].CanBeCastedFrom(otherType.Generics[i], true))
				{
					return false;
				}
			}

			if (exact)
			{
				if (this.Class == otherType.Class)
				{
					return true;
				}
			}
			else
			{
				Class classWalker = otherType.Class;
				do
				{
					if (this.Class == classWalker || this.Class.FullName == classWalker.FullName)
					{
						return true;
					}

					classWalker = classWalker.BaseClass;
				} while (classWalker != null);
			}
			return false;
		}


		public Expression GetDefaultValue(Token token)
		{
			switch (this.RootType)
			{
				case "void": throw new ParserException(token, "void is not valid here.");
				case "int": return new IntegerConstant(0, token);
				case "float": return new FloatConstant(0, token);
				case "bool": return new BooleanConstant(false, token);
				default: return new NullConstant(token);
			}
		}

		public static ZType ParseType(Tokens tokens, bool failSilently, bool verifyVariableNext)
		{
			int startIndex = tokens.CurrentIndex;
			Token token = tokens.Peek();

			switch (token.Value)
			{
				case "void": tokens.Pop(); return VOID;
				case "int": tokens.Pop(); return INT;
				case "bool": tokens.Pop(); return BOOL;
				case "string": tokens.Pop(); return STRING;
				case "float": tokens.Pop(); return FLOAT;
				default: break;
			}

			ZType type = TryParse(tokens);
			if (type == null)
			{
				tokens.RestoreIndex(startIndex);

				if (!failSilently)
				{
					Tokens.ThrowToken(tokens.Peek(), "type");
				}
				return null;
			}

			if (verifyVariableNext)
			{
				if (tokens.PopAlphaNoNum(true) == null)
				{
					if (failSilently)
					{
						tokens.RestoreIndex(startIndex);
						return null;
					}
					else
					{
						tokens.Back();
						Tokens.ThrowToken(tokens.Peek(), "type");
					}
				}
				else
				{
					tokens.Back(); // yup, it's there, now put it back.
				}
			}
			type.Token = token;
			return type;
		}

		private static ZType TryParse(Tokens tokens)
		{
			string type = tokens.PopAlphaNoNum(true);
			if (type == null)
			{
				return null;
			}

			switch (type)
			{
				case "void": return VOID;
				case "int": return INT;
				case "float": return FLOAT;
				case "string": return STRING;
				case "bool": return BOOL;
				default: break;
			}

			while (tokens.PopIfPresent("."))
			{
				string nextStep = tokens.PopAlphaNoNum(true);
				if (nextStep != null)
				{
					type += "." + nextStep;
				}
			}

			List<ZType> generics = new List<ZType>();
			ZType generic;
			
			if (tokens.PopIfPresent("<"))
			{
				if (tokens.AreNext(".", ".", ".", ">"))
				{
					for (int i = 0; i < 4; ++i)
					{
						tokens.Pop();
					}
					return new ZType(type, null);
				}
				generic = TryParse(tokens);
				if (generic != null)
				{
					generics.Add(generic);
					while (tokens.IsNext(","))
					{
						tokens.Pop();
						generic = TryParse(tokens);
						if (generic != null)
						{
							generics.Add(generic);
						}
						else
						{
							return null;
						}
					}

					if (tokens.PopIfPresent(">"))
					{
						return new ZType(type, generics.ToArray());
					}
					else
					{
						return null;
					}
				}

				return null;
			}

			return new ZType(type, ZType.NO_GENERICS);
		}

	}
}
