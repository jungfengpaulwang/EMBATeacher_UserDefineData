using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation;
//using Framework;
using FCode = FISCA.Permission.FeatureCodeAttribute;
using JHSchool.Data;
using Campus.Windows;


namespace UserDefineData
{
    // 自訂資料欄位
    [FCode("Teacher.Student.UserDefineData", "自訂資料欄位(Beta)")]
    public partial class UserDefineDataItem : DetailContent
    {

        private BackgroundWorker _BGWorker;
        private ChangeListen ChangeManager = new ChangeListen();
        private bool _isBusy = false;
        // 放待刪除資料
        private List<DAL.UserDefData> _DeleteDataList;
        // 放待新新增資料
        private List<DAL.UserDefData> _InsertDataList;

        private Dictionary<string, string> _UseDefineDataType;
        private Dictionary<string,DAL.UserDefData> _UserDefDataDict;
        private List<string> _CheckSameList;


        
        PermRecLogProcess prlp;
        public UserDefineDataItem()
        {
            InitializeComponent();
            _DeleteDataList = new List<UserDefineData.DAL.UserDefData>();
            _InsertDataList = new List<UserDefineData.DAL.UserDefData>();
            _UserDefDataDict = new Dictionary<string, UserDefineData.DAL.UserDefData>();
            _CheckSameList = new List<string>();
            // 取得使用者設定欄位型態
            _UseDefineDataType = Global.GetUserConfigData();
            prlp = new PermRecLogProcess();
            Group = "教師自訂資料欄位";
            _BGWorker = new BackgroundWorker();
            _BGWorker.DoWork += new DoWorkEventHandler(_BGWorker_DoWork);
            _BGWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWorker_RunWorkerCompleted);
            ChangeManager.Add(new DataGridViewSource(dgv));
            ChangeManager.StatusChanged += delegate(object sender, ChangeEventArgs e)
            {
                this.CancelButtonVisible = (e.Status == ValueStatus.Dirty);
                this.SaveButtonVisible = (e.Status == ValueStatus.Dirty);
            };
        }

