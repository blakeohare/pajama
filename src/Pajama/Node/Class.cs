using System.Collections.Generic;
using System.Linq;

namespace Pajama.Node
{
	internal class Class
	{
		public string SimpleName { get; private set; }
		// TODO: this will eventually be different once I allow nested classes...
		public string FullName { get { return this.SimpleName; } set { /* noop */ } }
		public Dictionary<string, ClassMember> Members { get; private set; }
		public Class Parent { get; private set; }
		public Dictionary<string, Class> NestedClasses { get; private set; }
		public Class BaseClass { get; private set; }
		public HashSet<Interface> Interfaces { get; private set; }
		public string[] Generics { get; private set; }
		public bool IsStatic { get; private set; }

		public Pair<ZType, string>[] ConstructorArgs { get; private set; }
		public Expression[] BaseConstructorArgs { get; private set; }
		public Executable[] ConstructorCode { get; private set; }
		public ZType Type { get; private set; }
		public bool VariableLengthGenerics { get; private set; }

		public Class(string name,
			bool isStatic,
			ZType type,
			Class parent,
			Class baseClass,
			Dictionary<string, Class> nestedClasses,
			Interface[] localInterfaces,
			string[] genericNames,
			Dictionary<string, Pair<ZType, string>[]> methodArgs,
			Dictionary<string, Executable[]> code,
			Dictionary<string, ZType> returnType,
			Dictionary<string, ZType> fieldType,
			Dictionary<string, Expression> fieldDefaultValue,
			Pair<ZType, string>[] constructorArgs,
			Expression[] baseConstructorArgs,
			Executable[] constructorCode,
			HashSet<string> theseMembersAreStatic,
			bool variableLengthGenerics)
		{
			this.Type = type;
			this.IsStatic = isStatic;
			this.VariableLengthGenerics = variableLengthGenerics;
			if (baseConstructorArgs == null && baseClass != null)
			{
				baseConstructorArgs = new Expression[0];
			}
			// TODO: verify arg length in type resolution phase.
			this.ConstructorArgs = constructorArgs ?? new Pair<ZType, string>[0];
			this.BaseConstructorArgs = baseConstructorArgs;
			this.ConstructorCode = constructorCode ?? new Executable[0];

			this.SimpleName = name;
			this.Parent = parent;
			// FullName must be resolved after all classes are created.
			this.NestedClasses = nestedClasses;
			this.Members = new Dictionary<string, ClassMember>();
			this.Interfaces = new HashSet<Interface>(localInterfaces);
			this.Generics = genericNames;

			if (baseClass != null)
			{
				foreach (Interface interfaze in baseClass.Interfaces)
				{
					if (!this.Interfaces.Contains(interfaze))
					{
						this.Interfaces.Add(interfaze);
					}
				}
			}

			// TODO: verify members comply with interfaces

			foreach (string methodName in methodArgs.Keys)
			{
				this.Members[methodName] = new Method(methodName, returnType[methodName], methodArgs[methodName], code[methodName], this, theseMembersAreStatic.Contains(methodName));
			}

			foreach (string fieldName in fieldType.Keys)
			{
				this.Members[fieldName] = new Field(fieldName, fieldType[fieldName], fieldDefaultValue[fieldName], this, theseMembersAreStatic.Contains(fieldName));
			}
		}

