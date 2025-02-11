using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class MainForm : Form
    {
        private IContainer components;
        private GroupBox BITSGroup;
        private GroupBox RepositoryGroup;
        private Button m_resetSelectedRepository;
        private Label m_revisiosForSelectedRepositoryLabel;
        private Label m_repositoryListLabel;
        private Button m_rebuild;
        private Button m_activateTo;
        private Button m_updateTo;
        private Button m_manageBITS;
        private Button m_resetBITS;
        private GroupBox updateServiceGroup;
        private Button m_pollUpdateService;
        private ListView m_changes;
        private ListView m_repositories;
        private ColumnHeader m_nameColumn;
        private ColumnHeader m_labelColumn;
        private ColumnHeader m_activeVersion;
        private ColumnHeader m_updatedVersion;
        private ColumnHeader m_hashColumn;
        private ColumnHeader m_labelColumnRevision;
        private Button m_details;
        private Button m_refresh;
        private Button m_refreshChanges;
        private ColumnHeader m_isActive;
        private Button m_optInOut;
        private GroupBox manageScheduleEvents;
        private Button m_resetScheduleEvent;
        private Button m_manageEventScheduler;
        private Button TrimButton;
        private Button m_serverPollUpdateService;

        public MainForm()
        {
            this.InitializeComponent();
            if (!UpdateManagerApplication.Instance.DeveloperMode)
                return;
            this.m_repositories.Columns[0].Width -= 86;
            this.m_repositories.Columns.Add(new ColumnHeader()
            {
                Text = "Subscription",
                Width = 86
            });
            this.m_optInOut.Visible = true;
        }

        private void ManageTransfersOnClick(object sender, EventArgs e)
        {
            try
            {
                new TransferManager().Show((IWin32Window)this);
            }
            catch (Exception ex)
            {
                ErrorList errorList1 = new ErrorList();
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Update Mananger in ManageTransfersOnClick.", ex));
                ErrorList errorList2 = errorList1;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList2)
                }.ShowDialog();
            }
        }

        private void OnResetBITS(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel all running transfer jobs?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
                return;
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<ITransferService>().CancelAll();
                if (!errors.ContainsError())
                    return;
                int num;
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errors)
                }.ShowDialog()));
            }));
        }

        private void OnPollUpdateService(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().Poll();
                if (!errors.ContainsError())
                    return;
                int num;
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errors)
                }.ShowDialog()));
            }));
        }

        private void OnLoad(object sender, EventArgs e)
        {
            try
            {
                this.LoadRepositories();
            }
            catch (Exception ex)
            {
                ErrorList errorList1 = new ErrorList();
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E998", "An unhandled exception was thrown in Update Mananger.", ex));
                ErrorList errorList2 = errorList1;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList2)
                }.ShowDialog();
            }
        }

        private void LoadRepositories(int? selected)
        {
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
            {
                this.m_repositories.BeginUpdate();
                this.m_repositories.Items.Clear();
            }));
            IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
            List<string> allRepositories = service.GetAllRepositories();
            Stopwatch stopwatch = new Stopwatch();
            foreach (string str in allRepositories)
            {
                stopwatch.Start();
                string repository = str;
                string activeLabel = service.GetActiveLabel(repository);
                string activeRevision = service.GetActiveRevision(repository);
                string stagedRevision = service.GetStagedRevision(repository);
                bool subscribed = service.Subscribed(repository);
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                {
                    ListViewItem listViewItem = new ListViewItem()
                    {
                        Text = repository,
                        Tag = (object)repository
                    };
                    listViewItem.SubItems.Add(activeLabel);
                    listViewItem.SubItems.Add(activeRevision.Substring(0, 8));
                    listViewItem.SubItems.Add(stagedRevision.Substring(0, 8));
                    if (UpdateManagerApplication.Instance.DeveloperMode)
                        listViewItem.SubItems.Add(subscribed ? "On" : "Off");
                    this.m_repositories.Items.Add(listViewItem);
                }));
                stopwatch.Stop();
            }
            this.m_repositories.SelectedIndices.Clear();
            if (selected.HasValue)
                this.m_repositories.SelectedIndices.Add(selected.Value);
            this.m_repositories.EndUpdate();
        }

        private void LoadRepositories()
        {
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.LoadRepositories(new int?())));
        }

        private void LoadChanges() => this.LoadChanges(new int?());

        private void LoadChanges(int? selected)
        {
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_changes.BeginUpdate()));
            try
            {
                int count = 0;
                string repositoryName = (string)null;
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                {
                    this.m_changes.Items.Clear();
                    count = this.m_repositories.SelectedItems.Count;
                    if (count != 1)
                        return;
                    repositoryName = (string)this.m_repositories.SelectedItems[0].Tag;
                }));
                if (count != 1)
                    return;
                IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                List<IRevLog> changes = service.GetAllChanges(repositoryName);
                string activeRevision = service.GetActiveRevision(repositoryName);
                string stagedRevision = service.GetStagedRevision(repositoryName);
                changes.Reverse();
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                {
                    foreach (IRevLog revLog in changes)
                    {
                        ListViewItem listViewItem = new ListViewItem()
                        {
                            Text = revLog.Hash.Substring(0, 8),
                            Tag = (object)revLog
                        };
                        listViewItem.SubItems.Add(revLog.Label);
                        if (revLog.Hash == activeRevision)
                            listViewItem.SubItems.Add("Active");
                        else
                            listViewItem.SubItems.Add("");
                        if (revLog.Hash == stagedRevision)
                            listViewItem.SubItems.Add("Staged");
                        else
                            listViewItem.SubItems.Add("");
                        this.m_changes.Items.Add(listViewItem);
                    }
                    this.m_changes.SelectedIndices.Clear();
                    if (!selected.HasValue)
                        return;
                    this.m_changes.SelectedIndices.Add(selected.Value);
                }));
            }
            catch (Exception ex)
            {
                UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(ex);
            }
            finally
            {
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_changes.EndUpdate()));
            }
        }

        private void OnRepositorySelectionChange(object sender, EventArgs e)
        {
            try
            {
                this.LoadChanges();
            }
            catch (Exception ex)
            {
                ErrorList errorList1 = new ErrorList();
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E997", "An unhandled exception was thrown in Update Mananger.", ex));
                ErrorList errorList2 = errorList1;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList2)
                }.ShowDialog();
            }
        }

        private void UpdateToSelectedVersion(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1 || this.m_changes.SelectedItems.Count < 1)
                return;
            string selectedRepository = (string)this.m_repositories.SelectedItems[0].Tag;
            if (!(this.m_changes.SelectedItems[0].Tag is IRevLog tag))
                return;
            string selectedChange = tag.Hash;
            int selectedRepositoryIndex = this.m_repositories.SelectedIndices[0];
            int selcetedChangeIndex = this.m_changes.SelectedIndices[0];
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_updateTo.Enabled = false));
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().UpdateTo(selectedRepository, selectedChange);
                    if (errors.ContainsError())
                    {
                        int num;
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                        {
                            Errors = ((IEnumerable)errors)
                        }.ShowDialog()));
                    }
                    else
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                  {
                      this.LoadRepositories(new int?(selectedRepositoryIndex));
                      this.LoadChanges(new int?(selcetedChangeIndex));
                  }));
                }
                catch (Exception ex)
                {
                    ErrorList errors = new ErrorList()
                {
            Redbox.UpdateManager.ComponentModel.Error.NewError("E996", "An unhandled exception occurred while polling update service.", ex)
                };
                    int num;
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                    {
                        Errors = ((IEnumerable)errors)
                    }.ShowDialog()));
                }
                finally
                {
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_updateTo.Enabled = true));
                }
            }));
        }

        private void ActivateToSelectedVersion(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1 || this.m_changes.SelectedItems.Count < 1)
                return;
            string selectedRepository = (string)this.m_repositories.SelectedItems[0].Tag;
            if (!(this.m_changes.SelectedItems[0].Tag is IRevLog tag))
                return;
            string selectedChange = tag.Hash;
            int selectedRepositoryIndex = this.m_repositories.SelectedIndices[0];
            int selcetedChangeIndex = this.m_changes.SelectedIndices[0];
            bool shouldReboot = false;
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_activateTo.Enabled = false));
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    bool deferredMove;
                    ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().ActivateTo(selectedRepository, selectedChange, out deferredMove);
                    if (errors.ContainsError())
                    {
                        int num;
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                        {
                            Errors = ((IEnumerable)errors)
                        }.ShowDialog()));
                    }
                    else
                    {
                        if (deferredMove)
                            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                      {
                        if (MessageBox.Show("To complete this activation this computer must restart. Would you like to reboot now?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
                            return;
                        shouldReboot = true;
                    }));
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                  {
                      this.LoadRepositories(new int?(selectedRepositoryIndex));
                      this.LoadChanges(new int?(selcetedChangeIndex));
                  }));
                    }
                }
                catch (Exception ex)
                {
                    ErrorList errors = new ErrorList()
                {
            Redbox.UpdateManager.ComponentModel.Error.NewError("E995", "An unhandled exception occurred while polling update service.", ex)
                };
                    int num;
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                    {
                        Errors = ((IEnumerable)errors)
                    }.ShowDialog()));
                }
                finally
                {
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_activateTo.Enabled = true));
                    if (shouldReboot)
                        ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.MinorUpgrade);
                }
            }));
        }

        private void OnRebuildToSelectedVersion(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1 || this.m_changes.SelectedItems.Count < 1)
                return;
            string selectedRepository = (string)this.m_repositories.SelectedItems[0].Tag;
            if (!(this.m_changes.SelectedItems[0].Tag is IRevLog tag))
                return;
            string selectedChange = tag.Hash;
            int selectedRepositoryIndex = this.m_repositories.SelectedIndices[0];
            int selcetedChangeIndex = this.m_changes.SelectedIndices[0];
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_rebuild.Enabled = false));
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().RebuildTo(selectedRepository, selectedChange);
                    if (errors.ContainsError())
                    {
                        int num;
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                        {
                            Errors = ((IEnumerable)errors)
                        }.ShowDialog()));
                    }
                    else
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                  {
                      this.LoadRepositories(new int?(selectedRepositoryIndex));
                      this.LoadChanges(new int?(selcetedChangeIndex));
                  }));
                }
                catch (Exception ex)
                {
                    ErrorList errors = new ErrorList()
                {
            Redbox.UpdateManager.ComponentModel.Error.NewError("E994", "An unhandled exception occurred while polling update service.", ex)
                };
                    int num;
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                    {
                        Errors = ((IEnumerable)errors)
                    }.ShowDialog()));
                }
                finally
                {
                    UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => this.m_rebuild.Enabled = true));
                }
            }));
        }

        private void OnResetSelectedRepository(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1)
                return;
            bool shouldReboot = false;
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                try
                {
                    foreach (object selectedItem in this.m_repositories.SelectedItems)
                    {
                        if (selectedItem is ListViewItem listViewItem2)
                        {
                            bool defferedDelete;
                            ServiceLocator.Instance.GetService<IRepositoryService>().Reset((string)listViewItem2.Tag, out defferedDelete);
                            if (defferedDelete)
                                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
                          {
                          if (MessageBox.Show("To complete this activation this computer must restart. Would you like to reboot now?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
                              return;
                          shouldReboot = true;
                      }));
                        }
                    }
                }
                finally
                {
                    this.LoadRepositories();
                    this.LoadChanges();
                    if (shouldReboot)
                        ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.MinorUpgrade);
                }
            }));
        }

        private void OnDetails(object sender, EventArgs e)
        {
            try
            {
                if (this.m_changes.SelectedItems.Count < 1 || !(this.m_changes.SelectedItems[0].Tag is IRevLog tag))
                    return;
                new ChangeDetails(tag).Show((IWin32Window)this);
            }
            catch (Exception ex)
            {
                ErrorList errorList1 = new ErrorList();
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E993", "An unhandled exception was thrown in Update Mananger.", ex));
                ErrorList errorList2 = errorList1;
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList2)
                }.ShowDialog();
            }
        }

        private void OnRefreshRepositories(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, new Action(this.LoadRepositories));
        }

        private void OnRefreshChanges(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, new Action(this.LoadChanges));
        }

        private void OnToggleSubscription(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1)
                return;
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                foreach (object selectedItem in this.m_repositories.SelectedItems)
                {
                    if (selectedItem is ListViewItem listViewItem2)
                    {
                        IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                        string tag = (string)listViewItem2.Tag;
                        if (service.Subscribed(tag))
                            service.DisableSubscription(tag);
                        else
                            service.EndableSubscription(tag);
                    }
                }
                this.LoadRepositories();
                this.LoadChanges();
            }));
        }

        private void OnManageEventSchedulerClick(object sender, EventArgs e)
        {
            new ManageScheduleEvents().Show((IWin32Window)this);
        }

        private void OnResetScheduleEventClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all running schedule events?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
                return;
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() => ServiceLocator.Instance.GetService<ITaskScheduler>().Clear()));
        }

        private void OnSelectedChangeChanged(object sender, EventArgs e)
        {
            if (this.m_changes.SelectedItems.Count > 0)
                this.m_activateTo.Enabled = this.m_rebuild.Enabled = this.m_updateTo.Enabled = this.m_details.Enabled = true;
            else
                this.m_activateTo.Enabled = this.m_rebuild.Enabled = this.m_updateTo.Enabled = this.m_details.Enabled = false;
        }

        private void TrimButton_Click(object sender, EventArgs e)
        {
            if (this.m_repositories.SelectedItems.Count < 1)
                return;
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                try
                {
                    foreach (object selectedItem in this.m_repositories.SelectedItems)
                    {
                        if (selectedItem is ListViewItem listViewItem2)
                            ServiceLocator.Instance.GetService<IRepositoryService>().TrimRepository((string)listViewItem2.Tag);
                    }
                }
                finally
                {
                    this.LoadRepositories();
                    this.LoadChanges();
                }
            }));
        }

        private void m_serverPollUpdateService_Click(object sender, EventArgs e)
        {
            UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, (Action)(() =>
            {
                ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll();
                if (!errors.ContainsError())
                    return;
                int num;
                UpdateManagerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errors)
                }.ShowDialog()));
            }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MainForm));
            this.BITSGroup = new GroupBox();
            this.m_manageBITS = new Button();
            this.m_resetBITS = new Button();
            this.RepositoryGroup = new GroupBox();
            this.TrimButton = new Button();
            this.m_optInOut = new Button();
            this.m_refreshChanges = new Button();
            this.m_refresh = new Button();
            this.m_details = new Button();
            this.m_changes = new ListView();
            this.m_hashColumn = new ColumnHeader();
            this.m_labelColumnRevision = new ColumnHeader();
            this.m_isActive = new ColumnHeader();
            this.m_repositories = new ListView();
            this.m_nameColumn = new ColumnHeader();
            this.m_labelColumn = new ColumnHeader();
            this.m_activeVersion = new ColumnHeader();
            this.m_updatedVersion = new ColumnHeader();
            this.m_rebuild = new Button();
            this.m_activateTo = new Button();
            this.m_updateTo = new Button();
            this.m_resetSelectedRepository = new Button();
            this.m_revisiosForSelectedRepositoryLabel = new Label();
            this.m_repositoryListLabel = new Label();
            this.updateServiceGroup = new GroupBox();
            this.m_serverPollUpdateService = new Button();
            this.m_pollUpdateService = new Button();
            this.manageScheduleEvents = new GroupBox();
            this.m_resetScheduleEvent = new Button();
            this.m_manageEventScheduler = new Button();
            this.BITSGroup.SuspendLayout();
            this.RepositoryGroup.SuspendLayout();
            this.updateServiceGroup.SuspendLayout();
            this.manageScheduleEvents.SuspendLayout();
            this.SuspendLayout();
            this.BITSGroup.Controls.Add((Control)this.m_manageBITS);
            this.BITSGroup.Controls.Add((Control)this.m_resetBITS);
            this.BITSGroup.Location = new Point(12, 12);
            this.BITSGroup.Name = "BITSGroup";
            this.BITSGroup.Size = new Size(187, 56);
            this.BITSGroup.TabIndex = 1;
            this.BITSGroup.TabStop = false;
            this.BITSGroup.Text = "Manage Transfers";
            this.m_manageBITS.Location = new Point(13, 22);
            this.m_manageBITS.Name = "m_manageBITS";
            this.m_manageBITS.Size = new Size(75, 23);
            this.m_manageBITS.TabIndex = 1;
            this.m_manageBITS.Text = "Manage";
            this.m_manageBITS.UseVisualStyleBackColor = true;
            this.m_manageBITS.Click += new EventHandler(this.ManageTransfersOnClick);
            this.m_resetBITS.Location = new Point(94, 22);
            this.m_resetBITS.Name = "m_resetBITS";
            this.m_resetBITS.Size = new Size(75, 23);
            this.m_resetBITS.TabIndex = 2;
            this.m_resetBITS.Text = "Reset";
            this.m_resetBITS.UseVisualStyleBackColor = true;
            this.m_resetBITS.Click += new EventHandler(this.OnResetBITS);
            this.RepositoryGroup.Controls.Add((Control)this.TrimButton);
            this.RepositoryGroup.Controls.Add((Control)this.m_optInOut);
            this.RepositoryGroup.Controls.Add((Control)this.m_refreshChanges);
            this.RepositoryGroup.Controls.Add((Control)this.m_refresh);
            this.RepositoryGroup.Controls.Add((Control)this.m_details);
            this.RepositoryGroup.Controls.Add((Control)this.m_changes);
            this.RepositoryGroup.Controls.Add((Control)this.m_repositories);
            this.RepositoryGroup.Controls.Add((Control)this.m_rebuild);
            this.RepositoryGroup.Controls.Add((Control)this.m_activateTo);
            this.RepositoryGroup.Controls.Add((Control)this.m_updateTo);
            this.RepositoryGroup.Controls.Add((Control)this.m_resetSelectedRepository);
            this.RepositoryGroup.Controls.Add((Control)this.m_revisiosForSelectedRepositoryLabel);
            this.RepositoryGroup.Controls.Add((Control)this.m_repositoryListLabel);
            this.RepositoryGroup.Location = new Point(8, 69);
            this.RepositoryGroup.Name = "RepositoryGroup";
            this.RepositoryGroup.Size = new Size(843, 417);
            this.RepositoryGroup.TabIndex = 9;
            this.RepositoryGroup.TabStop = false;
            this.RepositoryGroup.Text = "Manage Repositories";
            this.TrimButton.Location = new Point(288, 351);
            this.TrimButton.Name = "TrimButton";
            this.TrimButton.Size = new Size(104, 23);
            this.TrimButton.TabIndex = 17;
            this.TrimButton.Text = "Trim Revisions";
            this.TrimButton.UseVisualStyleBackColor = true;
            this.TrimButton.Click += new EventHandler(this.TrimButton_Click);
            this.m_optInOut.Location = new Point(87, 352);
            this.m_optInOut.Name = "m_optInOut";
            this.m_optInOut.Size = new Size(113, 23);
            this.m_optInOut.TabIndex = 16;
            this.m_optInOut.Text = "Toggle Subscription";
            this.m_optInOut.UseVisualStyleBackColor = true;
            this.m_optInOut.Visible = false;
            this.m_optInOut.Click += new EventHandler(this.OnToggleSubscription);
            this.m_refreshChanges.Location = new Point(497, 380);
            this.m_refreshChanges.Name = "m_refreshChanges";
            this.m_refreshChanges.Size = new Size(75, 23);
            this.m_refreshChanges.TabIndex = 34;
            this.m_refreshChanges.Text = "Refresh";
            this.m_refreshChanges.UseVisualStyleBackColor = true;
            this.m_refreshChanges.Click += new EventHandler(this.OnRefreshChanges);
            this.m_refresh.Location = new Point(6, 352);
            this.m_refresh.Name = "m_refresh";
            this.m_refresh.Size = new Size(75, 23);
            this.m_refresh.TabIndex = 14;
            this.m_refresh.Text = "Refresh";
            this.m_refresh.UseVisualStyleBackColor = true;
            this.m_refresh.Click += new EventHandler(this.OnRefreshRepositories);
            this.m_details.Enabled = false;
            this.m_details.Location = new Point(740, 351);
            this.m_details.Name = "m_details";
            this.m_details.Size = new Size(75, 23);
            this.m_details.TabIndex = 33;
            this.m_details.Text = "Details";
            this.m_details.UseVisualStyleBackColor = true;
            this.m_details.Click += new EventHandler(this.OnDetails);
            this.m_changes.Columns.AddRange(new ColumnHeader[3]
            {
        this.m_hashColumn,
        this.m_labelColumnRevision,
        this.m_isActive
            });
            this.m_changes.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_changes.FullRowSelect = true;
            this.m_changes.HideSelection = false;
            this.m_changes.Location = new Point(498, 43);
            this.m_changes.Name = "m_changes";
            this.m_changes.Size = new Size(339, 303);
            this.m_changes.TabIndex = 12;
            this.m_changes.UseCompatibleStateImageBehavior = false;
            this.m_changes.View = View.Details;
            this.m_changes.SelectedIndexChanged += new EventHandler(this.OnSelectedChangeChanged);
            this.m_hashColumn.Text = "Hash";
            this.m_hashColumn.Width = 96;
            this.m_labelColumnRevision.Text = "Label";
            this.m_labelColumnRevision.Width = 100;
            this.m_isActive.Text = "Status";
            this.m_isActive.Width = 136;
            this.m_repositories.Columns.AddRange(new ColumnHeader[4]
            {
        this.m_nameColumn,
        this.m_labelColumn,
        this.m_activeVersion,
        this.m_updatedVersion
            });
            this.m_repositories.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_repositories.FullRowSelect = true;
            this.m_repositories.HideSelection = false;
            this.m_repositories.Location = new Point(6, 43);
            this.m_repositories.Name = "m_repositories";
            this.m_repositories.ShowGroups = false;
            this.m_repositories.Size = new Size(486, 303);
            this.m_repositories.TabIndex = 10;
            this.m_repositories.UseCompatibleStateImageBehavior = false;
            this.m_repositories.View = View.Details;
            this.m_repositories.SelectedIndexChanged += new EventHandler(this.OnRepositorySelectionChange);
            this.m_nameColumn.Text = "Name";
            this.m_nameColumn.Width = 204;
            this.m_labelColumn.Text = "Active Label";
            this.m_labelColumn.Width = 85;
            this.m_activeVersion.Text = "Active Version";
            this.m_activeVersion.Width = 96;
            this.m_updatedVersion.Text = "Staged Version";
            this.m_updatedVersion.Width = 96;
            this.m_rebuild.Enabled = false;
            this.m_rebuild.Location = new Point(659, 351);
            this.m_rebuild.Name = "m_rebuild";
            this.m_rebuild.Size = new Size(75, 23);
            this.m_rebuild.TabIndex = 32;
            this.m_rebuild.Text = "Rebuild To";
            this.m_rebuild.UseVisualStyleBackColor = true;
            this.m_rebuild.Click += new EventHandler(this.OnRebuildToSelectedVersion);
            this.m_activateTo.Enabled = false;
            this.m_activateTo.Location = new Point(578, 351);
            this.m_activateTo.Name = "m_activateTo";
            this.m_activateTo.Size = new Size(75, 23);
            this.m_activateTo.TabIndex = 31;
            this.m_activateTo.Text = "Activate To";
            this.m_activateTo.UseVisualStyleBackColor = true;
            this.m_activateTo.Click += new EventHandler(this.ActivateToSelectedVersion);
            this.m_updateTo.Enabled = false;
            this.m_updateTo.Location = new Point(498, 351);
            this.m_updateTo.Name = "m_updateTo";
            this.m_updateTo.Size = new Size(75, 23);
            this.m_updateTo.TabIndex = 30;
            this.m_updateTo.Text = "Update To";
            this.m_updateTo.UseVisualStyleBackColor = true;
            this.m_updateTo.Click += new EventHandler(this.UpdateToSelectedVersion);
            this.m_resetSelectedRepository.Location = new Point(394, 351);
            this.m_resetSelectedRepository.Name = "m_resetSelectedRepository";
            this.m_resetSelectedRepository.Size = new Size(75, 23);
            this.m_resetSelectedRepository.TabIndex = 18;
            this.m_resetSelectedRepository.Text = "Reset";
            this.m_resetSelectedRepository.UseVisualStyleBackColor = true;
            this.m_resetSelectedRepository.Click += new EventHandler(this.OnResetSelectedRepository);
            this.m_revisiosForSelectedRepositoryLabel.AutoSize = true;
            this.m_revisiosForSelectedRepositoryLabel.Location = new Point(314, 26);
            this.m_revisiosForSelectedRepositoryLabel.Name = "m_revisiosForSelectedRepositoryLabel";
            this.m_revisiosForSelectedRepositoryLabel.Size = new Size(167, 13);
            this.m_revisiosForSelectedRepositoryLabel.TabIndex = 3;
            this.m_revisiosForSelectedRepositoryLabel.Text = "Avaliable Revisions for Repository";
            this.m_repositoryListLabel.AutoSize = true;
            this.m_repositoryListLabel.Location = new Point(10, 26);
            this.m_repositoryListLabel.Name = "m_repositoryListLabel";
            this.m_repositoryListLabel.Size = new Size(111, 13);
            this.m_repositoryListLabel.TabIndex = 1;
            this.m_repositoryListLabel.Text = "Avaliable Repositories";
            this.updateServiceGroup.Controls.Add((Control)this.m_serverPollUpdateService);
            this.updateServiceGroup.Controls.Add((Control)this.m_pollUpdateService);
            this.updateServiceGroup.Location = new Point(205, 12);
            this.updateServiceGroup.Name = "updateServiceGroup";
            this.updateServiceGroup.Size = new Size(182, 56);
            this.updateServiceGroup.TabIndex = 3;
            this.updateServiceGroup.TabStop = false;
            this.updateServiceGroup.Text = "Manage Update Service";
            this.m_serverPollUpdateService.Location = new Point(11, 22);
            this.m_serverPollUpdateService.Name = "m_serverPollUpdateService";
            this.m_serverPollUpdateService.Size = new Size(75, 23);
            this.m_serverPollUpdateService.TabIndex = 3;
            this.m_serverPollUpdateService.Text = "Server Poll";
            this.m_serverPollUpdateService.UseVisualStyleBackColor = true;
            this.m_serverPollUpdateService.Click += new EventHandler(this.m_serverPollUpdateService_Click);
            this.m_pollUpdateService.Location = new Point(96, 22);
            this.m_pollUpdateService.Name = "m_pollUpdateService";
            this.m_pollUpdateService.Size = new Size(75, 23);
            this.m_pollUpdateService.TabIndex = 4;
            this.m_pollUpdateService.Text = "Poll";
            this.m_pollUpdateService.UseVisualStyleBackColor = true;
            this.m_pollUpdateService.Click += new EventHandler(this.OnPollUpdateService);
            this.manageScheduleEvents.Controls.Add((Control)this.m_resetScheduleEvent);
            this.manageScheduleEvents.Controls.Add((Control)this.m_manageEventScheduler);
            this.manageScheduleEvents.Location = new Point(393, 12);
            this.manageScheduleEvents.Name = "manageScheduleEvents";
            this.manageScheduleEvents.Size = new Size(216, 56);
            this.manageScheduleEvents.TabIndex = 6;
            this.manageScheduleEvents.TabStop = false;
            this.manageScheduleEvents.Text = "Manage Event Scheduler";
            this.m_resetScheduleEvent.Location = new Point(97, 22);
            this.m_resetScheduleEvent.Name = "m_resetScheduleEvent";
            this.m_resetScheduleEvent.Size = new Size(106, 23);
            this.m_resetScheduleEvent.TabIndex = 7;
            this.m_resetScheduleEvent.Text = "Delete All Events";
            this.m_resetScheduleEvent.UseVisualStyleBackColor = true;
            this.m_resetScheduleEvent.Click += new EventHandler(this.OnResetScheduleEventClick);
            this.m_manageEventScheduler.Location = new Point(12, 22);
            this.m_manageEventScheduler.Name = "m_manageEventScheduler";
            this.m_manageEventScheduler.Size = new Size(75, 23);
            this.m_manageEventScheduler.TabIndex = 6;
            this.m_manageEventScheduler.Text = "Manage";
            this.m_manageEventScheduler.UseVisualStyleBackColor = true;
            this.m_manageEventScheduler.Click += new EventHandler(this.OnManageEventSchedulerClick);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(858, 489);
            this.Controls.Add((Control)this.manageScheduleEvents);
            this.Controls.Add((Control)this.updateServiceGroup);
            this.Controls.Add((Control)this.RepositoryGroup);
            this.Controls.Add((Control)this.BITSGroup);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.MaximumSize = new Size(864, 517);
            this.MinimizeBox = false;
            this.MinimumSize = new Size(864, 517);
            this.Name = nameof(MainForm);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Update Manager";
            this.Load += new EventHandler(this.OnLoad);
            this.BITSGroup.ResumeLayout(false);
            this.RepositoryGroup.ResumeLayout(false);
            this.RepositoryGroup.PerformLayout();
            this.updateServiceGroup.ResumeLayout(false);
            this.manageScheduleEvents.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
