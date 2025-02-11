using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.Macros
{
    internal abstract class ExpressionEvalBase
    {
        private ExpressionTokenizer _tokenizer;
        private ExpressionEvalBase.EvalMode _evalMode;

        public object Evaluate(ExpressionTokenizer tokenizer)
        {
            this._evalMode = ExpressionEvalBase.EvalMode.Evaluate;
            this._tokenizer = tokenizer;
            return this.ParseExpression();
        }

        public object Evaluate(string s)
        {
            this._tokenizer = new ExpressionTokenizer();
            this._evalMode = ExpressionEvalBase.EvalMode.Evaluate;
            this._tokenizer.InitTokenizer(s);
            object expression = this.ParseExpression();
            if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EOF)
                return expression;
            throw this.BuildParseError("Unexpected token at the end of expression", this._tokenizer.CurrentPosition);
        }

        public void CheckSyntax(string s)
        {
            this._tokenizer = new ExpressionTokenizer();
            this._evalMode = ExpressionEvalBase.EvalMode.ParseOnly;
            this._tokenizer.InitTokenizer(s);
            this.ParseExpression();
            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                throw this.BuildParseError("Unexpected token at the end of expression", this._tokenizer.CurrentPosition);
        }

        private bool SyntaxCheckOnly() => this._evalMode == ExpressionEvalBase.EvalMode.ParseOnly;

        private object ParseExpression() => this.ParseBooleanOr();

        private object ParseBooleanOr()
        {
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object source = this.ParseBooleanAnd();
            ExpressionEvalBase.EvalMode evalMode = this._evalMode;
            try
            {
                while (this._tokenizer.IsKeyword("or"))
                {
                    bool flag1 = true;
                    if (!this.SyntaxCheckOnly())
                    {
                        flag1 = (bool)this.SafeConvert(typeof(bool), source, "the left hand side of the 'or' operator", currentPosition1, this._tokenizer.CurrentPosition);
                        if (flag1)
                            this._evalMode = ExpressionEvalBase.EvalMode.ParseOnly;
                    }
                    this._tokenizer.GetNextToken();
                    ExpressionTokenizer.Position currentPosition2 = this._tokenizer.CurrentPosition;
                    object booleanAnd = this.ParseBooleanAnd();
                    ExpressionTokenizer.Position currentPosition3 = this._tokenizer.CurrentPosition;
                    if (!this.SyntaxCheckOnly())
                    {
                        bool flag2 = (bool)this.SafeConvert(typeof(bool), booleanAnd, "the right hand side of the 'or' operator", currentPosition2, currentPosition3);
                        source = (object)(flag1 | flag2);
                    }
                }
                return source;
            }
            finally
            {
                this._evalMode = evalMode;
            }
        }

        private object ParseBooleanAnd()
        {
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object source = this.ParseRelationalExpression();
            ExpressionEvalBase.EvalMode evalMode = this._evalMode;
            try
            {
                while (this._tokenizer.IsKeyword("and"))
                {
                    bool flag1 = true;
                    if (!this.SyntaxCheckOnly())
                    {
                        flag1 = (bool)this.SafeConvert(typeof(bool), source, "the left hand side of the 'and' operator", currentPosition1, this._tokenizer.CurrentPosition);
                        if (!flag1)
                            this._evalMode = ExpressionEvalBase.EvalMode.ParseOnly;
                    }
                    this._tokenizer.GetNextToken();
                    ExpressionTokenizer.Position currentPosition2 = this._tokenizer.CurrentPosition;
                    object relationalExpression = this.ParseRelationalExpression();
                    ExpressionTokenizer.Position currentPosition3 = this._tokenizer.CurrentPosition;
                    if (!this.SyntaxCheckOnly())
                    {
                        bool flag2 = (bool)this.SafeConvert(typeof(bool), relationalExpression, "the right hand side of the 'and' operator", currentPosition2, currentPosition3);
                        source = (object)(flag1 & flag2);
                    }
                }
                return source;
            }
            finally
            {
                this._evalMode = evalMode;
            }
        }

        private object ParseRelationalExpression()
        {
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object addSubtract1 = this.ParseAddSubtract();
            if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EQ || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.NE || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LT || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GT || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LE || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GE)
            {
                ExpressionTokenizer.TokenType currentToken = this._tokenizer.CurrentToken;
                this._tokenizer.GetNextToken();
                object addSubtract2 = this.ParseAddSubtract();
                ExpressionTokenizer.Position currentPosition2 = this._tokenizer.CurrentPosition;
                if (this.SyntaxCheckOnly())
                    return (object)null;
                switch (currentToken)
                {
                    case ExpressionTokenizer.TokenType.EQ:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case bool _ when addSubtract2 is bool:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is int:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is long:
                                return (object)Convert.ToInt64(addSubtract1).Equals(addSubtract2);
                            case int _ when addSubtract2 is double:
                                return (object)Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case long _ when addSubtract2 is long:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case long _ when addSubtract2 is int:
                                return (object)addSubtract1.Equals((object)Convert.ToInt64(addSubtract2));
                            case long _ when addSubtract2 is double:
                                return (object)Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case double _ when addSubtract2 is double:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case double _ when addSubtract2 is int:
                                return (object)addSubtract1.Equals((object)Convert.ToDouble(addSubtract2));
                            case double _ when addSubtract2 is long:
                                return (object)addSubtract1.Equals((object)Convert.ToDouble(addSubtract2));
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)addSubtract1.Equals(addSubtract2);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)addSubtract1.Equals(addSubtract2);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)addSubtract1.Equals(addSubtract2);
                                if (addSubtract1.GetType().IsEnum)
                                    return addSubtract2 is string ? (object)addSubtract1.Equals(Enum.Parse(addSubtract1.GetType(), (string)addSubtract2, false)) : (object)addSubtract1.Equals(Enum.ToObject(addSubtract1.GetType(), addSubtract2));
                                if (!addSubtract2.GetType().IsEnum)
                                    throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1038"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                                return addSubtract1 is string ? (object)addSubtract2.Equals(Enum.Parse(addSubtract2.GetType(), (string)addSubtract1, false)) : (object)addSubtract2.Equals(Enum.ToObject(addSubtract2.GetType(), addSubtract1));
                        }
                    case ExpressionTokenizer.TokenType.NE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case bool _ when addSubtract2 is bool:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is int:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case int _ when addSubtract2 is long:
                                return (object)!Convert.ToInt64(addSubtract1).Equals(addSubtract2);
                            case int _ when addSubtract2 is double:
                                return (object)!Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case long _ when addSubtract2 is long:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case long _ when addSubtract2 is int:
                                return (object)!addSubtract1.Equals((object)Convert.ToInt64(addSubtract2));
                            case long _ when addSubtract2 is double:
                                return (object)!Convert.ToDouble(addSubtract1).Equals(addSubtract2);
                            case double _ when addSubtract2 is double:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case double _ when addSubtract2 is int:
                                return (object)!addSubtract1.Equals((object)Convert.ToDouble(addSubtract2));
                            case double _ when addSubtract2 is long:
                                return (object)!addSubtract1.Equals((object)Convert.ToDouble(addSubtract2));
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)!addSubtract1.Equals(addSubtract2);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)!addSubtract1.Equals(addSubtract2);
                                if (addSubtract1.GetType().IsEnum)
                                    return addSubtract2 is string ? (object)!addSubtract1.Equals(Enum.Parse(addSubtract1.GetType(), (string)addSubtract2, false)) : (object)!addSubtract1.Equals(Enum.ToObject(addSubtract1.GetType(), addSubtract2));
                                if (!addSubtract2.GetType().IsEnum)
                                    throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1042"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                                return addSubtract1 is string ? (object)!addSubtract2.Equals(Enum.Parse(addSubtract2.GetType(), (string)addSubtract1, false)) : (object)!addSubtract2.Equals(Enum.ToObject(addSubtract2.GetType(), addSubtract1));
                        }
                    case ExpressionTokenizer.TokenType.LT:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)(string.Compare((string)addSubtract1, (string)addSubtract2, false, CultureInfo.InvariantCulture) < 0);
                            case bool _ when addSubtract2 is bool:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            case int _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            case int _ when addSubtract2 is long:
                                return (object)(((IComparable)Convert.ToInt64(addSubtract1)).CompareTo(addSubtract2) < 0);
                            case int _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) < 0);
                            case long _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            case long _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToInt64(addSubtract2)) < 0);
                            case long _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) < 0);
                            case double _ when addSubtract2 is double:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            case double _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) < 0);
                            case double _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) < 0);
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) < 0);
                                throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1051"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.GT:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)(string.Compare((string)addSubtract1, (string)addSubtract2, false, CultureInfo.InvariantCulture) > 0);
                            case bool _ when addSubtract2 is bool:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            case int _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            case int _ when addSubtract2 is long:
                                return (object)(((IComparable)Convert.ToInt64(addSubtract1)).CompareTo(addSubtract2) > 0);
                            case int _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) > 0);
                            case long _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            case long _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToInt64(addSubtract2)) > 0);
                            case long _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) > 0);
                            case double _ when addSubtract2 is double:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            case double _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) > 0);
                            case double _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) > 0);
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) > 0);
                                throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1037"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.LE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)(string.Compare((string)addSubtract1, (string)addSubtract2, false, CultureInfo.InvariantCulture) <= 0);
                            case bool _ when addSubtract2 is bool:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            case int _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            case int _ when addSubtract2 is long:
                                return (object)(((IComparable)Convert.ToInt64(addSubtract1)).CompareTo(addSubtract2) <= 0);
                            case int _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) <= 0);
                            case long _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            case long _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToInt64(addSubtract2)) <= 0);
                            case long _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) <= 0);
                            case double _ when addSubtract2 is double:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            case double _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) <= 0);
                            case double _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) <= 0);
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) <= 0);
                                throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1049"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                    case ExpressionTokenizer.TokenType.GE:
                        switch (addSubtract1)
                        {
                            case string _ when addSubtract2 is string:
                                return (object)(string.Compare((string)addSubtract1, (string)addSubtract2, false, CultureInfo.InvariantCulture) >= 0);
                            case bool _ when addSubtract2 is bool:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            case int _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            case int _ when addSubtract2 is long:
                                return (object)(((IComparable)Convert.ToInt64(addSubtract1)).CompareTo(addSubtract2) >= 0);
                            case int _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) >= 0);
                            case long _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            case long _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToInt64(addSubtract2)) >= 0);
                            case long _ when addSubtract2 is double:
                                return (object)(((IComparable)Convert.ToDouble(addSubtract1)).CompareTo(addSubtract2) >= 0);
                            case double _ when addSubtract2 is double:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            case double _ when addSubtract2 is int:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) >= 0);
                            case double _ when addSubtract2 is long:
                                return (object)(((IComparable)addSubtract1).CompareTo((object)Convert.ToDouble(addSubtract2)) >= 0);
                            case DateTime _ when addSubtract2 is DateTime:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            case TimeSpan _ when addSubtract2 is TimeSpan:
                                return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                            default:
                                if ((object)(addSubtract1 as Version) != null && (object)(addSubtract2 as Version) != null)
                                    return (object)(((IComparable)addSubtract1).CompareTo(addSubtract2) >= 0);
                                throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1050"), (object)this.GetSimpleTypeName(addSubtract1.GetType()), (object)this.GetSimpleTypeName(addSubtract2.GetType())), currentPosition1, currentPosition2);
                        }
                }
            }
            return addSubtract1;
        }

        private object ParseAddSubtract()
        {
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object addSubtract = this.ParseMulDiv();
            object mulDiv1;
            ExpressionTokenizer.Position currentPosition2;
            while (true)
            {
                do
                {
                    while (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Plus)
                    {
                        if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Minus)
                            return addSubtract;
                        this._tokenizer.GetNextToken();
                        object mulDiv2 = this.ParseMulDiv();
                        ExpressionTokenizer.Position currentPosition3 = this._tokenizer.CurrentPosition;
                        if (!this.SyntaxCheckOnly())
                        {
                            switch (addSubtract)
                            {
                                case int _ when mulDiv2 is int num1:
                                    addSubtract = (object)((int)addSubtract - num1);
                                    continue;
                                case int _ when mulDiv2 is long num2:
                                    addSubtract = (object)((long)(int)addSubtract - num2);
                                    continue;
                                case int _ when mulDiv2 is double num3:
                                    addSubtract = (object)((double)(int)addSubtract - num3);
                                    continue;
                                case long _ when mulDiv2 is long num4:
                                    addSubtract = (object)((long)addSubtract - num4);
                                    continue;
                                case long _ when mulDiv2 is int num5:
                                    addSubtract = (object)((long)addSubtract - (long)num5);
                                    continue;
                                case long _ when mulDiv2 is double num6:
                                    addSubtract = (object)((double)(long)addSubtract - num6);
                                    continue;
                                case double _ when mulDiv2 is double num7:
                                    addSubtract = (object)((double)addSubtract - num7);
                                    continue;
                                case double _ when mulDiv2 is int num8:
                                    addSubtract = (object)((double)addSubtract - (double)num8);
                                    continue;
                                case double _ when mulDiv2 is long num9:
                                    addSubtract = (object)((double)addSubtract - (double)num9);
                                    continue;
                                case DateTime _ when mulDiv2 is DateTime dateTime:
                                    addSubtract = (object)((DateTime)addSubtract - dateTime);
                                    continue;
                                case DateTime _ when mulDiv2 is TimeSpan timeSpan1:
                                    addSubtract = (object)((DateTime)addSubtract - timeSpan1);
                                    continue;
                                case TimeSpan _ when mulDiv2 is TimeSpan timeSpan2:
                                    addSubtract = (object)((TimeSpan)addSubtract - timeSpan2);
                                    continue;
                                default:
                                    throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1048"), (object)this.GetSimpleTypeName(addSubtract.GetType()), (object)this.GetSimpleTypeName(mulDiv2.GetType())), currentPosition1, currentPosition3);
                            }
                        }
                    }
                    this._tokenizer.GetNextToken();
                    mulDiv1 = this.ParseMulDiv();
                    currentPosition2 = this._tokenizer.CurrentPosition;
                }
                while (this.SyntaxCheckOnly());
                switch (addSubtract)
                {
                    case string _ when mulDiv1 is string:
                        addSubtract = (object)((string)addSubtract + (string)mulDiv1);
                        continue;
                    case int _ when mulDiv1 is int num10:
                        addSubtract = (object)((int)addSubtract + num10);
                        continue;
                    case int _ when mulDiv1 is long num11:
                        addSubtract = (object)((long)(int)addSubtract + num11);
                        continue;
                    case int _ when mulDiv1 is double num12:
                        addSubtract = (object)((double)(int)addSubtract + num12);
                        continue;
                    case long _ when mulDiv1 is long num13:
                        addSubtract = (object)((long)addSubtract + num13);
                        continue;
                    case long _ when mulDiv1 is int num14:
                        addSubtract = (object)((long)addSubtract + (long)num14);
                        continue;
                    case long _ when mulDiv1 is double num15:
                        addSubtract = (object)((double)(long)addSubtract + num15);
                        continue;
                    case double _ when mulDiv1 is double num16:
                        addSubtract = (object)((double)addSubtract + num16);
                        continue;
                    case double _ when mulDiv1 is int num17:
                        addSubtract = (object)((double)addSubtract + (double)num17);
                        continue;
                    case double _ when mulDiv1 is long num18:
                        addSubtract = (object)((double)addSubtract + (double)num18);
                        continue;
                    case DateTime _ when mulDiv1 is TimeSpan timeSpan3:
                        addSubtract = (object)((DateTime)addSubtract + timeSpan3);
                        continue;
                    case TimeSpan _ when mulDiv1 is TimeSpan timeSpan4:
                        addSubtract = (object)((TimeSpan)addSubtract + timeSpan4);
                        continue;
                    default:
                        goto label_16;
                }
            }
        label_16:
            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1041"), (object)this.GetSimpleTypeName(addSubtract.GetType()), (object)this.GetSimpleTypeName(mulDiv1.GetType())), currentPosition1, currentPosition2);
        }

        private object ParseMulDiv()
        {
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object mulDiv = this.ParseValue();
            object obj1;
            ExpressionTokenizer.Position currentPosition2;
            while (true)
            {
                do
                {
                    while (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mul)
                    {
                        if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Div)
                        {
                            this._tokenizer.GetNextToken();
                            ExpressionTokenizer.Position currentPosition3 = this._tokenizer.CurrentPosition;
                            object obj2 = this.ParseValue();
                            ExpressionTokenizer.Position currentPosition4 = this._tokenizer.CurrentPosition;
                            if (!this.SyntaxCheckOnly())
                            {
                                switch (mulDiv)
                                {
                                    case int _ when obj2 is int num1:
                                        if (num1 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((int)mulDiv / (int)obj2);
                                        continue;
                                    case int _ when obj2 is long num2:
                                        if (num2 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((long)(int)mulDiv / (long)obj2);
                                        continue;
                                    case int _ when obj2 is double num3:
                                        if (num3 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((double)(int)mulDiv / (double)obj2);
                                        continue;
                                    case long _ when obj2 is long num4:
                                        if (num4 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((long)mulDiv / (long)obj2);
                                        continue;
                                    case long _ when obj2 is int num5:
                                        if (num5 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((long)mulDiv / (long)(int)obj2);
                                        continue;
                                    case long _ when obj2 is double num6:
                                        if (num6 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((double)(long)mulDiv / (double)obj2);
                                        continue;
                                    case double _ when obj2 is double num7:
                                        if (num7 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((double)mulDiv / (double)obj2);
                                        continue;
                                    case double _ when obj2 is int num8:
                                        if (num8 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((double)mulDiv / (double)(int)obj2);
                                        continue;
                                    case double _ when obj2 is long num9:
                                        if (num9 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
                                        mulDiv = (object)((double)mulDiv / (double)(long)obj2);
                                        continue;
                                    default:
                                        throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1039"), (object)this.GetSimpleTypeName(mulDiv.GetType()), (object)this.GetSimpleTypeName(obj2.GetType())), currentPosition1, currentPosition4);
                                }
                            }
                        }
                        else
                        {
                            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mod)
                                return mulDiv;
                            this._tokenizer.GetNextToken();
                            ExpressionTokenizer.Position currentPosition5 = this._tokenizer.CurrentPosition;
                            object obj3 = this.ParseValue();
                            ExpressionTokenizer.Position currentPosition6 = this._tokenizer.CurrentPosition;
                            if (!this.SyntaxCheckOnly())
                            {
                                switch (mulDiv)
                                {
                                    case int _ when obj3 is int num10:
                                        if (num10 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((int)mulDiv % (int)obj3);
                                        continue;
                                    case int _ when obj3 is long num11:
                                        if (num11 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((long)(int)mulDiv % (long)obj3);
                                        continue;
                                    case int _ when obj3 is double num12:
                                        if (num12 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((double)(int)mulDiv % (double)obj3);
                                        continue;
                                    case long _ when obj3 is long num13:
                                        if (num13 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((long)mulDiv % (long)obj3);
                                        continue;
                                    case long _ when obj3 is int num14:
                                        if (num14 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((long)mulDiv % (long)(int)obj3);
                                        continue;
                                    case long _ when obj3 is double num15:
                                        if (num15 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((double)(long)mulDiv % (double)obj3);
                                        continue;
                                    case double _ when obj3 is double num16:
                                        if (num16 == 0.0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((double)mulDiv % (double)obj3);
                                        continue;
                                    case double _ when obj3 is int num17:
                                        if (num17 == 0)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((double)mulDiv % (double)(int)obj3);
                                        continue;
                                    case double _ when obj3 is long num18:
                                        if (num18 == 0L)
                                            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
                                        mulDiv = (object)((double)mulDiv % (double)(long)obj3);
                                        continue;
                                    default:
                                        throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1047"), (object)this.GetSimpleTypeName(mulDiv.GetType()), (object)this.GetSimpleTypeName(obj3.GetType())), currentPosition1, currentPosition6);
                                }
                            }
                        }
                    }
                    this._tokenizer.GetNextToken();
                    obj1 = this.ParseValue();
                    currentPosition2 = this._tokenizer.CurrentPosition;
                }
                while (this.SyntaxCheckOnly());
                switch (mulDiv)
                {
                    case int _ when obj1 is int num19:
                        mulDiv = (object)((int)mulDiv * num19);
                        continue;
                    case int _ when obj1 is long num20:
                        mulDiv = (object)((long)(int)mulDiv * num20);
                        continue;
                    case int _ when obj1 is double num21:
                        mulDiv = (object)((double)(int)mulDiv * num21);
                        continue;
                    case long _ when obj1 is long num22:
                        mulDiv = (object)((long)mulDiv * num22);
                        continue;
                    case long _ when obj1 is int num23:
                        mulDiv = (object)((long)mulDiv * (long)num23);
                        continue;
                    case long _ when obj1 is double num24:
                        mulDiv = (object)((double)(long)mulDiv * num24);
                        continue;
                    case double _ when obj1 is double num25:
                        mulDiv = (object)((double)mulDiv * num25);
                        continue;
                    case double _ when obj1 is int num26:
                        mulDiv = (object)((double)mulDiv * (double)num26);
                        continue;
                    case double _ when obj1 is long num27:
                        mulDiv = (object)((double)mulDiv * (double)num27);
                        continue;
                    default:
                        goto label_13;
                }
            }
        label_13:
            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1036"), (object)this.GetSimpleTypeName(mulDiv.GetType()), (object)this.GetSimpleTypeName(obj1.GetType())), currentPosition1, currentPosition2);
        }

        private object ParseConditional()
        {
            this._tokenizer.GetNextToken();
            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
                throw this.BuildParseError("'(' expected.", this._tokenizer.CurrentPosition);
            this._tokenizer.GetNextToken();
            ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
            object expression1 = this.ParseExpression();
            ExpressionTokenizer.Position currentPosition2 = this._tokenizer.CurrentPosition;
            bool flag = false;
            if (!this.SyntaxCheckOnly())
                flag = (bool)this.SafeConvert(typeof(bool), expression1, "the conditional expression", currentPosition1, currentPosition2);
            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                throw this.BuildParseError("',' expected.", this._tokenizer.CurrentPosition);
            this._tokenizer.GetNextToken();
            ExpressionEvalBase.EvalMode evalMode = this._evalMode;
            try
            {
                this._evalMode = flag ? evalMode : ExpressionEvalBase.EvalMode.ParseOnly;
                object expression2 = this.ParseExpression();
                this._evalMode = evalMode;
                if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                    throw this.BuildParseError("',' expected.", this._tokenizer.CurrentPosition);
                this._tokenizer.GetNextToken();
                this._evalMode = !flag ? evalMode : ExpressionEvalBase.EvalMode.ParseOnly;
                object expression3 = this.ParseExpression();
                this._evalMode = evalMode;
                if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                    throw this.BuildParseError("')' expected.", this._tokenizer.CurrentPosition);
                this._tokenizer.GetNextToken();
                return flag ? expression2 : expression3;
            }
            finally
            {
                this._evalMode = evalMode;
            }
        }

        private object ParseValue()
        {
            if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.String)
            {
                string tokenText = this._tokenizer.TokenText;
                this._tokenizer.GetNextToken();
                return (object)tokenText;
            }
            if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
            {
                string tokenText = this._tokenizer.TokenText;
                ExpressionTokenizer.Position currentPosition = this._tokenizer.CurrentPosition;
                this._tokenizer.GetNextToken();
                ExpressionTokenizer.Position p1 = new ExpressionTokenizer.Position(this._tokenizer.CurrentPosition.CharIndex - 1);
                if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot)
                {
                    string str = tokenText + ".";
                    this._tokenizer.GetNextToken();
                    if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Number)
                        throw this.BuildParseError("Fractional part expected.", this._tokenizer.CurrentPosition);
                    string s = str + this._tokenizer.TokenText;
                    this._tokenizer.GetNextToken();
                    p1 = this._tokenizer.CurrentPosition;
                    try
                    {
                        return (object)double.Parse(s, (IFormatProvider)CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException ex)
                    {
                        throw this.BuildParseError("Value was either too large or too small for type 'double'.", currentPosition, p1);
                    }
                }
                else
                {
                    try
                    {
                        return (object)int.Parse(tokenText, (IFormatProvider)CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException ex1)
                    {
                        try
                        {
                            return (object)long.Parse(tokenText, (IFormatProvider)CultureInfo.InvariantCulture);
                        }
                        catch (OverflowException ex2)
                        {
                            throw this.BuildParseError("Value was either too large or too small for type 'long'.", currentPosition, p1);
                        }
                    }
                }
            }
            else if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus)
            {
                this._tokenizer.GetNextToken();
                ExpressionTokenizer.Position currentPosition1 = this._tokenizer.CurrentPosition;
                object obj = this.ParseValue();
                ExpressionTokenizer.Position currentPosition2 = this._tokenizer.CurrentPosition;
                if (this.SyntaxCheckOnly())
                    return (object)null;
                switch (obj)
                {
                    case int num1:
                        return (object)-num1;
                    case long num2:
                        return (object)-num2;
                    case double num3:
                        return (object)-num3;
                    default:
                        throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1040"), (object)this.GetSimpleTypeName(obj.GetType())), currentPosition1, currentPosition2);
                }
            }
            else
            {
                if (this._tokenizer.IsKeyword("not"))
                {
                    this._tokenizer.GetNextToken();
                    ExpressionTokenizer.Position currentPosition3 = this._tokenizer.CurrentPosition;
                    object source = this.ParseValue();
                    ExpressionTokenizer.Position currentPosition4 = this._tokenizer.CurrentPosition;
                    return !this.SyntaxCheckOnly() ? (object)!(bool)this.SafeConvert(typeof(bool), source, "the argument of 'not' operator", currentPosition3, currentPosition4) : (object)null;
                }
                if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftParen)
                {
                    this._tokenizer.GetNextToken();
                    object expression = this.ParseExpression();
                    if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                        throw this.BuildParseError("')' expected.", this._tokenizer.CurrentPosition);
                    this._tokenizer.GetNextToken();
                    return expression;
                }
                if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
                    return this.UnexpectedToken();
                ExpressionTokenizer.Position currentPosition5 = this._tokenizer.CurrentPosition;
                string str1 = this._tokenizer.TokenText;
                switch (str1)
                {
                    case "if":
                        return this.ParseConditional();
                    case "true":
                        this._tokenizer.GetNextToken();
                        return (object)true;
                    case "false":
                        this._tokenizer.GetNextToken();
                        return (object)false;
                    default:
                        this._tokenizer.IgnoreWhitespace = false;
                        this._tokenizer.GetNextToken();
                        ArrayList arrayList = new ArrayList();
                        bool flag = false;
                        if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.DoubleColon)
                        {
                            flag = true;
                            string str2 = str1 + "::";
                            this._tokenizer.GetNextToken();
                            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
                                throw this.BuildParseError("Function name expected.", currentPosition5, this._tokenizer.CurrentPosition);
                            str1 = str2 + this._tokenizer.TokenText;
                            this._tokenizer.GetNextToken();
                        }
                        else
                        {
                            while (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword || this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
                            {
                                str1 += this._tokenizer.TokenText;
                                this._tokenizer.GetNextToken();
                            }
                        }
                        this._tokenizer.IgnoreWhitespace = true;
                        if (this._tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Whitespace)
                            this._tokenizer.GetNextToken();
                        MethodInfo method = (MethodInfo)null;
                        if (flag)
                        {
                            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
                                throw this.BuildParseError("'(' expected.", this._tokenizer.CurrentPosition);
                            this._tokenizer.GetNextToken();
                            int index1 = 0;
                            while (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen && this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                            {
                                ExpressionTokenizer.Position currentPosition6 = this._tokenizer.CurrentPosition;
                                object expression = this.ParseExpression();
                                ExpressionTokenizer.Position currentPosition7 = this._tokenizer.CurrentPosition;
                                arrayList.Add((object)new FunctionArgument(str1, index1, expression, currentPosition6, currentPosition7));
                                ++index1;
                                if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                                {
                                    if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
                                        throw this.BuildParseError("',' expected.", this._tokenizer.CurrentPosition);
                                    this._tokenizer.GetNextToken();
                                }
                                else
                                    break;
                            }
                            if (this._tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
                                throw this.BuildParseError("')' expected.", this._tokenizer.CurrentPosition);
                            this._tokenizer.GetNextToken();
                            if (!this.SyntaxCheckOnly())
                            {
                                FunctionArgument[] args = new FunctionArgument[arrayList.Count];
                                arrayList.CopyTo(0, (Array)args, 0, arrayList.Count);
                                try
                                {
                                    method = FunctionFactory.LookupFunction(str1, args);
                                }
                                catch (EvaluatorException ex)
                                {
                                    throw this.BuildParseError(ex.Message, currentPosition5, this._tokenizer.CurrentPosition);
                                }
                                ParameterInfo[] parameters = method.GetParameters();
                                arrayList.Clear();
                                for (int index2 = 0; index2 < args.Length; ++index2)
                                {
                                    FunctionArgument functionArgument = args[index2];
                                    ParameterInfo parameterInfo = parameters[index2];
                                    object obj = this.SafeConvert(parameterInfo.ParameterType, functionArgument.Value, string.Format((IFormatProvider)CultureInfo.InvariantCulture, "argument {1} ({0}) of {2}()", (object)parameterInfo.Name, (object)functionArgument.Index, (object)functionArgument.Name), functionArgument.BeforeArgument, functionArgument.AfterArgument);
                                    arrayList.Add(obj);
                                }
                            }
                        }
                        try
                        {
                            if (this.SyntaxCheckOnly())
                                return (object)null;
                            return flag ? this.EvaluateFunction(method, arrayList.ToArray()) : this.EvaluateProperty(str1);
                        }
                        catch (Exception ex)
                        {
                            if (flag)
                                throw this.BuildParseError("Function call failed.", currentPosition5, this._tokenizer.CurrentPosition, ex);
                            throw this.BuildParseError("Property evaluation failed.", currentPosition5, this._tokenizer.CurrentPosition, ex);
                        }
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
                bool flag = false;
                if (source == null)
                {
                    if (returnType == typeof(string))
                        return (object)string.Empty;
                    throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1045"), (object)description, (object)this.GetSimpleTypeName(returnType)), p0, p1);
                }
                if (source is bool && returnType != typeof(string) && returnType != typeof(bool))
                    flag = true;
                if (returnType == typeof(bool))
                {
                    switch (source)
                    {
                        case string _:
                        case bool _:
                            break;
                        default:
                            flag = true;
                            break;
                    }
                }
                if (source is DateTime && returnType != typeof(string) && returnType != typeof(DateTime))
                    flag = true;
                if (returnType == typeof(DateTime))
                {
                    switch (source)
                    {
                        case DateTime _:
                        case string _:
                            break;
                        default:
                            flag = true;
                            break;
                    }
                }
                if (source is TimeSpan && returnType != typeof(TimeSpan))
                    flag = true;
                if (returnType == typeof(TimeSpan) && !(source is TimeSpan))
                    flag = true;
                if (returnType == typeof(string))
                {
                    if (source is DirectoryInfo)
                        return (object)((FileSystemInfo)source).FullName;
                    if (source is FileInfo)
                        return (object)((FileSystemInfo)source).FullName;
                }
                if (returnType.IsEnum)
                {
                    if (!(source is string str1))
                        return Enum.ToObject(returnType, source);
                    string[] strArray = str1.Split(' ', ',');
                    StringBuilder stringBuilder = new StringBuilder(str1.Length);
                    for (int index = 0; index < strArray.Length; ++index)
                    {
                        string str2 = strArray[index].Trim();
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
                    throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", (object)description, (object)this.GetSimpleTypeName(returnType), (object)this.GetSimpleTypeName(source.GetType())), p0, p1);
                return Convert.ChangeType(source, returnType, (IFormatProvider)CultureInfo.InvariantCulture);
            }
            catch (ExpressionParseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", (object)description, (object)this.GetSimpleTypeName(returnType), (object)this.GetSimpleTypeName(source.GetType())), p0, p1, ex);
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
            throw this.BuildParseError(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Unexpected token '{0}'.", (object)this._tokenizer.CurrentToken), this._tokenizer.CurrentPosition);
        }

        public enum EvalMode
        {
            Evaluate,
            ParseOnly,
        }
    }
}
