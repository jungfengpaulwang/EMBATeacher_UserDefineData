using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FISCA.UDT;

namespace UserDefineData
{
    public class UDTTransfer
    {
        /// <summary>
        /// 取得單筆學生UDT資料
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static List<DAL.UserDefData> GetDataFromUDT(string ID)
        {       
            AccessHelper accHelper = new AccessHelper();
            string query = "RefID='" + ID+"'";
            return accHelper.Select<DAL.UserDefData>(query);        
        }

        /// <summary>
        /// 取得單筆學生UDT資料，並轉換成 dic
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Dictionary<string, DAL.UserDefData> GetDataFromUDTDict(string ID)
        {
            // 回傳資料
            Dictionary<string, DAL.UserDefData> retValue = new Dictionary<string, UserDefineData.DAL.UserDefData>();

            // 刪除可能多餘資料
            List<DAL.UserDefData> DeleteList = new List<UserDefineData.DAL.UserDefData>();

            // 取得 UDT 內
            foreach (DAL.UserDefData ud in GetDataFromUDT(ID))
                if (!retValue.ContainsKey(ud.FieldName))
                {
                    ud.isNull = false;
                    retValue.Add(ud.FieldName, ud);
                }
                else
                {
                    ud.Deleted = true;
                    DeleteList.Add(ud);                    
                }

            if (DeleteList.Count > 0)
                DeleteDataToUDT(DeleteList);

            // 取得自訂欄位設定，沒有使用空白
            foreach (string FName in Global.GetUserConfigData().Keys)
                if (!retValue.ContainsKey(FName))
                {
                    DAL.UserDefData ud = new UserDefineData.DAL.UserDefData();
                    ud.RefID = ID;
                    ud.FieldName = FName;
                    ud.Value = "";                    
                    retValue.Add(FName, ud);
                }
            return retValue;
        }

        /// <summary>
        /// 取得多筆學生UDT資料
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <returns></returns>
        public static List<DAL.UserDefData> GetDataFromUDT(List<string> IDList)
        {
            AccessHelper accHelper = new AccessHelper();
            string query = "RefID in ('" + String.Join("','", IDList.ToArray()) + "')";
            return accHelper.Select<DAL.UserDefData>(query);            
        }

        /// <summary>
        /// 新增資料到 UDT
        /// </summary>
        /// <param name="?"></param>
        public static void InsertDataToUDT(List<DAL.UserDefData> data)
        {
            AccessHelper accHelper = new AccessHelper();
            accHelper.InsertValues(data.ToArray());        
        }

        /// <summary>
        /// 刪除 UDT 內資料
        /// </summary>
        /// <param name="data"></param>
        public static void DeleteDataToUDT(List<DAL.UserDefData> data)
        {
            AccessHelper accHelper = new AccessHelper();
            accHelper.DeletedValues(data.ToArray());        
        }

    }
}
