using Redbox.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment
{
  public static class SchemaMigrator
  {
    public static void Migrate(string path)
    {
      if (!File.Exists(path))
      {
        LogHelper.Instance.Log("...Create database file: {0}", (object) path);
        using (VistaDBConnection vistaDbConnection = new VistaDBConnection())
        {
          using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
          {
            vistaDbCommand.Connection = vistaDbConnection;
            vistaDbCommand.CommandText = string.Format("CREATE DATABASE '{0}', PAGE SIZE 1, LCID 1033, CASE SENSITIVE FALSE;", (object) path);
            vistaDbCommand.ExecuteNonQuery();
          }
        }
      }
      int? nullable1 = new int?();
      using (VistaDBConnection vistaDbConnection = new VistaDBConnection(string.Format("Data Source={0};Open Mode=ExclusiveReadWrite;", (object) path)))
      {
        vistaDbConnection.Open();
        using (VistaDBCommand vistaDbCommand1 = new VistaDBCommand())
        {
          vistaDbCommand1.Connection = vistaDbConnection;
          vistaDbCommand1.CommandText = "SELECT COUNT(*) FROM [database schema] WHERE typeid = 1 AND name = @name";
          vistaDbCommand1.Parameters.Add("@name", (object) "System");
          if ((int) vistaDbCommand1.ExecuteScalar() == 0)
          {
            LogHelper.Instance.Log("...System table does not exist, create and run all migrations.");
            using (VistaDBCommand vistaDbCommand2 = new VistaDBCommand())
            {
              vistaDbCommand2.Connection = vistaDbConnection;
              vistaDbCommand2.CommandText = "\r\n                                CREATE TABLE System( [Key] varchar(16) NOT NULL PRIMARY KEY, Value varchar(256) );\r\n                                INSERT INTO System([Key],Value) VALUES( 'SchemaVersion', '-1' );";
              vistaDbCommand2.ExecuteNonQuery();
              nullable1 = new int?(-1);
            }
          }
        }
        if (!nullable1.HasValue)
        {
          using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
          {
            vistaDbCommand.Connection = vistaDbConnection;
            vistaDbCommand.CommandText = "SELECT Value FROM System WHERE [Key]='SchemaVersion'";
            nullable1 = new int?(Convert.ToInt32(vistaDbCommand.ExecuteScalar()));
            LogHelper.Instance.Log("...SchemaVersion is: {0}", (object) nullable1);
          }
        }
        Assembly assembly = typeof (SchemaMigrator).Assembly;
        string str = string.Format("{0}.Migrations.{1}", (object) typeof (SchemaMigrator).Namespace, (object) Path.GetFileName(path));
        List<string> stringList = new List<string>();
        foreach (string manifestResourceName in assembly.GetManifestResourceNames())
        {
          if (manifestResourceName.StartsWith(str))
            stringList.Add(manifestResourceName);
        }
        stringList.Sort();
        int? nullable2 = nullable1;
        int count = stringList.Count;
        if (nullable2.GetValueOrDefault() == count & nullable2.HasValue)
        {
          LogHelper.Instance.Log("...Schema is up-to-date.");
        }
        else
        {
          try
          {
            LogHelper.Instance.Log("...Migrate schema to current version.");
            nullable2 = nullable1;
            int num = -1;
            if (!(nullable2.GetValueOrDefault() == num & nullable2.HasValue))
            {
              nullable2 = nullable1;
              nullable1 = nullable2.HasValue ? new int?(nullable2.GetValueOrDefault() - 1) : new int?();
            }
            for (int index = nullable1.Value + 1; index < stringList.Count; ++index)
            {
              string name = stringList[index];
              LogHelper.Instance.Log("...Executing migration script: {0}", (object) name.Replace(str + ".", string.Empty));
              using (Stream manifestResourceStream = assembly.GetManifestResourceStream(name))
              {
                using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
                {
                  vistaDbCommand.Connection = vistaDbConnection;
                  vistaDbCommand.CommandText = Encoding.ASCII.GetString(manifestResourceStream.GetBytes());
                  vistaDbCommand.ExecuteNonQuery();
                }
              }
            }
            using (VistaDBCommand vistaDbCommand = new VistaDBCommand())
            {
              vistaDbCommand.Connection = vistaDbConnection;
              vistaDbCommand.CommandText = "UPDATE System SET Value = @Value WHERE [Key] = @Key";
              vistaDbCommand.Parameters.Add("@Key", (object) "SchemaVersion");
              vistaDbCommand.Parameters.Add("@Value", (object) stringList.Count);
              vistaDbCommand.ExecuteNonQuery();
            }
          }
          catch (Exception ex)
          {
            LogHelper.Instance.Log("An unhandled exception was raised from script migration.", ex);
          }
        }
      }
    }
  }
}
