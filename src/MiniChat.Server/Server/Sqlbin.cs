using System.Data.SqlClient;
using System.Data;
using MiniChat.Transmitting;

namespace MiniChat.Server.Server
{
    public static class Sqlbin
    {
        /// <summary>
        /// SQL Server数据库连接字符串
        /// </summary>
        public static string SQLServerString;

        /// <summary>
        /// 异步的校验SQL Server数据库是否可连接。如果连接成功返回1，连接失败返回-255
        /// </summary>
        public static async Task<int> ValidationConnectionStateAsync()
        {
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = new SqlConnection(SQLServerString);
                await sqlConnection.OpenAsync();
                return 1;
            }
            catch (Exception ex)
            {
                WriteErrorInfo(ex);
                return -255;
            }
            finally
            {
                sqlConnection?.Close();
                sqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// 异步的执行T-SQL命令，并返回受到影响的行数。如果执行失败则返回-255
        /// </summary>
        /// <param name="cmdText">T-SQL命令</param>
        /// <param name="parameters">T-SQL命令参数</param>
        public static async Task<int> ExecuteNonQueryAsync(string cmdText, SqlParameter[] parameters = null)
        {
            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            try
            {
                sqlConnection = new SqlConnection(SQLServerString);
                sqlCommand = new SqlCommand(cmdText, sqlConnection);
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                await sqlConnection.OpenAsync();
                int result = await sqlCommand.ExecuteNonQueryAsync();
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorInfo(ex);
                return -255;
            }
            finally
            {
                sqlConnection?.Close();
                sqlConnection?.Dispose();
                sqlCommand?.Dispose();
            }
        }

        /// <summary>
        /// 异步的执行T-SQL命令，并返回查询到的第一行第一列的数据。如果查询结果为空返回null，执行失败返回-255
        /// </summary>
        /// <param name="cmdText">T-SQL命令</param>
        /// <param name="parameters">T-SQL命令参数</param>
        public static async Task<object> ExecuteScalarAsync(string cmdText, SqlParameter[] parameters = null)
        {
            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            try
            {
                sqlConnection = new SqlConnection(SQLServerString);
                sqlCommand = new SqlCommand(cmdText, sqlConnection);
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                await sqlConnection.OpenAsync();
                object result = await sqlCommand.ExecuteScalarAsync();
                return result ?? null;
            }
            catch (Exception ex)
            {
                WriteErrorInfo(ex);
                return -255;
            }
            finally
            {
                sqlConnection?.Close();
                sqlConnection?.Dispose();
                sqlCommand?.Dispose();
            }
        }

        /// <summary>
        /// 异步的执行T-SQL命令，并返回一个数据表。如果执行失败返回null
        /// </summary>
        /// <param name="cmdText">T-SQL命令</param>
        /// <param name="tableName">数据表名称</param>
        /// <param name="parameters">T-SQL命令参数</param>
        public static async Task<DataTable> GetDataTableAsync(string cmdText, string tableName, SqlParameter[] parameters = null)
        {
            return await Task.Run<DataTable>(() =>
            {
                SqlConnection sqlConnection = null;
                SqlCommand sqlCommand = null;
                SqlDataAdapter sqlDataAdapter = null;
                try
                {
                    sqlConnection = new SqlConnection(SQLServerString);
                    sqlCommand = new SqlCommand(cmdText, sqlConnection);
                    sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    if (parameters != null)
                    {
                        sqlCommand.Parameters.AddRange(parameters);
                    }
                    DataTable dataTable = new DataTable(tableName);
                    sqlDataAdapter.Fill(dataTable);
                    return dataTable;
                }
                catch (Exception ex)
                {
                    WriteErrorInfo(ex);
                    return null;
                }
                finally
                {
                    sqlConnection?.Dispose();
                    sqlCommand?.Dispose();
                    sqlDataAdapter?.Dispose();
                }
            });
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="exception">异常类型</param>
        private static void WriteErrorInfo(Exception exception)
        {
            Serverbin.OutputErrorLog(exception, "数据库错误");
        }
    }
}
