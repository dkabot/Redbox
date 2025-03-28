using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class ManageViewsForm : Form
  {
    private bool m_isActivated;
    private IContainer components;
    private ListView m_viewListView;
    private Button m_popButton;
    private Button m_pushButton;
    private Button m_showButton;
    private Button m_closeButton;
    private ColumnHeader m_viewNameColumnHeader;
    private ListView m_stackListView;
    private Button m_popDiscardButton;
    private ColumnHeader m_stackViewNameColumnHeader;
    private Label label1;
    private Label label2;
    private Button m_refreshButton;
    private ColumnHeader m_clearColumnHeader;
    private ColumnHeader m_sceneNameColumnHeader;
    private ColumnHeader m_widthColumnHeader;
    private ColumnHeader m_heightColumnHeader;

    public ManageViewsForm() => this.InitializeComponent();

    private void OnActivated(object sender, EventArgs e)
    {
      if (this.m_isActivated)
        return;
      Cursor.Show();
      this.RefreshViewResourceList();
      this.RefreshStackList();
      this.m_isActivated = true;
    }

    private void OnFormClosing(object sender, FormClosingEventArgs e) => Cursor.Hide();

    private void OnPush(object sender, EventArgs e)
    {
      try
      {
        this.Cursor = Cursors.WaitCursor;
        ViewService.Instance.Push(this.m_viewListView.SelectedItems[0].Text);
        this.RefreshStackList();
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void OnPop(object sender, EventArgs e)
    {
      try
      {
        this.Cursor = Cursors.WaitCursor;
        ViewService.Instance.Pop();
        this.RefreshStackList();
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void OnPopDiscard(object sender, EventArgs e)
    {
      try
      {
        this.Cursor = Cursors.WaitCursor;
        ViewService.Instance.PopDiscard();
        this.RefreshStackList();
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void OnShow(object sender, EventArgs e)
    {
      try
      {
        this.Cursor = Cursors.WaitCursor;
        ViewService.Instance.Show();
        this.RefreshStackList();
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void OnClose(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void OnRefresh(object sender, EventArgs e) => this.RefreshStackList();

    private void OnSelectedViewResourceChanged(object sender, EventArgs e)
    {
      this.m_pushButton.Enabled = this.m_viewListView.SelectedItems.Count > 0;
    }

    private void RefreshViewResourceList()
    {
      this.m_viewListView.BeginUpdate();
      this.m_viewListView.Items.Clear();
      IResourceBundleSet activeBundleSet = ResourceBundleService.Instance.ActiveBundleSet;
      if (activeBundleSet == null)
        return;
      foreach (IResource resource in activeBundleSet.GetResources("view", ResourceBundleService.Instance.Filter))
        this.m_viewListView.Items.Add(new ListViewItem(resource.Name)
        {
          Tag = (object) resource
        });
      this.m_viewListView.EndUpdate();
    }

    private void RefreshStackList()
    {
      this.m_stackListView.BeginUpdate();
      this.m_stackListView.Items.Clear();
      foreach (IViewFrameInstance viewFrameInstance in ViewService.Instance.Stack)
      {
        ListViewItem listViewItem = new ListViewItem(viewFrameInstance.ViewFrame.ViewName)
        {
          Tag = (object) viewFrameInstance
        };
        IViewFrame viewFrame = viewFrameInstance?.ViewFrame as IViewFrame;
        listViewItem.SubItems.Add(viewFrame != null ? viewFrame.SceneName : viewFrameInstance?.ViewFrame?.Scene?.Name);
        listViewItem.SubItems.Add(viewFrameInstance?.ViewFrame?.Clear.ToString());
        ListViewItem.ListViewSubItemCollection subItems1 = listViewItem.SubItems;
        int num;
        string text1;
        if (viewFrame == null)
        {
          text1 = (string) null;
        }
        else
        {
          num = viewFrame.Width;
          text1 = num.ToString();
        }
        subItems1.Add(text1);
        ListViewItem.ListViewSubItemCollection subItems2 = listViewItem.SubItems;
        string text2;
        if (viewFrame == null)
        {
          text2 = (string) null;
        }
        else
        {
          num = viewFrame.Height;
          text2 = num.ToString();
        }
        subItems2.Add(text2);
        this.m_stackListView.Items.Add(listViewItem);
      }
      this.m_stackListView.EndUpdate();
      this.m_popButton.Enabled = this.m_stackListView.Items.Count > 0;
      this.m_popDiscardButton.Enabled = this.m_stackListView.Items.Count > 0;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.m_viewListView = new ListView();
      this.m_viewNameColumnHeader = new ColumnHeader();
      this.m_popButton = new Button();
      this.m_pushButton = new Button();
      this.m_showButton = new Button();
      this.m_closeButton = new Button();
      this.m_stackListView = new ListView();
      this.m_stackViewNameColumnHeader = new ColumnHeader();
      this.m_sceneNameColumnHeader = new ColumnHeader();
      this.m_clearColumnHeader = new ColumnHeader();
      this.m_widthColumnHeader = new ColumnHeader();
      this.m_heightColumnHeader = new ColumnHeader();
      this.m_popDiscardButton = new Button();
      this.label1 = new Label();
      this.label2 = new Label();
      this.m_refreshButton = new Button();
      this.SuspendLayout();
      this.m_viewListView.Columns.AddRange(new ColumnHeader[1]
      {
        this.m_viewNameColumnHeader
      });
      this.m_viewListView.FullRowSelect = true;
      this.m_viewListView.Location = new Point(12, 25);
      this.m_viewListView.MultiSelect = false;
      this.m_viewListView.Name = "m_viewListView";
      this.m_viewListView.Size = new Size(190, 280);
      this.m_viewListView.TabIndex = 1;
      this.m_viewListView.UseCompatibleStateImageBehavior = false;
      this.m_viewListView.View = View.Details;
      this.m_viewListView.SelectedIndexChanged += new EventHandler(this.OnSelectedViewResourceChanged);
      this.m_viewNameColumnHeader.Text = "View Name";
      this.m_viewNameColumnHeader.Width = 185;
      this.m_popButton.Enabled = false;
      this.m_popButton.Location = new Point(690, 25);
      this.m_popButton.Name = "m_popButton";
      this.m_popButton.Size = new Size(86, 23);
      this.m_popButton.TabIndex = 5;
      this.m_popButton.Text = "Pop";
      this.m_popButton.UseVisualStyleBackColor = true;
      this.m_popButton.Click += new EventHandler(this.OnPop);
      this.m_pushButton.Enabled = false;
      this.m_pushButton.Location = new Point(208, 25);
      this.m_pushButton.Name = "m_pushButton";
      this.m_pushButton.Size = new Size(75, 23);
      this.m_pushButton.TabIndex = 2;
      this.m_pushButton.Text = "Push >>";
      this.m_pushButton.UseVisualStyleBackColor = true;
      this.m_pushButton.Click += new EventHandler(this.OnPush);
      this.m_showButton.Location = new Point(690, 83);
      this.m_showButton.Name = "m_showButton";
      this.m_showButton.Size = new Size(86, 23);
      this.m_showButton.TabIndex = 7;
      this.m_showButton.Text = "Show";
      this.m_showButton.UseVisualStyleBackColor = true;
      this.m_showButton.Click += new EventHandler(this.OnShow);
      this.m_closeButton.Location = new Point(690, 282);
      this.m_closeButton.Name = "m_closeButton";
      this.m_closeButton.Size = new Size(86, 23);
      this.m_closeButton.TabIndex = 9;
      this.m_closeButton.Text = "Close";
      this.m_closeButton.UseVisualStyleBackColor = true;
      this.m_closeButton.Click += new EventHandler(this.OnClose);
      this.m_stackListView.Columns.AddRange(new ColumnHeader[5]
      {
        this.m_stackViewNameColumnHeader,
        this.m_sceneNameColumnHeader,
        this.m_clearColumnHeader,
        this.m_widthColumnHeader,
        this.m_heightColumnHeader
      });
      this.m_stackListView.FullRowSelect = true;
      this.m_stackListView.Location = new Point(289, 25);
      this.m_stackListView.MultiSelect = false;
      this.m_stackListView.Name = "m_stackListView";
      this.m_stackListView.Size = new Size(395, 280);
      this.m_stackListView.TabIndex = 4;
      this.m_stackListView.UseCompatibleStateImageBehavior = false;
      this.m_stackListView.View = View.Details;
      this.m_stackViewNameColumnHeader.Text = "View Name";
      this.m_stackViewNameColumnHeader.Width = 125;
      this.m_sceneNameColumnHeader.Text = "Scene Name";
      this.m_sceneNameColumnHeader.Width = 100;
      this.m_clearColumnHeader.Text = "Clear?";
      this.m_clearColumnHeader.Width = 45;
      this.m_widthColumnHeader.Text = "Width";
      this.m_widthColumnHeader.TextAlign = HorizontalAlignment.Right;
      this.m_heightColumnHeader.Text = "Height";
      this.m_heightColumnHeader.TextAlign = HorizontalAlignment.Right;
      this.m_popDiscardButton.Enabled = false;
      this.m_popDiscardButton.Location = new Point(690, 54);
      this.m_popDiscardButton.Name = "m_popDiscardButton";
      this.m_popDiscardButton.Size = new Size(86, 23);
      this.m_popDiscardButton.TabIndex = 6;
      this.m_popDiscardButton.Text = "Pop Discard";
      this.m_popDiscardButton.UseVisualStyleBackColor = true;
      this.m_popDiscardButton.Click += new EventHandler(this.OnPopDiscard);
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new Size(87, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "View Resources:";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(286, 9);
      this.label2.Name = "label2";
      this.label2.Size = new Size(64, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "View Stack:";
      this.m_refreshButton.Location = new Point(690, 112);
      this.m_refreshButton.Name = "m_refreshButton";
      this.m_refreshButton.Size = new Size(86, 23);
      this.m_refreshButton.TabIndex = 8;
      this.m_refreshButton.Text = "Refresh";
      this.m_refreshButton.UseVisualStyleBackColor = true;
      this.m_refreshButton.Click += new EventHandler(this.OnRefresh);
      this.AcceptButton = (IButtonControl) this.m_closeButton;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(787, 319);
      this.ControlBox = false;
      this.Controls.Add((Control) this.m_refreshButton);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.m_popDiscardButton);
      this.Controls.Add((Control) this.m_stackListView);
      this.Controls.Add((Control) this.m_closeButton);
      this.Controls.Add((Control) this.m_showButton);
      this.Controls.Add((Control) this.m_pushButton);
      this.Controls.Add((Control) this.m_popButton);
      this.Controls.Add((Control) this.m_viewListView);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Name = nameof (ManageViewsForm);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Manage Views";
      this.Activated += new EventHandler(this.OnActivated);
      this.FormClosing += new FormClosingEventHandler(this.OnFormClosing);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
