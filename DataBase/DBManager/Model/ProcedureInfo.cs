namespace Whir.Framework.DataBase.DBManager.Model
{
    /// <summary>
    ///     存储过程实体
    /// </summary>
    public class ProcedureInfo
    {
        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     脚本
        /// </summary>
        public string Sql { get; set; }
    }
}