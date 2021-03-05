using System;
using System.Collections.Generic;
using Pajama.Node;

namespace Pajama.JavaScript
{
	internal class JavaScriptExecutableSerializer : ExecutableSerializerBase
	{
		public JavaScriptExecutableSerializer(ExpressionSerializerBase exprSerializer)
			: base(exprSerializer)
		{ }

		protected override void SerializeAssignment(string indent, AssignmentStatement exec, List<string> lines)
		{
			string op = exec.Op + '=';
			if (op == "==") op = "=";

			lines.Add(indent + this.exprSerializer.Serialize(exec.Root) + " " + exec.Op + " " + this.exprSerializer.Serialize(exec.Value) + ";");
		}

		protected override void SerializeExpression(string indent, ExpressionAsExecutable exec, List<string> lines)
		{
			lines.Add(indent + this.exprSerializer.Serialize(exec.Expression) + ";");
		}

		private void SerializeIfHelper(string indent, IfStatement exec, List<string> lines, bool appendToPrevious)
		{
			if (appendToPrevious)
			{
				lines[lines.Count - 1] += "if (" + this.exprSerializer.Serialize(exec.Condition) + ") {";
			}
			else
			{
				lines.Add(indent + "if (" + this.exprSerializer.Serialize(exec.Condition) + ") {");
			}
			indent += '\t';
			for (int i = 0; i < exec.TrueLines.Length; ++i)
			{
				this.Serialize(indent, exec.TrueLines[i], lines);
			}
			indent = indent.Substring(1);

			if (exec.FalseLines != null && exec.FalseLines.Length > 0)
			{
				if (exec.FalseLines.Length == 1 && exec.FalseLines[0] is IfStatement)
				{
					lines.Add(indent + "} else ");
					this.SerializeIfHelper(indent, (IfStatement)exec.FalseLines[0], lines, true);
				}
				else
				{
					lines.Add(indent + "} else {");
					indent += '\t';

					for (int i = 0; i < exec.FalseLines.Length; ++i)
					{
						this.Serialize(indent, exec.FalseLines[i], lines);
					}

					indent = indent.Substring(1);
					lines.Add(indent + "}");
				}
			}
			else
			{
				lines.Add(indent + "}");
			}
		}

		protected override void SerializeIf(string indent, IfStatement exec, List<string> lines)
		{
			this.SerializeIfHelper(indent, exec, lines, false);
		}

		protected override void SerializeLoop(string indent, LoopStatement exec, List<string> lines)
		{
			foreach (Executable initLine in exec.Init)
			{
				this.Serialize(indent, initLine, lines);
			}

			if (exec.ConditionAtBeginning)
			{
				lines.Add(indent + "while (" + this.exprSerializer.Serialize(exec.Condition) + ") {");
			}

			indent += "\t";
			int lineCount = 0;

			foreach (Executable loopLine in exec.Body)
			{
				this.Serialize(indent, loopLine, lines);
				++lineCount;
			}

			foreach (Executable stepLine in exec.Step)
			{
				this.Serialize(indent, stepLine, lines);
				++lineCount;
			}

			if (!exec.ConditionAtBeginning)
			{
				lines.Add(indent + "if (" + this.exprSerializer.Serialize(exec.Condition) + ") break;");
			}
			
			indent = indent.Substring(1);
			lines.Add(indent + "}");
		}

		protected override void SerializeVariableDeclaration(string indent, VariableDeclaration exec, List<string> lines)
		{
			lines.Add(indent + "var " + exec.Name + " = " + this.exprSerializer.Serialize(exec.Value) + ";");
		}

		protected override void SerializeReturn(string indent, ReturnStatement exec, List<string> lines)
		{
			lines.Add(indent + "return " + (exec.Expression == null ? "null" : this.exprSerializer.Serialize(exec.Expression)) + ";");
		}

		protected override void SerializeNoop(string indent, Noop exec, List<string> lines)
		{
			// noop
		}
	}
}
