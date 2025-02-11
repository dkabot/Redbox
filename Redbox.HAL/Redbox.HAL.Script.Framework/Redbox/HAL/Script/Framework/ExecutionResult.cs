using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    public sealed class ExecutionResult : IExecutionResult
    {
        public ExecutionResult()
        {
            Errors = new ErrorList();
        }

        internal bool RestartAtCurrentIP { get; set; }

        internal Instruction SwitchToContext { get; set; }
        public ErrorList Errors { get; }

        public TimeSpan? ExecutionTime { get; set; }

        internal void Reset()
        {
            Errors.Clear();
            ExecutionTime = new TimeSpan?();
            SwitchToContext = null;
            RestartAtCurrentIP = false;
        }

        internal void UpdateExecutionTime(TimeSpan timeSpan)
        {
            if (!ExecutionTime.HasValue)
            {
                ExecutionTime = timeSpan;
            }
            else
            {
                var executionTime = ExecutionTime;
                var timeSpan1 = timeSpan;
                ExecutionTime = executionTime.HasValue
                    ? executionTime.GetValueOrDefault() + timeSpan1
                    : new TimeSpan?();
            }
        }

        internal void AddMissingOperandError(string details)
        {
            AddError(ExecutionErrors.MissingOperand, details);
        }

        internal void AddInvalidOperandError(string details)
        {
            AddError(ExecutionErrors.InvalidOperand, details);
        }

        internal void AddError(ExecutionErrors error, string details)
        {
            Errors.Add(Error.NewError(ExecutionErrorsTable.ErrorCodeTable[error],
                ExecutionErrorsTable.ErrorTable[error], details));
        }
    }
}