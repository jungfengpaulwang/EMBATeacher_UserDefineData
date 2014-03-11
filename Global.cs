using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UserDefineData
{
    /// <summary>
    /// 取得公用全域資料與工具
    /// </summary>
    class Global
    {

        /// <summary>
        /// 設定畫面選項
        /// </summary>
        public static Dictionary<string, string> _SelectItemList = new Dictionary<string, string>();

        /// <summary>
        /// 啟動預設值
        /// </summary>
        public static void StartDefaultValue()
        {         
            _SelectItemList.Add("文字", "String");
            _SelectItemList.Add("數字", "Number");
            _SelectItemList.Add("日期", "Date");        
        }

        /// <summary>
        /// XMLString轉成 Dictionary<string, string>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> XMLToDictP1(string XmlDataStr)
        {
            // 如果重複FieldName 重複跳過
            Dictionary<string, string> retValue = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(XmlDataStr))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(XmlDataStr);

                XmlElement elms = doc.SelectSingleNode("UserConfigData") as XmlElement;

                if (elms != null)
                    foreach (XmlElement xe in elms)
                        if (!retValue.ContainsKey(xe.GetAttribute("FieldName")))
                            retValue.Add(xe.GetAttribute("FieldName"), xe.GetAttribute("FieldType"));
            }
            return retValue;
        }


        /// <summary>
        /// 取得系統內設定自訂欄位資料項目與型態
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetUserConfigData()
        {
            K12.Data.Configuration.ConfigData cd = K12.Data.School.Configuration["Teacher_ischoolUserDefineData"];
            return XMLToDictP1(cd["UserConfigData"]);
        }


        /// <summary>
        /// Dictionary<string, string>轉成 XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DictToXMLP1(Dictionary<string, string> data)
        {
            XmlElement elm = new XmlDocument().CreateElement("UserConfigData");
            foreach (KeyValuePair<string, string> value in data)
            {
                XmlElement elmData = elm.OwnerDocument.CreateElement("Data");
                elmData.SetAttribute("FieldName", value.Key);
                elmData.SetAttribute("FieldType", value.Value);
                elm.AppendChild(elmData);
            }
            return elm.OuterXml;
        }
    }
}
