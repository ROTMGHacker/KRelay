using System;
using System.Diagnostics;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Forms;

namespace MapRipper
{
    public partial class HandleForm : MetroForm
    {
        private readonly MapRipper _mPlugin;
        private readonly FixedStyleManager _mStyleManager;

        public HandleForm(MapRipper plugin)
        {
            _mPlugin = plugin;
            InitializeComponent();
            _mStyleManager = new FixedStyleManager(this);

            clrStyle.SelectedValueChanged += clrStyle_SelectedValueChanged;
            clrTheme.SelectedValueChanged += clrTheme_SelectedValueChanged;

            clrStyle.Items.AddRange(Enum.GetNames(typeof(MetroColorStyle)));
            clrTheme.Items.AddRange(Enum.GetNames(typeof(MetroThemeStyle)));

            widthLabel.Text = string.Format(widthLabel.Text, _mPlugin.Map?.Width.ToString() ?? "Not in world");
            heightLabel.Text = string.Format(heightLabel.Text, _mPlugin.Map?.Height.ToString() ?? "Not in world");

            metroTile1.Text = _mPlugin.Map != null ? _mPlugin.Map.Name : "Not in world";

            clrTheme.SelectedItem = clrStyle.SelectedItem = "Default";

            if (_mPlugin.Map != null)
            {
                _mPlugin.Map.TilesAdded += Map_TilesAdded;
                //this.metroProgressBar1.Maximum = this.m_plugin.Map.Tiles[0].Length * this.m_plugin.Map.Tiles[1].Length;
            }
        }

        private void Map_TilesAdded(int currentTiles)
        {
            Invoke(new Action(() =>
            {
                //this.metroProgressBar1.Value = currentTiles;
            }));
        }

        private void clrTheme_SelectedValueChanged(object sender, EventArgs e)
        {
            _mStyleManager.Theme = (MetroThemeStyle)Enum.Parse(typeof(MetroThemeStyle), (string)clrTheme.SelectedItem, true);
        }

        private void clrStyle_SelectedValueChanged(object sender, EventArgs e)
        {
            _mStyleManager.Style = (MetroColorStyle)Enum.Parse(typeof(MetroColorStyle), (string)clrStyle.SelectedItem, true);
        }

        private void saveMapButton_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "", "Asterisk", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            MetroMessageBox.Show(this, "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MetroMessageBox.Show(this, "", "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            MetroMessageBox.Show(this, "", "Hand", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            MetroMessageBox.Show(this, "", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MetroMessageBox.Show(this, "", "None", MessageBoxButtons.OK, MessageBoxIcon.None);
            MetroMessageBox.Show(this, "", "Question", MessageBoxButtons.OK, MessageBoxIcon.Question);
            MetroMessageBox.Show(this, "", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            MetroMessageBox.Show(this, "", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private class FixedStyleManager
        {
            private readonly MetroStyleManager _mManager;

            private MetroColorStyle _mColorStyle;
            private MetroThemeStyle _mThemeStyle;
            

            public FixedStyleManager(MetroForm form)
            {
                _mManager = new MetroStyleManager(form.Container) {Owner = form};
            }

            public MetroColorStyle Style
            {
                get => _mColorStyle;
                set
                {
                    _mColorStyle = value;
                    Update();
                }
            }

            public MetroThemeStyle Theme
            {
                get => _mThemeStyle;
                set
                {
                    _mThemeStyle = value;
                    Update();
                }
            }

            private void Update()
            {
                ((MetroForm) _mManager.Owner).Theme = _mThemeStyle;
                ((MetroForm) _mManager.Owner).Style = _mColorStyle;

                _mManager.Theme = _mThemeStyle;
                _mManager.Style = _mColorStyle;
            }
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.realmeye.com/wiki-search?q=" + metroTile1.Text);
        }
    }
}
