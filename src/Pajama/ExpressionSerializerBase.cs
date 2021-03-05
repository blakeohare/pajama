using System;
using System.Text;
using Pajama.Node;

namespace Pajama
{
	internal abstract class ExpressionSerializerBase
	{
		public string Serialize(Expression expr)
		{
			expr = expr.EffectiveValue;

			if (expr is IntegerConstant) return this.SerializeInteger(expr as IntegerConstant);
			if (expr is FloatConstant) return this.SerializeFloat(expr as FloatConstant);
			if (expr is BooleanConstant) return this.SerializeBoolean(expr as BooleanConstant);
			if (expr is NullConstant) return this.SerializeNull(expr as NullConstant);
			if (expr is StringConstant) return this.SerializeString(expr as StringConstant);
			if (expr is BooleanNot) return this.SerializeBooleanNot(expr as BooleanNot);
			if (expr is DotStep) return this.SerializeDotStep(expr as DotStep);
			if (expr is VariableUsage) return this.SerializeVariableUsage(expr as VariableUsage);
			if (expr is ThisExpression) return this.SerializeThis(expr as ThisExpression);
			if (expr is FunctionCall) return this.SerializeFunctionCall(expr as FunctionCall);
			if (expr is ConstructorCall) return this.SerializeConstructorCall(expr as ConstructorCall);
			if (expr is OpChain) return this.SerializeOpChain(expr as OpChain);
			if (expr is BracketIndex) return this.SerializeBracketIndex(expr as BracketIndex);
			if (expr is StaticHost) return this.SerializeStaticHost(expr as StaticHost);
			if (expr is StaticMethod) return this.SerializeStaticMethod(expr as StaticMethod);
			if (expr is Negative) return this.SerializeNegative(expr as Negative);
			if (expr is OpTree) return this.SerializeOpChain(new OpChain(expr as OpTree)); // TODO: figure out why this didn't resolve into an OpChain.
			throw new NotImplementedException();
		}

		protected abstract string SerializeNegative(Negative expr);
		protected abstract string SerializeInteger(IntegerConstant expr);
		protected abstract string SerializeFloat(FloatConstant expr);
		protected abstract string SerializeBoolean(BooleanConstant expr);
		protected abstract string SerializeNull(NullConstant expr);
		protected abstract string SerializeString(StringConstant expr);
		protected abstract string SerializeBooleanNot(BooleanNot expr);
		protected abstract string SerializeVariableUsage(VariableUsage expr);
		protected abstract string SerializeThis(ThisExpression expr);
		protected abstract string SerializeBracketIndex(BracketIndex expr);
		protected abstract string SerializeStaticHost(StaticHost expr);

		protected string SerializeFunctionCall(FunctionCall expr)
		{
			// TODO: there is a bug here.
			// x = myMap.contains;
			// containsFoo = x('foo'); 
			// ^ won't work. Need to tag the Func with some sort of virtualized ID so that it knows it's contains and what its myMap context is.
			// same for other virtualized functions.
			DotStep function = expr.Root as DotStep;

			string key = null;
		
			// Check static references
			if (function != null && function.Root.ResolvesTo.RootType == "Class")
			{
				key = "[STATIC]" + function.Root.ResolvesTo.Generics[0].RootType + "|" + function.Step;
			}
			
			// Non-statics...
			if (function != null && function.Root.ResolvesTo.Class != null) // 2nd expression is for static hosts, which mess things up
			{
				// TODO: this would go great in a helper class. Makes language-specific quirks easy to spot.
				key = function.Root.ResolvesTo.Class.FullName + "|" + function.Step;
			}

			if (key != null)
			{
				switch (key)
				{
					case "List|add":
						return this.SerializeQuirkListAdd(expr, function);

					case "List|insert":
						return this.SerializeQuirkListInsert(expr, function);

					case "List|removeAt":
						return this.SerializeQuirkListRemoveAt(expr, function);

					case "Map|contains":
						return this.SerializeQuirkMapContains(expr, function);

					case "[STATIC]Math|randomInt":
						return this.SerializeQuirkMathRandomInt(expr, function);

					default:
						break;
				}
			}

			StringBuilder sb = new StringBuilder();

			// There's a bug here in JavaScript. AFAICT, "this" needs to be injected as a parameter
			// in order for it to behave the same way as intended in other OO languages.
			sb.Append(this.Serialize(expr.Root));
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


		protected abstract string QuirkListLength(DotStep expr);
		protected abstract string QuirkStringLength(DotStep expr);

		protected string SerializeDotStepQuirk(DotStep expr)
		{
			if (expr.Root.ResolvesTo.Class != null)
			{
				string key = expr.Root.ResolvesTo.Class.FullName + "|" + expr.Step;
				switch (key)
				{
					case "List|length": return this.QuirkListLength(expr);
					default: break;
				}
			}

			if (expr.Root.ResolvesTo == ZType.STRING)
			{
				if (expr.Step == "length")
				{
					return this.QuirkStringLength(expr);
				}
			}

			return null;
		}


		protected string SerializeDotStep(DotStep expr)
		{
			string quirk = this.SerializeDotStepQuirk(expr);
			if (quirk != null)
			{
				return quirk;
			}

			return this.SerializeDotStepNoQuirk(expr);

		}


		protected abstract string SerializeDotStepNoQuirk(DotStep expr);

		protected string SanitizeString(string originalValue)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('"');
			foreach (char c in originalValue)
			{
				switch (c)
				{
					case '\\': sb.Append("\\\\"); break;
					case '\n': sb.Append("\\n"); break;
					case '\r': sb.Append("\\r"); break;
					case '\t': sb.Append("\\t"); break;
					case '\0': sb.Append("\\0"); break;
					case '"': sb.Append("\\\""); break;
					case '\'': sb.Append("\\'"); break;
					default: sb.Append(c); break;
				}
			}
			sb.Append('"');
			return sb.ToString();
		}