		public static Class Parse(Class parent, Tokens tokens, bool isStatic)
		{
			Token classToken = tokens.PopExpected("class");

			ZType classNameAsType = ZType.ParseType(tokens, false, false);
			string name = classNameAsType.RootType;

			if (name.Length == 1)
			{
				throw new ParserException(classNameAsType.Token, "Class name must be at least 2 characters.");
			}

			if (name.Contains('.'))
			{
				throw new ParserException(classNameAsType.Token, "Class name cannot contain '.' characters.");
			}
			bool variableLengthGenerics = classNameAsType.IsVariableLength;
			List<string> generics = new List<string>();
			if (!variableLengthGenerics)
			{
				foreach (ZType generic in classNameAsType.Generics)
				{
					if (generic.Generics.Length != 0 || generic.RootType.Length != 1)
					{
						throw new ParserException(generic.Token, "Invalid expression. Generics must be single letter expressions.");
					}
					generics.Add(generic.RootType);
				}
			}

			Dictionary<string, Pair<ZType, string>[]> argsByMethodName = new Dictionary<string, Pair<ZType, string>[]>();
			Dictionary<string, Executable[]> methodCodeByName = new Dictionary<string, Executable[]>();
			Dictionary<string, ZType> returnTypeByName = new Dictionary<string, ZType>();
			Dictionary<string, ZType> fieldTypeByName = new Dictionary<string, ZType>();
			Dictionary<string, Expression> fieldStartingValue = new Dictionary<string, Expression>();
			Executable[] constructorCode = null;
			Pair<ZType, string>[] constructorArgs = null;
			List<Expression> baseConstructorArgs = null;
			List<string> staticMembers = new List<string>();

			// TODO: pop parent classes

			tokens.PopExpected("{");

			while (!tokens.IsNext("}"))
			{
				bool nextIsStatic = tokens.PopIfPresent("static");

				// constructor
				if (tokens.IsNext(name) && tokens.IsNext("(", 1))
				{
					if (isStatic && !nextIsStatic)
					{
						throw new ParserException(tokens.Peek(), "Static classes cannot have a non-static constructor.");
					}

					tokens.Pop();
					constructorArgs = Class.ParseParams(tokens, false);

					if (tokens.PopIfPresent(":"))
					{
						tokens.PopExpected("base");
						baseConstructorArgs = new List<Expression>();
						tokens.PopExpected("(");
						if (!tokens.IsNext(")"))
						{
							baseConstructorArgs.Add(ExpressionParser.Parse(tokens));
							while (tokens.PopIfPresent(","))
							{
								baseConstructorArgs.Add(ExpressionParser.Parse(tokens));
							}
						}
						tokens.PopExpected(")");

					}
					tokens.PopExpected("{");
					constructorCode = Executable.ParseLines(tokens, true);
					tokens.PopExpected("}");
				}
				else
				{
					Token memberToken = tokens.Peek();

					ZType type = ZType.ParseType(tokens, false, true);
					string memberName = tokens.PopAlpha(false);

					if (nextIsStatic)
					{
						staticMembers.Add(memberName);
					}
					else if (isStatic)
					{
						throw new ParserException(memberToken, "Static classes cannot have non-static members.");
					}

					if (tokens.PopIfPresent("="))
					{
						// setting a field
						fieldTypeByName[memberName] = type;
						Expression defaultValue = ExpressionParser.Parse(tokens);
						tokens.PopExpected(";");
						fieldStartingValue[memberName] = defaultValue;
					}
					else if (tokens.PopIfPresent(";"))
					{
						// declaring a field
						fieldTypeByName[memberName] = type;
						fieldStartingValue[memberName] = null;
					}
					else if (tokens.PopIfPresent("("))
					{
						if (memberName == "start")
						{

						}
						// method
						Pair<ZType, string>[] args = Class.ParseParams(tokens, true);
						tokens.PopExpected("{");
						Executable[] lines = Executable.ParseLines(tokens, true);
						tokens.PopExpected("}");
						argsByMethodName[memberName] = args;
						methodCodeByName[memberName] = lines;
						returnTypeByName[memberName] = type;
					}
					else
					{
						Tokens.ThrowUnexpectedTokenException(tokens.Peek());
					}
				}
			}

			tokens.PopExpected("}");

			Interface[] interfaces = new Interface[0]; // TODO: interfaces
			Class baseClass = null; // TODO: base classes

			if (isStatic)
			{
				if (interfaces.Length != 0)
				{
					throw new ParserException(classToken, "Static classes cannot implement an interface.");
				}

				if (baseClass != null)
				{
					throw new ParserException(classToken, "Static classes cannot extend from a base class."); 
				}
			}

			// TODO: nested classes
			return new Class(
				name,
				isStatic,
				classNameAsType,
				parent,
				baseClass,
				new Dictionary<string, Class>(),
				interfaces,
				generics.ToArray(),
				argsByMethodName,
				methodCodeByName,
				returnTypeByName,
				fieldTypeByName,
				fieldStartingValue,
				constructorArgs == null ? null : constructorArgs.ToArray(),
				baseConstructorArgs == null ? null : baseConstructorArgs.ToArray(),
				constructorCode == null ? null : constructorCode.ToArray(),
				new HashSet<string>(staticMembers),
				variableLengthGenerics);
		}

		private static Pair<ZType, string>[] ParseParams(Tokens tokens, bool openParenPopped)
		{
			if (!openParenPopped)
			{
				tokens.PopExpected("(");
			}

			List<Pair<ZType, string>> args = new List<Pair<ZType, string>>();
			bool first = true;

			HashSet<string> argNames = new HashSet<string>();

			while (!tokens.IsNext(")"))
			{
				if (!first)
				{
					tokens.PopExpected(",");
				}
				first = false;

				Token argToken = tokens.Peek();
				ZType type = ZType.ParseType(tokens, false, true);
				string name = tokens.PopAlpha(false);
				if (argNames.Contains(name))
				{
					throw new ParserException(argToken, "Multiple arguments have the same name.");
				}
				argNames.Add(name);
				args.Add(new Pair<ZType, string>(type, name));
			}

			tokens.PopExpected(")");

			return args.ToArray();
		}

	}
}
