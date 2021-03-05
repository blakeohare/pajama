using System.Collections.Generic;
using Pajama.Node;

namespace Pajama
{
	internal abstract class SerializerBase
	{
		protected Class[] classes;
		protected ExecutableSerializerBase execSerializer;
		protected ExpressionSerializerBase exprSerializer;
		protected PyGameStandins pygameStandins;


		public SerializerBase(Class[] classes)
		{
			this.classes = classes;
		}

		public string Serialize()
		{
			List<string> output = new List<string>();

			this.SerializeHeader(output);

			foreach (Class c in this.classes)
			{
				switch (c.FullName)
				{
					// These types are replaced by native types and so we don't need their definitions anymore.
					// They only existed for the sake of compiler metadata and type safety enforcement.
					case "Map":
					case "List":
					case "Func":
					case "Class":
						break;

					default:
						this.SerializeClass(c, "", output);
						break;
				}
			}
			this.SerializeFooter(output);

			return string.Join("\n", output);
		}

		protected abstract void SerializeHeader(List<string> lines);
		protected abstract void SerializeFooter(List<string> lines);
		protected abstract void CreateStaticHost(string indent, string staticHostName, List<string> lines);
		protected abstract void SerializeStaticMethod(string indent, Method method, List<string> lines);
		protected abstract void SerializeStaticField(string indent, Field field, string staticHostName, List<string> lines);

		protected void SerializeClass(Class cls, string indention, List<string> lines)
		{
			List<string> staticInitLines = new List<string>();
			string staticHostName = "sh_" + cls.FullName.Replace('.', '_');
			foreach (string memberName in cls.Members.Keys)
			{
				ClassMember member = cls.Members[memberName];
				if (member.IsStatic)
				{
					if (member is Field)
					{
						Field field = (Field)member;
						this.SerializeStaticField(indention, field, staticHostName, staticInitLines);
					}
					else if (member is Method)
					{
						Method method = (Method)member;
						this.SerializeStaticMethod(indention, method, lines);
					}
				}
			}

			if (staticInitLines.Count > 0)
			{
				this.CreateStaticHost(indention, staticHostName, lines);
				lines.AddRange(staticInitLines);
				lines.Add("");
			}

			this.SerializeClassImpl(cls, indention, lines);

		}

		protected abstract void SerializeClassImpl(Class cls, string indent, List<string> lines);

		protected string GetStaticMethodName(Method method)
		{
			return "sm_" + method.Parent.FullName.Replace('.', '_') + "__" + method.Name;
		}

		public void SerializeExecutable(string indention, Executable executable, List<string> lines)
		{
			this.execSerializer.Serialize(indention, executable, lines);
		}
	}
}
