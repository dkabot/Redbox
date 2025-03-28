using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Rental.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

namespace Redbox.KioskEngine.Environment
{
  public class QueueService : IQueueService, IDisposable
  {
    internal const int DefaultQueueReadPeriod = 60000;
    private TimeSpan _overnightPriorityStartTime = TimeSpan.FromHours(1.0);
    private TimeSpan _overnightPriorityEndTime = TimeSpan.FromHours(5.0);
    private TimeSpan _peakPriorityStartTime = QueueServiceConstants.PeakPriorityStartTimeDefault;
    private TimeSpan _peakPriorityEndTime = QueueServiceConstants.PeakPriorityEndTimeDefault;
    private IQueueServiceData _queueServiceData;
    private readonly List<QueueService.QueueServicePriority> _queueServicePriorities = new List<QueueService.QueueServicePriority>();
    private readonly List<IQueueMessageProcessorService> _queueMessageProcessorServices = new List<IQueueMessageProcessorService>();
    private Timer m_queueTimer;
    private volatile bool m_isProcessingMessage;
    private const string EncryptedPrefix = "RedboxEncrypted";

    public static QueueService Instance => Singleton<QueueService>.Instance;

    public void StopQueueWorker()
    {
      if (this.m_queueTimer == null)
        return;
      this.m_queueTimer.Dispose();
      this.m_queueTimer = (Timer) null;
      LogHelper.Instance.Log("Queue worker stopped.");
    }

    public bool IsWorkerStarted() => this.m_queueTimer != null;

    public T GetObjectFromMessageData<T>(IMessage message)
    {
      T objectFromMessageData = default (T);
      if (message?.Data != null)
      {
        string json = Encoding.ASCII.GetString(message.Data);
        try
        {
          objectFromMessageData = json.ToObject<T>();
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log(string.Format("Unable to convert message.data to {0}", (object) typeof (T)), ex);
        }
      }
      return objectFromMessageData;
    }

    public bool RegisterMessageProcessorService(
      IQueueMessageProcessorService queueMessageProcessorService)
    {
      bool flag = false;
      if (queueMessageProcessorService != null)
      {
        lock (this._queueMessageProcessorServices)
        {
          if (this._queueMessageProcessorServices.Any<IQueueMessageProcessorService>((Func<IQueueMessageProcessorService, bool>) (x => x.Name == queueMessageProcessorService?.Name)))
            LogHelper.Instance.Log("Error in RegisterMessageProcessorService.  Duplicate name " + queueMessageProcessorService?.Name);
          else if (queueMessageProcessorService.IsDefault && this._queueMessageProcessorServices.Count<IQueueMessageProcessorService>((Func<IQueueMessageProcessorService, bool>) (x => x.IsDefault)) >= 1)
          {
            LogHelper.Instance.Log("Warning: More than one default Queue Message Processor Service.");
          }
          else
          {
            this._queueMessageProcessorServices.Add(queueMessageProcessorService);
            LogHelper.Instance.Log("QueueService registerd Queue Message Processor Service " + queueMessageProcessorService?.Name);
            flag = true;
          }
        }
      }
      if (!flag)
        LogHelper.Instance.Log("Error in RegisterMessageProcessorService. " + queueMessageProcessorService?.Name + " not registered.");
      return flag;
    }

    public bool UnRegisterMessageProcessorService(
      IQueueMessageProcessorService queueMessageProcessorService)
    {
      bool flag = false;
      lock (this._queueMessageProcessorServices)
      {
        if (this._queueMessageProcessorServices.Contains(queueMessageProcessorService))
        {
          this._queueMessageProcessorServices.Remove(queueMessageProcessorService);
          LogHelper.Instance.Log("QueueService Unregistered Queue Message Processor Service " + queueMessageProcessorService?.Name);
          flag = true;
        }
      }
      return flag;
    }

