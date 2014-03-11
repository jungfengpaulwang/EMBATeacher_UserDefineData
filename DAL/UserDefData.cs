using System;
using System.Collections.Generic;
using System.Text;
using FISCA.UDT;

namespace UserDefineData.DAL
{
    // 使用者自訂資料欄位
    [TableName("Tea.UserDefineData")]
    public class UserDefData:ActiveRecord 
    {
        public enum DataType {String,Number,DateTime};

        /// <summary>
        /// 資料ID
        /// </summary>
        [Field(Field = "ID", Indexed = true)]
        public string ID { get; set; }
        
        /// <summary>
        /// 欄位名稱
        /// </summary>
        [Field(Field = "FieldName", Indexed = false)]
        public string FieldName { get; set; }

        /// <summary>
        /// 資料型態
        /// </summary>
        [Field(Field = "Type", Indexed = false)]
        public DataType Type { get; set; }

        /// <summary>
        /// 資料值
        /// </summary>
        [Field(Field = "Value", Indexed = false)]
        public string Value { get; set; }

        /// <summary>
        /// 參照ID(ex.StudentID)
        /// </summary>
        [Field(Field = "RefID", Indexed = false)]
        public string RefID { get; set; }

        /// <summary>
        /// 檢查是否空的
        /// </summary>
        public bool isNull = true ;

        public UserDefData Clone()
        {
            return (MemberwiseClone() as UserDefData);
        }
    }
}