		protected abstract string SerializeQuirkListAdd(FunctionCall expr, DotStep function);
		protected abstract string SerializeQuirkListInsert(FunctionCall expr, DotStep function);
		protected abstract string SerializeQuirkListRemoveAt(FunctionCall expr, DotStep function);

		protected abstract string SerializeQuirkMapContains(FunctionCall expr, DotStep function);
		protected abstract string SerializeQuirkMathRandomInt(FunctionCall expr, DotStep function);

		protected abstract string SerializeStaticMethod(StaticMethod expr);

		protected abstract string SerializeQuirkBooleanCombinator(string left, string right, OpTree.OpTypes op);

		private string SerializeOpChain(OpChain expr)
		{
			// LATER: be smarter about parenthesis.
			// update: see comment at top of class.

			string left = this.Serialize(expr.OpTree.Left);
			string right = this.Serialize(expr.OpTree.Right);

			//if (expr.OpTree.Left is OpTree)
			//{
				left = "(" + left + ")";
			//}

			//if (expr.OpTree.Right is OpTree)
			//{
				right = "(" + right + ")";
			//}

			left = left + ' ';
			right = ' ' + right;

			switch (expr.OpTree.Op)
			{
				case OpTree.OpTypes.BitshiftLeft: return left + "<<" + right;
				case OpTree.OpTypes.BitshiftRight: return left + ">>" + right;
				case OpTree.OpTypes.BitwiseAnd: return left + "&" + right;
				case OpTree.OpTypes.BitwiseOr: return left + "|" + right;
				case OpTree.OpTypes.BitwiseXor: return left + "^" + right;
				case OpTree.OpTypes.BooleanAnd: 
				case OpTree.OpTypes.BooleanOr: return this.SerializeQuirkBooleanCombinator(left, right, expr.OpTree.Op);
				case OpTree.OpTypes.Equal: return left + "==" + right;
				case OpTree.OpTypes.NotEqual: return left + "!=" + right;
				case OpTree.OpTypes.FloatAdd: return left + "+" + right;
				case OpTree.OpTypes.FloatDivide: return left + "/" + right;
				case OpTree.OpTypes.FloatModulo: return left + "%" + right;
				case OpTree.OpTypes.FloatMultiply: return left + "*" + right;
				case OpTree.OpTypes.FloatSubtract: return left + "-" + right;
				case OpTree.OpTypes.GreaterThan: return left + ">" + right;
				case OpTree.OpTypes.GreaterThanOrEqual: return left + ">=" + right;
				case OpTree.OpTypes.IntAdd: return left + "+" + right;
				case OpTree.OpTypes.IntDivide: return this.SerializeQuirkIntegerDivision(left, right);
				case OpTree.OpTypes.IntModulo: return left + "%" + right;
				case OpTree.OpTypes.IntMultiply: return left + "*" + right;
				case OpTree.OpTypes.IntSubtract: return left + "-" + right;
				case OpTree.OpTypes.LessThan: return left + "<" + right;
				case OpTree.OpTypes.LessThanOrEqual: return left + "<=" + right;
				case OpTree.OpTypes.StringConcat:
					return this.SerializeQuirkStringConcat(
						expr.OpTree.Left.ResolvesTo == ZType.STRING,
						expr.OpTree.Right.ResolvesTo == ZType.STRING,
						left,
						right);
				default:
					throw new NotImplementedException();
			}

		}

		private string MaybeSerializeQuirkConstructor(ConstructorCall expr)
		{
			string className = expr.Class.Class.FullName;
			switch (className)
			{
				case "Map": return this.SerializeQuirkMapConstruct(expr);
				case "List": return this.SerializeQuirkListConstruct(expr);
				default: return null;
			}
		}

		private string SerializeConstructorCall(ConstructorCall expr)
		{
			string output = this.MaybeSerializeQuirkConstructor(expr);
			if (output != null) return output;

			return this.SerializeConstructorCallNoQuirks(expr);
		}

		protected abstract string SerializeQuirkStringConcat(bool isLeftString, bool isRightString, string left, string right);
		protected abstract string SerializeQuirkIntegerDivision(string left, string right);
		protected abstract string SerializeQuirkListConstruct(ConstructorCall expr);
		protected abstract string SerializeQuirkMapConstruct(ConstructorCall expr);
		protected abstract string SerializeConstructorCallNoQuirks(ConstructorCall expr);
	}
}
