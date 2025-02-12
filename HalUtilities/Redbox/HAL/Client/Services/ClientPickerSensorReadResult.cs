using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Services
{
  internal sealed class ClientPickerSensorReadResult : 
    IPickerSensorReadResult,
    IReadInputsResult<PickerInputs>
  {
    private const int SensorCount = 6;
    private readonly bool[] m_sensors = new bool[6];
    private static readonly PickerInputs[] PickerSensors = new PickerInputs[6]
    {
      PickerInputs.Sensor1,
      PickerInputs.Sensor2,
      PickerInputs.Sensor3,
      PickerInputs.Sensor4,
      PickerInputs.Sensor5,
      PickerInputs.Sensor6
    };

    public void Log()
    {
    }

    public void Log(LogEntryType type)
    {
    }

    public bool IsInputActive(PickerInputs input)
    {
      int index = Array.IndexOf<PickerInputs>(ClientPickerSensorReadResult.PickerSensors, input);
      return -1 != index ? this.m_sensors[index] : throw new ArgumentException();
    }

    public bool IsInState(PickerInputs input, InputState state)
    {
      throw new NotImplementedException();
    }

    public void Foreach(Action<PickerInputs> action)
    {
      Array.ForEach<PickerInputs>(ClientPickerSensorReadResult.PickerSensors, (Action<PickerInputs>) (each => action(each)));
    }

    public ErrorCodes Error { get; private set; }

    public bool Success => this.Error == ErrorCodes.Success;

    public int InputCount => 6;

    public bool IsFull => this.BlockedCount > 0;

    public int BlockedCount { get; private set; }

    internal ClientPickerSensorReadResult(ErrorCodes error)
    {
      this.BlockedCount = 6;
      Array.ForEach<bool>(this.m_sensors, (Action<bool>) (each => each = false));
      this.Error = error;
    }

    internal ClientPickerSensorReadResult(Stack<string> jobStack)
    {
      for (int index = 5; index >= 0; --index)
      {
        string str = jobStack.Pop();
        this.m_sensors[index] = str.Contains("BLOCKED");
        if (this.m_sensors[index])
          ++this.BlockedCount;
      }
      this.Error = ErrorCodes.Success;
    }
  }
}
