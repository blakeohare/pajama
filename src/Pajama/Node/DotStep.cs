using System;
using System.Collections.Generic;

namespace Pajama.Node
{
	internal class DotStep : Expression
	{
		public string Step { get; private set; }
		public Expression Root { get; private set; }

		public DotStep(Expression root, string step, Token token)
			: base(token)
		{
			this.Root = root;
			this.Step = step;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			this.Root.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			this.Root = this.Root.EffectiveValue;

			ZType rootClassType = this.Root.ResolvesTo;
			bool staticRequired = false;
			Class rootClass;
			if (rootClassType.RootType == "Class")
			{
				staticRequired = true;
				rootClassType = rootClassType.Generics[0];
				rootClass = typeResolver.GetClass(rootClassType.RootType, scope);
			}
			else
			{
				rootClass = this.Root.ResolvesTo.Class;
			}

			if (this.Root.ResolvesTo == ZType.STRING)
			{
				switch (this.Step)
				{
					case "length":
						this.ResolvesTo = ZType.INT;
						return;

					case "split":
						// string.split(string) --> List<string>
						this.ResolvesTo = new ZType("Func", new ZType[] { ZType.STRING, new ZType("List", new ZType[] { ZType.STRING }) });
						return;

					default:
						break;
				}
			}

			ClassMember member;
			if (!this.Root.ResolvesTo.IsPrimitive && rootClass.Members.TryGetValue(this.Step, out member))
			{
				if (member.IsStatic != staticRequired)
				{
					throw new ParserException(this.Token,
						staticRequired
							? "Attempted to access a non static member in a static way."
							: "Attempted to access a static member from an instance.");
				}

				if (member is Field)
				{
					this.ResolvesTo = member.Type;
				}
				else if (member is Method)
				{
					Method method = (Method)member;
					List<ZType> argTypes = new List<ZType>();
					int i= 0;
					foreach (Pair<ZType, string> arg in method.Args)
					{
						if (arg.First.RootType.Length == 1)
						{
							argTypes.Add(rootClassType.Generics[i]);
						}
						else
						{
							argTypes.Add(arg.First);
						}
						++i;
					}

					argTypes.Add(member.Type);
					this.ResolvesTo = new ZType("Func", argTypes.ToArray());

					if (staticRequired)
					{
						this.EffectiveValue = new StaticMethod(this.Root.ResolvesTo.Class, method, member.Name, this.Token);
						this.EffectiveValue.ResolvesTo = this.ResolvesTo;
					}

					typeResolver.ResolveType(scope, this.ResolvesTo);
				}
				else
				{
					throw new Exception("This shouldn't happen.");
				}
			}
			else
			{
				throw new ParserException(this.Token, this.Root.ResolvesTo.RootType + " has no member called " + this.Step);
			}
		}
	}
}
