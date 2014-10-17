using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Whir.Framework.Extension
{
    public static class DataTableExtension
    {
        /// <summary>
        ///     DataTable转Json文本
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ToJson(this DataTable dataTable)
        {
            var javaScriptSerializer = new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
            var arrayList = new ArrayList();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Dictionary<string, object> dictionary =
                    dataTable.Columns.Cast<DataColumn>().ToDictionary<DataColumn, string, object>(
                        dataColumn => dataColumn.ColumnName, dataColumn => dataRow[dataColumn.ColumnName].ToStr());
                //实例化一个参数集合
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }
            return javaScriptSerializer.Serialize(arrayList);
        }

        /// <summary>
        ///     DataTable转Xml字符串
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ToXml(this DataTable dataTable)
        {
            dataTable.TableName = dataTable.TableName.IsEmpty() ? "data" : dataTable.TableName;
            using (var sw = new StringWriter())
            {
                var xz = new XmlSerializer(typeof (DataTable));
                xz.Serialize(sw, dataTable);
                return sw.ToString();
            }
        }
    }
}