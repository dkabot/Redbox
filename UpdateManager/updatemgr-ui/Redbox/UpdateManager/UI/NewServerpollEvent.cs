using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class NewServerpollEvent : Form
    {
        private const string JobName = "Serverpoll";
        private IContainer components;
        private ErrorProvider m_errors;
        private Button m_cancel;
        private Button m_submit;
        private TabControl tabControl1;
        private TabPage BasicPage;
        private TextBox m_recurrance_sec;
        private TextBox m_initial_sec;
        private Label m_recurrenceLabel;
        private Label label2;
        private TabPage AdvancedPage;
        private TextBox Instructions;
        private TextBox m_cron;
        private Label label5;
        private TextBox m_initial_sec_adv;
        private Label label1;

        public NewServerpollEvent() => this.InitializeComponent();

        private void OnSubmitClick(object sender, EventArgs e)
        {
            this.m_errors.SetError((Control)this.m_cron, string.Empty);
            this.m_errors.SetError((Control)this.m_initial_sec, string.Empty);
            this.m_errors.SetError((Control)this.m_initial_sec_adv, string.Empty);
            bool flag = false;
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            ErrorList errorList = new ErrorList();
            if (this.tabControl1.SelectedTab == this.BasicPage)
            {
                TimeSpan? startOffset = new TimeSpan?();
                TimeSpan? nullable = new TimeSpan?();
                if (!string.IsNullOrEmpty(this.m_initial_sec.Text))
                {
                    int result;
                    if (int.TryParse(this.m_initial_sec.Text, out result))
                    {
                        startOffset = new TimeSpan?(TimeSpan.FromSeconds((double)result));
                    }
                    else
                    {
                        this.m_errors.SetError((Control)this.m_initial_sec, "Initial Offset is not a valid integer value.");
                        flag = true;
                    }
                }
                if (string.IsNullOrEmpty(this.m_recurrance_sec.Text))
                {
                    this.m_errors.SetError((Control)this.m_recurrance_sec, "Recurrence is a required field.");
                    flag = true;
                }
                else
                {
                    int result;
                    if (int.TryParse(this.m_recurrance_sec.Text, out result))
                    {
                        nullable = new TimeSpan?(TimeSpan.FromSeconds((double)result));
                    }
                    else
                    {
                        this.m_errors.SetError((Control)this.m_initial_sec, "Initial Offset is not a valid integer value.");
                        flag = true;
                    }
                }
                if (flag)
                    return;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleSimpleJob("Serverpoll", "Serverpoll", PayloadType.ServerPoll, startOffset, new DateTime?(), new DateTime?(), nullable.GetValueOrDefault()));
            }
            else
            {
                TimeSpan? startOffset = new TimeSpan?();
                if (!string.IsNullOrEmpty(this.m_initial_sec_adv.Text))
                {
                    int result;
                    if (int.TryParse(this.m_initial_sec_adv.Text, out result))
                    {
                        startOffset = new TimeSpan?(TimeSpan.FromSeconds((double)result));
                    }
                    else
                    {
                        this.m_errors.SetError((Control)this.m_initial_sec_adv, "Initial Offset is not a valid integer value.");
                        flag = true;
                    }
                }
                if (string.IsNullOrEmpty(this.m_cron.Text))
                {
                    this.m_errors.SetError((Control)this.m_cron, "Expression is a required field.");
                    flag = true;
                }
                if (flag)
                    return;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleCronJob("Serverpoll", "Serverpoll", PayloadType.ServerPoll, startOffset, new DateTime?(), new DateTime?(), this.m_cron.Text));
            }
            if (errorList.ContainsError())
            {
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList)
                }.ShowDialog();
            }
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e) => this.Close();

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new System.ComponentModel.Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(NewServerpollEvent));
            this.m_errors = new ErrorProvider(this.components);
            this.m_cancel = new Button();
            this.m_submit = new Button();
            this.tabControl1 = new TabControl();
            this.BasicPage = new TabPage();
            this.m_recurrance_sec = new TextBox();
            this.m_initial_sec = new TextBox();
            this.m_recurrenceLabel = new Label();
            this.label2 = new Label();
            this.AdvancedPage = new TabPage();
            this.Instructions = new TextBox();
            this.m_cron = new TextBox();
            this.label5 = new Label();
            this.m_initial_sec_adv = new TextBox();
            this.label1 = new Label();
            ((ISupportInitialize)this.m_errors).BeginInit();
            this.tabControl1.SuspendLayout();
            this.BasicPage.SuspendLayout();
            this.AdvancedPage.SuspendLayout();
            this.SuspendLayout();
            this.m_errors.ContainerControl = (ContainerControl)this;
            this.m_cancel.DialogResult = DialogResult.Cancel;
            this.m_cancel.Location = new Point(452, 64);
            this.m_cancel.Name = "m_cancel";
            this.m_cancel.Size = new Size(75, 23);
            this.m_cancel.TabIndex = 7;
            this.m_cancel.Text = "Cancel";
            this.m_cancel.UseVisualStyleBackColor = true;
            this.m_cancel.Click += new EventHandler(this.OnCancelClick);
            this.m_submit.Location = new Point(452, 30);
            this.m_submit.Name = "m_submit";
            this.m_submit.Size = new Size(75, 23);
            this.m_submit.TabIndex = 6;
            this.m_submit.Text = "OK";
            this.m_submit.UseVisualStyleBackColor = true;
            this.m_submit.Click += new EventHandler(this.OnSubmitClick);
            this.tabControl1.Controls.Add((Control)this.BasicPage);
            this.tabControl1.Controls.Add((Control)this.AdvancedPage);
            this.tabControl1.Location = new Point(12, 9);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(434, 195);
            this.tabControl1.TabIndex = 14;
            this.BasicPage.Controls.Add((Control)this.m_recurrance_sec);
            this.BasicPage.Controls.Add((Control)this.m_initial_sec);
            this.BasicPage.Controls.Add((Control)this.m_recurrenceLabel);
            this.BasicPage.Controls.Add((Control)this.label2);
            this.BasicPage.Location = new Point(4, 22);
            this.BasicPage.Name = "BasicPage";
            this.BasicPage.Padding = new Padding(3);
            this.BasicPage.Size = new Size(426, 169);
            this.BasicPage.TabIndex = 0;
            this.BasicPage.Text = "Basic";
            this.BasicPage.UseVisualStyleBackColor = true;
            this.m_recurrance_sec.Location = new Point(119, 32);
            this.m_recurrance_sec.Name = "m_recurrance_sec";
            this.m_recurrance_sec.Size = new Size(92, 20);
            this.m_recurrance_sec.TabIndex = 5;
            this.m_initial_sec.Location = new Point(119, 6);
            this.m_initial_sec.Name = "m_initial_sec";
            this.m_initial_sec.Size = new Size(92, 20);
            this.m_initial_sec.TabIndex = 4;
            this.m_recurrenceLabel.AutoSize = true;
            this.m_recurrenceLabel.Location = new Point(6, 35);
            this.m_recurrenceLabel.Name = "m_recurrenceLabel";
            this.m_recurrenceLabel.Size = new Size(92, 13);
            this.m_recurrenceLabel.TabIndex = 18;
            this.m_recurrenceLabel.Text = "Recurrence: (sec)";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(6, 9);
            this.label2.Name = "label2";
            this.label2.Size = new Size(91, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Initial Offset: (sec)";
            this.AdvancedPage.Controls.Add((Control)this.m_initial_sec_adv);
            this.AdvancedPage.Controls.Add((Control)this.label1);
            this.AdvancedPage.Controls.Add((Control)this.Instructions);
            this.AdvancedPage.Controls.Add((Control)this.m_cron);
            this.AdvancedPage.Controls.Add((Control)this.label5);
            this.AdvancedPage.Location = new Point(4, 22);
            this.AdvancedPage.Name = "AdvancedPage";
            this.AdvancedPage.Padding = new Padding(3);
            this.AdvancedPage.Size = new Size(426, 169);
            this.AdvancedPage.TabIndex = 1;
            this.AdvancedPage.Text = "Advanced";
            this.AdvancedPage.UseVisualStyleBackColor = true;
            this.Instructions.BackColor = SystemColors.Control;
            this.Instructions.BorderStyle = BorderStyle.None;
            this.Instructions.Cursor = Cursors.Default;
            this.Instructions.Font = new Font("Lucida Console", 7f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.Instructions.Location = new Point(108, 63);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new Size(263, 88);
            this.Instructions.TabIndex = 4;
            this.Instructions.Text = componentResourceManager.GetString("Instructions.Text");
            this.m_cron.Location = new Point(103, 37);
            this.m_cron.Name = "m_cron";
            this.m_cron.Size = new Size(268, 20);
            this.m_cron.TabIndex = 5;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(11, 40);
            this.label5.Name = "label5";
            this.label5.Size = new Size(86, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Cron Expression:";
            this.m_initial_sec_adv.Location = new Point(106, 6);
            this.m_initial_sec_adv.Name = "m_initial_sec_adv";
            this.m_initial_sec_adv.Size = new Size(92, 20);
            this.m_initial_sec_adv.TabIndex = 17;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(91, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Initial Offset: (sec)";
            this.AcceptButton = (IButtonControl)this.m_submit;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = (IButtonControl)this.m_cancel;
            this.ClientSize = new Size(537, 228);
            this.Controls.Add((Control)this.tabControl1);
            this.Controls.Add((Control)this.m_cancel);
            this.Controls.Add((Control)this.m_submit);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = nameof(NewServerpollEvent);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Create New Shell Event";
            ((ISupportInitialize)this.m_errors).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.BasicPage.ResumeLayout(false);
            this.BasicPage.PerformLayout();
            this.AdvancedPage.ResumeLayout(false);
            this.AdvancedPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
