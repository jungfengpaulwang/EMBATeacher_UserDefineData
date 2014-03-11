using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;

namespace UserDefineData.ImportExport
{
    // 匯出自訂資料欄位
    class ExportUserDefData:SmartSchool.API.PlugIn.Export.Exporter 
    {
        List<string> ExportItemList;
        
        public ExportUserDefData()
        {
            this.Image = null;
            this.Text = "匯出自訂資料欄位(Beta)";
            ExportItemList = new List<string>();
            ExportItemList.Add("欄位名稱");
            ExportItemList.Add("值");        
        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange(ExportItemList);
            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                int RowCount = 0;
                foreach (DAL.UserDefData udd in UDTTransfer.GetDataFromUDT(e.List))
                {
                    RowData row = new RowData();
                    row.ID = udd.RefID;

                    foreach (string field in e.ExportFields)
                    {
                        if (wizard.ExportableFields.Contains(field))
                        {
                            switch (field)
                            {
                                case "欄位名稱": row.Add(field, udd.FieldName); break;
                                case "值": row.Add(field, udd.Value); break;
                            }
                        }
                    
                    }
                    RowCount++;
                    e.Items.Add(row);                
                }
            };
        }
    }
}
