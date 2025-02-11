using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class ManageScheduleEvents : Form
    {
        private IContainer components;
        private ListView m_events;
        private Button m_deleteEvent;
        private Button m_stopScheduler;
        private Button m_resetScheduler;
        private Button m_forceEvent;
        private Button m_startScheduler;
        private Button m_newScriptEvent;
        private Button m_newShellEvent;
        private Button m_refreshButton;
        private ColumnHeader payload;
        private ColumnHeader type;
        private ColumnHeader cronexpression;
        private ColumnHeader TaskName;
        private Button m_newServerpollEvent;

        public ManageScheduleEvents() => this.InitializeComponent();

        private void OnRestartSchdeuler(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Restart();
                if (errors.ContainsError())
                    UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate(new MethodInvoker(this.SetButtonState));
            }));
        }

        private void OnStopScheduler(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Stop();
                if (errors.ContainsError())
                    UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate(new MethodInvoker(this.SetButtonState));
            }));
        }

        private void OnStartScheduler(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Start();
                if (errors.ContainsError())
                    UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate(new MethodInvoker(this.SetButtonState));
            }));
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.LoadScheduleEvents();
            this.SetButtonState();
        }

        private void LoadScheduleEvents()
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)null, (Action)(() =>
            {
                ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
                List<ITask> tasks;
                try
                {
                    service.List(out tasks);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Could not load list", LogEntryType.Fatal);
                    UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(ex);
                    return;
                }
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
          {
                  try
                  {
                      this.m_events.BeginUpdate();
                      this.m_events.Items.Clear();
                      foreach (ITask task in tasks)
                      {
                          ListViewItem listViewItem = new ListViewItem()
                          {
                              Text = task.Name,
                              Tag = (object)task
                          };
                          listViewItem.SubItems.Add(task.Payload);
                          listViewItem.SubItems.Add(Enum.GetName(typeof(PayloadType), (object)task.PayloadType));
                          TimeSpan? nullable;
                          string cronExpression;
                          if (task.ScheduleType != ScheduleType.Cron)
                          {
                              nullable = task.RepeatInterval;
                              cronExpression = nullable.ToString();
                          }
                          else
                              cronExpression = task.CronExpression;
                          string text = cronExpression;
                          nullable = task.StartOffset;
                          if (nullable.HasValue)
                          {
                              string str3 = text;
                              nullable = task.StartOffset;
                              string str4 = nullable.ToString();
                              text = str3 + " Offset:" + str4;
                          }
                          listViewItem.SubItems.Add(text);
                          this.m_events.Items.Add(listViewItem);
                      }
                      this.m_events.SelectedIndices.Clear();
                      this.m_events.EndUpdate();
                  }
                  catch (Exception ex)
                  {
                      UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(ex);
                  }
              }));
            }));
        }

        private void OnNewScriptEvent(object sender, EventArgs e)
        {
            int num = (int)new NewScriptEvent().ShowDialog();
            this.LoadScheduleEvents();
        }

        private void OnNewShellEvent(object sender, EventArgs e)
        {
            int num = (int)new NewShellEvent().ShowDialog();
            this.LoadScheduleEvents();
        }

        private void OnNewServerpollEvent(object sender, EventArgs e)
        {
            int num = (int)new NewServerpollEvent().ShowDialog();
            this.LoadScheduleEvents();
        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (this.m_events.SelectedItems.Count < 1)
                return;
            try
            {
                ITaskScheduler scheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_deleteEvent.Enabled = false));
                try
                {
                    IEnumerable<ListViewItem> source = this.m_events.SelectedItems.Cast<ListViewItem>().Where<ListViewItem>((Func<ListViewItem, bool>)(item => item != null));
                    using (IEnumerator<ErrorForm> enumerator = source.Select<ListViewItem, ErrorList>((Func<ListViewItem, ErrorList>)(item => scheduler.Delete(item.SubItems[0].Text))).Where<ErrorList>((Func<ErrorList, bool>)(errors => errors.ContainsError())).Select<ErrorList, ErrorForm>((Func<ErrorList, ErrorForm>)(errors => new ErrorForm()
                    {
                        Errors = (IEnumerable)errors
                    })).GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            int num = (int)enumerator.Current.ShowDialog();
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorList errorList = new ErrorList();
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E992", "An unhandled exception occurred while polling update service.", ex));
                    ErrorList foo = errorList;
                    int num;
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                    {
                        Errors = ((IEnumerable)foo)
                    }.ShowDialog()));
                }
                finally
                {
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_deleteEvent.Enabled = true));
                }
                ErrorList errorList1 = scheduler.Restart();
                if (errorList1.ContainsError())
                {
                    int num1 = (int)new ErrorForm()
                    {
                        Errors = ((IEnumerable)errorList1)
                    }.ShowDialog();
                }
                this.LoadScheduleEvents();
            }
            catch (Exception ex)
            {
                ErrorList errorList2 = new ErrorList();
                errorList2.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E991", "An unhandled exception was thrown in Update Mananger.", ex));
                ErrorList errorList3 = errorList2;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList3)
                }.ShowDialog();
            }
        }

        private void OnForce(object sender, EventArgs e)
        {
            if (this.m_events.SelectedItems.Count < 1)
                return;
            try
            {
                List<ITask> list = this.m_events.SelectedItems.OfType<ListViewItem>().Select<ListViewItem, ITask>((Func<ListViewItem, ITask>)(item => (ITask)item.Tag)).Where<ITask>((Func<ITask, bool>)(scheduleEvent => scheduleEvent != null)).ToList<ITask>();
                if (list.Count > 0)
                {
                    ITaskScheduler scheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
                    UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
                    {
                        foreach (ITask task in list)
                        {
                            ErrorList errors = scheduler.ForceTask(task.Name, out bool _);
                            if (errors.ContainsError())
                            {
                                UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
                                break;
                            }
                        }
                    }));
                }
                this.LoadScheduleEvents();
            }
            catch (Exception ex)
            {
                ErrorList errorList1 = new ErrorList();
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E990", "An unhandled exception was thrown in Update Mananger.", ex));
                ErrorList errorList2 = errorList1;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList2)
                }.ShowDialog();
            }
        }

        private void OnRefresh(object sender, EventArgs e) => this.LoadScheduleEvents();

        private void SetButtonState()
        {
            bool isRunning = ServiceLocator.Instance.GetService<ITaskScheduler>().IsRunning;
            this.m_startScheduler.Enabled = !isRunning;
            this.m_stopScheduler.Enabled = isRunning;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.m_events = new ListView();
            this.TaskName = new ColumnHeader();
            this.payload = new ColumnHeader();
            this.type = new ColumnHeader();
            this.cronexpression = new ColumnHeader();
            this.m_deleteEvent = new Button();
            this.m_stopScheduler = new Button();
            this.m_resetScheduler = new Button();
            this.m_forceEvent = new Button();
            this.m_startScheduler = new Button();
            this.m_newScriptEvent = new Button();
            this.m_newShellEvent = new Button();
            this.m_refreshButton = new Button();
            this.m_newServerpollEvent = new Button();
            this.SuspendLayout();
            this.m_events.Columns.AddRange(new ColumnHeader[4]
            {
        this.TaskName,
        this.payload,
        this.type,
        this.cronexpression
            });
            this.m_events.FullRowSelect = true;
            this.m_events.Location = new Point(12, 83);
            this.m_events.Name = "m_events";
            this.m_events.ShowGroups = false;
            this.m_events.Size = new Size(608, 208);
            this.m_events.TabIndex = 6;
            this.m_events.UseCompatibleStateImageBehavior = false;
            this.m_events.View = View.Details;
            this.TaskName.Text = "Name";
            this.payload.DisplayIndex = 3;
            this.payload.Text = "Payload";
            this.payload.Width = 169;
            this.type.Text = "Type";
            this.type.Width = 78;
            this.cronexpression.DisplayIndex = 1;
            this.cronexpression.Text = "Schedule";
            this.cronexpression.Width = 312;
            this.m_deleteEvent.Location = new Point(546, 298);
            this.m_deleteEvent.Name = "m_deleteEvent";
            this.m_deleteEvent.Size = new Size(75, 23);
            this.m_deleteEvent.TabIndex = 9;
            this.m_deleteEvent.Text = "Delete";
            this.m_deleteEvent.UseVisualStyleBackColor = true;
            this.m_deleteEvent.Click += new EventHandler(this.OnDelete);
            this.m_stopScheduler.Enabled = false;
            this.m_stopScheduler.Location = new Point(94, 12);
            this.m_stopScheduler.Name = "m_stopScheduler";
            this.m_stopScheduler.Size = new Size(75, 23);
            this.m_stopScheduler.TabIndex = 1;
            this.m_stopScheduler.Text = "Stop";
            this.m_stopScheduler.UseVisualStyleBackColor = true;
            this.m_stopScheduler.Click += new EventHandler(this.OnStopScheduler);
            this.m_resetScheduler.Location = new Point(13, 12);
            this.m_resetScheduler.Name = "m_resetScheduler";
            this.m_resetScheduler.Size = new Size(75, 23);
            this.m_resetScheduler.TabIndex = 0;
            this.m_resetScheduler.Text = "Restart";
            this.m_resetScheduler.UseVisualStyleBackColor = true;
            this.m_resetScheduler.Click += new EventHandler(this.OnRestartSchdeuler);
            this.m_forceEvent.Location = new Point(94, 298);
            this.m_forceEvent.Name = "m_forceEvent";
            this.m_forceEvent.Size = new Size(75, 23);
            this.m_forceEvent.TabIndex = 8;
            this.m_forceEvent.Text = "Force";
            this.m_forceEvent.UseVisualStyleBackColor = true;
            this.m_forceEvent.Click += new EventHandler(this.OnForce);
            this.m_startScheduler.Enabled = false;
            this.m_startScheduler.Location = new Point(175, 12);
            this.m_startScheduler.Name = "m_startScheduler";
            this.m_startScheduler.Size = new Size(75, 23);
            this.m_startScheduler.TabIndex = 2;
            this.m_startScheduler.Text = "Start";
            this.m_startScheduler.UseVisualStyleBackColor = true;
            this.m_startScheduler.Click += new EventHandler(this.OnStartScheduler);
            this.m_newScriptEvent.Location = new Point(12, 55);
            this.m_newScriptEvent.Name = "m_newScriptEvent";
            this.m_newScriptEvent.Size = new Size(101, 23);
            this.m_newScriptEvent.TabIndex = 3;
            this.m_newScriptEvent.Text = "New Script Event";
            this.m_newScriptEvent.UseVisualStyleBackColor = true;
            this.m_newScriptEvent.Click += new EventHandler(this.OnNewScriptEvent);
            this.m_newShellEvent.Location = new Point(119, 55);
            this.m_newShellEvent.Name = "m_newShellEvent";
            this.m_newShellEvent.Size = new Size(101, 23);
            this.m_newShellEvent.TabIndex = 4;
            this.m_newShellEvent.Text = " New Shell Event";
            this.m_newShellEvent.UseVisualStyleBackColor = true;
            this.m_newShellEvent.Click += new EventHandler(this.OnNewShellEvent);
            this.m_refreshButton.Location = new Point(12, 298);
            this.m_refreshButton.Name = "m_refreshButton";
            this.m_refreshButton.Size = new Size(75, 23);
            this.m_refreshButton.TabIndex = 7;
            this.m_refreshButton.Text = "Refresh";
            this.m_refreshButton.UseVisualStyleBackColor = true;
            this.m_refreshButton.Click += new EventHandler(this.OnRefresh);
            this.m_newServerpollEvent.Location = new Point(226, 55);
            this.m_newServerpollEvent.Name = "m_newServerpollEvent";
            this.m_newServerpollEvent.Size = new Size(123, 23);
            this.m_newServerpollEvent.TabIndex = 10;
            this.m_newServerpollEvent.Text = " New Serverpoll Event";
            this.m_newServerpollEvent.UseVisualStyleBackColor = true;
            this.m_newServerpollEvent.Click += new EventHandler(this.OnNewServerpollEvent);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(633, 333);
            this.Controls.Add((Control)this.m_newServerpollEvent);
            this.Controls.Add((Control)this.m_refreshButton);
            this.Controls.Add((Control)this.m_newShellEvent);
            this.Controls.Add((Control)this.m_newScriptEvent);
            this.Controls.Add((Control)this.m_startScheduler);
            this.Controls.Add((Control)this.m_forceEvent);
            this.Controls.Add((Control)this.m_resetScheduler);
            this.Controls.Add((Control)this.m_stopScheduler);
            this.Controls.Add((Control)this.m_deleteEvent);
            this.Controls.Add((Control)this.m_events);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = nameof(ManageScheduleEvents);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Manage Schedule Events";
            this.Load += new EventHandler(this.OnLoad);
            this.ResumeLayout(false);
        }
    }
}
