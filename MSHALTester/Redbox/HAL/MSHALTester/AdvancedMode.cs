using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

public class AdvancedMode : Form
{
    private readonly BindingList<HardwareJobAdapter> BindingList;
    private readonly BindingSource BindingSource;
    private readonly List<HardwareJobAdapter> JobList = new();
    private readonly object ListLock = new();
    private readonly ButtonAspectsManager Manager;
    private readonly ConfigPropertyList PropertyList;
    private readonly HardwareService Service;
    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;
    private IContainer components;
    private GroupBox groupBox1;
    private Button m_exitButton;
    private DataGridView m_jobListDataView;
    private Timer m_jobTimer;
    private Button m_saveConfigButton;
    private TextBox m_vendMoveOutput;
    private Panel panel1;

    public AdvancedMode(HardwareService service)
    {
        InitializeComponent();
        Manager = new ButtonAspectsManager();
        Service = service;
        PropertyList = new ConfigPropertyList(service);
        panel1.Controls.Add(PropertyList);
        PropertyList.OnConfigItemChange += PropertyList_OnConfigItemChange;
        PropertyList.OnSave += PropertyList_OnSave;
        BindingList = new BindingList<HardwareJobAdapter>(JobList);
        BindingSource = new BindingSource(BindingList, null);
        m_jobListDataView.DataSource = BindingSource;
        RefreshJobs();
        m_jobListDataView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        m_jobListDataView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        m_jobTimer.Enabled = true;
    }

    private void PropertyList_OnSave()
    {
        m_saveConfigButton.Enabled = false;
    }

    private void PropertyList_OnConfigItemChange()
    {
        m_saveConfigButton.Enabled = true;
    }

    private void m_exitButton_Click(object sender, EventArgs e)
    {
        m_jobTimer.Enabled = false;
        PropertyList.OnSave -= PropertyList_OnSave;
        PropertyList.Dispose();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        m_jobTimer.Enabled = false;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        try
        {
            var num = (int)new DeckConfigurationForm(Service).ShowDialog();
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("Unable to run Decks form.", ex);
        }
    }

    private void RefreshJobs()
    {
        JobList.ForEach(s => s.Removable = true);
        HardwareJob[] jobs;
        if (Service.GetJobs(out jobs).Success)
            foreach (var hardwareJob in jobs)
            {
                var nj = hardwareJob;
                var hardwareJobAdapter = JobList.Find(each => each.ID == nj.ID);
                if (hardwareJobAdapter == null)
                {
                    BindingSource.Add(new HardwareJobAdapter(nj));
                }
                else
                {
                    hardwareJobAdapter.Merge(nj);
                    hardwareJobAdapter.Removable = false;
                }
            }

        foreach (object obj in JobList.FindAll(each => each.Removable))
            BindingSource.Remove(obj);
    }

    private void m_saveConfigButton_Click(object sender, EventArgs e)
    {
        PropertyList.Save();
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
        OnListAction(j =>
        {
            if (j.Status == HardwareJobStatus.Suspended)
                return;
            j.Job.Suspend();
        });
    }

