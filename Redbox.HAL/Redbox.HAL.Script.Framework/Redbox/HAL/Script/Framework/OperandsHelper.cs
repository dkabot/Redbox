using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal struct OperandsHelper : IDisposable
    {
        internal const string TimeoutMessage = "TIMEOUT";
        internal const string FailureMessage = "FAILURE";
        internal const string SuccessMessage = "SUCCESS";
        private readonly TokenList Tokens;
        private readonly ErrorList Errors;
        private bool Disposed;

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
        }

        internal int? GetNullableNumericOperand(string name, ExecutionContext context)
        {
            var keyValuePairValue = GetKeyValuePairValue<int?>(Tokens.GetKeyValuePair(name), context);
            return Errors.Count <= 0 ? keyValuePairValue : new int?();
        }

        internal static object[] ConvertSymbolsToParameterArray(
            TokenList tl,
            ErrorList el,
            ExecutionContext context)
        {
            using (var operandsHelper = new OperandsHelper(tl, el))
            {
                return operandsHelper.ConvertSymbolsToParameterArray(context);
            }
        }

        internal object[] ConvertSymbolsToParameterArray(ExecutionContext context)
        {
            var objectList = new List<object>();
            for (var index = 1; index < Tokens.Count; ++index)
            {
                var token = Tokens[index];
                if (!token.IsKeyValuePair)
                {
                    var obj = token.ConvertValue();
                    if ((token.Type == TokenType.Symbol || token.Type == TokenType.ConstSymbol) &&
                        token.Value.Equals(obj))
                        obj = context.GetSymbolValue(token.Value, Errors);
                    objectList.Add(obj);
                }
            }

            return objectList.ToArray();
        }

        internal bool GetKeyValuePairValue<T>(string key, out T val, ExecutionContext context)
        {
            var keyValuePair = Tokens.GetKeyValuePair(key);
            if (keyValuePair == null)
            {
                val = default;
                return false;
            }

            val = GetKeyValuePairValue<T>(keyValuePair, context);
            return true;
        }

        internal T GetKeyValuePairValue<T>(string key, ExecutionContext ctx)
        {
            return GetKeyValuePairValue<T>(Tokens.GetKeyValuePair(key), ctx);
        }

        internal T GetKeyValuePairValue<T>(KeyValuePair keyValuePair, ExecutionContext context)
        {
            if (keyValuePair == null)
                return default;
            var obj = (object)null;
            if (keyValuePair.Type == TokenType.NumericLiteral)
            {
                obj = int.Parse(keyValuePair.Value);
            }
            else if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
            {
                var symbolValue = context.GetSymbolValue(keyValuePair.Value, Errors);
                if (symbolValue != null)
                    obj = symbolValue;
            }
            else
            {
                obj = keyValuePair.Value;
            }

            var keyValuePairValue = default(T);
            try
            {
                keyValuePairValue = ConversionHelper.ChangeType<T>(obj);
            }
            catch (Exception ex)
            {
                Errors.Add(Error.NewError("E002", "Type conversion error.", ex));
            }

            return keyValuePairValue;
        }

        internal bool GetLocation(ExecutionContext context, out int deck, out int slot)
        {
            return GetLocationInner(context, out deck, out slot);
        }

        internal string GetID(ExecutionContext context)
        {
            var keyValuePair = Tokens.GetKeyValuePair("ID");
            if (keyValuePair == null)
            {
                Errors.Add(Error.NewError("E099", "Missing operand.", "The ID operand is missing."));
                return null;
            }

            var keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, context);
            return Errors.Count != 0 ? string.Empty : keyValuePairValue;
        }

        internal int? GetDeckOperand(ExecutionContext context)
        {
            var keyValuePairValue = GetKeyValuePairValue<int?>(Tokens.GetKeyValuePair("DECK"), context);
            return Errors.Count > 0 ? new int?() : keyValuePairValue;
        }

        internal string GetDateRepresentation(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString() : "NONE";
        }

        internal int? GetTimeOutValue(ExecutionContext context)
        {
            var timeOutValue = new int?();
            var keyValuePair = Tokens.GetKeyValuePair("TIMEOUT");
            if (keyValuePair == null)
                return new int?();
            if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
            {
                timeOutValue = (int?)context.GetSymbolValue(keyValuePair.Value, Errors);
            }
            else
            {
                int result;
                if (keyValuePair.Type == TokenType.NumericLiteral && int.TryParse(keyValuePair.Value, out result))
                    timeOutValue = result;
            }

            return timeOutValue;
        }

        internal bool GetWaitFlag()
        {
            var waitFlag = false;
            var keyValuePair = Tokens.GetKeyValuePair("WAIT");
            bool result;
            if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out result))
                waitFlag = result;
            return waitFlag;
        }

        private bool GetLocationInner(ExecutionContext context, out int deck, out int slot)
        {
            deck = -1;
            slot = -1;
            var deckOperand = GetDeckOperand(context);
            if (!deckOperand.HasValue)
                return false;
            deck = deckOperand.Value;
            var keyValuePairValue = GetKeyValuePairValue<int?>(Tokens.GetKeyValuePair("SLOT"), context);
            if (Errors.Count > 0)
                return false;
            if (keyValuePairValue.HasValue)
                slot = keyValuePairValue.Value;
            return true;
        }

        internal OperandsHelper(TokenList tl, ErrorList el)
        {
            Tokens = tl;
            Errors = el;
            Disposed = false;
        }
    }
}