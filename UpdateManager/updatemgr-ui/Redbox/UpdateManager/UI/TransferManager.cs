using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class TransferManager : Form
    {
        private IContainer components;
        private Button m_cancel;
        private Button m_suspend;
        private Button m_details;
        private Button m_resume;
        private ListView m_jobs;
        private ColumnHeader JobName;
        private ColumnHeader ID;
        private ColumnHeader Status;
        private ColumnHeader JobType;
        private ColumnHeader StartTime;
        private ColumnHeader TotalBytes;
        private Button m_refresh;
        private ColumnHeader m_bytesTransfered;

        public TransferManager() => this.InitializeComponent();

        private void OnLoad(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)null, new Action(this.LoadJobsIntoList));
        }

        private void OnSuspend(object sender, EventArgs e)
        {
            List<ITransferJob> jobs = new List<ITransferJob>();
            ErrorList errors = new ErrorList();
            foreach (object selectedItem in this.m_jobs.SelectedItems)
            {
                if (selectedItem is ListViewItem listViewItem && listViewItem.Tag is ITransferJob tag)
                    jobs.Add(tag);
            }
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                foreach (ITransferJob transferJob in jobs)
                {
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)transferJob.Suspend());
                    if (errors.ContainsError())
                        UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                }
                this.LoadJobsIntoList();
            }));
        }

        private void OnCancel(object sender, EventArgs e)
        {
            List<ITransferJob> jobs = new List<ITransferJob>();
            ErrorList errors = new ErrorList();
            foreach (object selectedItem in this.m_jobs.SelectedItems)
            {
                if (selectedItem is ListViewItem listViewItem && listViewItem.Tag is ITransferJob tag)
                    jobs.Add(tag);
            }
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                foreach (ITransferJob transferJob in jobs)
                {
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)transferJob.Cancel());
                    if (errors.ContainsError())
                        UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                }
                this.LoadJobsIntoList();
            }));
        }

        private void OnResume(object sender, EventArgs e)
        {
            List<ITransferJob> jobs = new List<ITransferJob>();
            ErrorList errors = new ErrorList();
            foreach (object selectedItem in this.m_jobs.SelectedItems)
            {
                if (selectedItem is ListViewItem listViewItem && listViewItem.Tag is ITransferJob tag)
                    jobs.Add(tag);
            }
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                foreach (ITransferJob transferJob in jobs)
                {
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)transferJob.Resume());
                    if (errors.ContainsError())
                        UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                }
                this.LoadJobsIntoList();
            }));
        }

        private void OnDetails(object sender, EventArgs e)
        {
            if (this.m_jobs.SelectedItems.Count < 1)
                return;
            int num = (int)new TransferDetails(this.m_jobs.SelectedItems[0].Tag as ITransferJob).ShowDialog();
        }

        private void LoadJobsIntoList()
        {
            List<ITransferJob> jobs;
            ErrorList jobs1 = ServiceLocator.Instance.GetService<ITransferService>().GetJobs(out jobs, true);
            if (jobs1.ContainsError())
                UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(jobs1);
            else
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                {
                    this.m_jobs.SuspendLayout();
                    this.m_jobs.BeginUpdate();
                    this.m_jobs.Items.Clear();
                    foreach (ITransferJob transferJob in jobs)
                    {
                        ListViewItem listViewItem = new ListViewItem()
                        {
                            Text = transferJob.Name,
                            Tag = (object)transferJob
                        };
                        listViewItem.SubItems.Add(transferJob.ID.ToString());
                        listViewItem.SubItems.Add(transferJob.Status.ToString("G"));
                        listViewItem.SubItems.Add(transferJob.JobType.ToString("G"));
                        listViewItem.SubItems.Add(transferJob.StartTime.ToLongTimeString());
                        listViewItem.SubItems.Add(transferJob.TotalBytes == ulong.MaxValue ? "N/A" : transferJob.TotalBytes.FormatAsByteSize());
                        listViewItem.SubItems.Add(transferJob.TotalBytesTransfered == ulong.MaxValue ? "N/A" : transferJob.TotalBytesTransfered.FormatAsByteSize());
                        this.m_jobs.Items.Add(listViewItem);
                    }
                    this.m_jobs.EndUpdate();
                    this.m_jobs.ResumeLayout();
                }));
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, new Action(this.LoadJobsIntoList));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(TransferManager));
            this.m_cancel = new Button();
            this.m_suspend = new Button();
            this.m_details = new Button();
            this.m_resume = new Button();
            this.m_jobs = new ListView();
            this.JobName = new ColumnHeader();
            this.ID = new ColumnHeader();
            this.Status = new ColumnHeader();
            this.JobType = new ColumnHeader();
            this.StartTime = new ColumnHeader();
            this.TotalBytes = new ColumnHeader();
            this.m_refresh = new Button();
            this.m_bytesTransfered = new ColumnHeader();
            this.SuspendLayout();
            this.m_cancel.Location = new Point(174, 248);
            this.m_cancel.Name = "m_cancel";
            this.m_cancel.Size = new Size(75, 23);
            this.m_cancel.TabIndex = 3;
            this.m_cancel.Text = "Cancel";
            this.m_cancel.UseVisualStyleBackColor = true;
            this.m_cancel.Click += new EventHandler(this.OnCancel);
            this.m_suspend.Location = new Point(12, 248);
            this.m_suspend.Name = "m_suspend";
            this.m_suspend.Size = new Size(75, 23);
            this.m_suspend.TabIndex = 1;
            this.m_suspend.Text = "Suspend";
            this.m_suspend.UseVisualStyleBackColor = true;
            this.m_suspend.Click += new EventHandler(this.OnSuspend);
            this.m_details.Location = new Point((int)byte.MaxValue, 248);
            this.m_details.Name = "m_details";
            this.m_details.Size = new Size(75, 23);
            this.m_details.TabIndex = 4;
            this.m_details.Text = "Details";
            this.m_details.UseVisualStyleBackColor = true;
            this.m_details.Click += new EventHandler(this.OnDetails);
            this.m_resume.Location = new Point(93, 248);
            this.m_resume.Name = "m_resume";
            this.m_resume.Size = new Size(75, 23);
            this.m_resume.TabIndex = 2;
            this.m_resume.Text = "Resume";
            this.m_resume.UseVisualStyleBackColor = true;
            this.m_resume.Click += new EventHandler(this.OnResume);
            this.m_jobs.Columns.AddRange(new ColumnHeader[7]
            {
        this.JobName,
        this.ID,
        this.Status,
        this.JobType,
        this.StartTime,
        this.TotalBytes,
        this.m_bytesTransfered
            });
            this.m_jobs.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_jobs.FullRowSelect = true;
            this.m_jobs.Location = new Point(7, 8);
            this.m_jobs.Name = "m_jobs";
            this.m_jobs.Size = new Size(831, 230);
            this.m_jobs.TabIndex = 0;
            this.m_jobs.UseCompatibleStateImageBehavior = false;
            this.m_jobs.View = View.Details;
            this.JobName.Text = "Job Name";
            this.JobName.Width = 180;
            this.ID.Text = "ID";
            this.ID.Width = 220;
            this.Status.Text = "Status";
            this.Status.Width = 80;
            this.JobType.Text = "Job Type";
            this.JobType.Width = 70;
            this.StartTime.Text = "Start Time";
            this.StartTime.Width = 80;
            this.TotalBytes.Text = "Total Bytes";
            this.TotalBytes.Width = 80;
            this.m_refresh.Location = new Point(758, 248);
            this.m_refresh.Name = "m_refresh";
            this.m_refresh.Size = new Size(75, 23);
            this.m_refresh.TabIndex = 5;
            this.m_refresh.Text = "Refresh";
            this.m_refresh.UseVisualStyleBackColor = true;
            this.m_refresh.Click += new EventHandler(this.OnRefresh);
            this.m_bytesTransfered.Text = "Bytes Transfered";
            this.m_bytesTransfered.Width = 110;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(845, 281);
            this.Controls.Add((Control)this.m_refresh);
            this.Controls.Add((Control)this.m_jobs);
            this.Controls.Add((Control)this.m_resume);
            this.Controls.Add((Control)this.m_details);
            this.Controls.Add((Control)this.m_suspend);
            this.Controls.Add((Control)this.m_cancel);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.Name = nameof(TransferManager);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Manage Transfers";
            this.Load += new EventHandler(this.OnLoad);
            this.ResumeLayout(false);
        }
    }
}
