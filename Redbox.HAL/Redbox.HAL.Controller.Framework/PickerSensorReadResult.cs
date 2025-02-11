using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class PickerSensorReadResult :
        IPickerSensorReadResult,
        IReadInputsResult<PickerInputs>
    {
        private static readonly PickerInputs[] PickerSensors = new PickerInputs[6]
        {
            PickerInputs.Sensor1,
            PickerInputs.Sensor2,
            PickerInputs.Sensor3,
            PickerInputs.Sensor4,
            PickerInputs.Sensor5,
            PickerInputs.Sensor6
        };

        private readonly ReadPickerInputsResult ReadResult;

        internal PickerSensorReadResult(ErrorCodes notSuccess)
        {
            Error = notSuccess;
            BlockedCount = PickerSensors.Length;
            OnError();
        }

        internal PickerSensorReadResult(ReadPickerInputsResult result)
        {
            var sensorReadResult = this;
            ReadResult = result;
            Error = result.Error;
            if (Error != ErrorCodes.Success)
            {
                BlockedCount = PickerSensors.Length;
                OnError();
            }
            else
            {
                var bc = 0;
                Array.ForEach(PickerSensors, each =>
                {
                    if (!sensorReadResult.ReadResult.IsInputActive(each))
                        return;
                    ++bc;
                });
                BlockedCount = bc;
            }
        }

        public void Log()
        {
            Log(LogEntryType.Info);
        }

        public void Log(LogEntryType type)
        {
            if (!Success)
            {
                OnError();
            }
            else
            {
                var builder = new StringBuilder();
                builder.Append("PickerSensors: ");
                var count = 1;
                Array.ForEach(PickerSensors,
                    each => builder.AppendFormat("{0} = {1};", count++,
                        IsInputActive(each) ? "BLOCKED" : (object)"CLEAR"));
                LogHelper.Instance.Log(builder.ToString(), type);
            }
        }

        public bool IsInputActive(PickerInputs input)
        {
            return !Success || ReadResult.IsInputActive(input);
        }

        public bool IsInState(PickerInputs input, InputState state)
        {
            return ReadResult.IsInState(input, state);
        }

        public void Foreach(Action<PickerInputs> action)
        {
            Array.ForEach(PickerSensors, sensor => action(sensor));
        }

        public ErrorCodes Error { get; }

        public bool Success => Error == ErrorCodes.Success;

        public int InputCount => PickerSensors.Length;

        public bool IsFull => BlockedCount > 0;

        public int BlockedCount { get; }

        private void OnError()
        {
            if (Error == ErrorCodes.Success)
                return;
            LogHelper.Instance.WithContext(LogEntryType.Error, "Read Picker sensors failed with error {0}", Error);
        }
    }
}