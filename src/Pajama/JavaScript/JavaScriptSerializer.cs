using System;
using System.Collections.Generic;
using System.Linq;
using Pajama.Node;

namespace Pajama.JavaScript
{
	internal class JavaScriptSerializer : SerializerBase
	{
		private string gameJs;

		public JavaScriptSerializer(Class[] classes, string gameJs, List<string> imagesToPreload) : base(classes)
		{
			this.gameJs = gameJs.Replace("\r\n", "\n").Replace('\r', '\n');
			this.exprSerializer = new JavaScriptExpressionSerializer();
			this.execSerializer = new JavaScriptExecutableSerializer(this.exprSerializer);
			this.pygameStandins = new JavaScriptPyGameStandins(imagesToPreload);
		}

		protected override void SerializeHeader(List<string> lines)
		{
			lines.AddRange(this.gameJs.Split('\n'));

			lines.AddRange(new string[] {
				"",
				"J = {};",
				"" });

		}

		protected override void SerializeFooter(List<string> lines)
		{
			lines.AddRange(new string[] {
				"",
				"function setup() {",
				"	var program = new J.Program();",
				"	program.init();",
				"}",
				""
			});
		}

		protected override void CreateStaticHost(string indent, string staticHostName, List<string> lines)
		{
			lines.Add(indent + "J." + staticHostName + " = {};");
		}

		protected override void SerializeStaticMethod(string indent, Method method, List<string> lines)
		{
			Class cls = method.Parent;
			string name = method.Name;
			string staticMethodDef = indent + "J." + this.GetStaticMethodName(method) + " = function(";
			for (int i = 0; i < method.Args.Length; ++i)
			{
				if (i > 0)
				{
					staticMethodDef += ", ";
				}
				staticMethodDef += method.Args[i].Second;
			}
			staticMethodDef += ") {";
			lines.Add(staticMethodDef);
			indent += '\t';
			if (!this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, method))
			{
				foreach (Executable exec in method.Code)
				{
					this.execSerializer.Serialize(indent, exec, lines);
				}
			}
			indent = indent.Substring(1);
			lines.Add(indent + "};");
			lines.Add("");
		}

		protected override void SerializeStaticField(string indent, Field field, string staticHostName, List<string> lines)
		{
			lines.Add(indent + "J." + staticHostName + "." + field.Name + " = " + this.exprSerializer.Serialize(field.DefaultValue) + ";");
		}

		protected override void SerializeClassImpl(Class cls, string indent, List<string> lines)
		{
			string classJsName = "J." + cls.FullName.Replace('.', '_');

			Class baseClass = cls.BaseClass;

			if (baseClass != null)
			{
				throw new NotImplementedException();
			}

			string declareLine = classJsName + " = function(";

			bool first = true;
			foreach (Pair<ZType, string> arg in cls.ConstructorArgs)
			{
				if (!first)
				{
					declareLine += ", ";
				}
				first = false;
				declareLine += arg.Second;
			}

			declareLine += ") {";
			lines.Add(declareLine);

			indent += '\t';

			lines.Add(indent + "var _jsThis = this;");


			foreach (string name in cls.Members.Keys)
			{
				ClassMember member = cls.Members[name];
				if (member is Method)
				{
					Method method = (Method)member;

					if (!method.IsStatic)
					{
						string funcDeclare = indent + "this." + name + " = function(";
						for (int i = 0; i < method.Args.Length; ++i)
						{
							if (i > 0) funcDeclare += ", ";
							funcDeclare += method.Args[i].Second;
						}
						funcDeclare += ") {";

						lines.Add(funcDeclare);

						indent += '\t';
						if (!this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, method))
						{
							foreach (Executable exec in method.Code)
							{
								this.execSerializer.Serialize(indent, exec, lines);
							}
						}

						indent = indent.Substring(1);
						lines.Add(indent + "};");
						lines.Add("");
					}
				}
			}

			if (baseClass != null)
			{
				throw new NotImplementedException();
			}

			foreach (Field field in cls.Members.Values.OfType<Field>())
			{
				lines.Add(indent + "this." + field.Name + " = " + this.exprSerializer.Serialize(field.DefaultValue) + ";");
			}

			if (!this.pygameStandins.MaybeSerializeStandin(indent, lines, cls, null))
			{
				foreach (Executable exec in cls.ConstructorCode)
				{
					this.SerializeExecutable(indent, exec, lines);
				}
			}
			indent = indent.Substring(1);
			lines.Add(indent + "};");
			lines.Add("");
		}
	}
}
