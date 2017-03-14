using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Ppgz.Repository
{
    public static class Db
    {
        //TODO MOVER A LA CONFIGURACION
    
        private static readonly DbProviderFactory Factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        #region Insert Handlers
        public static void Insert(string sql, List<MySqlParameter> parameters = null)
        {
            using (var connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (var command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    // Si los parametros nos son null los recorremos
                    if (parameters != null)
                    {
                        foreach (MySqlParameter param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

        }

        public static void StoreProcedure(IList<MySqlParameter> parameters, string storeProcedure)
        {
            using (var connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = storeProcedure;
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (MySqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static Dictionary<string, string> ExecuteProcedureOut(IList<MySqlParameter> parameters, string storeProcedure)
        {
            MySqlParameter identity = new MySqlParameter();
            Dictionary<string, string> retornables = new Dictionary<string, string>();


            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = storeProcedure;
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {

                        foreach (MySqlParameter parameter in parameters)
                        {
                            if (parameter.Direction == ParameterDirection.InputOutput)
                            {
                                identity = parameter;
                                command.Parameters.Add(identity);
                            }
                            else
                            {
                                command.Parameters.Add(parameter);
                            }


                        }
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();



                }
            }

            foreach (MySqlParameter parameter in parameters)
            {
                if (parameter.Direction == ParameterDirection.InputOutput)
                {
                    retornables.Add(parameter.ParameterName, parameter.Value.ToString());

                }


            }
            return retornables;
        }


        public static int Update(string sql)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }


        public static int InsertIndentity(string sql)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    sql += "; SELECT @@IDENTITY";
                    command.Connection = connection;
                    command.CommandText = sql;

                    connection.Open();
                    return int.Parse(command.ExecuteScalar().ToString());
                }
            }
        }

        #endregion

        #region Data Retrieve Handlers

        public static DataSet GetDataSet(string sql)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    using (DbDataAdapter adapter = Factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public static DataSet GetDataSet(IList<MySqlParameter> parameters, string storeProcedure)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {

                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storeProcedure;
                    if (parameters != null)
                    {
                        foreach (MySqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }

                    }


                    using (var adapter = Factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        command.CommandTimeout = 0;

                        var ds = new DataSet();
                        adapter.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public static DataSet GetDataSet(string sql, List<MySqlParameter> parameters = null)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    // Si los parametros nos son null los recorremos
                    if (parameters != null)
                    {
                        foreach (MySqlParameter param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    using (DbDataAdapter adapter = Factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public static DataTable GetDataTable(string sql)
        {
            return GetDataSet(sql).Tables[0];
        }

        public static DataTable GetDataTable(string sql, List<MySqlParameter> parameters = null)
        {
            return GetDataSet(sql, parameters).Tables[0];
        }

        public static DataTable GetDataTable(IList<MySqlParameter> parameters, string storeProcedure)
        {
            return GetDataSet(parameters, storeProcedure).Tables[0];
        }

        public static object GetScalar(IList<string[]> parameters, string storeProcedure)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storeProcedure;
                    if (parameters != null)
                    {
                        foreach (string[] parameter in parameters)
                        {
                            DbParameter spParameter = Factory.CreateParameter();
                            spParameter.ParameterName = parameter[0];
                            spParameter.Value = parameter[1];
                            command.Parameters.Add(spParameter);
                        }
                    }

                    connection.Open();
                    int count = (int)(command.ExecuteScalar());
                    return count;

                }
            }
        }

        #endregion

        #region Utility methods


        public static string Escape(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
                return "'" + s.Trim().Replace("'", "''") + "'";
        }


        public static string Escape(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
            {
                s = s.Trim();
                if (s.Length > maxLength) s = s.Substring(0, maxLength - 1);
                return "'" + s.Trim().Replace("'", "''") + "'";
            }
        }

        #endregion
    }
}
