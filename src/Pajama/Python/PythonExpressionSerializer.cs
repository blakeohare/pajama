using System.Collections.Generic;
using System.Text;
using Pajama.Node;

/* TODO: I had an idea:
 * instead of returning strings, return a struct with the string and an order of operations level that this operation is atomic for.
 * For example, function returns <"a + b", '+'> and is passed to another op chain serializer for _ + c. Since it's the same, it would tack
 * it on to the end and return <"a + b + c", '+'>. If it was passed up to a _ * c instead, it would see that * is higher priority, so it'd
 * have to wrap the left: <"(a + b) * c", '*'>.
 */ 
namespace Pajama.Python
{
	internal class PythonExpressionSerializer : ExpressionSerializerBase
	{
		public PythonExpressionSerializer() { }

		protected override string SerializeNull(NullConstant expr)
		{
			return "None";
		}

		protected override string SerializeThis(ThisExpression expr)
		{
			return "self";
		}

		protected override string QuirkListLength(DotStep expr)
		{
			return "len(" + this.Serialize(expr.Root) + ")";
		}

		protected override string SerializeStaticHost(StaticHost expr)
		{
			return "sh_" + expr.FullName;
		}

		protected override string SerializeBracketIndex(BracketIndex expr)
		{
			return this.Serialize(expr.Root) + "[" + this.Serialize(expr.Index) + "]";
		}

		protected override string SerializeQuirkMapContains(FunctionCall expr, DotStep function)
		{
			// TODO: check if expr.args[0] is atomic, and if so, remove parens.
			return "((" + this.Serialize(expr.Args[0]) + ") in " + this.Serialize(function.Root) + ")";
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

		protected override string SerializeConstructorCallNoQuirks(ConstructorCall expr)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(expr.Class.Class.SimpleName.Replace('.', '_'));
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

		protected override string SerializeVariableUsage(VariableUsage expr)
		{
			// TODO: safe names
			return expr.Name;
		}

		protected override string SerializeDotStepNoQuirk(DotStep expr)
		{
			return this.Serialize(expr.Root) + "." + expr.Step;
		}

		protected override string SerializeBooleanNot(BooleanNot expr)
		{
			return "not (" + this.Serialize(expr.Expression) + ")";
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
			return expr.Value ? "True" : "False";
		}

		protected override string SerializeString(StringConstant expr)
		{
			return this.SanitizeString(expr.Value);
		}

		protected override string SerializeQuirkStringConcat(bool isLeftString, bool isRightString, string left, string right)
		{
			if (!isLeftString)
			{
				left = "str(" + left.TrimEnd() + ") ";
			}
			if (!isRightString)
			{
				right = " str(" + right.TrimStart() + ")";
			}
			return left + "+" + right;
		}

		protected override string SerializeStaticMethod(StaticMethod expr)
		{
			return "sm_" + expr.Member.Parent.FullName.Replace('.', '_') + "__" + expr.Member.Name;
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

		protected override string SerializeQuirkListAdd(FunctionCall expr, DotStep function)
		{
			return "(" + this.Serialize(function.Root) + ").append(" + this.Serialize(expr.Args[0]) + ")";
		}

		protected override string SerializeQuirkMathRandomInt(FunctionCall expr, DotStep function)
		{
			return "random.randint(0, (" + this.Serialize(expr.Args[0]) + ") - 1)";
		}

		protected override string SerializeQuirkBooleanCombinator(string left, string right, OpTree.OpTypes op)
		{
			return left + (op == OpTree.OpTypes.BooleanAnd ? "and" : "or") + right;
		}

		protected override string SerializeQuirkIntegerDivision(string left, string right)
		{
			return left + "//" + right;
		}

		protected override string SerializeQuirkListInsert(FunctionCall expr, DotStep function)
		{
			// list.insert(index, value);
			return "(" + this.Serialize(function.Root) + ").insert(" + this.Serialize(expr.Args[0]) + ", " + this.Serialize(expr.Args[1]) + ")";
		}

		protected override string SerializeQuirkListRemoveAt(FunctionCall expr, DotStep function)
		{
			// pop returns the value which is different than void, but since you can't compile with 
			// void being returned into an expression, this is okay.
			// .remove removes a value, not an index.
			return "(" + this.Serialize(function.Root) + ").pop(" + this.Serialize(expr.Args[0]) + ")";
		}

		protected override string QuirkStringLength(DotStep expr)
		{
			return "len(" + this.Serialize(expr.Root) + ")";
		}
	}
}
