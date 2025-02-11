using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class TransferDetails : Form
    {
        private readonly ErrorList m_errors;
        private IContainer components;
        private Label label1;
        private Label label2;
        private Label m_labelTotalBytes;
        private Label label4;
        private ProgressBar m_downloadProgress;
        private Label m_totalBytes;
        private Label m_statusDisplay;
        private Label label5;
        private Label label3;
        private Label m_startDate;
        private Label label9;
        private Label m_finishDate;
        private Label label10;
        private Button m_showErrors;
        private Label m_localFile;
        private Label label100;
        private Label label11;
        private ToolTip m_longUrl;
        private TextBox m_urlDisplay;
        private TextBox m_id;
        private TextBox m_name;

        public TransferDetails(ITransferJob job)
        {
            this.InitializeComponent();
            ErrorList errors;
            job.GetErrors(out errors);
            this.m_showErrors.Visible = errors.ContainsError();
            this.m_errors = errors;
            this.m_name.Text = job.Name;
            this.m_id.Text = job.ID.ToString();
            this.m_statusDisplay.Text = job.Status.ToString("G");
            if (job.Status != TransferStatus.Error)
            {
                this.m_downloadProgress.Maximum = (int)(job.TotalBytes / 1048576UL);
                this.m_downloadProgress.Value = (int)(job.TotalBytesTransfered / 1048576UL);
            }
            this.m_totalBytes.Text = job.TotalBytes == ulong.MaxValue ? "N/A" : job.TotalBytes.FormatAsByteSize();
            Label startDate = this.m_startDate;
            DateTime startTime = job.StartTime;
            string longTimeString = startTime.ToLongTimeString();
            startDate.Text = longTimeString;
            Label finishDate = this.m_finishDate;
            DateTime? finishTime = job.FinishTime;
            string str;
            if (!finishTime.HasValue)
            {
                str = "Job is still running.";
            }
            else
            {
                finishTime = job.FinishTime;
                startTime = finishTime.Value;
                str = startTime.ToLongTimeString();
            }
            finishDate.Text = str;
            List<ITransferItem> items;
            this.m_errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.GetItems(out items));
            ITransferItem transferItem = items.Count > 0 ? items[0] : (ITransferItem)null;
            if (transferItem == null)
                return;
            if (transferItem.RemoteURL.Length > 80)
            {
                this.m_longUrl.SetToolTip((Control)this.m_urlDisplay, transferItem.RemoteURL);
                this.m_urlDisplay.Text = transferItem.RemoteURL.Substring(0, 80) + ".....";
            }
            else
                this.m_urlDisplay.Text = transferItem.RemoteURL;
            this.m_localFile.Text = Path.Combine(transferItem.Path, transferItem.Name);
        }

        private void OnShowErrors(object sender, EventArgs e)
        {
            int num = (int)new ErrorForm()
            {
                Errors = ((IEnumerable)this.m_errors)
            }.ShowDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new System.ComponentModel.Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(TransferDetails));
            this.label1 = new Label();
            this.label2 = new Label();
            this.m_labelTotalBytes = new Label();
            this.label4 = new Label();
            this.m_downloadProgress = new ProgressBar();
            this.m_totalBytes = new Label();
            this.m_statusDisplay = new Label();
            this.label5 = new Label();
            this.label3 = new Label();
            this.m_startDate = new Label();
            this.label9 = new Label();
            this.m_finishDate = new Label();
            this.label10 = new Label();
            this.m_showErrors = new Button();
            this.m_localFile = new Label();
            this.label100 = new Label();
            this.label11 = new Label();
            this.m_longUrl = new ToolTip(this.components);
            this.m_urlDisplay = new TextBox();
            this.m_id = new TextBox();
            this.m_name = new TextBox();
            this.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label1.Location = new Point(49, 13);
            this.label1.Name = "label1";
            this.label1.Size = new Size(42, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            this.label2.AutoSize = true;
            this.label2.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label2.Location = new Point(63, 41);
            this.label2.Name = "label2";
            this.label2.Size = new Size(28, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "ID:";
            this.m_labelTotalBytes.AutoSize = true;
            this.m_labelTotalBytes.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_labelTotalBytes.Location = new Point(564, 13);
            this.m_labelTotalBytes.Name = "m_labelTotalBytes";
            this.m_labelTotalBytes.Size = new Size(119, 15);
            this.m_labelTotalBytes.TabIndex = 2;
            this.m_labelTotalBytes.Text = "Total Megabytes:";
            this.label4.AutoSize = true;
            this.label4.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label4.Location = new Point(262, 13);
            this.label4.Name = "label4";
            this.label4.Size = new Size(14, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = " ";
            this.m_downloadProgress.Location = new Point(649, 131);
            this.m_downloadProgress.Name = "m_downloadProgress";
            this.m_downloadProgress.Size = new Size(162, 23);
            this.m_downloadProgress.TabIndex = 4;
            this.m_totalBytes.AutoSize = true;
            this.m_totalBytes.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_totalBytes.Location = new Point(696, 13);
            this.m_totalBytes.Name = "m_totalBytes";
            this.m_totalBytes.Size = new Size(98, 15);
            this.m_totalBytes.TabIndex = 5;
            this.m_totalBytes.Text = "<total bytes>";
            this.m_statusDisplay.AutoSize = true;
            this.m_statusDisplay.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_statusDisplay.Location = new Point(296, 125);
            this.m_statusDisplay.Name = "m_statusDisplay";
            this.m_statusDisplay.Size = new Size(63, 15);
            this.m_statusDisplay.TabIndex = 9;
            this.m_statusDisplay.Text = "<status>";
            this.m_statusDisplay.TextAlign = ContentAlignment.TopCenter;
            this.label5.AutoSize = true;
            this.label5.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label5.Location = new Point(232, 125);
            this.label5.Name = "label5";
            this.label5.Size = new Size(56, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Status:";
            this.label3.AutoSize = true;
            this.label3.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label3.Location = new Point(510, 134);
            this.label3.Name = "label3";
            this.label3.Size = new Size(133, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Transfer Progress:";
            this.m_startDate.AutoSize = true;
            this.m_startDate.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_startDate.Location = new Point(99, 97);
            this.m_startDate.Name = "m_startDate";
            this.m_startDate.Size = new Size(84, 15);
            this.m_startDate.TabIndex = 13;
            this.m_startDate.Text = "<startdate>";
            this.label9.AutoSize = true;
            this.label9.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label9.Location = new Point(7, 97);
            this.label9.Name = "label9";
            this.label9.Size = new Size(84, 15);
            this.label9.TabIndex = 11;
            this.label9.Text = "Start Date:";
            this.m_finishDate.AutoSize = true;
            this.m_finishDate.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_finishDate.Location = new Point(99, 125);
            this.m_finishDate.Name = "m_finishDate";
            this.m_finishDate.Size = new Size(91, 15);
            this.m_finishDate.TabIndex = 16;
            this.m_finishDate.Text = "<finishdate>";
            this.label10.AutoSize = true;
            this.label10.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label10.Location = new Point(0, 125);
            this.label10.Name = "label10";
            this.label10.Size = new Size(91, 15);
            this.label10.TabIndex = 15;
            this.label10.Text = "Finish Date:";
            this.m_showErrors.Location = new Point(407, 131);
            this.m_showErrors.Name = "m_showErrors";
            this.m_showErrors.Size = new Size(97, 23);
            this.m_showErrors.TabIndex = 1;
            this.m_showErrors.Text = "Show Errors";
            this.m_showErrors.UseVisualStyleBackColor = true;
            this.m_showErrors.Click += new EventHandler(this.OnShowErrors);
            this.m_localFile.AutoSize = true;
            this.m_localFile.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_localFile.Location = new Point(303, 97);
            this.m_localFile.Name = "m_localFile";
            this.m_localFile.Size = new Size(35, 15);
            this.m_localFile.TabIndex = 21;
            this.m_localFile.Text = "<id>";
            this.label100.AutoSize = true;
            this.label100.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label100.Location = new Point(204, 97);
            this.label100.Name = "label100";
            this.label100.Size = new Size(84, 15);
            this.label100.TabIndex = 19;
            this.label100.Text = "Local File:";
            this.label11.AutoSize = true;
            this.label11.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label11.Location = new Point(49, 69);
            this.label11.Name = "label11";
            this.label11.Size = new Size(35, 15);
            this.label11.TabIndex = 18;
            this.label11.Text = "URL:";
            this.m_longUrl.Tag = (object)"m_urlDisplay";
            this.m_urlDisplay.BackColor = SystemColors.Control;
            this.m_urlDisplay.BorderStyle = BorderStyle.None;
            this.m_urlDisplay.Location = new Point(102, 69);
            this.m_urlDisplay.Name = "m_urlDisplay";
            this.m_urlDisplay.Size = new Size(692, 16);
            this.m_urlDisplay.TabIndex = 22;
            this.m_id.BackColor = SystemColors.Control;
            this.m_id.BorderStyle = BorderStyle.None;
            this.m_id.Location = new Point(97, 40);
            this.m_id.Name = "m_id";
            this.m_id.Size = new Size(692, 16);
            this.m_id.TabIndex = 23;
            this.m_name.BackColor = SystemColors.Control;
            this.m_name.BorderStyle = BorderStyle.None;
            this.m_name.Location = new Point(97, 12);
            this.m_name.Name = "m_name";
            this.m_name.Size = new Size(461, 16);
            this.m_name.TabIndex = 24;
            this.AutoScaleDimensions = new SizeF(7f, 15f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(814, 166);
            this.Controls.Add((Control)this.m_name);
            this.Controls.Add((Control)this.m_id);
            this.Controls.Add((Control)this.m_urlDisplay);
            this.Controls.Add((Control)this.m_localFile);
            this.Controls.Add((Control)this.label100);
            this.Controls.Add((Control)this.label11);
            this.Controls.Add((Control)this.m_showErrors);
            this.Controls.Add((Control)this.m_finishDate);
            this.Controls.Add((Control)this.label10);
            this.Controls.Add((Control)this.m_startDate);
            this.Controls.Add((Control)this.label9);
            this.Controls.Add((Control)this.label3);
            this.Controls.Add((Control)this.m_statusDisplay);
            this.Controls.Add((Control)this.label5);
            this.Controls.Add((Control)this.m_totalBytes);
            this.Controls.Add((Control)this.m_downloadProgress);
            this.Controls.Add((Control)this.label4);
            this.Controls.Add((Control)this.m_labelTotalBytes);
            this.Controls.Add((Control)this.label2);
            this.Controls.Add((Control)this.label1);
            this.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.Name = nameof(TransferDetails);
            this.ShowIcon = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Details";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
