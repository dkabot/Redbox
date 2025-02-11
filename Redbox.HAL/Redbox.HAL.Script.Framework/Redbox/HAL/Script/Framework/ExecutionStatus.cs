using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    public sealed class ExecutionStatus
    {
        public ExecutionContextStatus Value { get; internal set; }

        internal ExecutionContextStatus? NextValue { get; set; }

        public static bool operator ==(ExecutionStatus status, ExecutionContextStatus right)
        {
            return status != null && status.Value.Equals(right);
        }

        public static bool operator !=(ExecutionStatus status, ExecutionContextStatus right)
        {
            return !(status == right);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            var executionStatus = obj as ExecutionStatus;
            return executionStatus != null && executionStatus.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        internal bool ShouldPromote()
        {
            if (!NextValue.HasValue)
                return false;
            var num = (int)Value;
            var nextValue = NextValue;
            var valueOrDefault = (int)nextValue.GetValueOrDefault();
            return !((num == valueOrDefault) & nextValue.HasValue);
        }

        internal void Promote()
        {
            Promote(NextValue.Value);
        }

        internal void Promote(ExecutionContextStatus value)
        {
            Value = value;
            NextValue = new ExecutionContextStatus?();
        }
    }
}