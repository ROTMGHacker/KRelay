using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Utilities;

namespace TraderBot
{
    public partial class FrmTraderBot : Form
    {
        public Client C;

        public FrmTraderBot(Client c)
        {
            InitializeComponent();
            C = c;

            Dictionary<int, string> allitems = new Dictionary<int, string>();
            foreach (ItemStructure item in GameData.Items.Map.Values)
            {
                allitems.Add(item.ID, item.Name);
            }
            comboBox1.DataSource = new BindingSource(allitems, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            if (TraderBot.Selling[C].Count > 0)
            {
                foreach (ItemStruct item in TraderBot.Selling[C])
                {
                    ListViewItem lvi = new ListViewItem(item.Qty.ToString());
                    lvi.SubItems.Add(item.Name);
                    if (item.AltQty > 0)
                    {
                        lvi.SubItems.Add(item.AltQty.ToString());
                        lvi.SubItems.Add(item.Alt);
                    }
                    listView1.Items.Add(lvi);
                }
            }
            if (TraderBot.Buying[C].Count > 0)
            {
                foreach (ItemStruct item in TraderBot.Buying[C])
                {
                    ListViewItem lvi = new ListViewItem(item.Qty.ToString());
                    lvi.SubItems.Add(item.Name);
                    if (item.AltQty > 0)
                    {
                        lvi.SubItems.Add(item.AltQty.ToString());
                        lvi.SubItems.Add(item.Alt);
                    }
                    listView2.Items.Add(lvi);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int key = ((KeyValuePair<int, string>) comboBox1.SelectedItem).Key;
            string value = ((KeyValuePair<int, string>) comboBox1.SelectedItem).Value;
            int q = (int) numericUpDown1.Value;

            ListViewItem lvi = new ListViewItem(q.ToString());
            lvi.SubItems.Add(value);

            if (radioButton1.Checked)
            {
                if (listView1.SelectedIndices.Count <= 0)
                {
                    listView1.Items.Add(lvi);
                }
                else
                {
                    listView1.Items[listView1.SelectedIndices[0]].SubItems.Add(value);
                }
            }
            else if (radioButton2.Checked)
            {
                if (listView2.SelectedIndices.Count <= 0)
                {
                    listView2.Items.Add(lvi);
                }
                else
                {
                    listView2.Items[listView2.SelectedIndices[0]].SubItems.Add(q.ToString());
                    listView2.Items[listView2.SelectedIndices[0]].SubItems.Add(value);
                }
            }
            else
            {
                MessageBox.Show(@"Must select if you are buying or selling the item.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TraderBot.SpamMsg[C] = textBox1.Text;

            TraderBot.Selling[C] = new List<ItemStruct>();
            TraderBot.Buying[C] = new List<ItemStruct>();

            foreach (ListViewItem lvi in listView1.Items)
            {
                ItemStruct tmp = new ItemStruct(lvi.SubItems[1].Text, int.Parse(lvi.Text));
                TraderBot.Selling[C].Add(tmp);
            }

            foreach (ListViewItem lvi in listView2.Items)
            {
                if (lvi.SubItems.Count > 3)
                {
                    ItemStruct tmp = new ItemStruct(lvi.SubItems[1].Text, int.Parse(lvi.Text), lvi.SubItems[3].Text, int.Parse(lvi.SubItems[2].Text));
                    TraderBot.Buying[C].Add(tmp);
                }
                else
                {
                    ItemStruct tmp = new ItemStruct(lvi.SubItems[1].Text, int.Parse(lvi.Text));
                    TraderBot.Buying[C].Add(tmp);
                }
            }

            Close();

            C.SendToClient(PluginUtils.CreateNotification(C.ObjectId, "TraderBot Settings Saved!"));
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                listView2.SelectedItems[0].Remove();
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                listView1.SelectedItems[0].Remove();
            }
        }
    }
}
