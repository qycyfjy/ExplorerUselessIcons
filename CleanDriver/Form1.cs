using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CleanDriver
{
    public partial class Form1 : Form
    {
        private const string DataGridViewDeleteButtonColumnName = "DeleteButtonColumn";
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly RegistryKey _registryKeyDriver;

        private readonly RegistryKey _registryKeyNavigator;

        private readonly string _registryPathDriver =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\";

        private readonly string _registryPathNavigator =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace";

        public Form1()
        {
            InitializeComponent();

            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;

            _registryKeyDriver =
                Registry.CurrentUser.OpenSubKey(_registryPathDriver, RegistryKeyPermissionCheck.ReadWriteSubTree);
            _registryKeyNavigator =
                Registry.CurrentUser.OpenSubKey(_registryPathNavigator, RegistryKeyPermissionCheck.ReadWriteSubTree);

            LoadEntries();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("EntryColumn", "Entry");
            dataGridView1.Columns["EntryColumn"].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["EntryColumn"].ReadOnly = true;
            dataGridView1.Columns.Add(CreateButtonColumn());
            dataGridView1.Columns[DataGridViewDeleteButtonColumnName].Width = 50;

            LoadData();
        }

        private void LoadData()
        {
            dataGridView1.Rows.Clear();

            foreach (var entry in _entries)
            {
                var rowIndex = dataGridView1.Rows.Add(entry);

                var buttonCell = new DataGridViewButtonCell();
                buttonCell.Value = "X";
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.Columns.Count - 1] = buttonCell;
            }
        }

        private DataGridViewButtonColumn CreateButtonColumn()
        {
            var buttonColumn = new DataGridViewButtonColumn
            {
                Name = DataGridViewDeleteButtonColumnName,
                HeaderText = "Delete",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            };

            buttonColumn.DefaultCellStyle.BackColor = Color.White;
            buttonColumn.FlatStyle = FlatStyle.Flat;
            buttonColumn.DefaultCellStyle.ForeColor = Color.Black;

            return buttonColumn;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns[DataGridViewDeleteButtonColumnName].Index && e.RowIndex >= 0)
            {
                var entry = _entries[e.RowIndex];
                entry.RegistryKeyBase.DeleteSubKey(entry.SubKey);
                _entries.RemoveAt(e.RowIndex);
                LoadData();
            }
        }

        private void LoadEntries()
        {
            Load(_registryKeyDriver);
            Load(_registryKeyNavigator);

            void Load(RegistryKey registryBase)
            {
                var subkeyNames = registryBase.GetSubKeyNames();
                foreach (var subkey in subkeyNames)
                {
                    var key = registryBase.OpenSubKey(subkey);
                    var value = key.GetValue("");
                    if (value == null) continue;
                    var p = new Entry
                    {
                        Id = value as string,
                        SubKey = subkey,
                        RegistryKeyBase = registryBase
                    };
                    _entries.Add(p);
                }
            }
        }
    }

    internal class Entry
    {
        public string Id { get; set; }
        public string SubKey { get; set; }
        public RegistryKey RegistryKeyBase { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }
}