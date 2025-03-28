using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class ManageTimersForm : Form
  {
    private IContainer components;
    private Button m_closeButton;
    private ListView m_listView;
    private ColumnHeader m_nameColumnHeader;
    private ColumnHeader m_dueTimeColumnHeader;
    private ColumnHeader m_periodTimeColumnHeader;
    private ColumnHeader m_statusColumnHeader;
    private Button m_startButton;
    private Button m_stopButton;
    private Button m_removeButton;
    private Button m_clearButton;
    private Button m_refreshButton;
    private Button m_executeButton;

    public ManageTimersForm() => this.InitializeComponent();

    private void OnStart(object sender, EventArgs e)
    {
      if (this.m_listView.SelectedItems.Count == 0)
        return;
      foreach (ListViewItem selectedItem in this.m_listView.SelectedItems)
      {
        if (selectedItem.Tag is SoftTimer tag)
          tag.Enabled = true;
      }
      this.RefreshList();
    }

    private void OnStop(object sender, EventArgs e)
    {
      if (this.m_listView.SelectedItems.Count == 0)
        return;
      foreach (ListViewItem selectedItem in this.m_listView.SelectedItems)
      {
        if (selectedItem.Tag is SoftTimer tag)
          tag.Enabled = false;
      }
      this.RefreshList();
    }

    private void OnRemove(object sender, EventArgs e)
    {
      if (this.m_listView.SelectedItems.Count == 0)
        return;
      foreach (ListViewItem selectedItem in this.m_listView.SelectedItems)
      {
        if (selectedItem.Tag is SoftTimer tag)
          TimerService.Instance.RemoveTimer(tag.Name);
      }
      this.RefreshList();
    }

    private void OnClear(object sender, EventArgs e) => TimerService.Instance.Reset();

    private void OnClose(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void OnSelectedTimerChanged(object sender, EventArgs e) => this.RefreshButtonState();

    private void OnLoad(object sender, EventArgs e)
    {
      Cursor.Show();
      this.RefreshList();
    }

    private void OnFormClosed(object sender, FormClosedEventArgs e) => Cursor.Hide();

    private void OnRefresh(object sender, EventArgs e) => this.RefreshList();

    private void OnExecute(object sender, EventArgs e)
    {
      if (this.m_listView.SelectedItems.Count == 0)
        return;
      foreach (ListViewItem selectedItem in this.m_listView.SelectedItems)
      {
        if (selectedItem.Tag is SoftTimer tag)
          tag.RaiseFire();
      }
    }

    private void RefreshList()
    {
      this.m_listView.BeginUpdate();
      this.m_listView.Items.Clear();
      foreach (ITimer timer in TimerService.Instance.Timers)
      {
        ListViewItem listViewItem = new ListViewItem(timer.Name)
        {
          Tag = (object) timer
        };
        listViewItem.SubItems.Add(timer.DueTime.ToString());
        listViewItem.SubItems.Add(timer.Period.ToString());
        listViewItem.SubItems.Add(timer.Enabled ? "Enabled" : "Disabled");
        this.m_listView.Items.Add(listViewItem);
      }
      this.m_listView.EndUpdate();
      this.RefreshButtonState();
    }

    private void RefreshButtonState()
    {
      this.m_startButton.Enabled = this.m_listView.SelectedItems.Count > 0;
      this.m_stopButton.Enabled = this.m_listView.SelectedItems.Count > 0;
      this.m_removeButton.Enabled = this.m_listView.SelectedItems.Count > 0;
      this.m_clearButton.Enabled = this.m_listView.Items.Count > 0;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.m_closeButton = new Button();
      this.m_listView = new ListView();
      this.m_nameColumnHeader = new ColumnHeader();
      this.m_dueTimeColumnHeader = new ColumnHeader();
      this.m_periodTimeColumnHeader = new ColumnHeader();
      this.m_statusColumnHeader = new ColumnHeader();
      this.m_startButton = new Button();
      this.m_stopButton = new Button();
      this.m_removeButton = new Button();
      this.m_clearButton = new Button();
      this.m_refreshButton = new Button();
      this.m_executeButton = new Button();
      this.SuspendLayout();
      this.m_closeButton.Location = new Point(542, 251);
      this.m_closeButton.Name = "m_closeButton";
      this.m_closeButton.Size = new Size(75, 23);
      this.m_closeButton.TabIndex = 7;
      this.m_closeButton.Text = "Close";
      this.m_closeButton.UseVisualStyleBackColor = true;
      this.m_closeButton.Click += new EventHandler(this.OnClose);
      this.m_listView.Columns.AddRange(new ColumnHeader[4]
      {
        this.m_nameColumnHeader,
        this.m_dueTimeColumnHeader,
        this.m_periodTimeColumnHeader,
        this.m_statusColumnHeader
      });
      this.m_listView.FullRowSelect = true;
      this.m_listView.HideSelection = false;
      this.m_listView.Location = new Point(12, 12);
      this.m_listView.Name = "m_listView";
      this.m_listView.Size = new Size(605, 233);
      this.m_listView.TabIndex = 0;
      this.m_listView.UseCompatibleStateImageBehavior = false;
      this.m_listView.View = View.Details;
      this.m_listView.SelectedIndexChanged += new EventHandler(this.OnSelectedTimerChanged);
      this.m_nameColumnHeader.Text = "Name";
      this.m_nameColumnHeader.Width = 250;
      this.m_dueTimeColumnHeader.Text = "Due (ms)";
      this.m_dueTimeColumnHeader.Width = 105;
      this.m_periodTimeColumnHeader.Text = "Period (ms)";
      this.m_periodTimeColumnHeader.Width = 105;
      this.m_statusColumnHeader.Text = "Status";
      this.m_statusColumnHeader.Width = 95;
      this.m_startButton.Enabled = false;
      this.m_startButton.Location = new Point(114, 251);
      this.m_startButton.Name = "m_startButton";
      this.m_startButton.Size = new Size(75, 23);
      this.m_startButton.TabIndex = 2;
      this.m_startButton.Text = "Start";
      this.m_startButton.UseVisualStyleBackColor = true;
      this.m_startButton.Click += new EventHandler(this.OnStart);
      this.m_stopButton.Enabled = false;
      this.m_stopButton.Location = new Point(195, 251);
      this.m_stopButton.Name = "m_stopButton";
      this.m_stopButton.Size = new Size(75, 23);
      this.m_stopButton.TabIndex = 3;
      this.m_stopButton.Text = "Stop";
      this.m_stopButton.UseVisualStyleBackColor = true;
      this.m_stopButton.Click += new EventHandler(this.OnStop);
      this.m_removeButton.Enabled = false;
      this.m_removeButton.Location = new Point(357, 251);
      this.m_removeButton.Name = "m_removeButton";
      this.m_removeButton.Size = new Size(75, 23);
      this.m_removeButton.TabIndex = 5;
      this.m_removeButton.Text = "Remove";
      this.m_removeButton.UseVisualStyleBackColor = true;
      this.m_removeButton.Click += new EventHandler(this.OnRemove);
      this.m_clearButton.Enabled = false;
      this.m_clearButton.Location = new Point(438, 251);
      this.m_clearButton.Name = "m_clearButton";
      this.m_clearButton.Size = new Size(75, 23);
      this.m_clearButton.TabIndex = 6;
      this.m_clearButton.Text = "Clear";
      this.m_clearButton.UseVisualStyleBackColor = true;
      this.m_clearButton.Click += new EventHandler(this.OnClear);
      this.m_refreshButton.Location = new Point(12, 251);
      this.m_refreshButton.Name = "m_refreshButton";
      this.m_refreshButton.Size = new Size(75, 23);
      this.m_refreshButton.TabIndex = 1;
      this.m_refreshButton.Text = "Refresh";
      this.m_refreshButton.UseVisualStyleBackColor = true;
      this.m_refreshButton.Click += new EventHandler(this.OnRefresh);
      this.m_executeButton.Location = new Point(276, 251);
      this.m_executeButton.Name = "m_executeButton";
      this.m_executeButton.Size = new Size(75, 23);
      this.m_executeButton.TabIndex = 4;
      this.m_executeButton.Text = "Execute";
      this.m_executeButton.UseVisualStyleBackColor = true;
      this.m_executeButton.Click += new EventHandler(this.OnExecute);
      this.AcceptButton = (IButtonControl) this.m_closeButton;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(630, 286);
      this.Controls.Add((Control) this.m_executeButton);
      this.Controls.Add((Control) this.m_refreshButton);
      this.Controls.Add((Control) this.m_clearButton);
      this.Controls.Add((Control) this.m_removeButton);
      this.Controls.Add((Control) this.m_stopButton);
      this.Controls.Add((Control) this.m_startButton);
      this.Controls.Add((Control) this.m_listView);
      this.Controls.Add((Control) this.m_closeButton);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (ManageTimersForm);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Manage Timers";
      this.Load += new EventHandler(this.OnLoad);
      this.FormClosed += new FormClosedEventHandler(this.OnFormClosed);
      this.ResumeLayout(false);
    }
  }
}
