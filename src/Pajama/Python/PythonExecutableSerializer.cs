using System.Collections.Generic;
using Pajama.Node;

namespace Pajama.Python
{
	internal class PythonExecutableSerializer : ExecutableSerializerBase
	{
		public PythonExecutableSerializer(ExpressionSerializerBase exprSerializer)
			: base(exprSerializer)
		{ }

		protected override void SerializeReturn(string indent, ReturnStatement exec, List<string> buffer)
		{
			if (exec.Expression == null)
			{
				buffer.Add(indent + "return None");
			}
			else
			{
				buffer.Add(indent + "return " + this.exprSerializer.Serialize(exec.Expression));
			}
		}

		protected override void SerializeVariableDeclaration(string indent, VariableDeclaration exec, List<string> buffer)
		{
			buffer.Add(indent + exec.Name + " = " + this.exprSerializer.Serialize(exec.Value));
		}

		protected override void SerializeAssignment(string indent, AssignmentStatement exec, List<string> buffer)
		{
			string op = exec.Op + "=";
			if (op == "==")
			{
				op = "=";
			}
			buffer.Add(indent + this.exprSerializer.Serialize(exec.Root) + " " + op + " " + this.exprSerializer.Serialize(exec.Value));
		}

		protected override void SerializeIf(string indent, IfStatement exec, List<string> buffer)
		{
			buffer.Add(indent + "if " + this.exprSerializer.Serialize(exec.Condition) + ":");
			int lineCount = 0;
			indent += '\t';
			foreach (Executable line in exec.TrueLines)
			{
				this.Serialize(indent, line, buffer);
				++lineCount;
			}
			if (lineCount == 0)
			{
				buffer.Add(indent + "pass");
			}
			indent = indent.Substring(1);
			while (exec != null && exec.FalseLines != null && exec.FalseLines.Length > 0)
			{
				if (exec.FalseLines.Length == 1 && exec.FalseLines[0] is IfStatement)
				{
					exec = exec.FalseLines[0] as IfStatement;
					buffer.Add(indent + "elif " + this.exprSerializer.Serialize(exec.Condition) + ":");
					indent += '\t';
					foreach (Executable line in exec.TrueLines)
					{
						this.Serialize(indent, line, buffer);
						++lineCount;
					}
					if (lineCount == 0)
					{
						buffer.Add(indent + "pass");
					}
					indent = indent.Substring(1);
				}
				else
				{
					buffer.Add(indent + "else:");
					indent += '\t';
					foreach (Executable line in exec.FalseLines)
					{
						this.Serialize(indent, line, buffer);
					}
					indent = indent.Substring(1);
					exec = null;
				}
			}
		}

		protected override void SerializeLoop(string indent, LoopStatement exec, List<string> buffer)
		{
			foreach (Executable initLine in exec.Init)
			{
				this.Serialize(indent, initLine, buffer);
			}

			if (exec.ConditionAtBeginning)
			{
				buffer.Add(indent + "while " + this.exprSerializer.Serialize(exec.Condition) + ":");
			}

			indent += "\t";
			int lineCount = 0;

			foreach (Executable loopLine in exec.Body)
			{
				this.Serialize(indent, loopLine, buffer);
				++lineCount;
			}

			foreach (Executable stepLine in exec.Step)
			{
				this.Serialize(indent, stepLine, buffer);
				++lineCount;
			}

			if (lineCount == 0 && !exec.ConditionAtBeginning)
			{
				buffer.Add(indent + "pass");
			}

			if (!exec.ConditionAtBeginning)
			{
				buffer.Add(indent + "if " + this.exprSerializer.Serialize(exec.Condition) + ":");
				buffer.Add(indent + "\t" + "break");
			}
		}

		protected override void SerializeExpression(string indention, ExpressionAsExecutable exec, List<string> buffer)
		{
			buffer.Add(indention + this.exprSerializer.Serialize(exec.Expression));
		}

		protected override void SerializeNoop(string indent, Noop exec, List<string> lines)
		{
			// noop
		}
	}
}
