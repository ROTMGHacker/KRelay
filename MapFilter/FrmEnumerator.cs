﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MapFilter
{
    public partial class FrmEnumerator : Form
    {
        private readonly Dictionary<string, ushort> _enumeration;
        private readonly Action<string> _callback;

        public FrmEnumerator()
        {
            InitializeComponent();
        }

        public FrmEnumerator(Dictionary<string, ushort> enumeration, string title, Action<string> callback)
        {
            InitializeComponent();
            Text = title;
            _enumeration = enumeration;
            _callback = callback;

            listItems.SuspendLayout();
            foreach (string key in enumeration.Keys)
            {
                listItems.Items.Add(key);
            }

            listItems.ResumeLayout();
        }

        private void tbxSearch_TextChanged(object sender, EventArgs e)
        {
            listItems.SuspendLayout();
            listItems.Items.Clear();
            foreach (string key in _enumeration.Keys)
            {
                if (key.ToLower().Contains(tbxSearch.Text.ToLower()))
                {
                    listItems.Items.Add(key);
                }
            }

            listItems.ResumeLayout();
        }

        private void listItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listItems.SelectedItem != null)
            {
                _callback(listItems.SelectedItem.ToString());
            }

            Close();
        }
    }
}
