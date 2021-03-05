using System;
using System.Collections.Generic;
using Pajama.Node;

namespace Pajama
{
	internal abstract class ExecutableSerializerBase
	{
		protected ExpressionSerializerBase exprSerializer;

		public ExecutableSerializerBase(ExpressionSerializerBase exprSerializer)
		{
			this.exprSerializer = exprSerializer;
		}


		public void Serialize(string indention, Executable exec, List<string> buffer)
		{
			if (exec is ExpressionAsExecutable) this.SerializeExpression(indention, exec as ExpressionAsExecutable, buffer);
			else if (exec is IfStatement) this.SerializeIf(indention, exec as IfStatement, buffer);
			else if (exec is LoopStatement) this.SerializeLoop(indention, exec as LoopStatement, buffer);
			else if (exec is AssignmentStatement) this.SerializeAssignment(indention, exec as AssignmentStatement, buffer);
			else if (exec is VariableDeclaration) this.SerializeVariableDeclaration(indention, exec as VariableDeclaration, buffer);
			else if (exec is ReturnStatement) this.SerializeReturn(indention, exec as ReturnStatement, buffer);
			else if (exec is Noop) this.SerializeNoop(indention, exec as Noop, buffer);
			else throw new NotImplementedException();
		}

		protected abstract void SerializeExpression(string indent, ExpressionAsExecutable exec, List<string> lines);
		protected abstract void SerializeIf(string indent, IfStatement exec, List<string> lines);
		protected abstract void SerializeLoop(string indent, LoopStatement exec, List<string> lines);
		protected abstract void SerializeAssignment(string indent, AssignmentStatement exec, List<string> lines);
		protected abstract void SerializeVariableDeclaration(string indent, VariableDeclaration exec, List<string> lines);
		protected abstract void SerializeReturn(string indent, ReturnStatement exec, List<string> lines);

		// TODO: figure out where these are coming from.
		protected abstract void SerializeNoop(string indent, Noop exec, List<string> lines);
	}
}
