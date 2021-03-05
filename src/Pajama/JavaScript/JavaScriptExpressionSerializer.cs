using System;
using System.Collections.Generic;
using System.Text;
using Pajama.Node;

namespace Pajama.JavaScript
{
	internal class JavaScriptExpressionSerializer : ExpressionSerializerBase
	{
		public JavaScriptExpressionSerializer() { }

		protected override string QuirkListLength(DotStep expr)
		{
			return "(" + this.Serialize(expr.Root) + ").length";
		}

		protected override string SerializeVariableUsage(VariableUsage expr)
		{
			return expr.Name;
		}

		protected override string SerializeDotStepNoQuirk(DotStep expr)
		{
			return this.Serialize(expr.Root) + "." + expr.Step;
		}

		protected override string SerializeInteger(IntegerConstant expr)
		{
			return expr.Value + "";
		}

		protected override string SerializeFloat(FloatConstant expr)
		{
			return expr.Value + "";
		}

		protected override string SerializeBoolean(BooleanConstant expr)
		{
			return expr.Value ? "true" : "false";
		}

		protected override string SerializeNull(NullConstant expr)
		{
			return "null";
		}

		protected override string SerializeString(StringConstant expr)
		{
			return this.SanitizeString(expr.Value);
		}

		protected override string SerializeBooleanNot(BooleanNot expr)
		{
			return "!(" + this.Serialize(expr.Expression) + ")";
		}

		protected override string SerializeThis(ThisExpression expr)
		{
			return "_jsThis";
		}

		protected override string SerializeConstructorCallNoQuirks(ConstructorCall expr)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("new J." + expr.Class.Class.SimpleName.Replace('.', '_'));
			sb.Append("(");
			for (int i = 0; i < expr.Args.Length; ++i)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				sb.Append(this.Serialize(expr.Args[i]));
			}
			sb.Append(")");
			return sb.ToString();
		}

		protected override string SerializeQuirkMapConstruct(ConstructorCall expr)
		{
			if (expr.Keys != null && expr.Keys.Length > 0)
			{
				List<string> output = new List<string>();
				for (int i = 0; i < expr.Keys.Length; ++i)
				{
					output.Add(this.Serialize(expr.Keys[i]) + ": " + this.Serialize(expr.Values[i]));
				}
				return "{ " + string.Join(", ", output) + " }";
			}
			return "{}";
		}

		protected override string SerializeBracketIndex(BracketIndex expr)
		{
			return this.Serialize(expr.Root) + "[" + this.Serialize(expr.Index) + "]";
		}

		protected override string SerializeStaticHost(StaticHost expr)
		{
			return "J.sh_" + expr.FullName;
		}

		protected override string SerializeQuirkListAdd(FunctionCall expr, DotStep function)
		{
			return "(" + this.Serialize(function.Root) + ").push(" + this.Serialize(expr.Args[0]) + ")";
		}

		protected override string SerializeQuirkMapContains(FunctionCall expr, DotStep function)
		{
			return "(" + this.Serialize(function.Root) + "[" + this.Serialize(expr.Args[0]) + "] !== undefined)";
		}

		protected override string SerializeQuirkStringConcat(bool isLeftString, bool isRightString, string left, string right)
		{
			return "(" + left + ") +" + right;
		}

		protected override string SerializeStaticMethod(StaticMethod expr)
		{
			return "J.sm_" + expr.Member.Parent.FullName.Replace('.', '_') + "__" + expr.Member.Name;
		}

		protected override string SerializeNegative(Negative expr)
		{
			return "-(" + this.Serialize(expr.Expression) + ")";
		}

		protected override string SerializeQuirkListConstruct(ConstructorCall expr)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i < expr.Values.Length; ++i)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				sb.Append(this.Serialize(expr.Values[i]));
			}
			sb.Append("]");
			return sb.ToString();
		}

		protected override string SerializeQuirkMathRandomInt(FunctionCall expr, DotStep function)
		{
			return "Math.floor(Math.random() * (" + this.Serialize(expr.Args[0]) + "))";
		}

		protected override string SerializeQuirkBooleanCombinator(string left, string right, OpTree.OpTypes op)
		{
			return left + (op == OpTree.OpTypes.BooleanAnd ? "&&" : "||") + right;
		}

		protected override string SerializeQuirkIntegerDivision(string left, string right)
		{
			return "Math.floor(" + left + " / " + right + ")";
		}

		protected override string SerializeQuirkListInsert(FunctionCall expr, DotStep function)
		{
			// list.insert(index, value);
			return "(" + this.Serialize(function.Root) + ").splice(" + this.Serialize(expr.Args[0]) + ", 0, " + this.Serialize(expr.Args[1]) + ")";
		}

		protected override string SerializeQuirkListRemoveAt(FunctionCall expr, DotStep function)
		{
			return "(" + this.Serialize(function.Root) + ").splice(" + this.Serialize(expr.Args[0]) + ", 1)";
		}

		protected override string QuirkStringLength(DotStep expr)
		{
			return "(" + this.Serialize(expr.Root) + ").length";
		}
	}
}
