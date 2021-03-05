using System;
using System.Collections.Generic;
using Pajama.Node;

namespace Pajama
{
	internal class TypeResolver
	{
		private Dictionary<string, Node.Class> resolvedClasses;
		private Node.Class[] classes;
		private List<Node.Field> fieldsThatNeedDefaultValues = new List<Node.Field>();

		public TypeResolver(Node.Class[] classes)
		{
			this.classes = classes;
		}

		public void DoYourThing()
		{
			this.LoadClasses();
			this.ResolveVariableUsageTypes();
		}

		public void LoadClasses()
		{
			this.resolvedClasses = new Dictionary<string, Node.Class>();
			foreach (Node.Class clazz in this.classes)
			{
				this.resolvedClasses[clazz.SimpleName] = clazz;
			}

			foreach (Node.Class clazz in this.classes)
			{
				foreach (ClassMember member in clazz.Members.Values)
				{
					member.ResolveType(this);
				}
			}
		}

		private static Dictionary<string, Class> GENERIC_VIRTUALS = null;


		public Node.Class GetClass(string name, Node.Class scope)
		{
			if (GENERIC_VIRTUALS == null)
			{
				GENERIC_VIRTUALS = new Dictionary<string, Class>();
				foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")
				{
					GENERIC_VIRTUALS["" + c] = new Class("" + c,
						false,
						new ZType("" + c, new ZType[0]),
						null,
						null,
						new Dictionary<string, Class>(),
						new Interface[0],
						new string[0],
						new Dictionary<string, Pair<ZType, string>[]>(),
						new Dictionary<string, Executable[]>(),
						new Dictionary<string, ZType>(),
						new Dictionary<string, ZType>(),
						new Dictionary<string, Expression>(), 
						new Pair<ZType,string>[0],
						new Expression[0],
						new Executable[0],
						new	HashSet<string>(),
						false);
				}
			}

			if (name.Length == 1)
			{
				return GENERIC_VIRTUALS[name];
			}

			Node.Class output;
			if (this.resolvedClasses.TryGetValue(name, out output))
			{
				return output;
			}
			return null;
		}

		public void ResolveType(Node.Class scope, Node.ZType rawType)
		{
			if (!rawType.IsResolved)
			{
				Node.Class clazz = this.GetClass(rawType.RootType, scope);
				if (clazz == null)
				{
					throw new ParserException(rawType.Token, "Unrecognized type: " + rawType.RootType);
				}

				if (clazz.VariableLengthGenerics)
				{
					rawType.IsVariableLength = true;
				}
				else
				{
					if (rawType.Generics.Length != clazz.Generics.Length)
					{
						throw new ParserException(rawType.Token, "Incorrect generic count. Expected " + clazz.Generics.Length + ", found " + rawType.Generics.Length);
					}
				}

				rawType.Class = clazz;

				if (rawType.Generics != null)
				{
					foreach (Node.ZType genericType in rawType.Generics)
					{
						this.ResolveType(scope, genericType);
					}
				}

				rawType.IsResolved = true;
			}
		}

		public void ResolveVariableUsageTypes()
		{
			foreach (Node.Class clazz in this.classes)
			{
				if (clazz.ConstructorCode != null)
				{
					List<Dictionary<string, Node.ZType>> variableTypeStack = new List<Dictionary<string, Node.ZType>>();
					variableTypeStack.Add(new Dictionary<string, ZType>());
					foreach (Pair<ZType, string> arg in clazz.ConstructorArgs)
					{
						this.ResolveType(clazz, arg.First);
						variableTypeStack[variableTypeStack.Count - 1].Add(arg.Second, arg.First);
					}

					if (clazz.BaseConstructorArgs != null)
					{
						if (clazz.BaseClass == null)
						{
							throw new Exception("TODO: get token for base class call");
						}

						// TODO: verify correct arg count and type.

						foreach (Expression expr in clazz.BaseConstructorArgs)
						{
							expr.ResolveTypes(this, clazz, variableTypeStack);
						}
					}

					foreach (Executable exec in clazz.ConstructorCode)
					{
						exec.ResolveTypes(this, clazz, variableTypeStack, null);
					}
				}

				foreach (Node.ClassMember member in clazz.Members.Values)
				{
					this.ResolveType(clazz, member.Type);

					if (member is Node.Field)
					{
						Node.Field field = (Node.Field)member;
						if (field.DefaultValue == null)
						{
							field.DefaultValue = field.Type.GetDefaultValue(member.Type.Token);
						}
						field.DefaultValue.ResolveTypes(this, clazz, new List<Dictionary<string, ZType>>() { new Dictionary<string, ZType>() });
					}
					else if (member is Node.Method)
					{
						Node.Method method = (Node.Method)member;
						Dictionary<string, Node.ZType> variableTypes = new Dictionary<string, Node.ZType>();
						List<Dictionary<string, Node.ZType>> variableTypeStack = new List<Dictionary<string, Node.ZType>>() { variableTypes };
						foreach (Pair<Node.ZType, string> arg in method.Args)
						{
							Node.ZType argType = arg.First;
							this.ResolveType(clazz, argType);
							variableTypes[arg.Second] = argType;
						}

						this.ResolveTypeInCode(clazz, method.Code, variableTypeStack, member);
					}
				}
			}
		}

		public void ResolveTypeInCode(Node.Class scope, Executable[] lines, List<Dictionary<string, ZType>> variableStack, ClassMember member)
		{
			foreach (Executable line in lines)
			{
				line.ResolveTypes(this, scope, variableStack, member);
			}
		}
	}
}
