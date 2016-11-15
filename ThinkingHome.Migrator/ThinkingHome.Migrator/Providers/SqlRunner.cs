using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Logging;

namespace ThinkingHome.Migrator.Providers
{
    public class SqlRunner : IDisposable
    {
        protected SqlRunner(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            Connection = connection;
        }

        private bool connectionNeedClose;

        private IDbTransaction transaction;

        public int CommandTimeout { get; set; }

        public IDbConnection Connection { get; }

        public virtual string BatchSeparator => null;

        #region public methods

        public IDataReader ExecuteReader(string sql)
        {
            IDbCommand cmd = null;
            IDataReader reader = null;

            try
            {
                MigratorLogManager.Log.ExecuteSql(sql);
                cmd = GetCommand(sql);
                reader = OpenDataReader(cmd);
                return reader;
            }
            catch (Exception ex)
            {
                reader?.Dispose();

                if (cmd != null)
                {
                    MigratorLogManager.Log.Warn($"query failed: {cmd.CommandText}");
                    cmd.Dispose();
                }

                throw new SQLException(ex);
            }
        }

        public object ExecuteScalar(string sql)
        {
            using (var cmd = GetCommand(sql))
            {
                try
                {
                    MigratorLogManager.Log.ExecuteSql(sql);
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MigratorLogManager.Log.Warn($"Query failed: {cmd.CommandText}");
                    throw new SQLException(ex);
                }
            }
        }

        public int ExecuteNonQuery(string sql)
        {
            int result = 0;

            try
            {
                // если задан разделитель пакетов запросов, запускаем пакеты по очереди
                if (!string.IsNullOrWhiteSpace(BatchSeparator) &&
                    sql.IndexOf(BatchSeparator, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    sql += "\n" + BatchSeparator.Trim(); // make sure last batch is executed.

                    string[] lines = sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    var sqlBatch = new StringBuilder();

                    foreach (string line in lines)
                    {
                        if (line.ToUpperInvariant().Trim() == BatchSeparator.ToUpperInvariant())
                        {
                            string query = sqlBatch.ToString();
                            if (!string.IsNullOrWhiteSpace(query))
                            {
                                result = ExecuteNonQueryInternal(query);
                            }

                            sqlBatch.Clear();
                        }
                        else
                        {
                            sqlBatch.AppendLine(line.Trim());
                        }
                    }
                }
                else
                {
                    result = ExecuteNonQueryInternal(sql);
                }
            }
            catch (Exception ex)
            {
                MigratorLogManager.Log.Warn(ex);
                throw new SQLException(ex);
            }

            return result;
        }

        public void ExecuteFromResource(Assembly assembly, string path)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null) throw new NullReferenceException();

                using (var reader = new StreamReader(stream))
                {
                    string sql = reader.ReadToEnd();
                    ExecuteNonQuery(sql);
                }
            }
        }


        #endregion

        #region transactions

        /// <summary>
        /// Starts a transaction. Called by the migration mediator.
        /// </summary>
        public void BeginTransaction()
        {
            if (transaction == null && Connection != null)
            {
                EnsureHasConnection();
                transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
        }

        /// <summary>
        /// Commit the current transaction. Called by the migrations mediator.
        /// </summary>
        public void Commit()
        {
            if (transaction != null && Connection != null && Connection.State == ConnectionState.Open)
            {
                try
                {
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    MigratorLogManager.Log.Error(ex, "Failed to apply the transaction");
                }
            }

            transaction = null;
        }

        /// <summary>
        /// Rollback the current migration. Called by the migration mediator.
        /// </summary>
        public virtual void Rollback()
        {
            if (transaction != null && Connection != null && Connection.State == ConnectionState.Open)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex)
                {
                    MigratorLogManager.Log.Error(ex, "Failed to rollback the transaction");
                }
            }

            transaction = null;
        }

        #endregion

        #region helpers

        protected void EnsureHasConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                connectionNeedClose = true;
                Connection.Open();
            }
        }

        private int ExecuteNonQueryInternal(string sql)
        {
            MigratorLogManager.Log.ExecuteSql(sql);

            using (IDbCommand cmd = GetCommand(sql))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public virtual IDbCommand GetCommand(string sql = null)
        {
            EnsureHasConnection();

            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            if (CommandTimeout > 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            return cmd;
        }

        protected virtual IDataReader OpenDataReader(IDbCommand cmd)
        {
            return cmd.ExecuteReader();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (connectionNeedClose && Connection != null && Connection.State == ConnectionState.Open)
            {
                Connection.Close();
                connectionNeedClose = false;
            }
        }

        #endregion
    }
}