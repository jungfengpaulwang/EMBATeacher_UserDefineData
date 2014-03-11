using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Cells;

namespace UserDefineData.ImportExport
{
    class ImportUtil
    {
        /// <summary>
        /// 取得欄位驗證字串
        /// </summary>
        /// <returns></returns>
        public static string GetChekcDataStr(int idx, Worksheet wst, Dictionary<string, int> ColIndexDic)
        {
            string chkStr = string.Empty;
            //if (ColIndexDic.ContainsKey("教師系統編號"))
            //    chkStr += wst.Cells[idx, ColIndexDic["教師系統編號"]].StringValue;

            if (ColIndexDic.ContainsKey("姓名"))
                chkStr += wst.Cells[idx, ColIndexDic["姓名"]].StringValue;

            if (ColIndexDic.ContainsKey("暱稱"))
                chkStr += wst.Cells[idx, ColIndexDic["暱稱"]].StringValue;

            if (ColIndexDic.ContainsKey("欄位名稱"))
                chkStr += wst.Cells[idx, ColIndexDic["欄位名稱"]].StringValue;

            return chkStr;
        }
    }
}
