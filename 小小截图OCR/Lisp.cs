using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Windows.Data;
using System.Globalization;
using ExpressionBase = System.Linq.Expressions.Expression;
using System.Windows.Markup;

namespace 小小截图OCR {
	[ContentProperty(nameof(Expression))]
	[MarkupExtensionReturnType(typeof(object))]
	public class Lisp : MarkupExtension, IValueConverter, IMultiValueConverter {
		static readonly Regex lexer = new Regex(@"\(|\)|'(?:''|[^'])*'|-?\d+(\.\d*)?|-?\.\d+|\$\d*|[a-z_][a-z\d_]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		public delegate dynamic ParamArrayDelegate(params dynamic[] args);

		public static readonly Dictionary<string, Delegate> RegisterDelegates = new Dictionary<string, Delegate>() {
			["add"] = (ParamArrayDelegate) (args => {
				if (args == null || args.Length == 0) return null;
				var sum = args[0];
				for (int i = 1; i < args.Length; i++) sum += args[i];
				return sum;
			}),
			["sub"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x - y),
			["mul"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x * y),
			["div"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x / y),
			["true"] = (Func<dynamic>) (() => true),
			["false"] = (Func<dynamic>) (() => false),
			["if"] = (Func<dynamic, dynamic, dynamic, dynamic>) ((cond, T, F) => cond ? T : F),
			["lt"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x < y),
			["gt"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x > y),
			["le"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x <= y),
			["ge"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x >= y),
			["eq"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x == y),
			["ne"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x != y),
			["and"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x && y),
			["or"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => x || y),
			["not"] = (Func<dynamic, dynamic>) (x => !x),
			["concat"] = (ParamArrayDelegate) (args => string.Concat(args)),
			["join"] = (ParamArrayDelegate) (args => string.Join((string) args[0], new ArraySegment<dynamic>(args, 1, args.Length - 1))),
			["int"] = (Func<dynamic, dynamic>) (x => Math.Floor((double) x)),
			["max"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => Math.Max(x, y)),
			["min"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => Math.Min(x, y)),
			["in"] = (Func<dynamic, dynamic, dynamic>) ((x, y) => Array.IndexOf(y, x) >= 0),
			["format"] = (ParamArrayDelegate) (args => string.Format((string) args[0], new ArraySegment<dynamic>(args, 1, args.Length - 1).ToArray()))
		};

		static ExpressionBase CreateInvokeExpression(string funcName, IReadOnlyCollection<ExpressionBase> args) {
			var lambda = RegisterDelegates[funcName];
			var argTypes = lambda.GetType().GetMethod("Invoke").GetParameters();
			if (argTypes.Length == 0 || argTypes[0].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length == 0) {
				if (lambda.Method.GetParameters().Length != args.Count) {
					throw new InvalidOperationException($"函数{funcName}的参数数量不匹配。");
				}
			} else {
				args = new[] { ExpressionBase.NewArrayInit(typeof(object), args) };
			}
			var lambdaExpr = ExpressionBase.Constant(lambda);
			return ExpressionBase.Invoke(lambdaExpr, args);
		}

		static string NextToken(string expr, ref int index) {
			var m = lexer.Match(expr, index);
			if (m.Success) {
				index = m.Index + m.Length;
				return m.Value;
			}
			return null;
		}

		static ExpressionBase LispParse(string expr, ref int index, IDictionary<int, ParameterExpression> argsMap) {
			var token = NextToken(expr, ref index);
			if (token == "(") {
				var funcName = NextToken(expr, ref index);
				var args = new List<ExpressionBase>();
				while (LispParse(expr, ref index, argsMap) is ExpressionBase arg) args.Add(arg);

				return CreateInvokeExpression(funcName, args);
			} else if (token != null && token != ")") {
				if (token[0] == '\'') {
					return ExpressionBase.Constant(token.Substring(1, token.Length - 2).Replace("''", "'"));
				} else if (char.IsDigit(token[0]) || token[0] == '-' || token[0] == '.') {
					return ExpressionBase.Constant(double.Parse(token), typeof(object));
				} else {
					int argId = token != "$" ? int.Parse(token.Substring(1)) : int.MaxValue;
					if (!argsMap.TryGetValue(argId, out var param)) {
						argsMap[argId] = param = ExpressionBase.Parameter(typeof(object), token);
					}
					return param;
				}
			}
			return null;
		}

		public static Delegate Compile(string lispExpr) {
			int index = 0;
			var argsMap = new SortedDictionary<int, ParameterExpression>();
			var invoke = LispParse(lispExpr, ref index, argsMap);
			var lambda = ExpressionBase.Lambda(invoke, argsMap.Values);
			if (lambda.CanReduce) lambda = lambda.Reduce() as LambdaExpression;
			return lambda.Compile();
		}


		public class LispException : System.Exception {
			public LispException() { }
			public LispException(string message) : base(message) { }
		}


		private string expression;
		private Delegate @delegate;


		public Lisp() { }
		public Lisp(string expression) => Expression = expression;

		public string Expression {
			get => expression;
			set {
				if (expression != value) {
					expression = value;
					if (!string.IsNullOrWhiteSpace(value)) {
						@delegate = Compile(value);
					} else {
						@delegate = null;
					}
				}
			}
		}

		public Delegate Delegate => @delegate;

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (@delegate == null) throw new LispException($"{nameof(Expression)}无效。");
			return parameter != null
				? @delegate.DynamicInvoke(value, parameter)
				: @delegate.DynamicInvoke(value);
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

		object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (@delegate == null) throw new LispException($"{nameof(Expression)}无效。");
			if (parameter != null) {
				var newValues = new object[values.Length + 1];
				values.CopyTo(newValues, 0);
				newValues[values.Length] = parameter;
				return @delegate.DynamicInvoke(newValues);
			}
			return @delegate.DynamicInvoke(values);
		}

		object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}
	}
}
