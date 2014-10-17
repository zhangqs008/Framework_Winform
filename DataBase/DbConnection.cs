using Whir.Framework.Helper;

namespace Whir.Framework.DataBase
{
    /// <summary>
    ///     数据库连接
    /// </summary>
    public class DbConnection
    {
        /// <summary>
        ///     编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     连接名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     数据库服务器地址
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     数据库
        /// </summary>
        public string DataBase { get; set; }

        /// <summary>
        ///     用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     取得连接字符串
        /// </summary>
        /// <returns></returns>
        public string GetConnection()
        {
            return string.Format("Server={0};DataBase={1};User={2};Pwd={3};", Server, DataBase, UserName,
                               DESCrypt.Decrypt(Password));
        }
    }
}