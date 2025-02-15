using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.Macros
{
    public abstract class ExpressionEvalBase
    {
        public enum EvalMode
        {
            Evaluate,
            ParseOnly
        }

        private EvalMode _evalMode;
        private ExpressionTokenizer _tokenizer;

        public object Evaluate(ExpressionTokenizer tokenizer)
        {
            _evalMode = EvalMode.Evaluate;
            _tokenizer = tokenizer;
            return ParseExpression();
        }

        public object Evaluate(string s)
        {
            _tokenizer = new ExpressionTokenizer();
            _evalMode = EvalMode.Evaluate;
            _tokenizer.InitTokenizer(s);
            var expression = ParseExpression();
            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EOF)
                return expression;
            throw BuildParseError("Unexpected token at the end of expression", _tokenizer.CurrentPosition);
        }

        public void CheckSyntax(string s)
        {
            _tokenizer = new ExpressionTokenizer();
            _evalMode = EvalMode.ParseOnly;
            _tokenizer.InitTokenizer(s);
            ParseExpression();
            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                throw BuildParseError("Unexpected token at the end of expression", _tokenizer.CurrentPosition);
        }

        private bool SyntaxCheckOnly()
        {
            return _evalMode == EvalMode.ParseOnly;
        }

        private object ParseExpression()
        {
            return ParseBooleanOr();
        }

        private object ParseBooleanOr()
        {
            var currentPosition1 = _tokenizer.CurrentPosition;
            var source = ParseBooleanAnd();
            var evalMode = _evalMode;
            try
            {
                while (_tokenizer.IsKeyword("or"))
                {
                    var flag1 = true;
                    if (!SyntaxCheckOnly())
                    {
                        flag1 = (bool)SafeConvert(typeof(bool), source, "the left hand side of the 'or' operator",
                            currentPosition1, _tokenizer.CurrentPosition);
                        if (flag1)
                            _evalMode = EvalMode.ParseOnly;
                    }

                    _tokenizer.GetNextToken();
                    var currentPosition2 = _tokenizer.CurrentPosition;
                    var booleanAnd = ParseBooleanAnd();
                    var currentPosition3 = _tokenizer.CurrentPosition;
                    if (!SyntaxCheckOnly())
                    {
                        var flag2 = (bool)SafeConvert(typeof(bool), booleanAnd,
                            "the right hand side of the 'or' operator", currentPosition2, currentPosition3);
                        source = flag1 | flag2;
                    }
                }

                return source;
            }
            finally
            {
                _evalMode = evalMode;
            }
        }

        private object ParseBooleanAnd()
        {
            var currentPosition1 = _tokenizer.CurrentPosition;
            var source = ParseRelationalExpression();
            var evalMode = _evalMode;
            try
            {
                while (_tokenizer.IsKeyword("and"))
                {
                    var flag1 = true;
                    if (!SyntaxCheckOnly())
                    {
                        flag1 = (bool)SafeConvert(typeof(bool), source, "the left hand side of the 'and' operator",
                            currentPosition1, _tokenizer.CurrentPosition);
                        if (!flag1)
                            _evalMode = EvalMode.ParseOnly;
                    }

                    _tokenizer.GetNextToken();
                    var currentPosition2 = _tokenizer.CurrentPosition;
                    var relationalExpression = ParseRelationalExpression();
                    var currentPosition3 = _tokenizer.CurrentPosition;
                    if (!SyntaxCheckOnly())
                    {
                        var flag2 = (bool)SafeConvert(typeof(bool), relationalExpression,
                            "the right hand side of the 'and' operator", currentPosition2, currentPosition3);
                        source = flag1 & flag2;
                    }
                }

                return source;
            }
            finally
            {
                _evalMode = evalMode;
            }
        }

        private object ParseRelationalExpression()
        {
            var currentPosition1 = _tokenizer.CurrentPosition;
            var addSubtract1 = ParseAddSubtract();
            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EQ ||
                _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.NE ||
                _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LT ||
                _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GT ||
                _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LE ||
                _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GE)
            {
                var currentToken = _tokenizer.CurrentToken;
                _tokenizer.GetNextToken();
                var addSubtract2 = ParseAddSubtract();
                var currentPosition2 = _tokenizer.CurrentPosition;
                if (SyntaxCheckOnly())
                    return null;
                switch (currentToken)
                {
                    case ExpressionTokenizer.TokenType.EQ:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return addSubtract1.Equals(addSubtract2);
                            case bool _ when addSubtract2 is bool:
                                return addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is int:
                                return addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is long:
                                return Convert.ToInt64(addSubtract1).Equals(addSubtract2);
                            case int _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case long _ when addSubtract2 is long:
                                return addSubtract1.Equals(addSubtract2);
                            case long _ when addSubtract2 is int:
                                return addSubtract1.Equals(Convert.ToInt64(addSubtract2));
                            case long _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case double _ when addSubtract2 is double:
                                return addSubtract1.Equals(addSubtract2);
                            case double _ when addSubtract2 is int:
                                return addSubtract1.Equals(Convert.ToDouble(addSubtract2));
                            case double _ when addSubtract2 is long:
                                return addSubtract1.Equals(Convert.ToDouble(addSubtract2));
                            case DateTime _ when addSubtract2 is DateTime:
                                return addSubtract1.Equals(addSubtract2);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return addSubtract1.Equals(addSubtract2);
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return addSubtract1.Equals(addSubtract2);
                                if (addSubtract1.GetType().IsEnum)
                                    return addSubtract2 is string
                                        ? addSubtract1.Equals(Enum.Parse(addSubtract1.GetType(), (string)addSubtract2,
                                            false))
                                        : (object)addSubtract1.Equals(Enum.ToObject(addSubtract1.GetType(),
                                            addSubtract2));
                                if (!addSubtract2.GetType().IsEnum)
                                    throw BuildParseError(
                                        string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1038"),
                                            GetSimpleTypeName(addSubtract1.GetType()),
                                            GetSimpleTypeName(addSubtract2.GetType())), currentPosition1,
                                        currentPosition2);
                                return addSubtract1 is string
                                    ? addSubtract2.Equals(Enum.Parse(addSubtract2.GetType(), (string)addSubtract1,
                                        false))
                                    : (object)addSubtract2.Equals(Enum.ToObject(addSubtract2.GetType(), addSubtract1));
                        }
                    case ExpressionTokenizer.TokenType.NE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return !addSubtract1.Equals(addSubtract2);
                            case bool _ when addSubtract2 is bool:
                                return !addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is int:
                                return !addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is long:
                                return !Convert.ToInt64(addSubtract1).Equals(addSubtract2);
                            case int _ when addSubtract2 is double:
                                return !Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case long _ when addSubtract2 is long:
                                return !addSubtract1.Equals(addSubtract2);
                            case long _ when addSubtract2 is int:
                                return !addSubtract1.Equals(Convert.ToInt64(addSubtract2));
                            case long _ when addSubtract2 is double:
                                return !Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case double _ when addSubtract2 is double:
                                return !addSubtract1.Equals(addSubtract2);
                            case double _ when addSubtract2 is int:
                                return !addSubtract1.Equals(Convert.ToDouble(addSubtract2));
                            case double _ when addSubtract2 is long:
                                return !addSubtract1.Equals(Convert.ToDouble(addSubtract2));
                            case DateTime _ when addSubtract2 is DateTime:
                                return !addSubtract1.Equals(addSubtract2);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return !addSubtract1.Equals(addSubtract2);
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return !addSubtract1.Equals(addSubtract2);
                                if (addSubtract1.GetType().IsEnum)
                                    return addSubtract2 is string
                                        ? !addSubtract1.Equals(Enum.Parse(addSubtract1.GetType(), (string)addSubtract2,
                                            false))
                                        : (object)!addSubtract1.Equals(Enum.ToObject(addSubtract1.GetType(),
                                            addSubtract2));
                                if (!addSubtract2.GetType().IsEnum)
                                    throw BuildParseError(
                                        string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1042"),
                                            GetSimpleTypeName(addSubtract1.GetType()),
                                            GetSimpleTypeName(addSubtract2.GetType())), currentPosition1,
                                        currentPosition2);
                                return addSubtract1 is string
                                    ? !addSubtract2.Equals(Enum.Parse(addSubtract2.GetType(), (string)addSubtract1,
                                        false))
                                    : (object)!addSubtract2.Equals(Enum.ToObject(addSubtract2.GetType(), addSubtract1));
                        }
                    case ExpressionTokenizer.TokenType.LT:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return string.Compare((string)addSubtract1, (string)addSubtract2, false,
                                    CultureInfo.InvariantCulture) < 0;
                            case bool _ when addSubtract2 is bool:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            case int _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            case int _ when addSubtract2 is long:
                                return Convert.ToInt64(addSubtract1).CompareTo(addSubtract2) < 0;
                            case int _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) < 0;
                            case long _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            case long _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToInt64(addSubtract2)) < 0;
                            case long _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) < 0;
                            case double _ when addSubtract2 is double:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            case double _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) < 0;
                            case double _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) < 0;
                            case DateTime _ when addSubtract2 is DateTime:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return ((IComparable)addSubtract1).CompareTo(addSubtract2) < 0;
                                throw BuildParseError(
                                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1051"),
                                        GetSimpleTypeName(addSubtract1.GetType()),
                                        GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.GT:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return string.Compare((string)addSubtract1, (string)addSubtract2, false,
                                    CultureInfo.InvariantCulture) > 0;
                            case bool _ when addSubtract2 is bool:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            case int _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            case int _ when addSubtract2 is long:
                                return Convert.ToInt64(addSubtract1).CompareTo(addSubtract2) > 0;
                            case int _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) > 0;
                            case long _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            case long _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToInt64(addSubtract2)) > 0;
                            case long _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) > 0;
                            case double _ when addSubtract2 is double:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            case double _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) > 0;
                            case double _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) > 0;
                            case DateTime _ when addSubtract2 is DateTime:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return ((IComparable)addSubtract1).CompareTo(addSubtract2) > 0;
                                throw BuildParseError(
                                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1037"),
                                        GetSimpleTypeName(addSubtract1.GetType()),
                                        GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.LE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return string.Compare((string)addSubtract1, (string)addSubtract2, false,
                                    CultureInfo.InvariantCulture) <= 0;
                            case bool _ when addSubtract2 is bool:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            case int _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            case int _ when addSubtract2 is long:
                                return Convert.ToInt64(addSubtract1).CompareTo(addSubtract2) <= 0;
                            case int _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) <= 0;
                            case long _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            case long _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToInt64(addSubtract2)) <= 0;
                            case long _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) <= 0;
                            case double _ when addSubtract2 is double:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            case double _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) <= 0;
                            case double _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) <= 0;
                            case DateTime _ when addSubtract2 is DateTime:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return ((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0;
                                throw BuildParseError(
                                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1049"),
                                        GetSimpleTypeName(addSubtract1.GetType()),
                                        GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.GE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return string.Compare((string)addSubtract1, (string)addSubtract2, false,
                                    CultureInfo.InvariantCulture) >= 0;
                            case bool _ when addSubtract2 is bool:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            case int _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            case int _ when addSubtract2 is long:
                                return Convert.ToInt64(addSubtract1).CompareTo(addSubtract2) >= 0;
                            case int _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) >= 0;
                            case long _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            case long _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToInt64(addSubtract2)) >= 0;
                            case long _ when addSubtract2 is double:
                                return Convert.ToDouble(addSubtract1).CompareTo(addSubtract2) >= 0;
                            case double _ when addSubtract2 is double:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            case double _ when addSubtract2 is int:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) >= 0;
                            case double _ when addSubtract2 is long:
                                return ((IComparable)addSubtract1).CompareTo(Convert.ToDouble(addSubtract2)) >= 0;
                            case DateTime _ when addSubtract2 is DateTime:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                            default:
                                if ((object)(addSubtract1 as Version) != null &&
                                    (object)(addSubtract2 as Version) != null)
                                    return ((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0;
                                throw BuildParseError(
                                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1050"),
                                        GetSimpleTypeName(addSubtract1.GetType()),
                                        GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                }
            }

            return addSubtract1;
        }

        private object ParseAddSubtract()
        {
            var currentPosition1 = _tokenizer.CurrentPosition;
            var addSubtract = ParseMulDiv();
            object mulDiv1;
            ExpressionTokenizer.Position currentPosition2;
            while (true)
            {
                do
                {
                    while (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Plus)
                    {
                        if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Minus)
                            return addSubtract;
                        _tokenizer.GetNextToken();
                        var mulDiv2 = ParseMulDiv();
                        var currentPosition3 = _tokenizer.CurrentPosition;
                        if (!SyntaxCheckOnly())
                            switch (addSubtract)
                            {
                                case int _ when mulDiv2 is int num1:
                                    addSubtract = (int)addSubtract - num1;
                                    continue;
                                case int _ when mulDiv2 is long num2:
                                    addSubtract = (int)addSubtract - num2;
                                    continue;
                                case int _ when mulDiv2 is double num3:
                                    addSubtract = (int)addSubtract - num3;
                                    continue;
                                case long _ when mulDiv2 is long num4:
                                    addSubtract = (long)addSubtract - num4;
                                    continue;
                                case long _ when mulDiv2 is int num5:
                                    addSubtract = (long)addSubtract - num5;
                                    continue;
                                case long _ when mulDiv2 is double num6:
                                    addSubtract = (long)addSubtract - num6;
                                    continue;
                                case double _ when mulDiv2 is double num7:
                                    addSubtract = (double)addSubtract - num7;
                                    continue;
                                case double _ when mulDiv2 is int num8:
                                    addSubtract = (double)addSubtract - num8;
                                    continue;
                                case double _ when mulDiv2 is long num9:
                                    addSubtract = (double)addSubtract - num9;
                                    continue;
                                case DateTime _ when mulDiv2 is DateTime dateTime:
                                    addSubtract = (DateTime)addSubtract - dateTime;
                                    continue;
                                case DateTime _ when mulDiv2 is TimeSpan timeSpan1:
                                    addSubtract = (DateTime)addSubtract - timeSpan1;
                                    continue;
                                case TimeSpan _ when mulDiv2 is TimeSpan timeSpan2:
                                    addSubtract = (TimeSpan)addSubtract - timeSpan2;
                                    continue;
                                default:
                                    throw BuildParseError(
                                        string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1048"),
                                            GetSimpleTypeName(addSubtract.GetType()),
                                            GetSimpleTypeName(mulDiv2.GetType())), currentPosition1, currentPosition3);
                            }
                    }

                    _tokenizer.GetNextToken();
                    mulDiv1 = ParseMulDiv();
                    currentPosition2 = _tokenizer.CurrentPosition;
                } while (SyntaxCheckOnly());

                switch (addSubtract)
                {
                    case string _ when mulDiv1 is string:
                        addSubtract = (string)addSubtract + (string)mulDiv1;
                        continue;
                    case int _ when mulDiv1 is int num10:
                        addSubtract = (int)addSubtract + num10;
                        continue;
                    case int _ when mulDiv1 is long num11:
                        addSubtract = (int)addSubtract + num11;
                        continue;
                    case int _ when mulDiv1 is double num12:
                        addSubtract = (int)addSubtract + num12;
                        continue;
                    case long _ when mulDiv1 is long num13:
                        addSubtract = (long)addSubtract + num13;
                        continue;
                    case long _ when mulDiv1 is int num14:
                        addSubtract = (long)addSubtract + num14;
                        continue;
                    case long _ when mulDiv1 is double num15:
                        addSubtract = (long)addSubtract + num15;
                        continue;
                    case double _ when mulDiv1 is double num16:
                        addSubtract = (double)addSubtract + num16;
                        continue;
                    case double _ when mulDiv1 is int num17:
                        addSubtract = (double)addSubtract + num17;
                        continue;
                    case double _ when mulDiv1 is long num18:
                        addSubtract = (double)addSubtract + num18;
                        continue;
                    case DateTime _ when mulDiv1 is TimeSpan timeSpan3:
                        addSubtract = (DateTime)addSubtract + timeSpan3;
                        continue;
                    case TimeSpan _ when mulDiv1 is TimeSpan timeSpan4:
                        addSubtract = (TimeSpan)addSubtract + timeSpan4;
                        continue;
                    default:
                        goto label_16;
                }
            }

            label_16:
            throw BuildParseError(
                string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1041"),
                    GetSimpleTypeName(addSubtract.GetType()), GetSimpleTypeName(mulDiv1.GetType())), currentPosition1,
                currentPosition2);
        }

        private object ParseMulDiv()
        {
            var currentPosition1 = _tokenizer.CurrentPosition;
            var mulDiv = ParseValue();
            object obj1;
            ExpressionTokenizer.Position currentPosition2;
            while (true)
            {
                do
                {
                    while (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mul)
                        if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Div)
                        {
                            _tokenizer.GetNextToken();
                            var currentPosition3 = _tokenizer.CurrentPosition;
                            var obj2 = ParseValue();
                            var currentPosition4 = _tokenizer.CurrentPosition;
                            if (!SyntaxCheckOnly())
                                switch (mulDiv)
                                {
                                    case int _ when obj2 is int num1:
                                        if (num1 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (int)mulDiv / (int)obj2;
                                        continue;
                                    case int _ when obj2 is long num2:
                                        if (num2 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (int)mulDiv / (long)obj2;
                                        continue;
                                    case int _ when obj2 is double num3:
                                        if (num3 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (int)mulDiv / (double)obj2;
                                        continue;
                                    case long _ when obj2 is long num4:
                                        if (num4 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (long)mulDiv / (long)obj2;
                                        continue;
                                    case long _ when obj2 is int num5:
                                        if (num5 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (long)mulDiv / (int)obj2;
                                        continue;
                                    case long _ when obj2 is double num6:
                                        if (num6 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (long)mulDiv / (double)obj2;
                                        continue;
                                    case double _ when obj2 is double num7:
                                        if (num7 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (double)mulDiv / (double)obj2;
                                        continue;
                                    case double _ when obj2 is int num8:
                                        if (num8 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (double)mulDiv / (int)obj2;
                                        continue;
                                    case double _ when obj2 is long num9:
                                        if (num9 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition3,
                                                currentPosition4);
                                        mulDiv = (double)mulDiv / (long)obj2;
                                        continue;
                                    default:
                                        throw BuildParseError(
                                            string.Format(CultureInfo.InvariantCulture,
                                                ResourceUtils.GetString("NA1039"), GetSimpleTypeName(mulDiv.GetType()),
                                                GetSimpleTypeName(obj2.GetType())), currentPosition1, currentPosition4);
                                }
                        }
                        else
                        {
                            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mod)
                                return mulDiv;
                            _tokenizer.GetNextToken();
                            var currentPosition5 = _tokenizer.CurrentPosition;
                            var obj3 = ParseValue();
                            var currentPosition6 = _tokenizer.CurrentPosition;
                            if (!SyntaxCheckOnly())
                                switch (mulDiv)
                                {
                                    case int _ when obj3 is int num10:
                                        if (num10 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (int)mulDiv % (int)obj3;
                                        continue;
                                    case int _ when obj3 is long num11:
                                        if (num11 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (int)mulDiv % (long)obj3;
                                        continue;
                                    case int _ when obj3 is double num12:
                                        if (num12 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (int)mulDiv % (double)obj3;
                                        continue;
                                    case long _ when obj3 is long num13:
                                        if (num13 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (long)mulDiv % (long)obj3;
                                        continue;
                                    case long _ when obj3 is int num14:
                                        if (num14 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (long)mulDiv % (int)obj3;
                                        continue;
                                    case long _ when obj3 is double num15:
                                        if (num15 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (long)mulDiv % (double)obj3;
                                        continue;
                                    case double _ when obj3 is double num16:
                                        if (num16 == 0.0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (double)mulDiv % (double)obj3;
                                        continue;
                                    case double _ when obj3 is int num17:
                                        if (num17 == 0)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (double)mulDiv % (int)obj3;
                                        continue;
                                    case double _ when obj3 is long num18:
                                        if (num18 == 0L)
                                            throw BuildParseError(
                                                string.Format(CultureInfo.InvariantCulture,
                                                    ResourceUtils.GetString("NA1043")), currentPosition5,
                                                currentPosition6);
                                        mulDiv = (double)mulDiv % (long)obj3;
                                        continue;
                                    default:
                                        throw BuildParseError(
                                            string.Format(CultureInfo.InvariantCulture,
                                                ResourceUtils.GetString("NA1047"), GetSimpleTypeName(mulDiv.GetType()),
                                                GetSimpleTypeName(obj3.GetType())), currentPosition1, currentPosition6);
                                }
                        }

                    _tokenizer.GetNextToken();
                    obj1 = ParseValue();
                    currentPosition2 = _tokenizer.CurrentPosition;
                } while (SyntaxCheckOnly());

                switch (mulDiv)
                {
                    case int _ when obj1 is int num19:
                        mulDiv = (int)mulDiv * num19;
                        continue;
                    case int _ when obj1 is long num20:
                        mulDiv = (int)mulDiv * num20;
                        continue;
                    case int _ when obj1 is double num21:
                        mulDiv = (int)mulDiv * num21;
                        continue;
                    case long _ when obj1 is long num22:
                        mulDiv = (long)mulDiv * num22;
                        continue;
                    case long _ when obj1 is int num23:
                        mulDiv = (long)mulDiv * num23;
                        continue;
                    case long _ when obj1 is double num24:
                        mulDiv = (long)mulDiv * num24;
                        continue;
                    case double _ when obj1 is double num25:
                        mulDiv = (double)mulDiv * num25;
                        continue;
                    case double _ when obj1 is int num26:
                        mulDiv = (double)mulDiv * num26;
                        continue;
                    case double _ when obj1 is long num27:
                        mulDiv = (double)mulDiv * num27;
                        continue;
                    default:
                        goto label_13;
                }
            }

            label_13:
            throw BuildParseError(
                string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1036"),
                    GetSimpleTypeName(mulDiv.GetType()), GetSimpleTypeName(obj1.GetType())), currentPosition1,
                currentPosition2);
        }

        private object ParseConditional()
        {
            _tokenizer.GetNextToken();
            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
                throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
            _tokenizer.GetNextToken();
            var currentPosition1 = _tokenizer.CurrentPosition;
            var expression1 = ParseExpression();
            var currentPosition2 = _tokenizer.CurrentPosition;
            var flag = false;
            if (!SyntaxCheckOnly())
                flag = (bool)SafeConvert(typeof(bool), expression1, "the conditional expression", currentPosition1,
                    currentPosition2);
            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
            _tokenizer.GetNextToken();
            var evalMode = _evalMode;
            try
            {
                _evalMode = flag ? evalMode : EvalMode.ParseOnly;
                var expression2 = ParseExpression();
                _evalMode = evalMode;
                if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                    throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
                _tokenizer.GetNextToken();
                _evalMode = !flag ? evalMode : EvalMode.ParseOnly;
                var expression3 = ParseExpression();
                _evalMode = evalMode;
                if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                    throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
                _tokenizer.GetNextToken();
                return flag ? expression2 : expression3;
            }
            finally
            {
                _evalMode = evalMode;
            }
        }

        private object ParseValue()
        {
            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.String)
            {
                var tokenText = _tokenizer.TokenText;
                _tokenizer.GetNextToken();
                return tokenText;
            }

            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
            {
                var tokenText = _tokenizer.TokenText;
                var currentPosition = _tokenizer.CurrentPosition;
                _tokenizer.GetNextToken();
                var p1 = new ExpressionTokenizer.Position(_tokenizer.CurrentPosition.CharIndex - 1);
                if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot)
                {
                    var str = tokenText + ".";
                    _tokenizer.GetNextToken();
                    if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Number)
                        throw BuildParseError("Fractional part expected.", _tokenizer.CurrentPosition);
                    var s = str + _tokenizer.TokenText;
                    _tokenizer.GetNextToken();
                    p1 = _tokenizer.CurrentPosition;
                    try
                    {
                        return double.Parse(s, CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException ex)
                    {
                        throw BuildParseError("Value was either too large or too small for type 'double'.",
                            currentPosition, p1);
                    }
                }

                try
                {
                    return int.Parse(tokenText, CultureInfo.InvariantCulture);
                }
                catch (OverflowException ex1)
                {
                    try
                    {
                        return long.Parse(tokenText, CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException ex2)
                    {
                        throw BuildParseError("Value was either too large or too small for type 'long'.",
                            currentPosition, p1);
                    }
                }
            }

            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus)
            {
                _tokenizer.GetNextToken();
                var currentPosition1 = _tokenizer.CurrentPosition;
                var obj = ParseValue();
                var currentPosition2 = _tokenizer.CurrentPosition;
                if (SyntaxCheckOnly())
                    return null;
                switch (obj)
                {
                    case int num1:
                        return -num1;
                    case long num2:
                        return -num2;
                    case double num3:
                        return -num3;
                    default:
                        throw BuildParseError(
                            string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1040"),
                                GetSimpleTypeName(obj.GetType())), currentPosition1, currentPosition2);
                }
            }

            if (_tokenizer.IsKeyword("not"))
            {
                _tokenizer.GetNextToken();
                var currentPosition3 = _tokenizer.CurrentPosition;
                var source = ParseValue();
                var currentPosition4 = _tokenizer.CurrentPosition;
                return !SyntaxCheckOnly()
                    ? !(bool)SafeConvert(typeof(bool), source, "the argument of 'not' operator", currentPosition3,
                        currentPosition4)
                    : (object)null;
            }

            if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftParen)
            {
                _tokenizer.GetNextToken();
                var expression = ParseExpression();
                if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                    throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
                _tokenizer.GetNextToken();
                return expression;
            }

            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
                return UnexpectedToken();
            var currentPosition5 = _tokenizer.CurrentPosition;
            var str1 = _tokenizer.TokenText;
            switch (str1)
            {
                case "if":
                    return ParseConditional();
                case "true":
                    _tokenizer.GetNextToken();
                    return true;
                case "false":
                    _tokenizer.GetNextToken();
                    return false;
                default:
                    _tokenizer.IgnoreWhitespace = false;
                    _tokenizer.GetNextToken();
                    var arrayList = new ArrayList();
                    var flag = false;
                    if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.DoubleColon)
                    {
                        flag = true;
                        var str2 = str1 + "::";
                        _tokenizer.GetNextToken();
                        if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
                            throw BuildParseError("Function name expected.", currentPosition5,
                                _tokenizer.CurrentPosition);
                        str1 = str2 + _tokenizer.TokenText;
                        _tokenizer.GetNextToken();
                    }
                    else
                    {
                        while (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot ||
                               _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus ||
                               _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword ||
                               _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
                        {
                            str1 += _tokenizer.TokenText;
                            _tokenizer.GetNextToken();
                        }
                    }

                    _tokenizer.IgnoreWhitespace = true;
                    if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Whitespace)
                        _tokenizer.GetNextToken();
                    var method = (MethodInfo)null;
                    if (flag)
                    {
                        if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
                            throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
                        _tokenizer.GetNextToken();
                        var index1 = 0;
                        while (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen &&
                               _tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                        {
                            var currentPosition6 = _tokenizer.CurrentPosition;
                            var expression = ParseExpression();
                            var currentPosition7 = _tokenizer.CurrentPosition;
                            arrayList.Add(new FunctionArgument(str1, index1, expression, currentPosition6,
                                currentPosition7));
                            ++index1;
                            if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                            {
                                if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                                    throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
                                _tokenizer.GetNextToken();
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                            throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
                        _tokenizer.GetNextToken();
                        if (!SyntaxCheckOnly())
                        {
                            var args = new FunctionArgument[arrayList.Count];
                            arrayList.CopyTo(0, args, 0, arrayList.Count);
                            try
                            {
                                method = FunctionFactory.LookupFunction(str1, args);
                            }
                            catch (EvaluatorException ex)
                            {
                                throw BuildParseError(ex.Message, currentPosition5, _tokenizer.CurrentPosition);
                            }

                            var parameters = method.GetParameters();
                            arrayList.Clear();
                            for (var index2 = 0; index2 < args.Length; ++index2)
                            {
                                var functionArgument = args[index2];
                                var parameterInfo = parameters[index2];
                                var obj = SafeConvert(parameterInfo.ParameterType, functionArgument.Value,
                                    string.Format(CultureInfo.InvariantCulture, "argument {1} ({0}) of {2}()",
                                        parameterInfo.Name, functionArgument.Index, functionArgument.Name),
                                    functionArgument.BeforeArgument, functionArgument.AfterArgument);
                                arrayList.Add(obj);
                            }
                        }
                    }

                    try
                    {
                        if (SyntaxCheckOnly())
                            return null;
                        return flag ? EvaluateFunction(method, arrayList.ToArray()) : EvaluateProperty(str1);
                    }
                    catch (Exception ex)
                    {
                        if (flag)
                            throw BuildParseError("Function call failed.", currentPosition5, _tokenizer.CurrentPosition,
                                ex);
                        throw BuildParseError("Property evaluation failed.", currentPosition5,
                            _tokenizer.CurrentPosition, ex);
                    }
            }
        }

        protected ExpressionParseException BuildParseError(string desc, ExpressionTokenizer.Position p0)
        {
            return new ExpressionParseException(desc, p0.CharIndex);
        }

        protected ExpressionParseException BuildParseError(
            string desc,
            ExpressionTokenizer.Position p0,
            ExpressionTokenizer.Position p1)
        {
            return new ExpressionParseException(desc, p0.CharIndex, p1.CharIndex);
        }

        protected ExpressionParseException BuildParseError(
            string desc,
            ExpressionTokenizer.Position p0,
            ExpressionTokenizer.Position p1,
            Exception ex)
        {
            return new ExpressionParseException(desc, p0.CharIndex, p1.CharIndex, ex);
        }

        protected object SafeConvert(
            Type returnType,
            object source,
            string description,
            ExpressionTokenizer.Position p0,
            ExpressionTokenizer.Position p1)
        {
            try
            {
                var flag = false;
                if (source == null)
                {
                    if (returnType == typeof(string))
                        return string.Empty;
                    throw BuildParseError(
                        string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1045"), description,
                            GetSimpleTypeName(returnType)), p0, p1);
                }

                if (source is bool && returnType != typeof(string) && returnType != typeof(bool))
                    flag = true;
                if (returnType == typeof(bool))
                    switch (source)
                    {
                        case string _:
                        case bool _:
                            break;
                        default:
                            flag = true;
                            break;
                    }

                if (source is DateTime && returnType != typeof(string) && returnType != typeof(DateTime))
                    flag = true;
                if (returnType == typeof(DateTime))
                    switch (source)
                    {
                        case DateTime _:
                        case string _:
                            break;
                        default:
                            flag = true;
                            break;
                    }

                if (source is TimeSpan && returnType != typeof(TimeSpan))
                    flag = true;
                if (returnType == typeof(TimeSpan) && !(source is TimeSpan))
                    flag = true;
                if (returnType == typeof(string))
                {
                    if (source is DirectoryInfo)
                        return ((FileSystemInfo)source).FullName;
                    if (source is FileInfo)
                        return ((FileSystemInfo)source).FullName;
                }

                if (returnType.IsEnum)
                {
                    if (!(source is string str1))
                        return Enum.ToObject(returnType, source);
                    var strArray = str1.Split(' ', ',');
                    var stringBuilder = new StringBuilder(str1.Length);
                    for (var index = 0; index < strArray.Length; ++index)
                    {
                        var str2 = strArray[index].Trim();
                        if (str2.Length != 0)
                        {
                            if (stringBuilder.Length > 0)
                                stringBuilder.Append(',');
                            stringBuilder.Append(str2);
                        }
                    }

                    return Enum.Parse(returnType, stringBuilder.ToString(), true);
                }

                if (flag)
                    throw BuildParseError(
                        string.Format(CultureInfo.InvariantCulture,
                            "Cannot convert {0} to '{1}' (actual type was '{2}').", description,
                            GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1);
                return Convert.ChangeType(source, returnType, CultureInfo.InvariantCulture);
            }
            catch (ExpressionParseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw BuildParseError(
                    string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').",
                        description, GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1, ex);
            }
        }

        protected string GetSimpleTypeName(Type t)
        {
            if (t == typeof(int))
                return "int";
            if (t == typeof(long))
                return "long";
            if (t == typeof(double))
                return "double";
            if (t == typeof(string))
                return "string";
            if (t == typeof(bool))
                return "bool";
            if (t == typeof(DateTime))
                return "datetime";
            return t == typeof(TimeSpan) ? "timespan" : t.FullName;
        }

        protected abstract object EvaluateFunction(MethodInfo method, object[] args);

        protected abstract object EvaluateProperty(string propertyName);

        protected virtual object UnexpectedToken()
        {
            throw BuildParseError(
                string.Format(CultureInfo.InvariantCulture, "Unexpected token '{0}'.", _tokenizer.CurrentToken),
                _tokenizer.CurrentPosition);
        }
    }
}