    public void StartQueueWorker()
    {
      if (this.m_queueTimer != null)
        return;
      this.m_queueTimer = new Timer((TimerCallback) (_ =>
      {
        if (this.m_isProcessingMessage)
          return;
        try
        {
          this.m_isProcessingMessage = true;
          ErrorList errorList = this.Dequeue((Predicate<IMessage>) (message =>
          {
            bool errored = false;
            bool result = true;
            try
            {
              List<IQueueMessageProcessorService> processorsForMessageType = this.GetQueueProcessorsForMessageType(message?.Type);
              if (processorsForMessageType.Any<IQueueMessageProcessorService>())
              {
                processorsForMessageType.ForEach((Action<IQueueMessageProcessorService>) (q =>
                {
                  object clientData = (object) null;
                  try
                  {
                    BeforeMessageProcessingDelegate messageProcessing = q.BeforeMessageProcessing;
                    if (messageProcessing != null)
                      messageProcessing(message, out clientData);
                  }
                  catch (Exception ex)
                  {
                    LogHelper.Instance.Log("An unhandled exception was raised while running BeforeMessageProcessing in QueueService.StartQueueWorker.", ex);
                  }
                  if (q.ProcessMessage == null)
                    return;
                  IDictionary<string, object> response = (IDictionary<string, object>) null;
                  try
                  {
                    result &= q.ProcessMessage(message, out response);
                  }
                  catch (Exception ex)
                  {
                    LogHelper.Instance.Log("An unhandled exception was raised while running ProcessMessage in QueueService.StartQueueWorker.", ex);
                    errored = true;
                  }
                  if (result)
                  {
                    try
                    {
                      MessageProcessedSuccessfullyDelegate processedSuccessfully = q.MessageProcessedSuccessfully;
                      if (processedSuccessfully != null)
                        processedSuccessfully(message, response, clientData);
                    }
                    catch (Exception ex)
                    {
                      LogHelper.Instance.Log("An unhandled exception was raised while running MessageProcessed in QueueService.StartQueueWorker.", ex);
                      errored = true;
                    }
                  }
                  try
                  {
                    AfterMessageProcessedDelegate messageProcessed = q.AfterMessageProcessed;
                    if (messageProcessed == null)
                      return;
                    messageProcessed(message, clientData);
                  }
                  catch (Exception ex)
                  {
                    LogHelper.Instance.Log("An unhandled exception was raised while running AfterMessageProcessed in QueueService.StartQueueWorker.", ex);
                  }
                }));
              }
              else
              {
                lock (this._queueMessageProcessorServices)
                {
                  string[] array = this._queueMessageProcessorServices.Select<IQueueMessageProcessorService, string>((Func<IQueueMessageProcessorService, string>) (x => x.Name)).ToArray<string>();
                  LogHelper.Instance.Log("Error in QueueService. Unable to find QueueMessageProcessorService for message type " + message?.Type + ".  QueueMessageProcessorServices registered are: " + (array != null ? ((IEnumerable<string>) array).Join<string>(", ") : (string) null));
                }
                errored = true;
              }
            }
            catch (Exception ex)
            {
              LogHelper.Instance.Log("Exception in Dequeue predicate processing message type " + message?.Type + "!", ex);
              errored = true;
            }
            return !errored && result;
          }));
          if (!errorList.ContainsError())
            return;
          LogHelper.Instance.Log("Unable to dequeue message in Queue Service.StartQueueWorker; errors follow:");
          foreach (Redbox.KioskEngine.ComponentModel.Error error in (List<Redbox.KioskEngine.ComponentModel.Error>) errorList)
            LogHelper.Instance.Log("...{0}", (object) error);
        }
        finally
        {
          this.m_isProcessingMessage = false;
        }
      }), (object) null, this.QueueReadPeriod, this.QueueReadPeriod);
      LogHelper.Instance.Log("Queue worker started at read period of: {0}", (object) this.QueueReadPeriod);
    }

    public void Dispose()
    {
      this._queueServiceData?.Dispose();
      if (this.m_queueTimer == null)
        return;
      this.m_queueTimer.Dispose();
      this.m_queueTimer = (Timer) null;
    }

    public ErrorList Clear() => this._queueServiceData?.Clear() ?? new ErrorList();

    public ErrorList GetDepth(out int count)
    {
      count = 0;
      return this._queueServiceData?.GetDepth(out count) ?? new ErrorList();
    }

