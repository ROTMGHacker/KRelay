﻿using System.ComponentModel;
using System.Windows.Forms;

namespace MapFilter
{
    partial class FrmMapFilterSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabTiles = new System.Windows.Forms.TabPage();
            this.btnRemoveTile = new System.Windows.Forms.Button();
            this.btnAddTile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listTileFilters = new System.Windows.Forms.ListBox();
            this.tabObjects = new System.Windows.Forms.TabPage();
            this.btnRemoveObject = new System.Windows.Forms.Button();
            this.btnAddObject = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listObjectFilters = new System.Windows.Forms.ListBox();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.btnDone = new System.Windows.Forms.ToolStripMenuItem();
            this.btnReset = new System.Windows.Forms.ToolStripMenuItem();
            this.doneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabMain.SuspendLayout();
            this.tabTiles.SuspendLayout();
            this.tabObjects.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabTiles);
            this.tabMain.Controls.Add(this.tabObjects);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 26);
            this.tabMain.Margin = new System.Windows.Forms.Padding(2);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(373, 448);
            this.tabMain.TabIndex = 0;
            // 
            // tabTiles
            // 
            this.tabTiles.Controls.Add(this.btnRemoveTile);
            this.tabTiles.Controls.Add(this.btnAddTile);
            this.tabTiles.Controls.Add(this.label1);
            this.tabTiles.Controls.Add(this.listTileFilters);
            this.tabTiles.Location = new System.Drawing.Point(4, 25);
            this.tabTiles.Margin = new System.Windows.Forms.Padding(2);
            this.tabTiles.Name = "tabTiles";
            this.tabTiles.Padding = new System.Windows.Forms.Padding(2);
            this.tabTiles.Size = new System.Drawing.Size(365, 419);
            this.tabTiles.TabIndex = 0;
            this.tabTiles.Text = "Tiles (Ground)";
            this.tabTiles.UseVisualStyleBackColor = true;
            // 
            // btnRemoveTile
            // 
            this.btnRemoveTile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTile.Location = new System.Drawing.Point(323, 380);
            this.btnRemoveTile.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveTile.Name = "btnRemoveTile";
            this.btnRemoveTile.Size = new System.Drawing.Size(40, 38);
            this.btnRemoveTile.TabIndex = 3;
            this.btnRemoveTile.Text = "-";
            this.btnRemoveTile.UseVisualStyleBackColor = true;
            this.btnRemoveTile.Click += new System.EventHandler(this.btnRemoveTile_Click);
            // 
            // btnAddTile
            // 
            this.btnAddTile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddTile.Location = new System.Drawing.Point(4, 380);
            this.btnAddTile.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddTile.Name = "btnAddTile";
            this.btnAddTile.Size = new System.Drawing.Size(40, 38);
            this.btnAddTile.TabIndex = 2;
            this.btnAddTile.Text = "+";
            this.btnAddTile.UseVisualStyleBackColor = true;
            this.btnAddTile.Click += new System.EventHandler(this.btnAddTile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(236, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tile Filters: (Original=>ReplaceWith)";
            // 
            // listTileFilters
            // 
            this.listTileFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listTileFilters.FormattingEnabled = true;
            this.listTileFilters.ItemHeight = 16;
            this.listTileFilters.Location = new System.Drawing.Point(4, 22);
            this.listTileFilters.Margin = new System.Windows.Forms.Padding(2);
            this.listTileFilters.Name = "listTileFilters";
            this.listTileFilters.Size = new System.Drawing.Size(361, 308);
            this.listTileFilters.TabIndex = 0;
            // 
            // tabObjects
            // 
            this.tabObjects.Controls.Add(this.btnRemoveObject);
            this.tabObjects.Controls.Add(this.btnAddObject);
            this.tabObjects.Controls.Add(this.label2);
            this.tabObjects.Controls.Add(this.listObjectFilters);
            this.tabObjects.Location = new System.Drawing.Point(4, 25);
            this.tabObjects.Margin = new System.Windows.Forms.Padding(2);
            this.tabObjects.Name = "tabObjects";
            this.tabObjects.Padding = new System.Windows.Forms.Padding(2);
            this.tabObjects.Size = new System.Drawing.Size(365, 421);
            this.tabObjects.TabIndex = 1;
            this.tabObjects.Text = "Objects (Colliders)";
            this.tabObjects.UseVisualStyleBackColor = true;
            // 
            // btnRemoveObject
            // 
            this.btnRemoveObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveObject.Location = new System.Drawing.Point(323, 381);
            this.btnRemoveObject.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveObject.Name = "btnRemoveObject";
            this.btnRemoveObject.Size = new System.Drawing.Size(40, 38);
            this.btnRemoveObject.TabIndex = 7;
            this.btnRemoveObject.Text = "-";
            this.btnRemoveObject.UseVisualStyleBackColor = true;
            this.btnRemoveObject.Click += new System.EventHandler(this.btnRemoveObject_Click);
            // 
            // btnAddObject
            // 
            this.btnAddObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddObject.Location = new System.Drawing.Point(4, 381);
            this.btnAddObject.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddObject.Name = "btnAddObject";
            this.btnAddObject.Size = new System.Drawing.Size(40, 38);
            this.btnAddObject.TabIndex = 6;
            this.btnAddObject.Text = "+";
            this.btnAddObject.UseVisualStyleBackColor = true;
            this.btnAddObject.Click += new System.EventHandler(this.btnAddObject_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(254, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Object Filters: (Original=>ReplaceWith)";
            // 
            // listObjectFilters
            // 
            this.listObjectFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listObjectFilters.FormattingEnabled = true;
            this.listObjectFilters.ItemHeight = 16;
            this.listObjectFilters.Location = new System.Drawing.Point(4, 22);
            this.listObjectFilters.Margin = new System.Windows.Forms.Padding(2);
            this.listObjectFilters.Name = "listObjectFilters";
            this.listObjectFilters.Size = new System.Drawing.Size(361, 356);
            this.listObjectFilters.TabIndex = 4;
            // 
            // chkEnabled
            // 
            this.chkEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(235, 3);
            this.chkEnabled.Margin = new System.Windows.Forms.Padding(2);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(136, 21);
            this.chkEnabled.TabIndex = 2;
            this.chkEnabled.Text = "Enable MapFilter";
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.CheckedChanged += new System.EventHandler(this.chkEnabled_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.doneToolStripMenuItem,
            this.resetToDefaultToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(373, 26);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // btnDone
            // 
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(57, 24);
            this.btnDone.Text = "Done";
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // btnReset
            // 
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(134, 24);
            this.btnReset.Text = "Reset to Defaults";
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // doneToolStripMenuItem
            // 
            this.doneToolStripMenuItem.Name = "doneToolStripMenuItem";
            this.doneToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.doneToolStripMenuItem.Text = "Done";
            this.doneToolStripMenuItem.Click += new System.EventHandler(this.doneToolStripMenuItem_Click);
            // 
            // resetToDefaultToolStripMenuItem
            // 
            this.resetToDefaultToolStripMenuItem.Name = "resetToDefaultToolStripMenuItem";
            this.resetToDefaultToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.resetToDefaultToolStripMenuItem.Text = "Reset to Default";
            this.resetToDefaultToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultToolStripMenuItem_Click);
            // 
            // FrmMapFilterSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 474);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmMapFilterSettings";
            this.Text = "Map Filter Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMapFilterSettings_FormClosing);
            this.tabMain.ResumeLayout(false);
            this.tabTiles.ResumeLayout(false);
            this.tabTiles.PerformLayout();
            this.tabObjects.ResumeLayout(false);
            this.tabObjects.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabControl tabMain;
        private TabPage tabTiles;
        private TabPage tabObjects;
        private ListBox listTileFilters;
        private Label label1;
        private Button btnAddTile;
        private Button btnRemoveTile;
        private Button btnRemoveObject;
        private Button btnAddObject;
        private Label label2;
        private ListBox listObjectFilters;
        private CheckBox chkEnabled;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem btnDone;
        private ToolStripMenuItem btnReset;
        private ToolStripMenuItem doneToolStripMenuItem;
        private ToolStripMenuItem resetToDefaultToolStripMenuItem;

    }
}