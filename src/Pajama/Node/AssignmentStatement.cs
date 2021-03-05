using System;
using System.Collections.Generic;

namespace Pajama.Node
{
	internal class AssignmentStatement : Executable
	{
		public Expression Root { get; private set; }
		public Expression Value { get; private set; }
		public string Op { get; private set; }

		public AssignmentStatement(Expression root, string op, Expression value, Token token)
			: base(token)
		{
			this.Root = root;
			this.Op = op;
			this.Value = value;
		}

		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage, ClassMember member)
		{
			if (scope.FullName == "PJEvent")
			{

			}
			this.Root.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			this.Value.ResolveTypes(typeResolver, scope, typesByVariableUsage);
			this.Root = this.Root.EffectiveValue;
			this.Value = this.Value.EffectiveValue;

			// TODO: verify root is an acceptable target for assignment.
			ZType leftType = this.Root.ResolvesTo;
			ZType rightType = this.Value.ResolvesTo;

			if (this.Op == "=")
			{
				if (leftType.CanBeCastedFrom(rightType, false))
				{
					return;
				}
			}
			else
			{

			}
			throw new NotImplementedException("Need to do another op table to make sure assignment types and modifications are compatible");
		}

		private bool AreInts(ZType left, ZType right)
		{
			return left == ZType.INT && right == ZType.INT;
		}

		private bool AreNums(ZType left, ZType right)
		{
			return (left == ZType.INT || left == ZType.FLOAT) && (right == ZType.INT || right == ZType.FLOAT);
		}

		private OpTree.OpTypes GetResult(string op, ZType baseType, ZType newType, Token opToken)
		{
			if (newType == ZType.VOID)
			{
				throw new ParserException(opToken, "cannot use void type here.");
			}
			if (newType == null)
			{
				throw new ParserException(opToken, "cannot use null here.");
			}

			switch (op)
			{
				case "+":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.IntAdd;
					if (AreNums(baseType, newType)) return OpTree.OpTypes.FloatAdd;
					if (baseType == ZType.STRING && newType.IsPrimitive) return OpTree.OpTypes.StringConcat;
					break;
				case "-":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.IntSubtract;
					if (AreNums(baseType, newType)) return OpTree.OpTypes.FloatSubtract;
					break;
				case "*":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.IntMultiply;
					if (AreNums(baseType, newType)) return OpTree.OpTypes.FloatMultiply;
					break;;
				case "/":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.IntDivide;
					if (AreNums(baseType, newType)) return OpTree.OpTypes.FloatDivide;
					break;
				case "%":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.IntModulo;
					if (AreNums(baseType, newType)) return OpTree.OpTypes.FloatModulo;
					break;
				case "&":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.BitwiseAnd;
					break;
				case "|":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.BitwiseOr;
					break;
				case "^":
					if (AreInts(baseType, newType)) return OpTree.OpTypes.BitwiseXor;
					break;
				case "&&":
				case "||":
					if (baseType == ZType.BOOL && newType == ZType.BOOL) return op == "&&" ? OpTree.OpTypes.BooleanAnd : OpTree.OpTypes.BooleanOr;
					break;
				case "<<":
				case ">>":
					if (AreInts(baseType, newType)) return op == "<<" ? OpTree.OpTypes.BitshiftLeft : OpTree.OpTypes.BitshiftRight;
					break;
				
				default:
					throw new Exception("This should not happen.");
			}

			throw new ParserException(opToken, "This operator is not valid for these types.");
		}
	}
}