    public ErrorList Enqueue(byte priority, string type, object message)
    {
      ErrorList errorList = this._queueServiceData?.Enqueue(priority, type, this.ToEncryptedMessage(message)) ?? new ErrorList();
      if (!errorList.ContainsCode("Q999"))
        return errorList;
      this.HandleCorruptDatabase();
      return errorList;
    }

    public ErrorList EnqueueKioskEvent(object message)
    {
      return this._queueServiceData?.Enqueue((byte) 200, "KioskEvents", this.ToEncryptedMessage(message)) ?? new ErrorList();
    }

    public ErrorList EnqueueOnlyOne(
      byte priority,
      string type,
      object message,
      out int newMessageId)
    {
      int newMessageId1 = 0;
      ErrorList errorList = this._queueServiceData?.DeleteMessageByType(type) ?? new ErrorList();
      if (errorList == null || !errorList.ContainsError())
        errorList = this._queueServiceData?.Enqueue(priority, type, this.ToEncryptedMessage(message), out newMessageId1) ?? new ErrorList();
      newMessageId = newMessageId1;
      if (errorList.ContainsCode("Q999"))
        this.HandleCorruptDatabase();
      return errorList;
    }

    public ErrorList Update(int id, string message)
    {
      ErrorList errorList = this._queueServiceData?.Update(id, this.ToEncryptedMessage(message)) ?? new ErrorList();
      if (!errorList.ContainsCode("Q999"))
        return errorList;
      this.HandleCorruptDatabase();
      return errorList;
    }

    public void ForEachPriority(Action<IQueueServicePriority> action)
    {
      foreach (QueueService.QueueServicePriority queueServicePriority in this._queueServicePriorities)
        action((IQueueServicePriority) queueServicePriority);
    }

    public ErrorList ForEach(Action<IMessage> action)
    {
      return this._queueServiceData?.ForEach((Action<IMessage>) (encryptedMessage => action((IMessage) new QueueMessage()
      {
        ID = encryptedMessage.ID,
        Type = encryptedMessage.Type,
        CreatedOn = encryptedMessage.CreatedOn,
        Priority = encryptedMessage.Priority,
        Data = this.FromEncryptedMessage(encryptedMessage.Data)
      }))) ?? new ErrorList();
    }

    public ErrorList ForEach(Action<IMessage> action, string type)
    {
      return this._queueServiceData?.ForEach((Action<IMessage>) (encryptedMessage => action((IMessage) new QueueMessage()
      {
        ID = encryptedMessage.ID,
        Type = encryptedMessage.Type,
        CreatedOn = encryptedMessage.CreatedOn,
        Priority = encryptedMessage.Priority,
        Data = this.FromEncryptedMessage(encryptedMessage.Data)
      })), type) ?? new ErrorList();
    }

