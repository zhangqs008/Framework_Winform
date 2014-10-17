#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Whir.Framework.DataBase.DBManager.Model;

#endregion

namespace Whir.Framework.DataBase.DBManager
{
    public class ConnectionStatus
    {
        public ConnectionStatus(bool isSucess, string message)
        {
            IsSucess = isSucess;
            Message = message;
        }

        public bool IsSucess { get; set; }
        public string Message { get; set; }
    }
    /// <summary>
    ///     数据库助手类
    /// </summary>
    public class DBManager
    {
        #region EIP公共字段

        private static readonly List<string> CommonFields = new List<string>
            {
                "State",
                "IsDel",
                "Sort",
                "CreateDate",
                "CreateUser",
                "UpdateDate",
                "UpdateUser",
                "IsEnable"
            };

        #endregion

        #region 单例

        private static DBManager _oper;

        private DBManager()
        {
        }

        public static DBManager GetInstance()
        {
            return _oper ?? (_oper = new DBManager());
        }

        #endregion

        #region 测试连接

        /// <summary>
        ///     判断连接是否成功！
        /// </summary>
        /// <param name="con"> 链接字符串</param>
        /// <param name="dataServer"></param>
        /// <returns>true 表示链接成功，false表示连接失败</returns>
        public static ConnectionStatus CheckConnection(string con, string dataServer)
        {
            ConnectionStatus connectionStatus;
            switch (dataServer.ToLower())
            {
                case ".":
                case "local":
                    connectionStatus = CheckConnection(con); //本地连接时，使用同步方式
                    break;
                default:
                    connectionStatus = CheckConnectionSync(con); //远程连接时，使用异步方式
                    break;
            }
            return connectionStatus;
        }

        /// <summary>
        ///     判断连接是否成功！
        /// </summary>
        /// <param name="con"> 链接字符串</param>
        /// <returns>true 表示链接成功，false表示连接失败</returns>
        public static ConnectionStatus CheckConnectionSync(string con)
        {
            var status = new ConnectionStatus(false, "未知异常");
            var thread = new Thread(() =>
                {
                    try
                    {
                        var connection = new SqlConnection(con);
                        connection.Open();
                        connection.Close();
                        status = new ConnectionStatus(true, "");
                    }
                    catch (Exception ex)
                    {
                        status = new ConnectionStatus(false, ex.Message);

                    }
                }) { IsBackground = true };
            Stopwatch sw = Stopwatch.StartNew();
            thread.Start();
            TimeSpan timeout = TimeSpan.FromSeconds(3);
            while (sw.Elapsed < timeout)
            {
                thread.Join(TimeSpan.FromMilliseconds(200));
            }
            sw.Stop();
            return status;
        }