    private void OnListAction(Action<HardwareJobAdapter> action)
    {
        foreach (DataGridViewRow selectedRow in m_jobListDataView.SelectedRows)
        {
            var dataBoundItem = selectedRow.DataBoundItem as HardwareJobAdapter;
            action(dataBoundItem);
        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        OnListAction(j =>
        {
            if (j.Status != HardwareJobStatus.Suspended)
                return;
            j.Job.Pend();
        });
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        RefreshJobs();
    }

    private void button4_Click(object sender, EventArgs e)
    {
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            var tagInstruction = buttonAspects.GetTagInstruction();
            using (var instructionHelper = new InstructionHelper(Service))
            {
                m_vendMoveOutput.Text = instructionHelper.ExecuteErrorCode(tagInstruction).ToString().ToUpper();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        panel1 = new Panel();
        m_exitButton = new Button();
        button2 = new Button();
        m_saveConfigButton = new Button();
        m_jobListDataView = new DataGridView();
        button1 = new Button();
        button3 = new Button();
        button4 = new Button();
        m_jobTimer = new Timer(components);
        m_vendMoveOutput = new TextBox();
        groupBox1 = new GroupBox();
        ((ISupportInitialize)m_jobListDataView).BeginInit();
        groupBox1.SuspendLayout();
        SuspendLayout();
        panel1.Location = new Point(12, 12);
        panel1.Name = "panel1";
        panel1.Size = new Size(371, 492);
        panel1.TabIndex = 0;
        m_exitButton.BackColor = Color.GreenYellow;
        m_exitButton.Location = new Point(583, 461);
        m_exitButton.Name = "m_exitButton";
        m_exitButton.Size = new Size(112, 42);
        m_exitButton.TabIndex = 1;
        m_exitButton.Text = "Exit";
        m_exitButton.UseVisualStyleBackColor = false;
        m_exitButton.Click += m_exitButton_Click;
        button2.Location = new Point(437, 461);
        button2.Name = "button2";
        button2.Size = new Size(113, 43);
        button2.TabIndex = 2;
        button2.Text = "Cancel";
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        m_saveConfigButton.Enabled = false;
        m_saveConfigButton.Location = new Point(400, 364);
        m_saveConfigButton.Name = "m_saveConfigButton";
        m_saveConfigButton.Size = new Size(150, 65);
        m_saveConfigButton.TabIndex = 4;
        m_saveConfigButton.Text = "Save Configuration Change";
        m_saveConfigButton.UseVisualStyleBackColor = true;
        m_saveConfigButton.Click += m_saveConfigButton_Click;
        m_jobListDataView.AllowUserToAddRows = false;
        m_jobListDataView.AllowUserToDeleteRows = false;
        m_jobListDataView.AllowUserToOrderColumns = true;
        m_jobListDataView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        m_jobListDataView.Location = new Point(18, 19);
        m_jobListDataView.Name = "m_jobListDataView";
        m_jobListDataView.ReadOnly = true;
        m_jobListDataView.Size = new Size(536, 150);
        m_jobListDataView.TabIndex = 5;
        button1.Location = new Point(18, 175);
        button1.Name = "button1";
        button1.Size = new Size(145, 50);
        button1.TabIndex = 6;
        button1.Text = "Suspend Selected";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click_1;
        button3.Location = new Point(409, 175);
        button3.Name = "button3";
        button3.Size = new Size(145, 50);
        button3.TabIndex = 7;
        button3.Text = "Resume Selected";
        button3.UseVisualStyleBackColor = true;
        button3.Click += button3_Click;
        button4.Location = new Point(400, 284);
        button4.Name = "button4";
        button4.Size = new Size(150, 54);
        button4.TabIndex = 8;
        button4.Tag = "MOVEVEND";
        button4.Text = "Move To Vend";
        button4.UseVisualStyleBackColor = true;
        button4.Click += button4_Click;
        m_jobTimer.Interval = 1000;
        m_jobTimer.Tick += timer1_Tick;
        m_vendMoveOutput.Location = new Point(556, 318);
        m_vendMoveOutput.Name = "m_vendMoveOutput";
        m_vendMoveOutput.ReadOnly = true;
        m_vendMoveOutput.Size = new Size(139, 20);
        m_vendMoveOutput.TabIndex = 9;
        groupBox1.Controls.Add(m_jobListDataView);
        groupBox1.Controls.Add(button1);
        groupBox1.Controls.Add(button3);
        groupBox1.Location = new Point(400, 12);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(569, 240);
        groupBox1.TabIndex = 10;
        groupBox1.TabStop = false;
        groupBox1.Text = "Job Control";
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(990, 520);
        Controls.Add(groupBox1);
        Controls.Add(m_vendMoveOutput);
        Controls.Add(button4);
        Controls.Add(m_saveConfigButton);
        Controls.Add(button2);
        Controls.Add(m_exitButton);
        Controls.Add(panel1);
        Name = nameof(AdvancedMode);
        Text = nameof(AdvancedMode);
        ((ISupportInitialize)m_jobListDataView).EndInit();
        groupBox1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}