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
    public class NewScriptEvent : Form
    {
        private IContainer components;
        private Button m_submit;
        private Button m_cancel;
        private ErrorProvider m_errors;
        private TabControl m_newScriptTabControl;
        private TabPage BasicPage;
        private TextBox m_basic_path;
        private Label m_recurrenceLabel;
        private Label label2;
        private Label scriptlocationlabel;
        private TabPage AdvancedPage;
        private Label label5;
        private Label label3;
        private TextBox Instructions;
        private TextBox m_advanced_cronExpression;
        private TextBox m_advanced_path;
        private TextBox m_basic_recurrence;
        private TextBox m_basic_initial;
        private TextBox m_basic_name;
        private Label label1;
        private Label label4;
        private TextBox m_advanced_name;
        private TextBox m_advanced_initial;
        private Label label6;

        public NewScriptEvent() => this.InitializeComponent();

        private void OnSubmitClick(object sender, EventArgs e)
        {
            this.m_errors.SetError((Control)this.m_basic_path, string.Empty);
            this.m_errors.SetError((Control)this.m_basic_name, string.Empty);
            this.m_errors.SetError((Control)this.m_basic_initial, string.Empty);
            this.m_errors.SetError((Control)this.m_basic_recurrence, string.Empty);
            this.m_errors.SetError((Control)this.m_advanced_initial, string.Empty);
            this.m_errors.SetError((Control)this.m_advanced_cronExpression, string.Empty);
            this.m_errors.SetError((Control)this.m_advanced_path, string.Empty);
            this.m_errors.SetError((Control)this.m_advanced_name, string.Empty);
            bool flag = false;
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            ErrorList errorList = new ErrorList();
            if (this.m_newScriptTabControl.SelectedTab == this.BasicPage)
            {
                if (string.IsNullOrEmpty(this.m_basic_path.Text))
                {
                    this.m_errors.SetError((Control)this.m_basic_path, "Path is a required field.");
                    flag = true;
                }
                if (string.IsNullOrEmpty(this.m_basic_name.Text))
                {
                    this.m_errors.SetError((Control)this.m_basic_name, "Name is a required field.");
                    flag = true;
                }
                TimeSpan? startOffset = new TimeSpan?();
                if (!string.IsNullOrEmpty(this.m_basic_initial.Text))
                {
                    int result;
                    if (int.TryParse(this.m_basic_initial.Text, out result))
                    {
                        startOffset = new TimeSpan?(new TimeSpan(0, 0, result));
                    }
                    else
                    {
                        this.m_errors.SetError((Control)this.m_basic_initial, "Initial Offset is not a valid integer value.");
                        flag = true;
                    }
                }
                if (string.IsNullOrEmpty(this.m_basic_recurrence.Text))
                {
                    this.m_errors.SetError((Control)this.m_basic_recurrence, "Recurrance is a required field.");
                    flag = true;
                }
                if (flag)
                    return;
                TimeSpan repeatInterval = new TimeSpan(0, 0, int.Parse(this.m_basic_recurrence.Text));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleSimpleJob(this.m_basic_name.Text, this.m_basic_path.Text.Escaped(), PayloadType.Script, startOffset, new DateTime?(), new DateTime?(), repeatInterval));
            }
            else
            {
                if (string.IsNullOrEmpty(this.m_advanced_cronExpression.Text))
                {
                    this.m_errors.SetError((Control)this.m_advanced_cronExpression, "Expression is a required field.");
                    flag = true;
                }
                TimeSpan? startOffset = new TimeSpan?();
                if (!string.IsNullOrEmpty(this.m_advanced_initial.Text))
                {
                    int result;
                    if (int.TryParse(this.m_advanced_initial.Text, out result))
                    {
                        startOffset = new TimeSpan?(new TimeSpan(0, 0, result));
                    }
                    else
                    {
                        this.m_errors.SetError((Control)this.m_advanced_initial, "Initial Offset is not a valid integer value.");
                        flag = true;
                    }
                }
                if (string.IsNullOrEmpty(this.m_advanced_path.Text))
                {
                    this.m_errors.SetError((Control)this.m_advanced_path, "Path is a required field.");
                    flag = true;
                }
                if (string.IsNullOrEmpty(this.m_advanced_name.Text))
                {
                    this.m_errors.SetError((Control)this.m_advanced_name, "Name is a required field.");
                    flag = true;
                }
                if (flag)
                    return;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleCronJob(this.m_advanced_name.Text, this.m_advanced_path.Text.Escaped(), PayloadType.Script, startOffset, new DateTime?(), new DateTime?(), this.m_advanced_cronExpression.Text));
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

        private void OnCancel(object sender, EventArgs e) => this.Close();

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new System.ComponentModel.Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(NewScriptEvent));
            this.m_submit = new Button();
            this.m_cancel = new Button();
            this.m_errors = new ErrorProvider(this.components);
            this.m_newScriptTabControl = new TabControl();
            this.BasicPage = new TabPage();
            this.m_basic_name = new TextBox();
            this.label1 = new Label();
            this.m_basic_recurrence = new TextBox();
            this.m_basic_initial = new TextBox();
            this.m_basic_path = new TextBox();
            this.m_recurrenceLabel = new Label();
            this.label2 = new Label();
            this.scriptlocationlabel = new Label();
            this.AdvancedPage = new TabPage();
            this.label4 = new Label();
            this.m_advanced_name = new TextBox();
            this.Instructions = new TextBox();
            this.m_advanced_cronExpression = new TextBox();
            this.m_advanced_path = new TextBox();
            this.label5 = new Label();
            this.label3 = new Label();
            this.m_advanced_initial = new TextBox();
            this.label6 = new Label();
            ((ISupportInitialize)this.m_errors).BeginInit();
            this.m_newScriptTabControl.SuspendLayout();
            this.BasicPage.SuspendLayout();
            this.AdvancedPage.SuspendLayout();
            this.SuspendLayout();
            this.m_submit.Location = new Point(452, 12);
            this.m_submit.Name = "m_submit";
            this.m_submit.Size = new Size(75, 23);
            this.m_submit.TabIndex = 5;
            this.m_submit.Text = "OK";
            this.m_submit.UseVisualStyleBackColor = true;
            this.m_submit.Click += new EventHandler(this.OnSubmitClick);
            this.m_cancel.DialogResult = DialogResult.Cancel;
            this.m_cancel.Location = new Point(452, 41);
            this.m_cancel.Name = "m_cancel";
            this.m_cancel.Size = new Size(75, 23);
            this.m_cancel.TabIndex = 6;
            this.m_cancel.Text = "Cancel";
            this.m_cancel.UseVisualStyleBackColor = true;
            this.m_cancel.Click += new EventHandler(this.OnCancel);
            this.m_errors.ContainerControl = (ContainerControl)this;
            this.m_newScriptTabControl.Controls.Add((Control)this.BasicPage);
            this.m_newScriptTabControl.Controls.Add((Control)this.AdvancedPage);
            this.m_newScriptTabControl.Location = new Point(12, 12);
            this.m_newScriptTabControl.Name = "m_newScriptTabControl";
            this.m_newScriptTabControl.SelectedIndex = 0;
            this.m_newScriptTabControl.Size = new Size(434, 234);
            this.m_newScriptTabControl.TabIndex = 12;
            this.BasicPage.Controls.Add((Control)this.m_basic_name);
            this.BasicPage.Controls.Add((Control)this.label1);
            this.BasicPage.Controls.Add((Control)this.m_basic_recurrence);
            this.BasicPage.Controls.Add((Control)this.m_basic_initial);
            this.BasicPage.Controls.Add((Control)this.m_basic_path);
            this.BasicPage.Controls.Add((Control)this.m_recurrenceLabel);
            this.BasicPage.Controls.Add((Control)this.label2);
            this.BasicPage.Controls.Add((Control)this.scriptlocationlabel);
            this.BasicPage.Location = new Point(4, 22);
            this.BasicPage.Name = "BasicPage";
            this.BasicPage.Padding = new Padding(3);
            this.BasicPage.Size = new Size(426, 208);
            this.BasicPage.TabIndex = 0;
            this.BasicPage.Text = "Basic";
            this.BasicPage.UseVisualStyleBackColor = true;
            this.m_basic_name.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.m_basic_name.AutoCompleteSource = AutoCompleteSource.FileSystem;
            this.m_basic_name.Location = new Point(119, 36);
            this.m_basic_name.Name = "m_basic_name";
            this.m_basic_name.Size = new Size(186, 20);
            this.m_basic_name.TabIndex = 2;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(6, 39);
            this.label1.Name = "label1";
            this.label1.Size = new Size(38, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Name:";
            this.m_basic_recurrence.Location = new Point(119, 88);
            this.m_basic_recurrence.Name = "m_basic_recurrence";
            this.m_basic_recurrence.Size = new Size(92, 20);
            this.m_basic_recurrence.TabIndex = 4;
            this.m_basic_initial.Location = new Point(119, 62);
            this.m_basic_initial.Name = "m_basic_initial";
            this.m_basic_initial.Size = new Size(92, 20);
            this.m_basic_initial.TabIndex = 3;
            this.m_basic_path.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.m_basic_path.AutoCompleteSource = AutoCompleteSource.FileSystem;
            this.m_basic_path.Location = new Point(119, 10);
            this.m_basic_path.Name = "m_basic_path";
            this.m_basic_path.Size = new Size(297, 20);
            this.m_basic_path.TabIndex = 1;
            this.m_recurrenceLabel.AutoSize = true;
            this.m_recurrenceLabel.Location = new Point(6, 91);
            this.m_recurrenceLabel.Name = "m_recurrenceLabel";
            this.m_recurrenceLabel.Size = new Size(92, 13);
            this.m_recurrenceLabel.TabIndex = 18;
            this.m_recurrenceLabel.Text = "Recurrence: (sec)";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(6, 65);
            this.label2.Name = "label2";
            this.label2.Size = new Size(91, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Initial Offset: (sec)";
            this.scriptlocationlabel.AutoSize = true;
            this.scriptlocationlabel.Location = new Point(6, 13);
            this.scriptlocationlabel.Name = "scriptlocationlabel";
            this.scriptlocationlabel.Size = new Size(32, 13);
            this.scriptlocationlabel.TabIndex = 12;
            this.scriptlocationlabel.Text = "Path:";
            this.AdvancedPage.Controls.Add((Control)this.m_advanced_initial);
            this.AdvancedPage.Controls.Add((Control)this.label6);
            this.AdvancedPage.Controls.Add((Control)this.label4);
            this.AdvancedPage.Controls.Add((Control)this.m_advanced_name);
            this.AdvancedPage.Controls.Add((Control)this.Instructions);
            this.AdvancedPage.Controls.Add((Control)this.m_advanced_cronExpression);
            this.AdvancedPage.Controls.Add((Control)this.m_advanced_path);
            this.AdvancedPage.Controls.Add((Control)this.label5);
            this.AdvancedPage.Controls.Add((Control)this.label3);
            this.AdvancedPage.Location = new Point(4, 22);
            this.AdvancedPage.Name = "AdvancedPage";
            this.AdvancedPage.Padding = new Padding(3);
            this.AdvancedPage.Size = new Size(426, 208);
            this.AdvancedPage.TabIndex = 1;
            this.AdvancedPage.Text = "Advanced";
            this.AdvancedPage.UseVisualStyleBackColor = true;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(14, 10);
            this.label4.Name = "label4";
            this.label4.Size = new Size(38, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Name:";
            this.m_advanced_name.Location = new Point(106, 7);
            this.m_advanced_name.Name = "m_advanced_name";
            this.m_advanced_name.Size = new Size(268, 20);
            this.m_advanced_name.TabIndex = 1;
            this.Instructions.BackColor = SystemColors.Control;
            this.Instructions.BorderStyle = BorderStyle.None;
            this.Instructions.Cursor = Cursors.Default;
            this.Instructions.Font = new Font("Lucida Console", 7f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.Instructions.Location = new Point(111, 114);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new Size(312, 90);
            this.Instructions.TabIndex = 4;
            this.Instructions.Text = componentResourceManager.GetString("Instructions.Text");
            this.m_advanced_cronExpression.Location = new Point(106, 88);
            this.m_advanced_cronExpression.Name = "m_advanced_cronExpression";
            this.m_advanced_cronExpression.Size = new Size(268, 20);
            this.m_advanced_cronExpression.TabIndex = 4;
            this.m_advanced_path.Location = new Point(106, 34);
            this.m_advanced_path.Name = "m_advanced_path";
            this.m_advanced_path.Size = new Size(268, 20);
            this.m_advanced_path.TabIndex = 2;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(14, 91);
            this.label5.Name = "label5";
            this.label5.Size = new Size(86, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Cron Expression:";
            this.label3.AutoSize = true;
            this.label3.Location = new Point(14, 37);
            this.label3.Name = "label3";
            this.label3.Size = new Size(32, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Path:";
            this.m_advanced_initial.Location = new Point(106, 61);
            this.m_advanced_initial.Name = "m_advanced_initial";
            this.m_advanced_initial.Size = new Size(92, 20);
            this.m_advanced_initial.TabIndex = 3;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(14, 64);
            this.label6.Name = "label6";
            this.label6.Size = new Size(91, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Initial Offset: (sec)";
            this.AcceptButton = (IButtonControl)this.m_submit;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = (IButtonControl)this.m_cancel;
            this.ClientSize = new Size(540, 258);
            this.Controls.Add((Control)this.m_newScriptTabControl);
            this.Controls.Add((Control)this.m_cancel);
            this.Controls.Add((Control)this.m_submit);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = nameof(NewScriptEvent);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Create New Script Event";
            ((ISupportInitialize)this.m_errors).EndInit();
            this.m_newScriptTabControl.ResumeLayout(false);
            this.BasicPage.ResumeLayout(false);
            this.BasicPage.PerformLayout();
            this.AdvancedPage.ResumeLayout(false);
            this.AdvancedPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