        void _BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_isBusy)
            {
                _isBusy = false;
                _BGWorker.RunWorkerAsync();
                return;
            }
            DataBindToDataGridView();
        }

        void _BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _UserDefDataDict = UDTTransfer.GetDataFromUDTDict(PrimaryKey);
        }

        /// <summary>
        /// 將讀取資料填入DataGridView
        /// </summary>
        private void DataBindToDataGridView()
        {
            try
            {
                this.Loading = true;
                ChangeManager.SuspendListen();
                dgv.Rows.Clear();
                _DeleteDataList.Clear();
                
                int rowIdx=0;
                foreach (KeyValuePair<string,DAL.UserDefData> udd in _UserDefDataDict)
                {
                    dgv.Rows.Add();
                    if (string.IsNullOrEmpty(udd.Value.ID))
                        udd.Value.ID = udd.Value.RefID + rowIdx;

                    dgv.Rows[rowIdx].Tag = udd.Value;
                    dgv.Rows[rowIdx].Cells[FieldName.Index].Value = udd.Key;
                    dgv.Rows[rowIdx].Cells[Value.Index].Value = udd.Value.Value;
                    udd.Value.Deleted = true;
                    
                    // 當有資料才放入刪除
                    if(udd.Value.isNull==false)
                        _DeleteDataList.Add(udd.Value);

                    prlp.SetBeforeSaveText(udd.Key+"欄位名稱",udd.Key);
                    prlp.SetBeforeSaveText(udd.Key+"值", udd.Value.Value);
                   


                    rowIdx++;
                }
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show(ex.Message);
            }

            this.CancelButtonVisible = false;
            this.SaveButtonVisible = false;
            this.ContentValidated = true;

            ChangeManager.Reset();
            ChangeManager.ResumeListen();
            this.Loading = false;
            //dgv.Columns[FieldName.Index].ReadOnly = true;
        }

        /// <summary>
        /// 按下儲存
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSaveButtonClick(EventArgs e)
        {
            try
            {

                // 刪除舊資料 UDT
                if (_DeleteDataList.Count > 0)
                {
                    // 真實刪除
                    foreach (DAL.UserDefData ud in _DeleteDataList)
                        ud.Deleted = true;
                    UDTTransfer.DeleteDataToUDT(_DeleteDataList);
                }
                _InsertDataList.Clear();

                // 儲存資料到 UDT
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow)
                        continue;
                    DAL.UserDefData udd = new UserDefineData.DAL.UserDefData ();
                    // 資料轉型
                    if(row.Tag != null )
                        udd = (DAL.UserDefData)row.Tag;
                    if (row.Cells[Value.Index].Value != null)
                        udd.Value = row.Cells[Value.Index].Value.ToString();
                    udd.RefID = PrimaryKey;
                    
                    string key=string.Empty ;
                    if (row.Cells[FieldName.Index].Value != null)
                    {
                        key = row.Cells[FieldName.Index].Value.ToString();
                        udd.FieldName = key;
                    }

                    prlp.SetAfterSaveText(key + "欄位名稱", key);
                    prlp.SetAfterSaveText(key + "值", udd.Value);


                    _InsertDataList.Add(udd);
                }

                // 新增至 UDT
                UDTTransfer.InsertDataToUDT(_InsertDataList);

                //if (LoadManager.GetSystemType() == SystemType.國中)
                //{
                //    prlp.SetActionBy("學生", "自訂資料欄位");
                //    prlp.SetAction("修改自訂資料欄位");
                //    JHStudentRecord studRec = JHStudent.SelectByID(PrimaryKey);
                //    prlp.SetDescTitle("學生姓名:" + studRec.Name + ",學號:" + studRec.StudentNumber + ",");
                //    prlp.SaveLog("", "", "student", PrimaryKey);
                //}

                this.CancelButtonVisible = false;
                this.SaveButtonVisible = false;
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("儲存失敗!");
            }
        }

        /// <summary>
        /// 更換學生
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPrimaryKeyChanged(EventArgs e)
        {
            this.Loading = true;
            this.CancelButtonVisible = false;
            this.SaveButtonVisible = false;

            if (_BGWorker.IsBusy)
                _isBusy = true;
            else
                _BGWorker.RunWorkerAsync();    
        }

        /// <summary>
        /// 按下取消
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCancelButtonClick(EventArgs e)
        {
            _BGWorker.RunWorkerAsync();
            DataBindToDataGridView();
        }

        private void dgv_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dgv.EndEdit();
            bool validated = true;
            _CheckSameList.Clear();

            // 檢查資料
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // 清空錯誤訊息
                row.Cells[FieldName.Index].ErrorText = "";
                row.Cells[Value.Index].ErrorText = "";

                string FName = string.Empty;
                if (row.IsNewRow)
                    continue;
                decimal dd;
                DateTime dt;

                if (row.Cells[FieldName.Index].Value != null)
                    FName = row.Cells[FieldName.Index].Value.ToString();

                if (FName != string.Empty)
                {
                    if (_UseDefineDataType.ContainsKey(FName))
                    {
                        if (row.Cells[Value.Index].Value == null)
                            continue;

                        if (row.Cells[Value.Index].Value.ToString() == string.Empty)
                            continue;

                        string str = row.Cells[Value.Index].Value.ToString();
                        if (_UseDefineDataType[FName] == "Number")
                        {
                            if (!decimal.TryParse(str, out dd))
                            {
                                row.Cells[Value.Index].ErrorText = "非數字型態";
                                validated = false;
                            }
                        }

                        if (_UseDefineDataType[FName] == "Date")
                        {
                            if (!DateTime.TryParse(str, out dt))
                            {
                                row.Cells[Value.Index].ErrorText = "非日期型態";
                                validated = false;
                            }
                        }
                    }
                }
                else
                {
                    row.Cells[FieldName.Index].ErrorText = "不允許空白";
                    validated = false;
                }
            }

            dgv.BeginEdit(false);
            this.ContentValidated = validated;
            if (validated)
            {
                this.SaveButtonVisible = true;
                this.CancelButtonVisible = true;
            }
            else
            {
                this.SaveButtonVisible = true;
                this.CancelButtonVisible = true;
            }

            if (!FISCA.Permission.UserAcl.Current[GetType()].Editable)
            {
                this.SaveButtonVisible = false;
            }
        }

        private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            bool validated = true;
            _CheckSameList.Clear();

            // 檢查資料
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string FName = string.Empty;
                if (row.IsNewRow)
                    continue;

                if (row.Cells[FieldName.Index].Value != null)
                    FName = row.Cells[FieldName.Index].Value.ToString();

                if (_CheckSameList.Contains(FName))
                {
                    row.Cells[FieldName.Index].ErrorText = "欄位名稱重複";
                    validated = false;
                }

                _CheckSameList.Add(FName);
            }

            foreach (DataGridViewRow row in dgv.Rows)
                foreach (DataGridViewCell cell in row.Cells )
                    if (cell.ErrorText != string.Empty)
                    {
                        validated = false;
                        break;
                    }
            this.ContentValidated = validated;
            if (validated)
            {
                this.SaveButtonVisible = true;
                this.CancelButtonVisible = true;
            }
            else
            {
                this.SaveButtonVisible = true;
                this.CancelButtonVisible = true;
            }
        }

    }
}
