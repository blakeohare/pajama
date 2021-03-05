using System.Collections.Generic;
using System.Linq;
using Pajama.Node;

namespace Pajama.Python
{
	internal class PythonSerializer : SerializerBase
	{
		public PythonSerializer(Class[] classes)
			: base(classes)
		{
			// TODO: sort classes
			this.pygameStandins = new PythonPyGameStandins();
			this.exprSerializer = new PythonExpressionSerializer();
			this.execSerializer = new PythonExecutableSerializer(this.exprSerializer);
		}

		protected override void SerializeFooter(List<string> lines)
		{
			lines.AddRange(new string[] {
				"",
				"app = Program()",
				"app.init()",
				"",
			});	
		}

		protected override void SerializeHeader(List<string> lines)
		{
			List<string> output = new List<string>()
			{
				"import pygame",
				"import math",
				"import os",
				"import random",
				"import time",
				"",
			};

			output.Add("class PJ_StaticHost: pass");
			output.Add("");

			lines.AddRange(output);
		}

		protected override void SerializeStaticField(string indent, Field field, string staticHostName, List<string> lines)
		{
			lines.Add(indent + staticHostName + "." + field.Name + " = " + this.exprSerializer.Serialize(field.DefaultValue));
		}

		protected override void SerializeStaticMethod(string indent, Method method, List<string> lines)
		{
			Class cls = method.Parent;
			string name = method.Name;
			string staticMethodDef = indent + "def " + this.GetStaticMethodName(method) + "(";
			for (int i = 0; i < method.Args.Length; ++i)
			{
				if (i > 0)
				{
					staticMethodDef += ", ";
				}
				staticMethodDef += method.Args[i].Second;
			}
			staticMethodDef += "):";
			lines.Add(staticMethodDef);
			indent += '\t';
			int lineCount = 0;
			if (!this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, method))
			{
				foreach (Executable exec in method.Code)
				{
					this.execSerializer.Serialize(indent, exec, lines);
					++lineCount;
				}
				if (lineCount == 0)
				{
					lines.Add(indent + "pass");
				}
			}
			lines.Add("");
		}

		protected override void CreateStaticHost(string indent, string staticHostName, List<string> lines)
		{
			lines.Add(indent + staticHostName + " = PJ_StaticHost()");
		}

		protected override void SerializeClassImpl(Class cls, string indent, List<string> lines)
		{
			string declarationLine = "class " + cls.FullName.Replace('.', '_');

			Class baseClass = cls.BaseClass;

			if (baseClass != null)
			{
				declarationLine += "(" + cls.BaseClass.FullName.Replace('.', '_') + ")";
			}

			declarationLine += ":";

			lines.Add(indent + declarationLine);

			indent += "\t";

			// def __ini__(self, ...):
			string line = indent + "def __init__(self";

			foreach (Pair<ZType, string> arg in cls.ConstructorArgs)
			{
				line += ", " + arg.Second;
			}

			line += "):";
			lines.Add(line);

			indent += '\t';

			int codeLength = 0;

			if (baseClass != null)
			{
				line = indent + baseClass.FullName.Replace('.', '_') + ".__init__(self";
				foreach (Expression baseConsArg in cls.BaseConstructorArgs)
				{
					line += ", " + this.exprSerializer.Serialize(baseConsArg);
				}
				line += ")";
				lines.Add(line);
				++codeLength;
			}

			foreach (Field field in cls.Members.Values.OfType<Field>())
			{
				lines.Add(indent + "self." + field.Name + " = " + this.exprSerializer.Serialize(field.DefaultValue));
				++codeLength;
			}
			if (!this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, null))
			{
				foreach (Executable exec in cls.ConstructorCode)
				{
					this.SerializeExecutable(indent, exec, lines);
					++codeLength;
				}

				if (codeLength == 0)
				{
					lines.Add(indent + "pass");
				}
			}
			else
			{
				codeLength = 1; // assume if there's a stand-in, there's more than 0 lines of code in it
			}

			lines.Add("");
			indent = indent.Substring(1);

			// TODO: other methods
			foreach (string name in cls.Members.Keys)
			{
				ClassMember member = cls.Members[name];
				if (member is Method)
				{
					Method method = (Method)member;

					if (!method.IsStatic)
					{
						string funcDeclare = indent + "def " + name + "(self";
						for (int i = 0; i < method.Args.Length; ++i)
						{
							funcDeclare += ", " + method.Args[i].Second;
						}
						funcDeclare += "):";
						lines.Add(funcDeclare);

						codeLength = 0;
						indent += '\t';
						if (this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, method))
						{
							codeLength = 1;
						}
						else
						{
							foreach (Executable exec in method.Code)
							{
								this.execSerializer.Serialize(indent, exec, lines);
								++codeLength;
							}
						}

						if (codeLength == 0)
						{
							lines.Add(indent + "pass");
						}
						lines.Add("");
						indent = indent.Substring(1);
					}
				}
			}
		}
	}
}
