using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using VistaDB;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage
{
    public abstract class AbstractVistaDBDataTable<T> : IDataTable<T>
    {
        private readonly string ConnectionString;
        private readonly bool UseTransaction;

        protected AbstractVistaDBDataTable(IDataTableDescriptor descriptor, string tableName)
        {
            ConnectionString = new VistaDBConnectionStringBuilder
            {
                DataSource = descriptor.Source,
                Pooling = descriptor.SupportsPooling,
                OpenMode = descriptor.ExclusiveReadWrite
                    ? VistaDBDatabaseOpenMode.ExclusiveReadWrite
                    : VistaDBDatabaseOpenMode.NonexclusiveReadWrite
            }.ConnectionString;
            Name = tableName;
            UseTransaction = descriptor.UseTransaction;
        }

        protected virtual string PrimaryKeyColumns => string.Empty;

        protected virtual string PrimaryKeyToken => string.Empty;

        public List<T> LoadEntries(bool debug)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var objList = OnLoadEntries();
                executionTimer.Stop();
                if (debug)
                    LogHelper.Instance.Log("[{0}] Loaded {1} entries ( {2}ms )", Name, objList.Count,
                        executionTimer.ElapsedMilliseconds);
                return objList;
            }
        }

        public List<T> LoadEntries()
        {
            return LoadEntries(false);
        }

        public bool Update(T item)
        {
            return ExecuteStatement(UpdateStatement, item);
        }

        public bool Update(IList<T> entries)
        {
            return ExecuteStatements(UpdateStatement, entries);
        }

        public bool Insert(T p)
        {
            return ExecuteStatement(InsertStatement, p);
        }

        public bool Insert(IList<T> entries)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var flag = ExecuteStatements(InsertStatement, entries);
                executionTimer.Stop();
                LogHelper.Instance.Log("[{0}] Insertion of {1} entries ( {2}ms )", Name, entries.Count,
                    executionTimer.ElapsedMilliseconds);
                return flag;
            }
        }

        public bool Delete(T item)
        {
            return ExecuteStatement(DeleteStatement, item);
        }

        public bool Delete(IList<T> entries)
        {
            return ExecuteStatements(DeleteStatement, entries);
        }

        public int DeleteAll()
        {
            return ExecuteStatement(string.Format("DELETE FROM [{0}]", Name));
        }

        public bool Create()
        {
            return ExecuteStatement(CreateStatement()) == 0;
        }

        public bool Drop()
        {
            return ExecuteStatement(string.Format("DROP TABLE {0}", Name)) >= 0;
        }

        public int Clean()
        {
            var statement = CleanStatement();
            return string.IsNullOrEmpty(statement) ? 0 : ExecuteStatement(statement);
        }

        public int GetRowCount()
        {
            using (var connection = Connect())
            {
                using (var vistaDbCommand =
                       new VistaDBCommand(string.Format("select count(*) from {0}", Name), connection))
                {
                    return (int)vistaDbCommand.ExecuteScalar();
                }
            }
        }

        public void UpdateTable(TextWriter writer, ErrorList errors)
        {
            if (string.IsNullOrEmpty(PrimaryKeyToken))
                return;
            UpdateTableKeys(writer, errors);
        }

        public void ExectuteSelectQuery(OnSelectResult callback, string query)
        {
            using (var connection = Connect())
            {
                using (var vistaDbCommand = new VistaDBCommand(query, connection))
                {
                    using (var reader = vistaDbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                            if (callback != null)
                                callback(reader);
                    }
                }
            }
        }

        public bool Recreate(ErrorList errors)
        {
            if (!Drop())
            {
                errors.Add(Error.NewError("I345", "Drop table error",
                    string.Format("Failed to drop table '{0}'.", Name)));
                return false;
            }

            if (Create())
                return true;
            errors.Add(Error.NewError("I346", "Create table error",
                string.Format("Failed to create table '{0}'.", Name)));
            return false;
        }

        public bool Exists
        {
            get
            {
                using (var vistaDbConnection = Connect())
                {
                    try
                    {
                        vistaDbConnection.GetTableSchema(Name);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public string Name { get; }

        protected abstract List<T> OnLoadEntries();

        protected abstract string UpdateStatement(T obj);

        protected abstract string InsertStatement(T obj);

        protected abstract string DeleteStatement(T obj);

        protected abstract string CreateStatement();

        protected virtual string CleanStatement()
        {
            return string.Empty;
        }

        protected VistaDBConnection Connect()
        {
            var vistaDbConnection = new VistaDBConnection(ConnectionString);
            vistaDbConnection.Open();
            return vistaDbConnection;
        }

        private void UpdateTableKeys(TextWriter writer, ErrorList errors)
        {
            var requiresAlter = true;
            ExectuteSelectQuery(reader =>
            {
                if (!reader.GetString(0).Contains(PrimaryKeyToken))
                    return;
                requiresAlter = false;
            }, "select name from [database schema] where typeid = 2");
            if (requiresAlter)
                using (var vistaDbConnection = Connect())
                {
                    var str = string.Format("alter table {0} add constraint {1} PRIMARY KEY {2}", Name, PrimaryKeyToken,
                        PrimaryKeyColumns);
                    using (var vistaDbCommand = new VistaDBCommand())
                    {
                        vistaDbCommand.CommandText = str;
                        vistaDbCommand.Connection = vistaDbConnection;
                        try
                        {
                            vistaDbCommand.ExecuteNonQuery();
                            writer.WriteLine("[{0}] Successfully altered table with PK info.", Name);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(Error.NewError(string.Format("{0} table alter failure", Name),
                                string.Format("Failed to alter table {0} to include primary key", Name), ex));
                            writer.WriteLine("[RunStatement] of '{0}' caught an exception.", str);
                        }
                    }
                }
            else
                writer.WriteLine("[{0}] Table does not require alter.", Name);
        }

        private bool ExecuteStatement(Statement s, T obj)
        {
            var statement = s(obj);
            var num = ExecuteStatement(statement);
            if (1 == num)
                return true;
            LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statement '{0}' returned {1}", statement,
                num);
            return false;
        }

        private bool ExecuteStatements(Statement s, IList<T> entries)
        {
            var stringList = new List<string>();
            using (new DisposeableList<string>(stringList))
            {
                foreach (var entry in entries)
                    stringList.Add(s(entry));
                if (stringList.Count == 0)
                    return false;
                var num = UseTransaction ? ExecuteTransaction(stringList) : ExecuteStatements(stringList);
                if (entries.Count == num)
                    return true;
                LogHelper.Instance.Log("[AbstractSQLDataTable] ERROR: Execute statements returned {0} ( expected {1} )",
                    num, entries.Count);
                return false;
            }
        }

        private int ExecuteStatement(string statement)
        {
            using (var vistaDbConnection = Connect())
            {
                using (var vistaDbCommand = new VistaDBCommand())
                {
                    vistaDbCommand.Connection = vistaDbConnection;
                    vistaDbCommand.CommandText = statement;
                    try
                    {
                        return vistaDbCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("[RunStatement] of '{0}' caught an exception.", statement),
                            ex);
                        return -1;
                    }
                }
            }
        }

        private int ExecuteStatements(List<string> statements)
        {
            var num = 0;
            using (var vistaDbConnection = Connect())
            {
                using (var vistaDbCommand = new VistaDBCommand())
                {
                    vistaDbCommand.Connection = vistaDbConnection;
                    foreach (var statement in statements)
                    {
                        vistaDbCommand.CommandText = statement;
                        try
                        {
                            if (1 == vistaDbCommand.ExecuteNonQuery())
                                ++num;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log(
                                string.Format("[RunStatement] of '{0}' caught an exception.", statement), ex);
                        }
                    }
                }
            }

            return num;
        }

        private int ExecuteTransaction(List<string> statements)
        {
            using (var vistaDbConnection = Connect())
            {
                var vistaDbTransaction = vistaDbConnection.BeginTransaction();
                using (var vistaDbCommand = new VistaDBCommand())
                {
                    vistaDbCommand.Transaction = vistaDbTransaction;
                    vistaDbCommand.Connection = vistaDbConnection;
                    try
                    {
                        foreach (var statement in statements)
                        {
                            vistaDbCommand.CommandText = statement;
                            vistaDbCommand.ExecuteNonQuery();
                        }

                        vistaDbTransaction.Commit();
                        return statements.Count;
                    }
                    catch (Exception ex1)
                    {
                        LogHelper.Instance.Log("[ExecuteTransaction] of caught an exception.", ex1);
                        try
                        {
                            vistaDbTransaction.Rollback();
                            LogHelper.Instance.Log("[ExecuteTransaction] Rollback succeeded.");
                        }
                        catch (Exception ex2)
                        {
                            LogHelper.Instance.Log("[ExecuteTransaction] Rollback Exception ", ex2);
                        }

                        return 0;
                    }
                }
            }
        }

        private delegate string Statement(T obj);
    }
}