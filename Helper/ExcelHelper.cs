using System.Data;
using System.IO;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;

namespace Whir.Framework.Helper
{
    public class ExcelHelper
    {
        public static string BasePath
        {
            get { return VirtualPathUtility.AppendTrailingSlash(HttpContext.Current.Request.ApplicationPath); }
        }

        /// <summary>
        /// 将DataTable导出到Excel，返回文件的web路径
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string ExportDataTableToExcel(DataTable dt, string fileName)
        {
            #region 表头

            var hssfworkbook = new HSSFWorkbook();
            HSSFSheet hssfSheet = hssfworkbook.CreateSheet(fileName);
            hssfSheet.DefaultColumnWidth = 20;
            hssfSheet.SetColumnWidth(0, 35*256);
            hssfSheet.SetColumnWidth(3, 20*256);
            // 表头
            HSSFRow tagRow = hssfSheet.CreateRow(0);
            tagRow.Height = 22*20;

            // 标题样式
            HSSFCellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.Alignment = HSSFCellStyle.ALIGN_CENTER;
            cellStyle.VerticalAlignment = HSSFCellStyle.VERTICAL_CENTER;
            cellStyle.BorderBottom = HSSFCellStyle.BORDER_THIN;
            cellStyle.BorderBottom = HSSFCellStyle.BORDER_THIN;
            cellStyle.BottomBorderColor = HSSFColor.BLACK.index;
            cellStyle.BorderLeft = HSSFCellStyle.BORDER_THIN;
            cellStyle.LeftBorderColor = HSSFColor.BLACK.index;
            cellStyle.BorderRight = HSSFCellStyle.BORDER_THIN;
            cellStyle.RightBorderColor = HSSFColor.BLACK.index;
            cellStyle.BorderTop = HSSFCellStyle.BORDER_THIN;
            cellStyle.TopBorderColor = HSSFColor.BLACK.index;
            //NPOI.SS.UserModel.Font font = hssfworkbook.CreateFont();
            //font.Boldweight = 30 * 20;
            //font.FontHeight = 12 * 20;
            //cellStyle.SetFont(font);

            int colIndex;
            for (colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
            {
                tagRow.CreateCell(colIndex).SetCellValue(dt.Columns[colIndex].ColumnName);
                tagRow.GetCell(colIndex).CellStyle = cellStyle;
            }

            #endregion

            #region 表数据

            // 表数据 
            for (int k = 0; k < dt.Rows.Count; k++)
            {
                DataRow dr = dt.Rows[k];
                HSSFRow row = hssfSheet.CreateRow(k + 1);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(dr[i].ToString());
                    row.GetCell(i).CellStyle = cellStyle;
                }
            }

            #endregion

            string path = HttpContext.Current.Server.MapPath("~/UploadFiles/Export");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var file = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                hssfworkbook.Write(file);
                file.Close();
            }
            return string.Format("{0}UploadFiles/Export/{1}", BasePath, fileName);
        }
    }
}