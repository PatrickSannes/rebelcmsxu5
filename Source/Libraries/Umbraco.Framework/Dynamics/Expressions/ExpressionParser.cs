using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace Umbraco.Framework.Dynamics.Expressions
{
    public class ExpressionParser
    {
        private static readonly Type[] PredefinedTypes = new Type[20]
            {
                typeof (object),
                typeof (bool),
                typeof (char),
                typeof (string),
                typeof (sbyte),
                typeof (byte),
                typeof (short),
                typeof (ushort),
                typeof (int),
                typeof (uint),
                typeof (long),
                typeof (ulong),
                typeof (float),
                typeof (double),
                typeof (Decimal),
                typeof (DateTime),
                typeof (TimeSpan),
                typeof (Guid),
                typeof (Math),
                typeof (Convert)
            };

        private static readonly Expression TrueLiteral = Expression.Constant(true);
        private static readonly Expression FalseLiteral = Expression.Constant(false);
        private static readonly Expression NullLiteral = Expression.Constant(null);
        private const string KeywordIt = "it";
        private const string KeywordIif = "iif";
        private const string KeywordNew = "new";
        private static Dictionary<string, object> _keywords;
        private readonly Dictionary<Expression, string> _literals;
        private readonly Dictionary<string, object> _symbols;
        private readonly string _expressionText;
        private readonly int _textLen;
        private char _currentChar;
        private IDictionary<string, object> _externals;
        private ParameterExpression _it;
        private int _currentTextPosition;
        private Token _token;

        public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (_keywords == null)
                _keywords = CreateKeywords();
            this._symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this._literals = new Dictionary<Expression, string>();
            if (parameters != null)
                this.ProcessParameters(parameters);
            if (values != null)
                this.ProcessValues(values);
            this._expressionText = expression;
            this._textLen = this._expressionText.Length;
            this.SetTextPos(0);
            this.NextToken();
        }

        private void ProcessParameters(ParameterExpression[] parameters)
        {
            foreach (var parameterExpression in parameters)
            {
                if (!string.IsNullOrEmpty(parameterExpression.Name))
                    this.AddSymbol(parameterExpression.Name, parameterExpression);
            }
            if (parameters.Length != 1 || !string.IsNullOrEmpty(parameters[0].Name))
                return;
            this._it = parameters[0];
        }

        private void ProcessValues(object[] values)
        {
            for (int index = 0; index < values.Length; ++index)
            {
                object obj = values[index];
                if (index == values.Length - 1 && obj is IDictionary<string, object>)
                    this._externals = (IDictionary<string, object>) obj;
                else
                    this.AddSymbol("@" + index.ToString(CultureInfo.InvariantCulture), obj);
            }
        }

        private void AddSymbol(string name, object value)
        {
            if (this._symbols.ContainsKey(name))
                throw this.ParseError(
                    "The identifier '{0}' was defined more than once",
                    new object[1]
                        {
                            name
                        });
            else
                this._symbols.Add(name, value);
        }

        public Expression Parse(Type resultType)
        {
            int pos = this._token.Position;
            Expression expr = this.ParseExpression();
            if (resultType != null && (expr = this.PromoteExpression(expr, resultType, true)) == null)
            {
                throw ParseError(
                    pos,
                    "Expression of type '{0}' expected",
                    new object[1]
                        {
                            GetTypeName(resultType)
                        });
            }
            else
            {
                this.ValidateToken(TokenId.End, "Syntax error");
                return expr;
            }
        }

        public IEnumerable<DynamicOrdering> ParseOrdering()
        {
            var list = new List<DynamicOrdering>();
            while (true)
            {
                Expression expression = this.ParseExpression();
                bool flag = true;
                if (this.TokenIdentifierIs("asc") || this.TokenIdentifierIs("ascending"))
                    this.NextToken();
                else if (this.TokenIdentifierIs("desc") || this.TokenIdentifierIs("descending"))
                {
                    this.NextToken();
                    flag = false;
                }
                list.Add(
                    new DynamicOrdering
                        {
                            Selector = expression,
                            Ascending = flag
                        });
                if (this._token.Id == TokenId.Comma)
                    this.NextToken();
                else
                    break;
            }
            this.ValidateToken(TokenId.End, "Syntax error");
            return list;
        }

        private Expression ParseExpression()
        {
            int errorPos = this._token.Position;
            Expression test = this.ParseLogicalOr();
            if (this._token.Id == TokenId.Question)
            {
                this.NextToken();
                Expression expr1 = this.ParseExpression();
                this.ValidateToken(TokenId.Colon, "':' expected");
                this.NextToken();
                Expression expr2 = this.ParseExpression();
                test = this.GenerateConditional(test, expr1, expr2, errorPos);
            }
            return test;
        }

        private Expression ParseLogicalOr()
        {
            Expression left = this.ParseLogicalAnd();
            while (this._token.Id == TokenId.DoubleBar || this.TokenIdentifierIs("or"))
            {
                Token token = this._token;
                this.NextToken();
                Expression right = this.ParseLogicalAnd();
                this.CheckAndPromoteOperands(typeof (ILogicalSignatures), token.Text, ref left, ref right, token.Position);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        private Expression ParseLogicalAnd()
        {
            Expression left = this.ParseComparison();
            while (this._token.Id == TokenId.DoubleAmphersand || this.TokenIdentifierIs("and"))
            {
                Token token = this._token;
                this.NextToken();
                Expression right = this.ParseComparison();
                this.CheckAndPromoteOperands(typeof (ILogicalSignatures), token.Text, ref left, ref right, token.Position);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        private Expression ParseComparison()
        {
            Expression left = this.ParseAdditive();
            while (this._token.Id == TokenId.Equal || this._token.Id == TokenId.DoubleEqual ||
                   (this._token.Id == TokenId.ExclamationEqual || this._token.Id == TokenId.LessGreater) ||
                   (this._token.Id == TokenId.GreaterThan || this._token.Id == TokenId.GreaterThanEqual ||
                    (this._token.Id == TokenId.LessThan || this._token.Id == TokenId.LessThanEqual)))
            {
                Token token = this._token;
                this.NextToken();
                Expression right = this.ParseAdditive();
                bool flag = token.Id == TokenId.Equal || token.Id == TokenId.DoubleEqual ||
                            token.Id == TokenId.ExclamationEqual || token.Id == TokenId.LessGreater;
                if (flag && !left.Type.IsValueType && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type))
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                        else
                        {
                            if (!right.Type.IsAssignableFrom(left.Type))
                                throw this.IncompatibleOperandsError(token.Text, left, right, token.Position);
                            left = Expression.Convert(left, right.Type);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression expression1;
                        if ((expression1 = this.PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = expression1;
                        }
                        else
                        {
                            Expression expression2;
                            if ((expression2 = this.PromoteExpression(left, right.Type, true)) == null)
                                throw this.IncompatibleOperandsError(token.Text, left, right, token.Position);
                            left = expression2;
                        }
                    }
                }
                else
                    this.CheckAndPromoteOperands(
                        flag ? typeof (IEqualitySignatures) : typeof (IRelationalSignatures),
                        token.Text,
                        ref left,
                        ref right,
                        token.Position);
                switch (token.Id)
                {
                    case TokenId.LessThan:
                        left = this.GenerateLessThan(left, right);
                        continue;
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = this.GenerateEqual(left, right);
                        continue;
                    case TokenId.GreaterThan:
                        left = this.GenerateGreaterThan(left, right);
                        continue;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = this.GenerateNotEqual(left, right);
                        continue;
                    case TokenId.LessThanEqual:
                        left = this.GenerateLessThanEqual(left, right);
                        continue;
                    case TokenId.GreaterThanEqual:
                        left = this.GenerateGreaterThanEqual(left, right);
                        continue;
                    default:
                        continue;
                }
            }
            return left;
        }

        private Expression ParseAdditive()
        {
            Expression left = this.ParseMultiplicative();
            while (this._token.Id == TokenId.Plus || this._token.Id == TokenId.Minus ||
                   this._token.Id == TokenId.Amphersand)
            {
                Token token = this._token;
                this.NextToken();
                Expression right = this.ParseMultiplicative();
                switch (token.Id)
                {
                    case TokenId.Amphersand:
                        left = this.GenerateStringConcat(left, right);
                        continue;
                    case TokenId.Plus:
                        if (left.Type != typeof (string) && right.Type != typeof (string))
                        {
                            this.CheckAndPromoteOperands(
                                typeof (IAddSignatures), token.Text, ref left, ref right, token.Position);
                            left = this.GenerateAdd(left, right);
                            continue;
                        }
                        else
                            goto case TokenId.Amphersand;
                    case TokenId.Minus:
                        this.CheckAndPromoteOperands(
                            typeof (ISubtractSignatures), token.Text, ref left, ref right, token.Position);
                        left = this.GenerateSubtract(left, right);
                        continue;
                    default:
                        continue;
                }
            }
            return left;
        }

        private Expression ParseMultiplicative()
        {
            Expression left = this.ParseUnary();
            while (this._token.Id == TokenId.Asterisk || this._token.Id == TokenId.Slash ||
                   (this._token.Id == TokenId.Percent || this.TokenIdentifierIs("mod")))
            {
                Token token = this._token;
                this.NextToken();
                Expression right = this.ParseUnary();
                this.CheckAndPromoteOperands(typeof (IArithmeticSignatures), token.Text, ref left, ref right, token.Position);
                switch (token.Id)
                {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        continue;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        continue;
                    case TokenId.Identifier:
                    case TokenId.Percent:
                        left = Expression.Modulo(left, right);
                        continue;
                    default:
                        continue;
                }
            }
            return left;
        }

        private Expression ParseUnary()
        {
            if (this._token.Id != TokenId.Minus && this._token.Id != TokenId.Exclamation && !this.TokenIdentifierIs("not"))
                return this.ParsePrimary();
            Token token = this._token;
            this.NextToken();
            if (token.Id == TokenId.Minus &&
                (this._token.Id == TokenId.IntegerLiteral || this._token.Id == TokenId.RealLiteral))
            {
                this._token.Text = "-" + this._token.Text;
                this._token.Position = token.Position;
                return this.ParsePrimary();
            }
            else
            {
                Expression expr = this.ParseUnary();
                Expression expression;
                if (token.Id == TokenId.Minus)
                {
                    this.CheckAndPromoteOperand(typeof (INegationSignatures), token.Text, ref expr, token.Position);
                    expression = Expression.Negate(expr);
                }
                else
                {
                    this.CheckAndPromoteOperand(typeof (INotSignatures), token.Text, ref expr, token.Position);
                    expression = Expression.Not(expr);
                }
                return expression;
            }
        }

        private Expression ParsePrimary()
        {
            Expression expression = this.ParsePrimaryStart();
            while (true)
            {
                while (this._token.Id != TokenId.Dot)
                {
                    if (this._token.Id != TokenId.OpenBracket)
                        return expression;
                    expression = this.ParseElementAccess(expression);
                }
                this.NextToken();
                expression = this.ParseMemberAccess(null, expression);
            }
        }

        private Expression ParsePrimaryStart()
        {
            switch (this._token.Id)
            {
                case TokenId.Identifier:
                    return this.ParseIdentifier();
                case TokenId.StringLiteral:
                    return this.ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return this.ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return this.ParseRealLiteral();
                case TokenId.OpenParen:
                    return this.ParseParenExpression();
                default:
                    throw this.ParseError("Expression expected", new object[0]);
            }
        }

        private Expression ParseStringLiteral()
        {
            this.ValidateToken(TokenId.StringLiteral);
            char ch = this._token.Text[0];
            string text = this._token.Text.Substring(1, this._token.Text.Length - 2);
            int startIndex1 = 0;
            while (true)
            {
                int startIndex2 = text.IndexOf(ch, startIndex1);
                if (startIndex2 >= 0)
                {
                    text = text.Remove(startIndex2, 1);
                    startIndex1 = startIndex2 + 1;
                }
                else
                    break;
            }
            if (ch == 39)
            {
                if (text.Length != 1)
                    throw this.ParseError("Character literal must contain exactly one character", new object[0]);
                this.NextToken();
                return this.CreateLiteral(text[0], text);
            }
            else
            {
                this.NextToken();
                return this.CreateLiteral(text, text);
            }
        }

        private Expression ParseIntegerLiteral()
        {
            this.ValidateToken(TokenId.IntegerLiteral);
            string str = this._token.Text;
            if (str[0] != 45)
            {
                ulong result;
                if (!ulong.TryParse(str, out result))
                {
                    throw this.ParseError(
                        "Invalid integer literal '{0}'",
                        new object[1]
                            {
                                str
                            });
                }
                else
                {
                    this.NextToken();
                    if (result <= int.MaxValue || result <= uint.MaxValue || result > 9223372036854775807UL)
                        return this.CreateLiteral((int) result, str);
                    else
                        return this.CreateLiteral((long) result, str);
                }
            }
            else
            {
                long result;
                if (!long.TryParse(str, out result))
                {
                    throw this.ParseError(
                        "Invalid integer literal '{0}'",
                        new object[1]
                            {
                                str
                            });
                }
                else
                {
                    this.NextToken();
                    if (result >= int.MinValue && result <= int.MaxValue)
                        return this.CreateLiteral((int) result, str);
                    else
                        return this.CreateLiteral(result, str);
                }
            }
        }

        private Expression ParseRealLiteral()
        {
            this.ValidateToken(TokenId.RealLiteral);
            string str = this._token.Text;
            object obj = null;
            switch (str[str.Length - 1])
            {
                case 'F':
                case 'f':
                    float result1;
                    if (float.TryParse(str.Substring(0, str.Length - 1), out result1))
                    {
                        obj = result1;
                        break;
                    }
                    else
                        break;
                default:
                    double result2;
                    if (double.TryParse(str, out result2))
                    {
                        obj = result2;
                        break;
                    }
                    else
                        break;
            }
            if (obj == null)
            {
                throw this.ParseError(
                    "Invalid real literal '{0}'",
                    new object[1]
                        {
                            str
                        });
            }
            else
            {
                this.NextToken();
                return this.CreateLiteral(obj, str);
            }
        }

        private Expression CreateLiteral(object value, string text)
        {
            ConstantExpression constantExpression = Expression.Constant(value);
            this._literals.Add(constantExpression, text);
            return constantExpression;
        }

        private Expression ParseParenExpression()
        {
            this.ValidateToken(TokenId.OpenParen, "'(' expected");
            this.NextToken();
            Expression expression = this.ParseExpression();
            this.ValidateToken(TokenId.CloseParen, "')' or operator expected");
            this.NextToken();
            return expression;
        }

        private Expression ParseIdentifier()
        {
            this.ValidateToken(TokenId.Identifier);
            object obj;
            if (_keywords.TryGetValue(this._token.Text, out obj))
            {
                if (obj is Type)
                    return this.ParseTypeAccess((Type) obj);
                if (obj == KeywordIt)
                    return this.ParseIt();
                if (obj == KeywordIif)
                    return this.ParseIif();
                if (obj == KeywordNew)
                    return this.ParseNew();
                this.NextToken();
                return (Expression) obj;
            }
            else if (this._symbols.TryGetValue(this._token.Text, out obj) ||
                     this._externals != null && this._externals.TryGetValue(this._token.Text, out obj))
            {
                var expression = obj as Expression;
                if (expression == null)
                {
                    expression = Expression.Constant(obj);
                }
                else
                {
                    var lambda = expression as LambdaExpression;
                    if (lambda != null)
                        return this.ParseLambdaInvocation(lambda);
                }
                this.NextToken();
                return expression;
            }
            else
            {
                if (this._it != null)
                    return this.ParseMemberAccess(null, this._it);
                throw this.ParseError(
                    "Unknown identifier '{0}'",
                    new object[1]
                        {
                            this._token.Text
                        });
            }
        }

        private Expression ParseIt()
        {
            if (this._it == null)
                throw this.ParseError("No 'it' is in scope", new object[0]);
            this.NextToken();
            return this._it;
        }

        private Expression ParseIif()
        {
            int num = this._token.Position;
            this.NextToken();
            Expression[] expressionArray = this.ParseArgumentList();
            if (expressionArray.Length != 3)
                throw ParseError(num, "The 'iif' function requires three arguments", new object[0]);
            else
                return this.GenerateConditional(expressionArray[0], expressionArray[1], expressionArray[2], num);
        }

        private Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof (bool))
                throw ParseError(errorPos, "The first expression must be of type 'Boolean'", new object[0]);
            if (expr1.Type != expr2.Type)
            {
                Expression expression1 = expr2 != NullLiteral ? this.PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expression2 = expr1 != NullLiteral ? this.PromoteExpression(expr2, expr1.Type, true) : null;
                if (expression1 != null && expression2 == null)
                    expr1 = expression1;
                else if (expression2 != null && expression1 == null)
                {
                    expr2 = expression2;
                }
                else
                {
                    string str1 = expr1 != NullLiteral ? expr1.Type.Name : "null";
                    string str2 = expr2 != NullLiteral ? expr2.Type.Name : "null";
                    if (expression1 != null && expression2 != null)
                        throw ParseError(
                            errorPos,
                            "Both of the types '{0}' and '{1}' convert to the other",
                            (object) str1,
                            (object) str2);
                    else
                        throw ParseError(
                            errorPos,
                            "Neither of the types '{0}' and '{1}' converts to the other",
                            (object) str1,
                            (object) str2);
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        private Expression ParseNew()
        {
            this.NextToken();
            this.ValidateToken(TokenId.OpenParen, "'(' expected");
            this.NextToken();
            var list1 = new List<DynamicProperty>();
            var list2 = new List<Expression>();
            int pos;
            while (true)
            {
                pos = this._token.Position;
                Expression expression = this.ParseExpression();
                string name;
                if (this.TokenIdentifierIs("as"))
                {
                    this.NextToken();
                    name = this.GetIdentifier();
                    this.NextToken();
                }
                else
                {
                    var memberExpression = expression as MemberExpression;
                    if (memberExpression != null)
                        name = memberExpression.Member.Name;
                    else
                        break;
                }
                list2.Add(expression);
                list1.Add(new DynamicProperty(name, expression.Type));
                if (this._token.Id == TokenId.Comma)
                    this.NextToken();
                else
                    goto label_8;
            }
            throw ParseError(pos, "Expression is missing an 'as' clause", new object[0]);
            label_8:
            this.ValidateToken(TokenId.CloseParen, "')' or ',' expected");
            this.NextToken();
            Type @class = DynamicExpression.CreateClass(list1);
            var memberBindingArray = new MemberBinding[list1.Count];
            for (int index = 0; index < memberBindingArray.Length; ++index)
                memberBindingArray[index] = Expression.Bind(@class.GetProperty(list1[index].Name), list2[index]);
            return Expression.MemberInit(Expression.New(@class), memberBindingArray);
        }

        private Expression ParseLambdaInvocation(LambdaExpression lambda)
        {
            int pos = this._token.Position;
            this.NextToken();
            Expression[] args = this.ParseArgumentList();
            MethodBase method;
            if (this.FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(pos, "Argument list incompatible with lambda expression", new object[0]);
            else
                return Expression.Invoke(lambda, args);
        }

        private Expression ParseTypeAccess(Type type)
        {
            int num = this._token.Position;
            this.NextToken();
            if (this._token.Id == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                {
                    throw ParseError(
                        num,
                        "Type '{0}' has no nullable form",
                        new object[1]
                            {
                                GetTypeName(type)
                            });
                }
                else
                {
                    type = typeof (Nullable<>).MakeGenericType(
                        new Type[1]
                            {
                                type
                            });
                    this.NextToken();
                }
            }
            if (this._token.Id == TokenId.OpenParen)
            {
                Expression[] args = this.ParseArgumentList();
                MethodBase method;
                switch (this.FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                            return this.GenerateConversion(args[0], type, num);
                        throw ParseError(
                            num,
                            "No matching constructor in type '{0}'",
                            new object[1]
                                {
                                    GetTypeName(type)
                                });
                    case 1:
                        return Expression.New((ConstructorInfo) method, args);
                    default:
                        throw ParseError(
                            num,
                            "Ambiguous invocation of '{0}' constructor",
                            new object[1]
                                {
                                    GetTypeName(type)
                                });
                }
            }
            else
            {
                this.ValidateToken(TokenId.Dot, "'.' or '(' expected");
                this.NextToken();
                return this.ParseMemberAccess(type, null);
            }
        }

        private Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            Type type1 = expr.Type;
            if (type1 == type)
                return expr;
            if (type1.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(type1) || IsNullableType(type)) &&
                    GetNonNullableType(type1) == GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((IsNumericType(type1) || IsEnumType(type1)) && IsNumericType(type) || IsEnumType(type))
                    return Expression.ConvertChecked(expr, type);
            }
            if (type1.IsAssignableFrom(type) || type.IsAssignableFrom(type1) || (type1.IsInterface || type.IsInterface))
                return Expression.Convert(expr, type);
            throw ParseError(
                errorPos,
                "A value of type '{0}' cannot be converted to type '{1}'",
                (object) GetTypeName(type1),
                (object) GetTypeName(type));
        }

        private Expression ParseMemberAccess(Type type, Expression instance)
        {
            if (instance != null)
                type = instance.Type;
            int num = this._token.Position;
            string identifier = this.GetIdentifier();
            this.NextToken();
            if (this._token.Id == TokenId.OpenParen)
            {
                if (instance != null && type != typeof (string))
                {
                    Type genericType = FindGenericType(typeof (IEnumerable<>), type);
                    if (genericType != null)
                    {
                        Type elementType = genericType.GetGenericArguments()[0];
                        return this.ParseAggregate(instance, elementType, identifier, num);
                    }
                }
                Expression[] args = this.ParseArgumentList();
                MethodBase method1;
                switch (this.FindMethod(type, identifier, instance == null, args, out method1))
                {
                    case 0:
                        throw ParseError(
                            num,
                            "No applicable method '{0}' exists in type '{1}'",
                            (object) identifier,
                            (object) GetTypeName(type));
                    case 1:
                        var method2 = (MethodInfo) method1;
                        if (!IsPredefinedType(method2.DeclaringType))
                        {
                            throw ParseError(
                                num,
                                "Methods on type '{0}' are not accessible",
                                new object[1]
                                    {
                                        GetTypeName(method2.DeclaringType)
                                    });
                        }
                        else
                        {
                            if (method2.ReturnType != typeof (void))
                                return Expression.Call(instance, method2, args);
                            throw ParseError(
                                num,
                                "Method '{0}' in type '{1}' does not return a value",
                                (object) identifier,
                                (object) GetTypeName(method2.DeclaringType));
                        }
                    default:
                        throw ParseError(
                            num,
                            "Ambiguous invocation of method '{0}' in type '{1}'",
                            (object) identifier,
                            (object) GetTypeName(type));
                }
            }
            else
            {
                MemberInfo propertyOrField = this.FindPropertyOrField(type, identifier, instance == null);
                if (propertyOrField == null)
                {
                    //throw this.ParseError(num, "No property or field '{0}' exists in type '{1}'", (object)identifier, (object)ExpressionParser.GetTypeName(type));

                    // Attempt 1: Works, but Relinq does not support DynamicExpressions
                    //var dynamicCall =
                    //    Expression.Dynamic(
                    //        Binder.GetMember(
                    //            CSharpBinderFlags.InvokeSimpleName,
                    //            identifier,
                    //            null,
                    //            new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)}),
                    //        typeof (object),
                    //        instance);
                    //return dynamicCall;

                    return DynamicMemberMetadata.GetMethodCallForDynamic(identifier);
                }
                else if (!(propertyOrField is PropertyInfo))
                    return Expression.Field(instance, (FieldInfo) propertyOrField);
                else
                    return Expression.Property(instance, (PropertyInfo) propertyOrField);
            }
        }

        private static Type FindGenericType(Type generic, Type type)
        {
            for (; type != null && type != typeof (object); type = type.BaseType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                    return type;
                if (generic.IsInterface)
                {
                    foreach (var type1 in type.GetInterfaces())
                    {
                        Type genericType = FindGenericType(generic, type1);
                        if (genericType != null)
                            return genericType;
                    }
                }
            }
            return null;
        }

        private Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
        {
            ParameterExpression parameterExpression1 = this._it;
            ParameterExpression parameterExpression2 = Expression.Parameter(elementType, "");
            this._it = parameterExpression2;
            Expression[] args = this.ParseArgumentList();
            this._it = parameterExpression1;
            MethodBase method;
            if (this.FindMethod(typeof (IEnumerableSignatures), methodName, false, args, out method) != 1)
            {
                throw ParseError(
                    errorPos,
                    "No applicable aggregate method '{0}' exists",
                    new object[1]
                        {
                            methodName
                        });
            }
            else
            {
                Type[] typeArguments;
                if (method.Name == "Min" || method.Name == "Max")
                    typeArguments = new Type[2]
                        {
                            elementType,
                            args[0].Type
                        };
                else
                    typeArguments = new Type[1]
                        {
                            elementType
                        };
                Expression[] expressionArray1;
                if (args.Length == 0)
                {
                    expressionArray1 = new Expression[1]
                        {
                            instance
                        };
                }
                else
                {
                    var expressionArray2 = new Expression[2]
                        {
                            instance,
                            null
                        };
                    expressionArray2[1] = Expression.Lambda(
                        args[0],
                        new ParameterExpression[1]
                            {
                                parameterExpression2
                            });
                    expressionArray1 = expressionArray2;
                }
                return Expression.Call(typeof (Enumerable), method.Name, typeArguments, expressionArray1);
            }
        }

        private Expression[] ParseArgumentList()
        {
            this.ValidateToken(TokenId.OpenParen, "'(' expected");
            this.NextToken();
            Expression[] expressionArray = this._token.Id != TokenId.CloseParen
                                               ? this.ParseArguments()
                                               : new Expression[0];
            this.ValidateToken(TokenId.CloseParen, "')' or ',' expected");
            this.NextToken();
            return expressionArray;
        }

        private Expression[] ParseArguments()
        {
            var list = new List<Expression>();
            while (true)
            {
                list.Add(this.ParseExpression());
                if (this._token.Id == TokenId.Comma)
                    this.NextToken();
                else
                    break;
            }
            return list.ToArray();
        }

        private Expression ParseElementAccess(Expression expr)
        {
            int pos = this._token.Position;
            this.ValidateToken(TokenId.OpenBracket, "'(' expected");
            this.NextToken();
            Expression[] args = this.ParseArguments();
            this.ValidateToken(TokenId.CloseBracket, "']' or ',' expected");
            this.NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(pos, "Indexing of multi-dimensional arrays is not supported", new object[0]);
                Expression index = this.PromoteExpression(args[0], typeof (int), true);
                if (index == null)
                    throw ParseError(pos, "Array index must be an integer expression", new object[0]);
                else
                    return Expression.ArrayIndex(expr, index);
            }
            else
            {
                MethodBase method;
                switch (this.FindIndexer(expr.Type, args, out method))
                {
                    case 0:
                        throw ParseError(
                            pos,
                            "No applicable indexer exists in type '{0}'",
                            new object[1]
                                {
                                    GetTypeName(expr.Type)
                                });
                    case 1:
                        return Expression.Call(expr, (MethodInfo) method, args);
                    default:
                        throw ParseError(
                            pos,
                            "Ambiguous invocation of indexer in type '{0}'",
                            new object[1]
                                {
                                    GetTypeName(expr.Type)
                                });
                }
            }
        }

        private static bool IsPredefinedType(Type type)
        {
            foreach (var type1 in PredefinedTypes)
            {
                if (type1 == type)
                    return true;
            }
            return false;
        }

        private static bool IsNullableType(Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof (Nullable<>);
            else
                return false;
        }

        private static Type GetNonNullableType(Type type)
        {
            if (!IsNullableType(type))
                return type;
            else
                return type.GetGenericArguments()[0];
        }

        private static string GetTypeName(Type type)
        {
            Type nonNullableType = GetNonNullableType(type);
            string str = nonNullableType.Name;
            if (type != nonNullableType)
                str = str + '?';
            return str;
        }

        private static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        private static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        private static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        private static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum)
                return 0;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;
                default:
                    return 0;
            }
        }

        private static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        private void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            var args = new Expression[1]
                {
                    expr
                };
            MethodBase method;
            if (this.FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(
                    errorPos,
                    "Operator '{0}' incompatible with operand type '{1}'",
                    (object) opName,
                    (object) GetTypeName(args[0].Type));
            else
                expr = args[0];
        }

        private void CheckAndPromoteOperands(
            Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            var args = new Expression[2]
                {
                    left,
                    right
                };
            MethodBase method;

            // Start Umbraco mod: If this is a dynamic member, wrap the expression in a cast so that the types are equal
            var leftMethod = args[0] as MethodCallExpression;
            var rightConstant = args[1] as ConstantExpression;
            bool ignoreMethodCheck = false;
            if (leftMethod != null && rightConstant != null)
            {
                var leftDynamicMethod = leftMethod.Method;

                if (leftDynamicMethod == DynamicMemberMetadata.GetMemberMethod)
                {
                    // This is a dynamic access, the method normally returns Object, so if the opposite argument's
                    // type is not Object, wrap the method in a Convert to make it a Unary
                    if (leftDynamicMethod.ReturnType != rightConstant.Type)
                    {
                        args[0] = Expression.Convert(args[0], rightConstant.Type);
                        ignoreMethodCheck = true;
                    }
                }
            }
            // End Umbraco mod

            if (!ignoreMethodCheck && this.FindMethod(signatures, "F", false, args, out method) != 1)
                throw this.IncompatibleOperandsError(opName, left, right, errorPos);
            left = args[0];
            right = args[1];
        }

        private Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
        {
            return ParseError(
                pos,
                "Operator '{0}' incompatible with operand types '{1}' and '{2}'",
                (object) opName,
                (object) GetTypeName(left.Type),
                (object) GetTypeName(right.Type));
        }

        private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            var bindingAttr = (BindingFlags) (18 | (staticAccess ? 8 : 4));
            foreach (var type1 in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = type1.FindMembers(
                    MemberTypes.Field | MemberTypes.Property, bindingAttr, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0)
                    return members[0];
            }
            return null;
        }

        private int FindMethod(
            Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
        {
            var bindingAttr = (BindingFlags) (18 | (staticAccess ? 8 : 4));
            foreach (var type1 in SelfAndBaseTypes(type))
            {
                int bestMethod =
                    this.FindBestMethod(
                        (type1.FindMembers(MemberTypes.Method, bindingAttr, Type.FilterNameIgnoreCase, methodName)).Cast
                            <MethodBase>(),
                        args,
                        out method);
                if (bestMethod != 0)
                    return bestMethod;
            }
            method = null;
            return 0;
        }

        private int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            foreach (var type1 in SelfAndBaseTypes(type))
            {
                MemberInfo[] defaultMembers = type1.GetDefaultMembers();
                if (defaultMembers.Length != 0)
                {
                    int bestMethod =
                        this.FindBestMethod(
                            (defaultMembers).OfType<PropertyInfo>().Select((p => (MethodBase) p.GetGetMethod())).Where(
                                (m => m != null)),
                            args,
                            out method);
                    if (bestMethod != 0)
                        return bestMethod;
                }
            }
            method = null;
            return 0;
        }

        private static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (!type.IsInterface)
                return SelfAndBaseClasses(type);
            var types = new List<Type>();
            AddInterface(types, type);
            return types;
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            for (; type != null; type = type.BaseType)
                yield return type;
        }

        private static void AddInterface(List<Type> types, Type type)
        {
            if (types.Contains(type))
                return;
            types.Add(type);
            foreach (var type1 in type.GetInterfaces())
                AddInterface(types, type1);
        }

        private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            MethodData[] applicable = methods.Select(
                (m => new MethodData
                    {
                        MethodBase = m,
                        Parameters = m.GetParameters()
                    })).Where((m => this.IsApplicable(m, args))).ToArray();
            if (applicable.Length > 1)
                applicable = (applicable).Where(
                    (m => (applicable).All(
                        (n =>
                            {
                                if (m != n)
                                    return IsBetterThan(args, m, n);
                                else
                                    return true;
                            })))).ToArray();
            if (applicable.Length == 1)
            {
                MethodData methodData = applicable[0];
                for (int index = 0; index < args.Length; ++index)
                    args[index] = methodData.Args[index];
                method = methodData.MethodBase;
            }
            else
                method = null;
            return applicable.Length;
        }

        private bool IsApplicable(MethodData method, Expression[] args)
        {
            if (method.Parameters.Length != args.Length)
                return false;
            var expressionArray = new Expression[args.Length];
            for (int index = 0; index < args.Length; ++index)
            {
                ParameterInfo parameterInfo = method.Parameters[index];
                if (parameterInfo.IsOut)
                    return false;
                Expression expression = this.PromoteExpression(args[index], parameterInfo.ParameterType, false);
                if (expression == null)
                    return false;
                expressionArray[index] = expression;
            }
            method.Args = expressionArray;
            return true;
        }

        private Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type == type)
                return expr;
            if (expr is ConstantExpression)
            {
                var constantExpression = (ConstantExpression) expr;
                if (constantExpression == NullLiteral)
                {
                    if (!type.IsValueType || IsNullableType(type))
                        return Expression.Constant(null, type);
                }
                else
                {
                    string str;
                    if (this._literals.TryGetValue(constantExpression, out str))
                    {
                        Type nonNullableType = GetNonNullableType(type);
                        object obj = null;
                        switch (Type.GetTypeCode(constantExpression.Type))
                        {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                obj = ParseNumber(str, nonNullableType);
                                break;
                            case TypeCode.Double:
                                if (nonNullableType == typeof (Decimal))
                                {
                                    obj = ParseNumber(str, nonNullableType);
                                    break;
                                }
                                else
                                    break;
                            case TypeCode.String:
                                obj = ParseEnum(str, nonNullableType);
                                break;
                        }
                        if (obj != null)
                            return Expression.Constant(obj, type);
                    }
                }
            }
            if (!IsCompatibleWith(expr.Type, type))
                return null;
            if (type.IsValueType || exact)
                return Expression.Convert(expr, type);
            else
                return expr;
        }

        private static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte result1;
                    if (sbyte.TryParse(text, out result1))
                        return result1;
                    else
                        break;
                case TypeCode.Byte:
                    byte result2;
                    if (byte.TryParse(text, out result2))
                        return result2;
                    else
                        break;
                case TypeCode.Int16:
                    short result3;
                    if (short.TryParse(text, out result3))
                        return result3;
                    else
                        break;
                case TypeCode.UInt16:
                    ushort result4;
                    if (ushort.TryParse(text, out result4))
                        return result4;
                    else
                        break;
                case TypeCode.Int32:
                    int result5;
                    if (int.TryParse(text, out result5))
                        return result5;
                    else
                        break;
                case TypeCode.UInt32:
                    uint result6;
                    if (uint.TryParse(text, out result6))
                        return result6;
                    else
                        break;
                case TypeCode.Int64:
                    long result7;
                    if (long.TryParse(text, out result7))
                        return result7;
                    else
                        break;
                case TypeCode.UInt64:
                    ulong result8;
                    if (ulong.TryParse(text, out result8))
                        return result8;
                    else
                        break;
                case TypeCode.Single:
                    float result9;
                    if (float.TryParse(text, out result9))
                        return result9;
                    else
                        break;
                case TypeCode.Double:
                    double result10;
                    if (double.TryParse(text, out result10))
                        return result10;
                    else
                        break;
                case TypeCode.Decimal:
                    Decimal result11;
                    if (Decimal.TryParse(text, out result11))
                        return result11;
                    else
                        break;
            }
            return null;
        }

        private static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                MemberInfo[] members = type.FindMembers(
                    MemberTypes.Field,
                    BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public,
                    Type.FilterNameIgnoreCase,
                    name);
                if (members.Length != 0)
                    return ((FieldInfo) members[0]).GetValue(null);
            }
            return null;
        }

        private static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target)
                return true;
            if (!target.IsValueType)
                return target.IsAssignableFrom(source);
            Type nonNullableType1 = GetNonNullableType(source);
            Type nonNullableType2 = GetNonNullableType(target);
            if (nonNullableType1 != source && nonNullableType2 == target)
                return false;
            TypeCode typeCode1 = nonNullableType1.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType1);
            TypeCode typeCode2 = nonNullableType2.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType2);
            switch (typeCode1)
            {
                case TypeCode.SByte:
                    switch (typeCode2)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch (typeCode2)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCode2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCode2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCode2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCode2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default:
                    if (nonNullableType1 == nonNullableType2)
                        return true;
                    else
                        break;
            }
            return false;
        }

        private static bool IsBetterThan(Expression[] args, MethodData left, MethodData right)
        {
            bool flag = false;
            for (int index = 0; index < args.Length; ++index)
            {
                int num = CompareConversions(
                    args[index].Type, left.Parameters[index].ParameterType, right.Parameters[index].ParameterType);
                if (num < 0)
                    return false;
                if (num > 0)
                    flag = true;
            }
            return flag;
        }

        private static int CompareConversions(Type s, Type left, Type right)
        {
            if (left == right)
                return 0;
            if (s == left)
                return 1;
            if (s == right)
                return -1;
            bool compatibleLtr = IsCompatibleWith(left, right);
            bool compatibleRtl = IsCompatibleWith(right, left);
            if (compatibleLtr && !compatibleRtl)
                return 1;
            if (compatibleRtl && !compatibleLtr)
                return -1;
            if (IsSignedIntegralType(left) && IsUnsignedIntegralType(right))
                return 1;
            if (IsSignedIntegralType(right) && IsUnsignedIntegralType(left))
                return -1;
            else
                return 0;
        }

        private Expression GenerateEqual(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        private Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        private Expression GenerateGreaterThan(Expression left, Expression right)
        {
            if (left.Type == typeof (string))
                return Expression.GreaterThan(
                    this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            else
                return Expression.GreaterThan(left, right);
        }

        private Expression GenerateGreaterThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof (string))
                return Expression.GreaterThanOrEqual(
                    this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            else
                return Expression.GreaterThanOrEqual(left, right);
        }

        private Expression GenerateLessThan(Expression left, Expression right)
        {
            if (left.Type == typeof (string))
                return Expression.LessThan(
                    this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            else
                return Expression.LessThan(left, right);
        }

        private Expression GenerateLessThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof (string))
                return Expression.LessThanOrEqual(
                    this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            else
                return Expression.LessThanOrEqual(left, right);
        }

        private Expression GenerateAdd(Expression left, Expression right)
        {
            if (left.Type == typeof (string) && right.Type == typeof (string))
                return this.GenerateStaticMethodCall("Concat", left, right);
            else
                return Expression.Add(left, right);
        }

        private Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        private Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(
                null,
                typeof (string).GetMethod(
                    "Concat",
                    new Type[2]
                        {
                            typeof (object),
                            typeof (object)
                        }),
                new Expression[2]
                    {
                        left,
                        right
                    });
        }

        private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
        {
            return left.Type.GetMethod(
                methodName,
                new Type[2]
                    {
                        left.Type,
                        right.Type
                    });
        }

        private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            return Expression.Call(
                null,
                this.GetStaticMethod(methodName, left, right),
                new Expression[2]
                    {
                        left,
                        right
                    });
        }

        private void SetTextPos(int pos)
        {
            this._currentTextPosition = pos;
            this._currentChar = this._currentTextPosition < this._textLen ? this._expressionText[this._currentTextPosition] : char.MinValue;
        }

        private void NextChar()
        {
            if (this._currentTextPosition < this._textLen)
                ++this._currentTextPosition;
            this._currentChar = this._currentTextPosition < this._textLen ? this._expressionText[this._currentTextPosition] : char.MinValue;
        }

        private void NextToken()
        {
            while (char.IsWhiteSpace(this._currentChar))
                this.NextChar();
            int startIndex = this._currentTextPosition;
            TokenId tokenId;
            switch (this._currentChar)
            {
                case '!':
                    this.NextChar();
                    if (this._currentChar == 61)
                    {
                        this.NextChar();
                        tokenId = TokenId.ExclamationEqual;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.Exclamation;
                        break;
                    }
                case '"':
                case '\'':
                    char ch = this._currentChar;
                    do
                    {
                        this.NextChar();
                        while (this._currentTextPosition < this._textLen && this._currentChar != ch)
                            this.NextChar();
                        if (this._currentTextPosition == this._textLen)
                            throw ParseError(this._currentTextPosition, "Unterminated string literal", new object[0]);
                        this.NextChar();
                    } while (this._currentChar == ch);
                    tokenId = TokenId.StringLiteral;
                    break;
                case '%':
                    this.NextChar();
                    tokenId = TokenId.Percent;
                    break;
                case '&':
                    this.NextChar();
                    if (this._currentChar == 38)
                    {
                        this.NextChar();
                        tokenId = TokenId.DoubleAmphersand;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.Amphersand;
                        break;
                    }
                case '(':
                    this.NextChar();
                    tokenId = TokenId.OpenParen;
                    break;
                case ')':
                    this.NextChar();
                    tokenId = TokenId.CloseParen;
                    break;
                case '*':
                    this.NextChar();
                    tokenId = TokenId.Asterisk;
                    break;
                case '+':
                    this.NextChar();
                    tokenId = TokenId.Plus;
                    break;
                case ',':
                    this.NextChar();
                    tokenId = TokenId.Comma;
                    break;
                case '-':
                    this.NextChar();
                    tokenId = TokenId.Minus;
                    break;
                case '.':
                    this.NextChar();
                    tokenId = TokenId.Dot;
                    break;
                case '/':
                    this.NextChar();
                    tokenId = TokenId.Slash;
                    break;
                case ':':
                    this.NextChar();
                    tokenId = TokenId.Colon;
                    break;
                case '<':
                    this.NextChar();
                    if (this._currentChar == 61)
                    {
                        this.NextChar();
                        tokenId = TokenId.LessThanEqual;
                        break;
                    }
                    else if (this._currentChar == 62)
                    {
                        this.NextChar();
                        tokenId = TokenId.LessGreater;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.LessThan;
                        break;
                    }
                case '=':
                    this.NextChar();
                    if (this._currentChar == 61)
                    {
                        this.NextChar();
                        tokenId = TokenId.DoubleEqual;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.Equal;
                        break;
                    }
                case '>':
                    this.NextChar();
                    if (this._currentChar == 61)
                    {
                        this.NextChar();
                        tokenId = TokenId.GreaterThanEqual;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.GreaterThan;
                        break;
                    }
                case '?':
                    this.NextChar();
                    tokenId = TokenId.Question;
                    break;
                case '[':
                    this.NextChar();
                    tokenId = TokenId.OpenBracket;
                    break;
                case ']':
                    this.NextChar();
                    tokenId = TokenId.CloseBracket;
                    break;
                case '|':
                    this.NextChar();
                    if (this._currentChar == 124)
                    {
                        this.NextChar();
                        tokenId = TokenId.DoubleBar;
                        break;
                    }
                    else
                    {
                        tokenId = TokenId.Bar;
                        break;
                    }
                default:
                    if (char.IsLetter(this._currentChar) || this._currentChar == 64 || this._currentChar == 95)
                    {
                        do
                        {
                            this.NextChar();
                        } while (char.IsLetterOrDigit(this._currentChar) || this._currentChar == 95);
                        tokenId = TokenId.Identifier;
                        break;
                    }
                    else if (char.IsDigit(this._currentChar))
                    {
                        tokenId = TokenId.IntegerLiteral;
                        do
                        {
                            this.NextChar();
                        } while (char.IsDigit(this._currentChar));
                        if (this._currentChar == 46)
                        {
                            tokenId = TokenId.RealLiteral;
                            this.NextChar();
                            this.ValidateDigit();
                            do
                            {
                                this.NextChar();
                            } while (char.IsDigit(this._currentChar));
                        }
                        if (this._currentChar == 69 || this._currentChar == 101)
                        {
                            tokenId = TokenId.RealLiteral;
                            this.NextChar();
                            if (this._currentChar == 43 || this._currentChar == 45)
                                this.NextChar();
                            this.ValidateDigit();
                            do
                            {
                                this.NextChar();
                            } while (char.IsDigit(this._currentChar));
                        }
                        if (this._currentChar == 70 || this._currentChar == 102)
                        {
                            this.NextChar();
                            break;
                        }
                        else
                            break;
                    }
                    else if (this._currentTextPosition == this._textLen)
                    {
                        tokenId = TokenId.End;
                        break;
                    }
                    else
                        throw ParseError(
                            this._currentTextPosition,
                            "Syntax error '{0}'",
                            new object[1]
                                {
                                    this._currentChar
                                });
            }
            this._token.Id = tokenId;
            this._token.Text = this._expressionText.Substring(startIndex, this._currentTextPosition - startIndex);
            this._token.Position = startIndex;
        }

        private bool TokenIdentifierIs(string id)
        {
            return this._token.Id == TokenId.Identifier && string.Equals(id, this._token.Text, StringComparison.OrdinalIgnoreCase);
        }

        private string GetIdentifier()
        {
            this.ValidateToken(TokenId.Identifier, "Identifier expected");
            string str = this._token.Text;
            if (str.Length > 1 && str[0] == 64)
                str = str.Substring(1);
            return str;
        }

        private void ValidateDigit()
        {
            if (!char.IsDigit(this._currentChar))
                throw ParseError(this._currentTextPosition, "Digit expected", new object[0]);
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (this._token.Id != t)
                throw this.ParseError(errorMessage, new object[0]);
        }

        private void ValidateToken(TokenId t)
        {
            if (this._token.Id != t)
                throw this.ParseError("Syntax error", new object[0]);
        }

        private Exception ParseError(string format, params object[] args)
        {
            return ParseError(this._token.Position, format, args);
        }

        private static Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        private static Dictionary<string, object> CreateKeywords()
        {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"true", TrueLiteral},
                    {"false", FalseLiteral},
                    {"null", NullLiteral},
                    {KeywordIt, KeywordIt},
                    {KeywordIif, KeywordIif},
                    {KeywordNew, KeywordNew}
                };
            foreach (var type in PredefinedTypes)
                dictionary.Add(type.Name, type);
            return dictionary;
        }

        #region Nested type: IAddSignatures

        private interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);

            void F(TimeSpan x, TimeSpan y);

            void F(DateTime? x, TimeSpan? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        #endregion

        #region Nested type: IArithmeticSignatures

        private interface IArithmeticSignatures
        {
            void F(int x, int y);

            void F(uint x, uint y);

            void F(long x, long y);

            void F(ulong x, ulong y);

            void F(float x, float y);

            void F(double x, double y);

            void F(Decimal x, Decimal y);

            void F(int? x, int? y);

            void F(uint? x, uint? y);

            void F(long? x, long? y);

            void F(ulong? x, ulong? y);

            void F(float? x, float? y);

            void F(double? x, double? y);

            void F(Decimal? x, Decimal? y);
        }

        #endregion

        #region Nested type: IEnumerableSignatures

        private interface IEnumerableSignatures
        {
            void Where(bool predicate);

            void Any();

            void Any(bool predicate);

            void All(bool predicate);

            void Count();

            void Count(bool predicate);

            void Min(object selector);

            void Max(object selector);

            void Sum(int selector);

            void Sum(int? selector);

            void Sum(long selector);

            void Sum(long? selector);

            void Sum(float selector);

            void Sum(float? selector);

            void Sum(double selector);

            void Sum(double? selector);

            void Sum(Decimal selector);

            void Sum(Decimal? selector);

            void Average(int selector);

            void Average(int? selector);

            void Average(long selector);

            void Average(long? selector);

            void Average(float selector);

            void Average(float? selector);

            void Average(double selector);

            void Average(double? selector);

            void Average(Decimal selector);

            void Average(Decimal? selector);
        }

        #endregion

        #region Nested type: IEqualitySignatures

        private interface IEqualitySignatures : IRelationalSignatures, IArithmeticSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);
        }

        #endregion

        #region Nested type: ILogicalSignatures

        private interface ILogicalSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);
        }

        #endregion

        #region Nested type: INegationSignatures

        private interface INegationSignatures
        {
            void F(int x);

            void F(long x);

            void F(float x);

            void F(double x);

            void F(Decimal x);

            void F(int? x);

            void F(long? x);

            void F(float? x);

            void F(double? x);

            void F(Decimal? x);
        }

        #endregion

        #region Nested type: INotSignatures

        private interface INotSignatures
        {
            void F(bool x);

            void F(bool? x);
        }

        #endregion

        #region Nested type: IRelationalSignatures

        private interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);

            void F(char x, char y);

            void F(DateTime x, DateTime y);

            void F(TimeSpan x, TimeSpan y);

            void F(char? x, char? y);

            void F(DateTime? x, DateTime? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        #endregion

        #region Nested type: ISubtractSignatures

        private interface ISubtractSignatures : IAddSignatures, IArithmeticSignatures
        {
            void F(DateTime x, DateTime y);

            void F(DateTime? x, DateTime? y);
        }

        #endregion

        #region Nested type: MethodData

        private class MethodData
        {
            public Expression[] Args;
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
        }

        #endregion

        #region Nested type: Token

        private struct Token
        {
            public TokenId Id;
            public int Position;
            public string Text;
        }

        #endregion

        #region Nested type: TokenId

        private enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Amphersand,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar,
        }

        #endregion
    }
}