﻿using Luna.Resources.Langs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Luna.Views.WinForms
{
    public partial class FormDataGrid :
        Form,
        Apis.Interfaces.Lua.IWinFormControl<string>

    {
        readonly int MAX_TITLE_LEN = 60;
        readonly int UPDATE_INTERVAL = 500;
        private readonly AutoResetEvent done;
        private readonly string title;
        private DataTable dataSource;
        private readonly int defColumn;
        private string filterKeyword = string.Empty;

        string jsonResult = null;
        Apis.Libs.Tasks.LazyGuy lazyUiUpdater;


        public FormDataGrid(
            AutoResetEvent done,
            string title, DataTable dataSource, int defColumn)
        {
            InitializeComponent();
            this.done = done;
            this.title = title;
            this.dataSource = dataSource;
            this.defColumn = defColumn;
            Apis.Misc.UI.AutoSetFormIcon(this);
        }

        private void FormDataGrid_Load(object sender, EventArgs e)
        {
            InitControls();
            lazyUiUpdater = new Apis.Libs.Tasks.LazyGuy(UpdateUiWorker, UPDATE_INTERVAL, 1500)
            {
                Name = "Luna.DataGridUpdater",
            };

            this.FormClosing += (s, a) => Cleanup();
            this.FormClosed += (s, a) => done.Set();

            UpdateUiWorker();
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        #region public methods
        public string GetResult() => jsonResult;
        #endregion

        #region private methods
        List<string> GetHeaders(DataGridView dataGrid)
        {
            var r = new List<string>();
            var columns = dataGrid.Columns;
            foreach (DataGridViewColumn column in columns)
            {
                r.Add(column.HeaderText.ToString());
            }
            return r;
        }

        List<List<string>> GetColumns(bool isIncludeHeader, bool isSelectedOnly)
        {
            var r = new List<List<string>>();
            if (isIncludeHeader)
            {
                r.Add(GetHeaders(dgvData));
            }

            if (isSelectedOnly)
            {
                AddSelectedRowsToDataTable(r);
            }
            else
            {
                AddAllRows(r);
            }
            return r;
        }

        private void AddAllRows(List<List<string>> dataTable)
        {
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (!row.IsNewRow)
                {
                    dataTable.Add(RowToList(row));
                }
            }
        }

        DataTable GetSelectDatas()
        {
            var r = (dgvData.DataSource as DataTable).Copy();

            var idx = new List<int>();
            foreach (DataGridViewRow row in dgvData.SelectedRows)
            {
                idx.Add(row.Index);
            }

            foreach (DataGridViewCell cell in dgvData.SelectedCells)
            {
                idx.Add(cell.RowIndex);
            }

            for (int i = r.Rows.Count - 1; i >= 0; i--)
            {
                if (!idx.Contains(i))
                {
                    r.Rows.RemoveAt(i);
                }
            }

            return r;
        }

        private void AddSelectedRowsToDataTable(List<List<string>> dataTable)
        {
            List<int> cache = new List<int>();
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            foreach (DataGridViewRow row in dgvData.SelectedRows)
            {
                AddToRows(cache, rows, row);
            }

            foreach (DataGridViewCell cell in dgvData.SelectedCells)
            {
                var r = dgvData.Rows[cell.RowIndex];
                AddToRows(cache, rows, r);
            }

            rows.Reverse();

            foreach (DataGridViewRow row in rows)
            {
                dataTable.Add(RowToList(row));
            }
        }

        void AddToRows(List<int> cache, List<DataGridViewRow> rows, DataGridViewRow row)
        {
            var idx = row.Index;
            if (row.IsNewRow || cache.Contains(idx))
            {
                return;
            }
            rows.Add(row);
            cache.Add(idx);
        }

        List<string> RowToList(DataGridViewRow row)
        {
            var l = new List<string>();
            foreach (DataGridViewCell cell in row.Cells)
            {
                l.Add(cell.Value.ToString());
            }
            return l;
        }

        DataTable CsvToDataTable(string csv)
        {
            var result = new DataTable();
            if (string.IsNullOrEmpty(csv))
            {
                return result;
            }

            var lines = csv.Split('\n');
            var len = lines.Count();
            if (len < 1)
            {
                return result;
            }

            var headers = lines[0].Split(',').Select(c => c ?? @"").ToList();
            var count = headers.Count();
            if (count < 1)
            {
                return result;
            }

            foreach (var header in headers)
            {
                result.Columns.Add(header);
            }

            for (int i = 1; i < len; i++)
            {
                var cells = lines[i].Split(',').Select(c => c ?? @"").ToArray();
                if (cells.Length != count)
                {
                    continue;
                }
                result.Rows.Add(cells);
            }

            return result;
        }

        string List2Csv(IEnumerable<IEnumerable<string>> contents)
        {
            var sb = new StringBuilder();
            foreach (var line in contents)
            {
                sb.Append(string.Join(@",", line));
                sb.Append('\n');
            }
            return sb.ToString();
        }

        void Cleanup()
        {
            lazyUiUpdater?.Dispose();
        }

        void UpdateUiWorker()
        {
            Apis.Misc.UI.Invoke(() =>
           {
               var ds = GetFilteredDataTable();
               lbTotal.Text = ds.Rows.Count.ToString();
               dgvData.DataSource = ds;
           });
        }

        DataTable GetFilteredDataTable()
        {
            var r = dataSource.Copy();
            var idx = Math.Max(0, cboxColumnIdx.SelectedIndex);

            for (int i = r.Rows.Count - 1; i >= 0; i--)
            {
                var text = r.Rows[i].ItemArray[idx].ToString();
                if (Apis.Misc.Utils.MeasureSimilarityCi(text, filterKeyword) < 1)
                {
                    r.Rows.RemoveAt(i);
                }
            }
            return r;
        }


        void InitControls()
        {
            lbTitle.Text = Apis.Misc.Utils.AutoEllipsis(title, MAX_TITLE_LEN);
            toolTip1.SetToolTip(lbTitle, title);
            InitColumnsBox(dataSource);
        }

        void InitColumnsBox(DataTable dataSource)
        {
            var items = cboxColumnIdx.Items;
            items.Clear();
            foreach (DataColumn column in dataSource.Columns)
            {
                items.Add(column.ToString());
            }

            Apis.Misc.UI.ResetComboBoxDropdownMenuWidth(cboxColumnIdx);

            var count = cboxColumnIdx.Items.Count;
            if (count > 0)
            {
                // lua index starts from 1
                cboxColumnIdx.SelectedIndex = Apis.Misc.Utils.Clamp(
                    defColumn - 1, 0, count);
            }
        }

        void SetResult()
        {
            try
            {
                jsonResult = null;
                var idxs = JsonConvert.SerializeObject(GetSelectDatas(), Formatting.Indented);
                var datas = JsonConvert.SerializeObject(dgvData.DataSource as DataTable, Formatting.Indented);
                jsonResult = "{\n"
                            + $"\"selected\":{idxs},\n"
                            + $"\"all\":{datas}"
                            + "\n}";
            }
            catch { }
        }

        private void SaveColumnsToFile(bool isSelectedOnly)
        {
            var content = GetColumns(true, isSelectedOnly);
            var text = List2Csv(content);
            Apis.Models.Datas.Enums.SaveFileErrorCode ok = Apis.Models.Datas.Enums.SaveFileErrorCode.Cancel;
            ok = Apis.Misc.UI.ShowSaveFileDialog(Apis.Models.Consts.Files.CsvExt, text, out _);
            switch (ok)
            {
                case Apis.Models.Datas.Enums.SaveFileErrorCode.Fail:
                    Apis.Misc.UI.MsgBoxAsync(I18N.Fail);
                    break;
                case Apis.Models.Datas.Enums.SaveFileErrorCode.Success:
                    Apis.Misc.UI.MsgBoxAsync(I18N.Done);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region UI events
        private void btnCopy_Click(object sender, EventArgs e)
        {
            var content = GetColumns(true, true);
            var text = List2Csv(content);
            var success = false;
            success = Apis.Misc.Utils.CopyToClipboard(text);
            Apis.Misc.UI.MsgBoxAsync(success ? I18N.Done : I18N.Fail);
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SetResult();
            this.Close();
        }

        private void tboxFilter_TextChanged(object sender, EventArgs e)
        {
            filterKeyword = tboxFilter.Text;
            lazyUiUpdater?.Postpone();
        }

        private void cboxColumnIdx_SelectedIndexChanged(object sender, EventArgs e)
        {
            tboxFilter.Text = @"";
            lazyUiUpdater?.Throttle();
        }

        private void autosizeByHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
        }

        private void autosizeByContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void disableAutosizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        private void importFromCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = null;

            text = Apis.Misc.UI.ReadFileContentFromDialog(Apis.Models.Consts.Files.CsvExt);

            if (text == null)
            {
                return;
            }

            Apis.Misc.UI.Invoke(() =>
            {
                this.dataSource = CsvToDataTable(text);
                tboxFilter.Text = @"";
                lazyUiUpdater.Throttle();
            });
        }

        private void exportAllToCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveColumnsToFile(false);
        }

        private void exportSelectedToCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveColumnsToFile(true);
        }
        #endregion
    }
}
