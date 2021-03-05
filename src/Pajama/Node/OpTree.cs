using System;
using System.Collections.Generic;

namespace Pajama.Node
{
	internal class OpTree : Expression
	{
		public override void ResolveTypes(TypeResolver typeResolver, Class scope, List<Dictionary<string, ZType>> typesByVariableUsage)
		{
			// OpTree is resolved upon creation.
		}

		public enum OpTypes
		{
			FloatAdd,
			IntAdd,
			StringConcat,
			FloatSubtract,
			IntSubtract,
			FloatMultiply,
			IntMultiply,
			IntDivide,
			FloatDivide,
			IntModulo,
			FloatModulo,
			IntNegate,
			BoolNegate,
			BooleanAnd,
			BooleanOr,
			BitwiseAnd,
			BitwiseOr,
			BitwiseXor,
			BitshiftLeft,
			BitshiftRight,
			Equal,
			NotEqual,
			LessThan,
			GreaterThan,
			LessThanOrEqual,
			GreaterThanOrEqual
		}

		public Expression Left { get; private set; }
		public Expression Right { get; private set; }
		public OpTypes Op { get; private set; }

		private bool AreInts(ZType left, ZType right)
		{
			return left == ZType.INT && right == ZType.INT;
		}

		private bool AreNums(ZType left, ZType right)
		{
			return (left == ZType.INT || left == ZType.FLOAT) && (right == ZType.INT || right == ZType.FLOAT);
		}

		public OpTree(Expression resolvedLeft, Expression resolvedRight, string op, Token token)
			: base(token)
		{
			this.Left = resolvedLeft;
			this.Right = resolvedRight;
			ZType leftType = this.Left.ResolvesTo;
			ZType rightType = this.Right.ResolvesTo;
			switch (op)
			{
				case "+":
					if (leftType == ZType.STRING || rightType == ZType.STRING)
					{
						this.ResolvesTo = ZType.STRING;
						this.Op = OpTypes.StringConcat;
					}
					else if (AreInts(leftType, rightType))
					{
						this.ResolvesTo = ZType.INT;
						this.Op = OpTypes.IntAdd;
					}
					else if (AreNums(leftType, rightType))
					{
						this.ResolvesTo = ZType.FLOAT;
						this.Op = OpTypes.FloatAdd;
					}
					break;
				case "-":
					if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.IntSubtract;
						this.ResolvesTo = ZType.INT;
					}
					else if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.FloatSubtract;
						this.ResolvesTo = ZType.FLOAT;
					}
					break;
				case "*":
					if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.IntMultiply;
						this.ResolvesTo = ZType.INT;
					}
					else if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.FloatMultiply;
						this.ResolvesTo = ZType.FLOAT;
					}
					break;
				case "/":
					if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.IntDivide;
						this.ResolvesTo = ZType.INT;
					}
					else if (AreNums(leftType, rightType))
					{
						this.Op = OpTypes.FloatDivide;
						this.ResolvesTo = ZType.FLOAT;
					}
					break;
				case "%":
					if (AreInts(leftType, rightType))
					{
						this.Op = OpTypes.IntModulo;
						this.ResolvesTo = ZType.FLOAT;
					}
					else if (AreNums(leftType, rightType))
					{
						this.Op = OpTypes.FloatModulo;
						this.ResolvesTo = ZType.FLOAT;
					}
					break;
				case "&&":
				case "||":
					if (leftType == ZType.BOOL && rightType == ZType.BOOL)
					{
						this.Op = op == "&&" ? OpTypes.BooleanAnd : OpTypes.BooleanOr;
						this.ResolvesTo = ZType.BOOL;
					}
					break;
				case "==":
				case "!=":
					if (leftType == ZType.VOID || rightType == ZType.VOID)
					{
						throw new ParserException(this.Token, "Void type isn't allowed in comparison.");
					}
					this.Op = op == "==" ? OpTypes.Equal : OpTypes.NotEqual;
					this.ResolvesTo = ZType.BOOL;
					break;
				case "<<":
				case ">>":
					if (AreInts(leftType, rightType))
					{
						this.Op = op == "<<" ? OpTypes.BitshiftLeft : OpTypes.BitshiftRight;
						this.ResolvesTo = ZType.INT;
					}
					break;
				case "<=":
				case ">=":
				case "<":
				case ">":
					if (AreNums(leftType, rightType))
					{
						if (op == "<=") this.Op = OpTypes.LessThanOrEqual;
						else if (op == ">=") this.Op = OpTypes.GreaterThanOrEqual;
						else if (op == "<") this.Op = OpTypes.LessThan;
						else this.Op = OpTypes.GreaterThan;
						this.ResolvesTo = ZType.BOOL;
					}
					break;

				case "&":
				case "|":
				case "^":
					if (AreInts(leftType, rightType))
					{
						this.Op = op == "&" ? OpTypes.BitwiseAnd : (op == "|" ? OpTypes.BitwiseOr : OpTypes.BitwiseXor);
						this.ResolvesTo = ZType.INT;
					}
					break;


				default:
					throw new Exception("Invalid operator"); // This should not happen.
			}

			if (this.ResolvesTo == null)
			{
				// LATER: make this more specific since you have the info.
				throw new ParserException(this.Token, "This operator is not valid for these types.");
			}
		}
	}
}
