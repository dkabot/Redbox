using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment
{
  public class QueueServiceVistaDBData : IQueueServiceData, IDisposable
  {
    public ErrorList Initialize(string path, int retry = 2)
    {
      ErrorList errorList = new ErrorList();
      if (retry <= 0)
      {
        LogHelper.Instance.LogError("Q997", "retry limit exceeded.");
        errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q997", "Unable to initialize the Queue Service database.", "retry limit exceeded."));
        return errorList;
      }
      try
      {
        this.DatabasePath = path;
        if (File.Exists(this.DatabasePath))
        {
          LogHelper.Instance.Log("> Pack message database...");
          using (IVistaDBDDA vistaDbdda = VistaDBEngine.Connections.OpenDDA())
            vistaDbdda.PackDatabase(this.DatabasePath, (string) null, false, (GaugeHookInfo) null);
        }
        SchemaMigrator.Migrate(this.DatabasePath);
        this.Connection = new VistaDBConnection(string.Format("Data Source={0};Open Mode=ExclusiveReadWrite;", (object) this.DatabasePath));
        this.Connection.Open();
      }
      catch (Exception ex1)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in QueueServiceVistDBData.Initialize.", ex1);
        try
        {
          string destFileName = this.DatabasePath + ".corrupt";
          LogHelper.Instance.Log("Attempt to move '{0}' to '{1}'.", (object) this.DatabasePath, (object) destFileName);
          File.Move(this.DatabasePath, destFileName);
          LogHelper.Instance.Log("Attempt to retry QueueService.Initialize.");
          errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) this.Initialize(path, --retry));
        }
        catch (Exception ex2)
        {
          LogHelper.Instance.Log("An unhandled exception was raised renaming a corrupt Queue Service data file.", ex2);
          errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "Unable to initialize the Queue Service database.", ex1));
        }
      }
      return errorList;
    }

    public ErrorList Clear()
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "DELETE FROM Queue";
          vistaDbCommand.ExecuteNonQuery();
        }
      }), nameof (Clear));
    }

    public ErrorList GetDepth(out int count)
    {
      int tempCount = 0;
      ErrorList depth = this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "SELECT COUNT(*) FROM Queue";
          tempCount = (int) vistaDbCommand.ExecuteScalar();
        }
      }), nameof (GetDepth));
      count = tempCount;
      return depth;
    }

    public ErrorList DeleteMessage(int id)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "DELETE FROM Queue WHERE ID = @ID";
          vistaDbCommand.Parameters.Add("@ID", (object) id);
          vistaDbCommand.ExecuteNonQuery();
        }
      }), nameof (DeleteMessage));
    }

    public ErrorList DeleteMessageByType(string type)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "DELETE FROM Queue WHERE Type = @Type";
          vistaDbCommand.Parameters.Add("@Type", (object) type);
          vistaDbCommand.ExecuteNonQuery();
        }
      }), nameof (DeleteMessageByType));
    }

    public ErrorList Enqueue(byte priority, string type, byte[] encryptedMessage)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data )";
          vistaDbCommand.Parameters.Add("@Type", (object) type);
          vistaDbCommand.Parameters.Add("@Priority", (object) priority);
          vistaDbCommand.Parameters.Add("@Data", (object) encryptedMessage);
          vistaDbCommand.ExecuteNonQuery();
        }
      }), nameof (Enqueue));
    }

    public ErrorList Enqueue(
      byte priority,
      string type,
      byte[] encryptedMessage,
      out int newMessageId)
    {
      newMessageId = 0;
      int tempMessageId = 0;
      ErrorList errorList = this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data ); SELECT @@IDENTITY;";
          vistaDbCommand.Parameters.Add("@Type", (object) type);
          vistaDbCommand.Parameters.Add("@Priority", (object) priority);
          vistaDbCommand.Parameters.Add("@Data", (object) encryptedMessage);
          tempMessageId = Convert.ToInt32(vistaDbCommand.ExecuteScalar());
        }
      }), nameof (Enqueue));
      newMessageId = tempMessageId;
      return errorList;
    }

    public ErrorList Update(int id, byte[] message)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "UPDATE Queue SET Data = @Data WHERE ID = @Id";
          vistaDbCommand.Parameters.Add("@Id", (object) id);
          vistaDbCommand.Parameters.Add("@Data", (object) message);
          vistaDbCommand.ExecuteNonQuery();
        }
      }), nameof (Update));
    }

    public ErrorList ForEach(Action<IMessage> action)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "SELECT * FROM Queue ORDER BY Priority DESC, ID ASC";
          using (VistaDBDataReader reader = vistaDbCommand.ExecuteReader())
          {
            while (reader.Read())
              action(this.ToMessage(reader));
          }
        }
      }), nameof (ForEach));
    }

    public ErrorList ForEach(Action<IMessage> action, string type)
    {
      return this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "SELECT * FROM Queue WHERE Type = @Type ORDER BY Priority DESC, ID ASC";
          vistaDbCommand.Parameters.Add("@Type", (object) type);
          using (VistaDBDataReader reader = vistaDbCommand.ExecuteReader())
          {
            while (reader.Read())
              action(this.ToMessage(reader));
          }
        }
      }), nameof (ForEach));
    }

    public ErrorList SelectNextMessage(
      IQueueServicePriority queueServicePriority,
      out IMessage message)
    {
      IMessage resultMessage = (IMessage) null;
      ErrorList errorList = this.ProcessVistaDBCommand((Action) (() =>
      {
        using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
        {
          vistaDbCommand.Connection = this.Connection;
          vistaDbCommand.CommandText = "SELECT TOP 1 ID, Type, Priority, Data, CreatedOn FROM Queue WHERE Priority >= @PriorityValue ORDER BY Priority DESC, ID ASC;";
          VistaDBParameterCollection parameters = vistaDbCommand.Parameters;
          IQueueServicePriority queueServicePriority1 = queueServicePriority;
          int minimumPriorityValue = (queueServicePriority1 != null ? queueServicePriority1.MinimumPriorityValue : 0);
          parameters.Add("@PriorityValue", (object) minimumPriorityValue);
          using (VistaDBDataReader reader = vistaDbCommand.ExecuteReader())
          {
            if (!reader.Read())
              return;
            resultMessage = this.ToMessage(reader);
          }
        }
      }), nameof (SelectNextMessage));
      message = resultMessage;
      return errorList;
    }

    public void Dispose()
    {
      if (this.Connection == null)
        return;
      this.Connection.Dispose();
    }

    private ErrorList ProcessVistaDBCommand(Action action, string methodName)
    {
      ErrorList errors = new ErrorList();
      if (this.IsConnectionValid(errors))
      {
        try
        {
          action();
        }
        catch (Exception ex)
        {
          this.LogExceptionAndAddToErrors(errors, methodName, ex);
        }
      }
      return errors;
    }

    private void LogExceptionAndAddToErrors(
      ErrorList errors,
      string exceptionSourceMethodName,
      Exception e)
    {
      LogHelper.Instance.Log("An unhandled exception was raised in " + exceptionSourceMethodName + ".", e);
      errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueServiceVistaDBData." + exceptionSourceMethodName + ".", e));
    }

    private bool IsConnectionValid(ErrorList errors)
    {
      bool flag = true;
      if (this.Connection == null)
      {
        errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
        flag = false;
      }
      return flag;
    }

    private IMessage ToMessage(VistaDBDataReader reader)
    {
      return (IMessage) new QueueMessage()
      {
        ID = (int) reader["ID"],
        Type = (string) reader["Type"],
        CreatedOn = (DateTime) reader["CreatedOn"],
        Data = (byte[]) reader["Data"],
        Priority = (byte) reader["Priority"]
      };
    }

    internal VistaDBConnection Connection { get; set; }

    internal string DatabasePath { get; set; }
  }
}
