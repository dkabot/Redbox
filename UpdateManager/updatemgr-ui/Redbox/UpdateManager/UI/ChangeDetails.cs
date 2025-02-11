using Redbox.UpdateManager.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class ChangeDetails : Form
    {
        private readonly IRevLog m_log;
        private IContainer components;
        private ListView m_changes;
        private ColumnHeader m_name;
        private ColumnHeader m_path;
        private ColumnHeader m_targetHash;
        private ColumnHeader m_patchSignature;
        private ColumnHeader m_isComposite;
        private ColumnHeader m_isSeed;
        private Label hashLabel;
        private Label m_hash;
        private Label m_label;
        private Label labelLabel;

        public ChangeDetails(IRevLog log)
        {
            this.InitializeComponent();
            this.m_log = log;
        }

        private void LoadChageDetails(IRevLog log)
        {
            this.m_changes.BeginUpdate();
            this.m_hash.Text = log.Hash;
            this.m_label.Text = log.Label;
            foreach (ChangeItem change in log.Changes)
            {
                ListViewItem listViewItem = new ListViewItem()
                {
                    Text = change.TargetName,
                    Tag = (object)change
                };
                listViewItem.SubItems.Add(change.TargetPath);
                listViewItem.SubItems.Add(change.VersionHash);
                listViewItem.SubItems.Add(change.ContentHash);
                ListViewItem.ListViewSubItemCollection subItems1 = listViewItem.SubItems;
                bool flag = change.IsSeed;
                string text1 = flag.ToString();
                subItems1.Add(text1);
                ListViewItem.ListViewSubItemCollection subItems2 = listViewItem.SubItems;
                flag = change.Composite;
                string text2 = flag.ToString();
                subItems2.Add(text2);
                this.m_changes.Items.Add(listViewItem);
            }
            this.m_changes.EndUpdate();
        }

        private void OnLoad(object sender, EventArgs e) => this.LoadChageDetails(this.m_log);

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.m_changes = new ListView();
            this.m_name = new ColumnHeader();
            this.m_path = new ColumnHeader();
            this.m_targetHash = new ColumnHeader();
            this.m_patchSignature = new ColumnHeader();
            this.m_isComposite = new ColumnHeader();
            this.m_isSeed = new ColumnHeader();
            this.hashLabel = new Label();
            this.m_hash = new Label();
            this.m_label = new Label();
            this.labelLabel = new Label();
            this.SuspendLayout();
            this.m_changes.Columns.AddRange(new ColumnHeader[6]
            {
        this.m_name,
        this.m_path,
        this.m_targetHash,
        this.m_patchSignature,
        this.m_isComposite,
        this.m_isSeed
            });
            this.m_changes.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_changes.FullRowSelect = true;
            this.m_changes.Location = new Point(12, 85);
            this.m_changes.Name = "m_changes";
            this.m_changes.Size = new Size(642, 329);
            this.m_changes.TabIndex = 0;
            this.m_changes.UseCompatibleStateImageBehavior = false;
            this.m_changes.View = View.Details;
            this.m_name.Text = "File Name";
            this.m_name.Width = 78;
            this.m_path.Text = "Path";
            this.m_path.Width = 206;
            this.m_targetHash.Text = "Version Hash";
            this.m_targetHash.Width = 89;
            this.m_patchSignature.Text = "Patch Signature";
            this.m_patchSignature.Width = 112;
            this.m_isComposite.Text = "Is Composite?";
            this.m_isComposite.Width = 93;
            this.m_isSeed.Text = "Is Seed?";
            this.hashLabel.AutoSize = true;
            this.hashLabel.Location = new Point(12, 9);
            this.hashLabel.Name = "hashLabel";
            this.hashLabel.Size = new Size(35, 13);
            this.hashLabel.TabIndex = 1;
            this.hashLabel.Text = "Hash:";
            this.m_hash.AutoSize = true;
            this.m_hash.Location = new Point(78, 9);
            this.m_hash.Name = "m_hash";
            this.m_hash.Size = new Size(42, 13);
            this.m_hash.TabIndex = 2;
            this.m_hash.Text = "<hash>";
            this.m_label.AutoSize = true;
            this.m_label.Location = new Point(78, 43);
            this.m_label.Name = "m_label";
            this.m_label.Size = new Size(41, 13);
            this.m_label.TabIndex = 4;
            this.m_label.Text = "<label>";
            this.labelLabel.AutoSize = true;
            this.labelLabel.Location = new Point(12, 43);
            this.labelLabel.Name = "labelLabel";
            this.labelLabel.Size = new Size(36, 13);
            this.labelLabel.TabIndex = 3;
            this.labelLabel.Text = "Label:";
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(666, 426);
            this.Controls.Add((Control)this.m_label);
            this.Controls.Add((Control)this.labelLabel);
            this.Controls.Add((Control)this.m_hash);
            this.Controls.Add((Control)this.hashLabel);
            this.Controls.Add((Control)this.m_changes);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = nameof(ChangeDetails);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Change Details";
            this.Load += new EventHandler(this.OnLoad);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