        /// <summary>
        ///     判断连接是否成功！
        /// </summary>
        /// <param name="con"> 链接字符串</param>
        /// <returns>true 表示链接成功，false表示连接失败</returns>
        public static ConnectionStatus CheckConnection(string con)
        {
            ConnectionStatus status;
            var conn = new SqlConnection(con);
            try
            {
                conn.Open();
                status = new ConnectionStatus(true, "");
            }
            catch (Exception ex)
            {
                status = new ConnectionStatus(false, ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return status;
        }

        #endregion

        #region 01.获取所有数据库名

        /// <summary>
        ///     获取所有数据库名（排除系统数据库）
        /// </summary>
        /// <param name="connection">连接字符串</param>
        /// <returns>数据名称集合</returns>
        public static List<string> GetDataBaseNames(string connection)
        {
            var dbNames = new List<string>();
            const string sqlstr = "select master..sysdatabases.name from master..sysdatabases order by name";
            using (var sqlcon = new SqlConnection(connection))
            {
                var cmd = new SqlCommand(sqlstr, sqlcon);
                sqlcon.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    dbNames.Add(sdr[0].ToString());
                }
                for (int i = 0; i < dbNames.Count; i++)
                {
                    if (dbNames[i] == "master" || dbNames[i] == "msdb" ||
                        dbNames[i] == "model" || dbNames[i] == "tempdb")
                    {
                        dbNames.RemoveAt(i);
                    }
                }
            }
            return dbNames;
        }

        #endregion

        #region 02.获取指定数据库中所有表的名字

        /// <summary>
        ///     获取指定数据库中所有表的名字；
        /// </summary>
        /// <param name="dataBaseName">指定的数据库名</param>
        /// <param name="con">连接字符串</param>
        /// <returns>所有表名的集合</returns>
        public static List<string> GetTableNames(string dataBaseName, string con)
        {
            var tableNames = new List<string>();
            try
            {
                string sql =
                    "select  sysobjects.name   from   sysobjects,master..sysdatabases t  where type='U'and t.name='" +
                    dataBaseName + "' ORDER BY sysobjects.name ASC";
                var cmd = new SqlCommand();
                var sqlcon = new SqlConnection(con);
                using (sqlcon)
                {
                    cmd.Connection = sqlcon;
                    cmd.CommandText = sql;
                    sqlcon.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        tableNames.Add(sdr[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                tableNames.Add("异常：" + ex.Message);
            }
            return tableNames;
        }

        /// <summary>
        ///     获取指定数据库中所有表的名字；
        /// </summary>
        /// <param name="dataBaseName">指定的数据库名</param>
        /// <param name="con">连接字符串</param>
        /// <returns>所有表名的集合</returns>
        public static DataTable GetTables(string dataBaseName, string con)
        {
            var tableNames = new DataTable();
            tableNames.Columns.Add("表名");
            tableNames.Columns.Add("表备注");
            tableNames.Columns.Add("建表语句");
            try
            {
                string sql = @"SELECT sysobjects.name
                                    FROM   sysobjects,
                                           MASTER..sysdatabases t
                                    WHERE  TYPE = 'U'
                                           AND t.name = '{0}'
                                    ORDER BY
                                           sysobjects.name ASC";
                sql = string.Format(sql, dataBaseName);
                var cmd = new SqlCommand();
                var sqlcon = new SqlConnection(con);
                using (sqlcon)
                {
                    cmd.Connection = sqlcon;
                    cmd.CommandText = sql;
                    sqlcon.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        DataRow row = tableNames.NewRow();
                        row["表名"] = sdr[0].ToString();
                        row["表备注"] = GetTableChineseName(sdr[0].ToString(), con);
                        row["建表语句"] = GetCreateTableSql(con, sdr[0].ToString());
                        tableNames.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                DataRow row = tableNames.NewRow();
                row["表名"] = ex.Message;
                tableNames.Rows.Add(row);
            }
            return tableNames;
        }

        public static string GetCreateTableSql(string con, string tableName)
        {
            List<FieldInfo> fields = GetFieldInfoList(tableName, con);
            var creataSql = new StringBuilder();
            creataSql.AppendFormat("CREATE TABLE [dbo].[{0}](", tableName).Append(Environment.NewLine);
            foreach (FieldInfo field in fields)
            {
                string type = GetFieldTypeString(field);
                string isNull = field.IsAllowNull ? " NULL " : " NOT NULL ";
                if (field.IsPrimaryKey)
                {
                    if (field.IsFlag)
                    {
                        creataSql.AppendFormat("[" + field.Name + "] " + type + " IDENTITY(1,1) NOT NULL,")
                                 .Append(Environment.NewLine);
                    }
                    else
                    {
                        creataSql.AppendFormat("[" + field.Name + "] " + type + " PrimaryKey NOT NULL,")
                                 .Append(Environment.NewLine);
                    }
                }
                else
                {
                    creataSql.AppendFormat("[" + field.Name + "] " + type + isNull + ",").Append(Environment.NewLine);
                }
            }
            string create = creataSql.ToString().TrimEnd('\n').TrimEnd('\r').TrimEnd(',') + Environment.NewLine + ") " +
                            Environment.NewLine;
            return create;
        }

        public static string RepairTablePrimaryKey(string con, string tableName)
        {
            List<FieldInfo> fields = GetFieldInfoList(tableName, con);
            string primary = "";
            foreach (FieldInfo field in fields)
            {
                if (field.IsPrimaryKey)
                {
                    primary = string.Format("ALTER TABLE {0} ADD PRIMARY KEY({1})", tableName, field.Name);
                    break;
                }
            }
            return primary;
        }

        public static string RepairIdentityKey(string con, string tableName)
        {
            List<FieldInfo> fields = GetFieldInfoList(tableName, con);
            string primary = "";
            foreach (FieldInfo field in fields)
            {
                if (field.IsPrimaryKey)
                {
                    primary = "ALTER TABLE " + tableName + " ADD PRIMARY KEY(" + field.Name + ")";
                    break;
                }
            }
            return primary;
        }

        public static object ExecuteScalar(string sql, string con)
        {
            var cmd = new SqlCommand();
            var connection = new SqlConnection(con);
            try
            {
                using (connection)
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    connection.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string GetFieldTypeString(FieldInfo field)
        {
            string type = "";
            switch (field.Type.ToLower())
            {
                case "int":
                case "tinyint":
                case "smallint":

                case "bit":
                case "bigint":
                case "ntext":
                case "datetime":
                    type = "[" + field.Type + "]";
                    break;
                case "nvarchar":
                    type = "[" + field.Type + "] (" + field.Length + ")";
                    break;
            }
            return type;
        }

        #endregion

        #region 03.获取指定表的所有字段名

        /// <summary>
        ///     获取指定表的所有字段名；
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">连接字符串</param>
        /// <returns>所有字段名集合</returns>
        public static List<string> GetFieldNames(string tableName, string con)
        {
            var list = new List<string>();
            string sql = "Select name from syscolumns Where ID=OBJECT_ID('" + tableName + "')ORDER BY name ASC";
            var cmd = new SqlCommand();
            var connection = new SqlConnection(con);
            using (connection)
            {
                cmd.Connection = connection;
                cmd.CommandText = sql;
                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    list.Add(sdr[0].ToString());
                }
            }
            return list;
        }

        #endregion

        #region 04.获取指定表中所有字段名和类型

        /// <summary>
        ///     获取指定表中所有字段名和类型；
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">连接字符串</param>
        /// <param name="isContainCommonFields">是否包含公共字段</param>
        /// <returns>字段名和类型的集合</returns>
        public static List<FieldInfo> GetFieldInfoList(string tableName, string con, bool isContainCommonFields = true)
        {
            var itemlist = new List<FieldInfo>();
            string sql = string.Format(@"SELECT   
                                表名=CASE WHEN C.column_id=1 THEN O.name ELSE N'' END,  
                                表说明=ISNULL(CASE WHEN C.column_id=1 THEN PTB.[value] END,N''),  
                                字段序号=C.column_id,  
                                字段名=C.name,  
                                主键=ISNULL(IDX.PrimaryKey,N''),  
                                标识=CASE WHEN C.is_identity=1 THEN N'√'ELSE N'' END,  
                                计算列=CASE WHEN C.is_computed=1 THEN N'√'ELSE N'' END,  
                                类型=T.name,  
                                字节数=C.max_length,  
                                长度=COLUMNPROPERTY(C.[object_id],C.name,'precision'),  
                                小数位数=C.scale,  
                                允许空=CASE WHEN C.is_nullable=1 THEN N'√'ELSE N'' END,  
                                默认值=ISNULL(D.definition,N''),  
                                字段说明=ISNULL(PFD.[value],N''),  
                                索引=ISNULL(IDX.IndexName,N''),  
                                索引排序=ISNULL(IDX.Sort,N''),  
                                创建日期=O.Create_Date,  
                                修改日期=O.Modify_date  
                               FROM sys.columns C  
                                INNER JOIN sys.objects O  
                                    ON C.[object_id]=O.[object_id]  
                                        AND O.type='U'  
                                        AND O.is_ms_shipped=0  
                                INNER JOIN sys.types T  
                                    ON C.user_type_id=T.user_type_id  
                                LEFT JOIN sys.default_constraints D  
                                    ON C.[object_id]=D.parent_object_id  
                                        AND C.column_id=D.parent_column_id  
                                        AND C.default_object_id=D.[object_id]  
                                LEFT JOIN sys.extended_properties PFD  
                                    ON PFD.class=1   
                                        AND C.[object_id]=PFD.major_id   
                                        AND C.column_id=PFD.minor_id  
                            --             AND PFD.name='Caption'  -- 字段说明对应的描述名称(一个字段可以添加多个不同name的描述)  
                                LEFT JOIN sys.extended_properties PTB  
                                    ON PTB.class=1   
                                        AND PTB.minor_id=0   
                                        AND C.[object_id]=PTB.major_id  
                            --             AND PFD.name='Caption'  -- 表说明对应的描述名称(一个表可以添加多个不同name的描述)   
  
                                LEFT JOIN                       -- 索引及主键信息  
                                (  
                                    SELECT   
                                        IDXC.[object_id],  
                                        IDXC.column_id,  
                                        Sort=CASE INDEXKEY_PROPERTY(IDXC.[object_id],IDXC.index_id,IDXC.index_column_id,'IsDescending')  
                                            WHEN 1 THEN 'DESC' WHEN 0 THEN 'ASC' ELSE '' END,  
                                        PrimaryKey=CASE WHEN IDX.is_primary_key=1 THEN N'√'ELSE N'' END,  
                                        IndexName=IDX.Name  
                                    FROM sys.indexes IDX  
                                    INNER JOIN sys.index_columns IDXC  
                                        ON IDX.[object_id]=IDXC.[object_id]  
                                            AND IDX.index_id=IDXC.index_id  
                                    LEFT JOIN sys.key_constraints KC  
                                        ON IDX.[object_id]=KC.[parent_object_id]  
                                            AND IDX.index_id=KC.unique_index_id  
                                    INNER JOIN  -- 对于一个列包含多个索引的情况,只显示第个索引信息  
                                    (  
                                        SELECT [object_id], Column_id, index_id=MIN(index_id)  
                                        FROM sys.index_columns  
                                        GROUP BY [object_id], Column_id  
                                    ) IDXCUQ  
                                        ON IDXC.[object_id]=IDXCUQ.[object_id]  
                                            AND IDXC.Column_id=IDXCUQ.Column_id  
                                            AND IDXC.index_id=IDXCUQ.index_id  
                                ) IDX  
                                    ON C.[object_id]=IDX.[object_id]  
                                        AND C.column_id=IDX.column_id   
  
                            WHERE O.name=N'{0}'       -- 如果只查询指定表,加上此条件  
                            ORDER BY O.name,C.column_id", tableName);
            var cmd = new SqlCommand();
            var connection = new SqlConnection(con);
            using (connection)
            {
                cmd.Connection = connection;
                cmd.CommandText = sql;
                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    var item = new FieldInfo
                        {
                            Name = sdr["字段名"].ToString(),
                            Type = sdr["类型"].ToString(),
                            Length = Convert.ToInt32(sdr["长度"]),
                            IsPrimaryKey = (sdr["主键"] != null && sdr["主键"].ToString() == "√"),
                            IsAllowNull = (sdr["允许空"] != null && sdr["允许空"].ToString() == "√"),
                            Note = !string.IsNullOrEmpty(sdr["字段说明"].ToString())
                                       ? sdr["字段说明"].ToString()
                                       : sdr["字段名"].ToString(),
                            IsFlag = (sdr["标识"] != null && sdr["标识"].ToString() == "√")
                        };
                    if (!isContainCommonFields)
                    {
                        if (!CommonFields.Contains(item.Name))
                        {
                            itemlist.Add(item);
                        }
                    }
                    else
                    {
                        itemlist.Add(item);
                    }
                }
            }
            return itemlist;
        }

        #endregion

        public static DataTable GetAllInfo(string con)
        {
            string sql = string.Format(@"SELECT   
                                表名=CASE WHEN C.column_id=1 THEN O.name ELSE N'' END,  
                                表说明=ISNULL(CASE WHEN C.column_id=1 THEN PTB.[value] END,N''),  
                                字段序号=C.column_id,  
                                字段名=C.name,  
                                主键=ISNULL(IDX.PrimaryKey,N''),  
                                标识=CASE WHEN C.is_identity=1 THEN N'√'ELSE N'' END,  
                                计算列=CASE WHEN C.is_computed=1 THEN N'√'ELSE N'' END,  
                                类型=T.name,  
                                字节数=C.max_length,  
                                长度=COLUMNPROPERTY(C.[object_id],C.name,'precision'),  
                                小数位数=C.scale,  
                                允许空=CASE WHEN C.is_nullable=1 THEN N'√'ELSE N'' END,  
                                默认值=ISNULL(D.definition,N''),  
                                字段说明=ISNULL(PFD.[value],N''),  
                                索引=ISNULL(IDX.IndexName,N''),  
                                索引排序=ISNULL(IDX.Sort,N''),  
                                创建日期=O.Create_Date,  
                                修改日期=O.Modify_date  
                               FROM sys.columns C  
                                INNER JOIN sys.objects O  
                                    ON C.[object_id]=O.[object_id]  
                                        AND O.type='U'  
                                        AND O.is_ms_shipped=0  
                                INNER JOIN sys.types T  
                                    ON C.user_type_id=T.user_type_id  
                                LEFT JOIN sys.default_constraints D  
                                    ON C.[object_id]=D.parent_object_id  
                                        AND C.column_id=D.parent_column_id  
                                        AND C.default_object_id=D.[object_id]  
                                LEFT JOIN sys.extended_properties PFD  
                                    ON PFD.class=1   
                                        AND C.[object_id]=PFD.major_id   
                                        AND C.column_id=PFD.minor_id  
                            --             AND PFD.name='Caption'  -- 字段说明对应的描述名称(一个字段可以添加多个不同name的描述)  
                                LEFT JOIN sys.extended_properties PTB  
                                    ON PTB.class=1   
                                        AND PTB.minor_id=0   
                                        AND C.[object_id]=PTB.major_id  
                            --             AND PFD.name='Caption'  -- 表说明对应的描述名称(一个表可以添加多个不同name的描述)   
  
                                LEFT JOIN                       -- 索引及主键信息  
                                (  
                                    SELECT   
                                        IDXC.[object_id],  
                                        IDXC.column_id,  
                                        Sort=CASE INDEXKEY_PROPERTY(IDXC.[object_id],IDXC.index_id,IDXC.index_column_id,'IsDescending')  
                                            WHEN 1 THEN 'DESC' WHEN 0 THEN 'ASC' ELSE '' END,  
                                        PrimaryKey=CASE WHEN IDX.is_primary_key=1 THEN N'√'ELSE N'' END,  
                                        IndexName=IDX.Name  
                                    FROM sys.indexes IDX  
                                    INNER JOIN sys.index_columns IDXC  
                                        ON IDX.[object_id]=IDXC.[object_id]  
                                            AND IDX.index_id=IDXC.index_id  
                                    LEFT JOIN sys.key_constraints KC  
                                        ON IDX.[object_id]=KC.[parent_object_id]  
                                            AND IDX.index_id=KC.unique_index_id  
                                    INNER JOIN  -- 对于一个列包含多个索引的情况,只显示第个索引信息  
                                    (  
                                        SELECT [object_id], Column_id, index_id=MIN(index_id)  
                                        FROM sys.index_columns  
                                        GROUP BY [object_id], Column_id  
                                    ) IDXCUQ  
                                        ON IDXC.[object_id]=IDXCUQ.[object_id]  
                                            AND IDXC.Column_id=IDXCUQ.Column_id  
                                            AND IDXC.index_id=IDXCUQ.index_id  
                                ) IDX  
                                    ON C.[object_id]=IDX.[object_id]  
                                        AND C.column_id=IDX.column_id    
                            ORDER BY O.name,C.column_id");
            return ExcuteDataTable(sql, con);
        }

        public static DataTable ExcuteDataTable(string sql, string con)
        {
            var cmd = new SqlCommand();
            var connection = new SqlConnection(con);
            try
            {
                using (connection)
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    connection.Open();
                    var da = new SqlDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                var dt = new DataTable();
                dt.Columns.Add("异常信息");
                DataRow row = dt.NewRow();
                row["异常信息"] = ex.Message;
                dt.Rows.Add(row);
                return dt;
            }
        }

        /// <summary>
        ///     获取单个字段的类型
        /// </summary>
        /// <param name="fieldList">字段集合</param>
        /// <param name="fieldName">字段名</param>
        /// <returns>字段类型</returns>
        public static string GetFieldType(List<FieldInfo> fieldList, string fieldName)
        {
            string reuslt = "";
            foreach (FieldInfo item in fieldList)
            {
                if (fieldName == item.Name)
                {
                    reuslt = item.Type;
                    break;
                }
            }
            return reuslt;
        }

        /// <summary>
        ///     获取单个字段的类型DBType形式
        /// </summary>
        /// <param name="fieldList">字段集合</param>
        /// <param name="fieldName">字段名</param>
        /// <returns>字段类型</returns>
        public static string GetFieldTypeDBType(List<FieldInfo> fieldList, string fieldName)
        {
            const string reuslt = "";
            foreach (FieldInfo item in fieldList)
            {
                if (fieldName == item.Name)
                {
                    return FieldTypeConverter.SqlToDbType(item.Type);
                }
            }
            return reuslt;
        }

        /// <summary>
        ///     获取字段供Dal层添加参数使用，形如：@Parms=parms
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">链接字符串</param>
        /// <returns></returns>
        public static string GetFieldsForParmas(string tableName, string con)
        {
            var builder = new StringBuilder();
            string sqlstr = "Select name from syscolumns Where ID=OBJECT_ID('" + tableName + "') ORDER BY name ASC";
            var connection = new SqlConnection(con);
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sqlstr };
                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    builder.Append("@" + sdr[0] + ",");
                }
            }
            return builder.ToString().TrimEnd(',');
        }


        /// <summary>
        ///     获取数据表名的注释,没注释，返回英文
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">数据库连接</param>
        /// <returns>数据表名注释</returns>
        public static string GetTableChineseName(string tableName, string con)
        {
            string tableChineseName;
            string sql =
                @"select top 1 value as TableName from sys.extended_properties  where major_id=(select object_id from sys.tables where name='" +
                tableName + "')";
            var connection = new SqlConnection(con);
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sql };

                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.Read())
                {
                    string str = sdr["TableName"].ToString();
                    if (str.Contains("表"))
                    {
                        str = str.Remove(str.IndexOf('表'));
                    }
                    tableChineseName = str;
                }
                else
                {
                    tableChineseName = tableName;
                }
            }
            return tableChineseName;
        }

        /// <summary>
        ///     根据表名和字段名获取字段的注释
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="field">字段名</param>
        /// <param name="con">数据连接</param>
        /// <returns>字段注释</returns>
        public static string GetFieldChineseName(string tableName, string field, string con)
        {
            string sql = @"select (select value from sys.extended_properties as ex where ex.major_id = c.object_id and ex.minor_id = c.column_id) as ChineseField 
                                from  sys.columns as c inner join sys.tables as ta on c.object_id=ta.object_id inner join (select name,system_type_id from sys.types where name<>'sysname') as t on c.system_type_id=t.system_type_id 
                                where  ta.name='" + tableName + "' and c.name='" + field +
                         "' order by c.column_id ";
            var connection = new SqlConnection(con);
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sql };

                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    if (sdr["ChineseField"].ToString() == "")
                    {
                        return "[**该字段没有注释**]";
                    }
                    return sdr["ChineseField"].ToString();
                }
            }
            return null;
        }

        /// <summary>
        ///     获取单个字段信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static FieldInfo GetFieldInfo(string tableName, string fieldName, string conn)
        {
            var field = new FieldInfo();
            string sql =
                string.Format(@"select ta.[name] AS '表名',c.[name] as '字段名',ex.[value]as '字段注释',t.[name] as '字段类型',
	                                c.max_length AS'长度'
	                                from sys.extended_properties as ex ,sys.columns as c 
	                                inner join sys.tables as ta on c.object_id=ta.object_id
	                                inner join 
                                    (select name,system_type_id from sys.types where name<>'sysname') as t on c.system_type_id=t.system_type_id 
	                                where ex.major_id = c.object_id and ex.minor_id = c.column_id	
	                                AND ta.[name]='{0}' AND c.[name]='{1}'", tableName,
                              fieldName);
            var connection = new SqlConnection(conn);
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sql };

                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    field.Name = sdr["字段名"].ToString();
                    field.Note = sdr["字段注释"].ToString();
                    field.Type = sdr["字段类型"].ToString();
                    field.Length = int.Parse(sdr["长度"].ToString());
                }
            }
            return field;
        }

        /// <summary>
        ///     根据字段名查找字段长度；
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="con">连接字符串</param>
        /// <returns>字段长度</returns>
        public static string GetFieldLength(string tableName, string fieldName, string con)
        {
            string sql = @"select CHARACTER_MAXIMUM_LENGTH as leng from information_schema.columns where table_name='" +
                         tableName + "' and column_name = '" + fieldName + "'";
            var sqlcon = new SqlConnection(con);
            using (sqlcon)
            {
                var cmd = new SqlCommand { Connection = sqlcon, CommandText = sql };

                sqlcon.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    if (sdr["leng"].ToString() == "")
                    {
                        return null;
                    }
                    if (sdr["leng"].ToString() == "-1")
                    {
                        return "(Max)";
                    }
                    return "(" + sdr["leng"] + ")";
                }
            }
            return null;
        }

        /// <summary>
        ///     获取对应字段的第一条非空值
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="con">连接字符串</param>
        /// <returns>第一条该字段的非空值</returns>
        public static string GetFirstNotNullFieldValue(string tableName, string fieldName, string con)
        {
            string fieldValue = string.Empty;
            try
            {
                string sql = string.Format("select top 1 {0} from {1} where {0} !=''", fieldName, tableName);
                var connection = new SqlConnection(con);
                using (connection)
                {
                    var cmd = new SqlCommand { Connection = connection, CommandText = sql };

                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        if (sdr[fieldName].ToString() != "")
                        {
                            fieldValue = sdr[fieldName].ToString();
                        }
                    }
                }
            }
            catch
            {
                fieldValue = "";
            }
            return fieldValue;
        }

        /// <summary>
        ///     ExcuteSql
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="conn">连接字符串</param>
        /// <returns>第一条该字段的非空值</returns>
        public static DataSet ExcuteSql(string sql, string conn)
        {
            var ds = new DataSet();
            try
            {
                var connection = new SqlConnection(conn);
                using (connection)
                {
                    var cmd = new SqlCommand { Connection = connection, CommandText = sql };

                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        string data = string.Empty;
                        for (int i = 0; i < sdr.FieldCount; i++)
                        {
                            data += sdr[i].ToString();
                        }
                        ds.Tables[0].Rows.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                var dt = new DataTable();
                dt.Columns.Add("异常");
                DataRow row = dt.NewRow();
                row["异常"] = ex.Message;
                dt.Rows.Add(row);
                ds.Tables.Add(dt);
            }
            return ds;
        }

        /// <summary>
        ///     ExcuteSql
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="conn">连接字符串</param>
        /// <returns>第一条该字段的非空值</returns>
        public static bool ExecuteNonQuery(string sql, string conn)
        {
            int flag;
            try
            {
                var connection = new SqlConnection(conn);
                using (connection)
                {
                    var cmd = new SqlCommand { CommandTimeout = 0, Connection = connection, CommandText = sql };
                    connection.Open();
                    flag = cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                flag = 0;
            }
            return flag > 0;
        }


        /// <summary>
        ///     取得数据库所有存储过程
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<ProcedureInfo> GetProcedures(string conn)
        {
            var procedures = new List<ProcedureInfo>();
            try
            {
                const string sql = @"SELECT su.name,
                                               so.name AS PName,
                                               sc.text AS [Sql]
                                        FROM   sysobjects so,
                                               syscomments sc,
                                               sysusers su
                                        WHERE  xtype = 'P'
                                               AND so.id = sc.id
                                               AND so.uid = su.uid
                                        ORDER BY
                                               su.name,
                                               so.name,
                                               sc.colid";
                var connection = new SqlConnection(conn);
                using (connection)
                {
                    var cmd = new SqlCommand { Connection = connection, CommandText = sql };
                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        string name = sdr["PName"].ToString();
                        string text = sdr["Sql"].ToString();
                        procedures.Add(new ProcedureInfo { Name = name, Sql = text });
                    }
                }
            }
            catch
            {
                procedures = new List<ProcedureInfo>();
            }
            return procedures;
        }

        /// <summary>
        ///     取得数据库所有存储过程(字典值字段长度有限制，脚本不能放在字典值里)
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Dictionary<string, ProcedureInfo> GetProceduresDic(string conn)
        {
            var procedures = new Dictionary<string, ProcedureInfo>();
            try
            {
                const string sql = @"SELECT su.name,
                                               so.name AS PName,
                                               sc.text AS [Sql]
                                        FROM   sysobjects so,
                                               syscomments sc,
                                               sysusers su
                                        WHERE  xtype = 'P'
                                               AND so.id = sc.id
                                               AND so.uid = su.uid
                                        ORDER BY
                                               su.name,
                                               so.name,
                                               sc.colid";
                var connection = new SqlConnection(conn);
                using (connection)
                {
                    var cmd = new SqlCommand { Connection = connection, CommandText = sql };
                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        string name = sdr["PName"].ToString();
                        string text = sdr["Sql"].ToString();
                        if (!procedures.ContainsKey(name))
                        {
                            procedures.Add(name, new ProcedureInfo { Name = name, Sql = text });
                        }
                    }
                }
            }
            catch
            {
                procedures = new Dictionary<string, ProcedureInfo>();
            }
            return procedures;
        }

        /// <summary>
        ///     获取表的添加语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">链接字符串</param>
        /// <returns></returns>
        public static string GetInsertSql(string tableName, string con)
        {
            var builder = new StringBuilder();
            builder.Append("const string sqlStr=\"INSERT INTO " + tableName);
            string sqlstr = "Select name from syscolumns Where ID=OBJECT_ID('" + tableName + "') ORDER BY name ASC";
            var connection = new SqlConnection(con);
            var fields = new StringBuilder();
            var parms = new StringBuilder();
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sqlstr };
                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    fields.Append(sdr[0] + ",");
                    parms.Append("@" + sdr[0] + ",");
                }
            }
            builder.Append("(" + fields.ToString().TrimEnd(',') + ")");
            builder.Append(" Values ");
            builder.Append("(" + parms.ToString().TrimEnd(',') + ")\";");
            return builder.ToString().TrimEnd(',');
        }

        /// <summary>
        ///     获取表的更新语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="con">链接字符串</param>
        /// <returns></returns>
        public static string GetUpdateSql(string tableName, string con)
        {
            var builder = new StringBuilder();
            builder.Append("const string sqlStr=\"UPDATE " + tableName);
            string sqlstr = "Select name from syscolumns Where ID=OBJECT_ID('" + tableName + "') ORDER BY name ASC";
            var connection = new SqlConnection(con);
            var parms = new StringBuilder();
            using (connection)
            {
                var cmd = new SqlCommand { Connection = connection, CommandText = sqlstr };
                connection.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    parms.Append(sdr[0] + "=@" + sdr[0] + ",");
                }
            }
            builder.Append(" SET ");
            builder.Append(parms.ToString().TrimEnd(','));
            builder.Append(" WHERE Id=@Id");
            return builder.ToString().TrimEnd(',') + "\";";
        }

        /// <summary>
        /// 需过滤掉的关键字
        /// </summary>
        public static string[] ForbidSqlKeyword = { "ALTER", "UPDATE", "DELETE", "DROP", "CREATE", "COMMIT" };
        public static bool IsContainDangerWords(string sql)
        {
            bool haveForbidWord = false;
            sql = sql.ToLower().Replace("\r", " ").Replace("\n", " ");
            string[] sqlWordArr = sql.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in sqlWordArr)
            {
                foreach (string forbidWord in ForbidSqlKeyword)
                {
                    if (String.Compare(forbidWord.Trim(), word.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        haveForbidWord = true;
                        break;
                    }
                }
            }
            return haveForbidWord;
        }

        #region 执行sql语句返回htmlTable

        /// <summary>
        ///  根据查询语句得到Html表格
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string SelectData(string sql, string conn)
        {
            return GetHtmlTableFromDataSet(SelectDataSet(sql, conn));
        }

        /// <summary>
        /// 根据查询语句得到DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DataSet SelectDataSet(string sql, string conn)
        {
            var ds = new DataSet();
            try
            {
                ds = GetDataSet(sql, conn);
            }
            catch (Exception ex)
            {
                var table = new DataTable();
                table.Columns.Add("异常信息");
                table.Rows.Add(ex.Message);
                ds.Tables.Add(table);
            }
            return ds;
        }
        private static DataSet GetDataSet(string sqlCommand, string connectionString)
        {
            var ds = new DataSet();
            using (var cmd = new SqlCommand(sqlCommand, new SqlConnection(connectionString)))
            {
                cmd.Connection.Open();
                var table = new DataTable();
                table.Load(cmd.ExecuteReader());
                ds.Tables.Add(table);
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();

            }
            return ds;
        }
        /// <summary>
        /// 将DataSet转换为Html表格
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private static string GetHtmlTableFromDataSet(DataSet ds)
        {
            var html = new StringBuilder();
            if (ds.Tables.Count == 0)
            {
                html.Append(
                    @"<table class='data'  cellspacing='1' cellpadding='0' border='1' style='width: 100%;'>
                                                            <tbody>
                                                                <tr class='tdbg' align='center' style='height: 100px;'>
                                                                    <td colspan='12'>
                                                                        没有任何记录！
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>");
            }
            else
            {
                DataTable dt = ds.Tables[0];

                #region 表头行

                html.Append(@"<table class='data' cellspacing='1' cellpadding='0' border='1'  style='width: 100%;'>");
                html.Append(@"<tr class='gridtitle'>");
                foreach (DataColumn columns in dt.Columns)
                {
                    string title = columns.ColumnName;
                    html.Append(string.Format(@"<th scope='col' class='data_th'>{0}</th>", title));
                }
                html.Append(@"</tr>");

                #endregion

                #region 表数据行

                foreach (DataRow row in dt.Rows)
                {
                    html.Append(string.Format("<tr class='data_tr'>"));
                    foreach (DataColumn columns in dt.Columns)
                    {
                        string cellValue = row[columns.ColumnName].ToString();
                        html.Append(string.Format("<td class='data_td'>{0}</td>", cellValue));
                    }
                    html.Append(@"</tr>");
                }

                #endregion

                #region 表尾

                html.Append(string.Format("<tr class='data_tr'>"));
                html.Append(string.Format("<td  class='data_td' colspan='{0}'>合计：{1}条记录</td>", dt.Columns.Count, dt.Rows.Count));
                html.Append(@"</tr>");
                html.Append(@"</table>");

                #endregion
            }
            return html.ToString();
        }
        #endregion
    }
}