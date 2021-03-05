using System.Collections.Generic;

namespace Pajama.Node
{
	internal class OpChain : Expression
	{
		private Token[] opTokens;
		
		public Expression[] Expressions { get; private set; }
		public string[] Ops { get; private set; }
		public ZType[] TypeResolutions { get; private set; }
		
		public OpTree OpTree { get; private set; }

		public OpChain(Expression[] expressions, string[] ops, Token[] opTokens)
			: base(null) // null reference okay. Want it to crash if something is attempting to use this token instead of op tokens
		{
			this.Expressions = expressions;
			this.Ops = ops;
			this.opTokens = opTokens;
		}

		public OpChain(OpTree opTree)
			: base(opTree.Token)
		{
			this.Expressions = new Expression[] { opTree.Left, opTree.Right };
			this.OpTree = opTree;

		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			foreach (Expression expr in this.Expressions)
			{
				expr.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			}

			Expression left = this.Expressions[0];
			Expression right = this.Expressions[1];
			string op = this.Ops[0];

			this.OpTree = this.MakeOpTreeNode(left, right, op, this.opTokens[0]);

			for (int i = 2; i < this.Expressions.Length; ++i)
			{
				right = this.Expressions[i];
				op = this.Ops[i - 1];
				this.OpTree = this.MakeOpTreeNode(this.OpTree, right, op, this.opTokens[i - 1]);
			}

			this.ResolvesTo = this.OpTree.ResolvesTo;
		}

		private OpTree MakeOpTreeNode(Expression left, Expression right, string op, Token token)
		{
			return new OpTree(left, right, op, token);
		}
	}
}
