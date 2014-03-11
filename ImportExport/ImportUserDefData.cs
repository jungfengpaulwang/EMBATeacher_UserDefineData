using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using JHSchool.Data;
using K12.Data;

namespace UserDefineData.ImportExport
{
    // 匯入自訂資料欄位
    class ImportUserDefData:SmartSchool.API.PlugIn.Import.Importer 
    {
        public ImportUserDefData()
        {

            this.Image = null;
            this.Text = "匯入自訂資料欄位(Beta)";    
        
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            // 取得教師資料
            Dictionary<string, K12.Data.TeacherRecord> Students = new Dictionary<string, K12.Data.TeacherRecord>();

            // 取得自訂資料欄位資料
            Dictionary<string,List<DAL.UserDefData>> UserDefDataDict = new Dictionary<string,List<UserDefineData.DAL.UserDefData>> ();

            // 取得使用這設定
            Dictionary<string, string> UserSetDataTypeDict = new Dictionary<string, string>();

            wizard.PackageLimit = 3000;
            wizard.ImportableFields.AddRange("欄位名稱","值");
            wizard.RequiredFields.AddRange("欄位名稱");
            wizard.ValidateStart += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                Students.Clear();
                UserDefDataDict.Clear();
                UserSetDataTypeDict.Clear();

                UserSetDataTypeDict = Global.GetUserConfigData();

                // 取得教師資料
                foreach (K12.Data.TeacherRecord studRec in K12.Data.Teacher.SelectByIDs(e.List))
                    if (!Students.ContainsKey(studRec.ID))
                        Students.Add(studRec.ID, studRec);

                // 取得自訂資料欄位                                
                MultiThreadWorker<string> loader1 = new MultiThreadWorker<string>();
                loader1.MaxThreads = 3;
                loader1.PackageSize = 250;
                loader1.PackageWorker += delegate(object sender1, PackageWorkEventArgs<string> e1)
                {
                    foreach (DAL.UserDefData udd in UDTTransfer.GetDataFromUDT(e.List.ToList<string>()))
                    {   
                        if(UserDefDataDict.ContainsKey(udd.RefID ))
                            UserDefDataDict[udd.RefID].Add(udd);
                        else
                        {
                            List<DAL.UserDefData> dd = new List<UserDefineData.DAL.UserDefData>();
                            dd.Add(udd);
                            UserDefDataDict.Add(udd.RefID,dd);
                        }
                    }
                };
                loader1.Run(e.List);
            };


            wizard.ValidateRow += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                int i = 0;                
                // 檢查學生是否存在
                K12.Data.TeacherRecord studRec = null;
                if (Students.ContainsKey(e.Data.ID))
                    studRec = Students[e.Data.ID];
                else
                {
                    e.ErrorMessage = "沒有這位教師" + e.Data.ID;
                    return;
                }

                // 驗證格式資料
                bool InputFormatPass = true;
                foreach (string field in e.SelectFields)
                {
                    string value = e.Data[field].Trim();
                    switch (field)
                    {
                        default:
                            break;

                        case "欄位名稱":
                            if (string.IsNullOrEmpty(value))
                            {
                                InputFormatPass &= false;
                                e.ErrorFields.Add(field, "不允許空白");

                            }
                            break;
                        case "值":
                            decimal dd; DateTime dt;
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (e.Data.ContainsKey("欄位名稱"))
                                {
                                    string str = e.Data["欄位名稱"];

                                    if (UserSetDataTypeDict.ContainsKey(str))
                                    {
                                        if (UserSetDataTypeDict[str] == "Number")
                                        {
                                            if (!decimal.TryParse(value, out dd))
                                            {
                                                e.ErrorFields.Add(field, "非數字型態");
                                                InputFormatPass &= false;
                                                break;
                                            }
                                        }

                                        if (UserSetDataTypeDict[str] == "Date")
                                        {
                                            if (!DateTime.TryParse(value, out dt))
                                            {
                                                e.ErrorFields.Add(field, "非日期型態");
                                                InputFormatPass &= false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }

            };

            wizard.ImportPackage += delegate(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
            { 
            
                Dictionary<string, List<RowData>> id_Rows = new Dictionary<string, List<RowData>>();
                foreach (RowData data in e.Items)
                {
                    if (!id_Rows.ContainsKey(data.ID))
                        id_Rows.Add(data.ID, new List<RowData>());
                    id_Rows[data.ID].Add(data);
                }

                List<DAL.UserDefData> InsertList = new List<UserDefineData.DAL.UserDefData> ();
                List<DAL.UserDefData> DeleteList = new List<UserDefineData.DAL.UserDefData>();


                foreach (string id in id_Rows.Keys)
                {
                    foreach (RowData data in id_Rows[id])
                    {
                        string FName = string.Empty, Value = string.Empty;
                        if (data.ContainsKey("欄位名稱"))
                            FName = data["欄位名稱"];

                        if (data.ContainsKey("值"))
                            Value = data["值"];

                        // 將需要刪除放入
                        if (UserDefDataDict.ContainsKey(id))
                        foreach (DAL.UserDefData udd in UserDefDataDict[id])
                            if (udd.FieldName == FName)
                            {
                                udd.Deleted = true;
                                DeleteList.Add(udd);
                            }

                        // 新增資料
                        DAL.UserDefData uddNew = new UserDefineData.DAL.UserDefData();
                        uddNew.FieldName = FName;
                        uddNew.RefID = id;
                        uddNew.Value = Value;
                        InsertList.Add(uddNew);
                    }
                }

                try
                {
                    // 先刪除舊的資料在新增新的
                    if(DeleteList.Count >0)
                        UDTTransfer.DeleteDataToUDT(DeleteList);

                    if (InsertList.Count > 0)
                        UDTTransfer.InsertDataToUDT(InsertList );

                    //JHSchool.Student.Instance.SyncAllBackground();
                }
                catch (Exception ex) { }
            
            };
       }
    }
}