    public ErrorList Initialize(string path, IQueueServiceData queueServiceData)
    {
      this._queueServiceData = queueServiceData;
      this.GetQueueServiceConfigurationValues();
      QueueService.QueueServicePriority queueServicePriority1 = new QueueService.QueueServicePriority()
      {
        PriorityType = QueueServicePriorityType.Peak,
        MinimumPriorityValue = 100,
        MaximumPriorityValue = (int) byte.MaxValue,
        StartTime = new TimeSpan?(this._peakPriorityStartTime),
        EndTime = new TimeSpan?(this._peakPriorityEndTime),
        Description = "Used to set the time range during which only the highest priority messages (peak) can be sent to Kiosk Services"
      };
      QueueService.QueueServicePriority queueServicePriority2 = new QueueService.QueueServicePriority()
      {
        PriorityType = QueueServicePriorityType.offPeak,
        MinimumPriorityValue = 50,
        MaximumPriorityValue = 99,
        Description = "Used to set the time range during which peak priority and offpeak priority messages can be sent to Kiosk Services"
      };
      queueServicePriority2.ExcludeTimeRanges.Add((IQueueServicePriority) queueServicePriority1);
      this._queueServicePriorities.Add(queueServicePriority1);
      this._queueServicePriorities.Add(queueServicePriority2);
      this._queueServicePriorities.Add(new QueueService.QueueServicePriority()
      {
        PriorityType = QueueServicePriorityType.overNight,
        MinimumPriorityValue = 0,
        MaximumPriorityValue = 49,
        StartTime = new TimeSpan?(this._overnightPriorityStartTime),
        EndTime = new TimeSpan?(this._overnightPriorityEndTime),
        Description = "Used to set the time range during which the lowest priority messages (over night) can be sent to Kiosk Services"
      });
      this._queueServicePriorities.ForEach((Action<QueueService.QueueServicePriority>) (x => LogHelper.Instance.Log(string.Format("Setting QueueService Priority: {0}", (object) x))));
      ErrorList errorList = new ErrorList();
      LogHelper.Instance.Log("Initialize queue service, storage path: {0}", (object) path);
      if (ServiceLocator.Instance.GetService<IQueueService>() == null)
        ServiceLocator.Instance.AddService(typeof (IQueueService), (object) QueueService.Instance);
      try
      {
        errorList = this._queueServiceData.Initialize(path);
        if (errorList != null && errorList.ContainsError())
          this.HandleCorruptDatabase();
        IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
        if (service != null)
        {
          service.AddPreferencePage("QueueServices_General", "Remote Services\\Queue Services", "Engine Core/Queue Services/Message Queue", PreferencePageTarget.LocalSystem, (IPreferencePageHost) new QueuePreferencePage());
          service.AddPreferencePage("QueueServices_Priorities", "Remote Services\\Queue Services", "Engine Core/Queue Services/Message Priorities", PreferencePageTarget.LocalSystem, (IPreferencePageHost) new QueuePrioritiesPreferencePage());
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Initialize.", ex);
        this.HandleCorruptDatabase();
      }
      return errorList;
    }

    public ErrorList Dequeue(Predicate<IMessage> predicate)
    {
      IMessage message1;
      ErrorList errorList = this._queueServiceData.SelectNextMessage(this.GetCurrentQueueServicePriority(), out message1);
      if ((errorList != null ? (errorList.ContainsError() ? 1 : 0) : 0) == 0 && message1 != null)
      {
        IMessage message2 = (IMessage) new QueueMessage()
        {
          ID = message1.ID,
          Priority = message1.Priority,
          CreatedOn = message1.CreatedOn,
          Type = message1.Type,
          Data = this.FromEncryptedMessage(message1.Data)
        };
        int num = predicate(message2) ? 1 : 0;
        this._queueServiceData.DeleteMessage(message2.ID);
      }
      if (!errorList.ContainsCode("Q999"))
        return errorList;
      this.HandleCorruptDatabase();
      return errorList;
    }

    public ErrorList DeleteMessage(int id) => this._queueServiceData?.DeleteMessage(id);

    public ErrorList DeleteMessageByType(string type)
    {
      return this._queueServiceData.DeleteMessageByType(type);
    }

    public ErrorList ExportToXml(string fileName)
    {
      ErrorList xml = new ErrorList();
      try
      {
        XmlTextWriter xmlWriter = new XmlTextWriter(fileName, Encoding.UTF8);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("queue-export");
        this._queueServiceData.ForEach((Action<IMessage>) (m =>
        {
          xmlWriter.WriteStartElement("message");
          xmlWriter.WriteAttributeString("id", m.ID.ToString());
          xmlWriter.WriteAttributeString("type", m.Type);
          xmlWriter.WriteAttributeString("priority", m.Priority.ToString());
          xmlWriter.WriteAttributeString("createdon", m.CreatedOn.ToString());
          xmlWriter.WriteCData(Encoding.ASCII.GetString(m.Data));
          xmlWriter.WriteEndElement();
        }));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in QueueService.ExportToXml.", ex);
        xml.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.ExportToXml.", ex));
      }
      return xml;
    }

    public int QueueReadPeriod => 60000;

    private void GetQueueServiceConfigurationValues()
    {
      IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
      TimeSpan? startTime;
      TimeSpan? endTime;
      if (service.TryGetTimeSpanRangeValue("system", nameof (QueueService), "PeakPriorityOnlyTimeRange", out startTime, out endTime))
      {
        this._peakPriorityStartTime = startTime.Value;
        this._peakPriorityEndTime = endTime.Value;
      }
      else
      {
        this._peakPriorityStartTime = QueueServiceConstants.PeakPriorityStartTimeDefault;
        this._peakPriorityEndTime = QueueServiceConstants.PeakPriorityEndTimeDefault;
      }
      if (service.TryGetTimeSpanRangeValue("system", nameof (QueueService), "OvernightPriorityTimeRange", out startTime, out endTime))
      {
        this._overnightPriorityStartTime = startTime.Value;
        this._overnightPriorityEndTime = endTime.Value;
      }
      else
      {
        this._overnightPriorityStartTime = QueueServiceConstants.OvernightPriorityStartTimeDefault;
        this._overnightPriorityEndTime = QueueServiceConstants.OvernightPriorityEndTimeDefault;
      }
    }

    private IQueueServicePriority GetCurrentQueueServicePriority()
    {
      TimeSpan currentTime = DateTime.Now.TimeOfDay;
      return (IQueueServicePriority) this._queueServicePriorities.Where<QueueService.QueueServicePriority>((Func<QueueService.QueueServicePriority, bool>) (x => x.IsInTimeRange(currentTime))).OrderBy<QueueService.QueueServicePriority, int>((Func<QueueService.QueueServicePriority, int>) (x => x.MinimumPriorityValue)).FirstOrDefault<QueueService.QueueServicePriority>();
    }

    private QueueService()
    {
    }

    private void HandleCorruptDatabase()
    {
      IMacroService service1 = ServiceLocator.Instance.GetService<IMacroService>();
      IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
      IApplicationState service3 = ServiceLocator.Instance.GetService<IApplicationState>();
      int num1 = service3 != null ? (service3.HasMaintenanceModeReason(MaintenanceModeSource.Database, "SOFT-FILE01") ? 1 : 0) : 0;
      ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
      if (num1 != 0)
        return;
      try
      {
        var data = new
        {
          MessageType = "KioskAlert",
          MessageId = Guid.NewGuid(),
          KioskId = int.Parse(service2.Filter["store_number"]),
          EngineVersion = service1["EngineVersion"],
          BundleVersion = service1["ProductVersion"],
          SubType = "mm:DataStoreError, SOFT-FILE01",
          Detail = "<h2>Kiosk Engine Data File Error</h2><p>There was an error accessing or updating the database: [ message.data ] <br><br>Placing Kiosk into Maintenance Mode.<br>SOFT-FILE01</p>",
          Type = "ApplicationCrash",
          Time = DateTime.Now
        };
        IEnumerable<IQueueMessageProcessorService> items;
        lock (this._queueMessageProcessorServices)
          items = this._queueMessageProcessorServices.Where<IQueueMessageProcessorService>((Func<IQueueMessageProcessorService, bool>) (x => x.Name.Equals("KSKioskAlertMessageProcessorService") || x.Name.Equals("AWSKioskAlertMessageProcessorService")));
        items.ForEach<IQueueMessageProcessorService>((Action<IQueueMessageProcessorService>) (q =>
        {
          if (q == null || q.ProcessMessage == null || q == null)
            return;
          ProcessMessageDelegate processMessage = q.ProcessMessage;
          QueueMessage queueMessage = new QueueMessage();
          queueMessage.ID = 0;
          queueMessage.Type = "KioskAlert";
          queueMessage.CreatedOn = DateTime.Now;
          queueMessage.Data = Encoding.ASCII.GetBytes(data.ToJson());
          queueMessage.Priority = byte.MaxValue;
            int num2 = processMessage((IMessage)queueMessage, out IDictionary<string, object> dictionary) ? 1 : 0;
        }));
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("HandleCorruptDatabase: Error creating and sending KioskAlert message.", ex);
      }
    }

    private static int GetRandomNumber(int min, int max)
    {
      RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
      byte[] data = new byte[4];
      cryptoServiceProvider.GetBytes(data);
      cryptoServiceProvider.Dispose();
      int int32 = BitConverter.ToInt32(data, 0);
      return min != max ? min + Math.Abs(int32 % (max - min)) : min;
    }

    private byte[] ToEncryptedMessage(object message) => this.ToEncryptedMessage(message.ToJson());

    private byte[] ToEncryptedMessage(string message)
    {
      byte[] src = Encoding.ASCII.GetBytes(message).Encrypt();
      byte[] dst = new byte["RedboxEncrypted".Length + src.Length];
      byte[] bytes = Encoding.ASCII.GetBytes("RedboxEncrypted");
      Buffer.BlockCopy((Array) bytes, 0, (Array) dst, 0, bytes.Length);
      Buffer.BlockCopy((Array) src, 0, (Array) dst, bytes.Length, src.Length);
      return dst;
    }

    private byte[] FromEncryptedMessage(byte[] data)
    {
      byte[] bytes = Encoding.ASCII.GetBytes("RedboxEncrypted");
      if (data.Length > bytes.Length)
      {
        byte[] numArray1 = new byte[bytes.Length];
        Buffer.BlockCopy((Array) data, 0, (Array) numArray1, 0, numArray1.Length);
        if ("RedboxEncrypted".Equals(Encoding.ASCII.GetString(numArray1)))
        {
          byte[] numArray2 = new byte[data.Length - bytes.Length];
          Buffer.BlockCopy((Array) data, bytes.Length, (Array) numArray2, 0, numArray2.Length);
          return numArray2.Decrypt();
        }
      }
      return data;
    }

    private List<IQueueMessageProcessorService> GetQueueProcessorsForMessageType(string type)
    {
      List<IQueueMessageProcessorService> source = new List<IQueueMessageProcessorService>();
      lock (this._queueMessageProcessorServices)
      {
        source.AddRange(this._queueMessageProcessorServices.Where<IQueueMessageProcessorService>((Func<IQueueMessageProcessorService, bool>) (x => x.SupportedMessageTypes.Contains(type))));
        if (!source.Any<IQueueMessageProcessorService>())
        {
          IQueueMessageProcessorService processorService = this._queueMessageProcessorServices.FirstOrDefault<IQueueMessageProcessorService>((Func<IQueueMessageProcessorService, bool>) (x => x.IsDefault));
          if (processorService != null)
            source.Add(processorService);
        }
      }
      return source;
    }

    private class QueueServicePriority : IQueueServicePriority
    {
      public QueueServicePriorityType PriorityType { get; set; }

      public string Description { get; set; }

      public int MinimumPriorityValue { get; set; }

      public int MaximumPriorityValue { get; set; }

      public TimeSpan? StartTime { get; set; }

      public TimeSpan? EndTime { get; set; }

      public bool IsInTimeRange(TimeSpan timeSpan)
      {
        int num;
        if (this.StartTime.HasValue || this.EndTime.HasValue)
        {
          TimeSpan? startTime = this.StartTime;
          TimeSpan timeSpan1 = timeSpan;
          if ((startTime.HasValue ? (startTime.GetValueOrDefault() <= timeSpan1 ? 1 : 0) : 0) != 0)
          {
            TimeSpan? endTime = this.EndTime;
            TimeSpan timeSpan2 = timeSpan;
            num = endTime.HasValue ? (endTime.GetValueOrDefault() > timeSpan2 ? 1 : 0) : 0;
          }
          else
            num = 0;
        }
        else
          num = 1;
        return num != 0 && !this.ExcludeTimeRanges.Any<IQueueServicePriority>((Func<IQueueServicePriority, bool>) (x => x.IsInTimeRange(timeSpan)));
      }

      public List<IQueueServicePriority> ExcludeTimeRanges => new List<IQueueServicePriority>();

      public override string ToString()
      {
        return string.Format("{0}: Min priority value: {1}, Max priority value: {2}, Start time: {3}, End time: {4}, Exclude priority time ranges: {5}", (object) this.PriorityType, (object) this.MinimumPriorityValue, (object) this.MaximumPriorityValue, (object) this.StartTime, (object) this.EndTime, (object) this.ExcludeTimeRanges.Select<IQueueServicePriority, string>((Func<IQueueServicePriority, string>) (y => y.PriorityType.ToString())).Join<string>(", "));
      }
    }
  }
}
