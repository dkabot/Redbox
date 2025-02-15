using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services
{
    internal sealed class ClientPickerSensorReadResult :
        IPickerSensorReadResult,
        IReadInputsResult<PickerInputs>
    {
        private const int SensorCount = 6;

        private static readonly PickerInputs[] PickerSensors = new PickerInputs[6]
        {
            PickerInputs.Sensor1,
            PickerInputs.Sensor2,
            PickerInputs.Sensor3,
            PickerInputs.Sensor4,
            PickerInputs.Sensor5,
            PickerInputs.Sensor6
        };

        private readonly bool[] m_sensors = new bool[6];

        internal ClientPickerSensorReadResult(ErrorCodes error)
        {
            BlockedCount = 6;
            Array.ForEach(m_sensors, each => each = false);
            Error = error;
        }

        internal ClientPickerSensorReadResult(Stack<string> jobStack)
        {
            for (var index = 5; index >= 0; --index)
            {
                var str = jobStack.Pop();
                m_sensors[index] = str.Contains("BLOCKED");
                if (m_sensors[index])
                    ++BlockedCount;
            }

            Error = ErrorCodes.Success;
        }

        public void Log()
        {
        }

        public void Log(LogEntryType type)
        {
        }

        public bool IsInputActive(PickerInputs input)
        {
            var index = Array.IndexOf(PickerSensors, input);
            return -1 != index ? m_sensors[index] : throw new ArgumentException();
        }

        public bool IsInState(PickerInputs input, InputState state)
        {
            throw new NotImplementedException();
        }

        public void Foreach(Action<PickerInputs> action)
        {
            Array.ForEach(PickerSensors, each => action(each));
        }

        public ErrorCodes Error { get; }

        public bool Success => Error == ErrorCodes.Success;

        public int InputCount => 6;

        public bool IsFull => BlockedCount > 0;

        public int BlockedCount { get; }
    }
}