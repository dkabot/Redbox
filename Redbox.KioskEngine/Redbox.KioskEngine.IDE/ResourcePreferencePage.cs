using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class ResourcePreferencePage : UserControl, IPreferencePageHost
    {
        private IContainer components;
        private ErrorProvider m_errorProvider;
        private Label m_ResourceToScoreLabel;
        private TextBox m_resourceNameTextBox;
        private Button m_resourceScoreButton;
        private ListView m_scoredResourceListView;
        private ColumnHeader m_colFullName;
        private ColumnHeader m_colScore;
        private ComboBox m_resourceTypeCombo;
        private RichTextBox m_resourceRichTextBox;
        private Label m_headerLabel;

        public ResourcePreferencePage()
        {
            InitializeComponent();
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            preferencePage.SetValue("ResourceName", (object)(m_resourceNameTextBox.Text ?? string.Empty));
            preferencePage.SetValue("ResourceType", (object)(m_resourceTypeCombo.Text ?? string.Empty));
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            m_resourceNameTextBox.Text = preferencePage.GetValue<string>("ResourceName", string.Empty);
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            if (service == null)
                return;
            m_resourceTypeCombo.Items.Clear();
            foreach (var resourceType in (IEnumerable<KeyValuePair<string, IResourceType>>)service.GetResourceTypes())
                m_resourceTypeCombo.Items.Add((object)resourceType.Key);
        }

        private void m_resourceScoreButton_Click(object sender, EventArgs e)
        {
            if (m_resourceTypeCombo.SelectedIndex < 0)
            {
                var num1 = (int)MessageBox.Show("Please select a resource Type first.");
            }
            else if (m_resourceNameTextBox.Text == null)
            {
                var num2 = (int)MessageBox.Show("Please enter the name of the resource to score.");
            }
            else
            {
                m_scoredResourceListView.Items.Clear();
                var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
                if (service == null)
                    return;
                var resources = service.GetResourcesUnfiltered(m_resourceTypeCombo.Text)
                    .Where<IResource>((Func<IResource, bool>)(r => r.Name == m_resourceNameTextBox.Text));
                var filter = service.Filter;
                var scoredResourceList = new List<ScoredResource>();
                foreach (var resource in resources)
                {
                    var each = resource;
                    var num3 = 0;
                    if (!IsResourceEffective(each))
                    {
                        num3 = -1;
                    }
                    else if (filter != null)
                    {
                        var list = each.Type.GetFilterProperties()
                            .Where<IPropertyType>((Func<IPropertyType, bool>)(f => each[f.Name] != null))
                            .ToList<IPropertyType>();
                        if (list.Count == 0)
                            ++num3;
                        else if (list.Count<IPropertyType>((Func<IPropertyType, bool>)(eachFilter =>
                                     each.PropertyEquals2(eachFilter.Name,
                                         eachFilter.Name.Substring(0, 2) == "!="
                                             ? (object)filter[eachFilter.Name.Substring(2)]
                                             : (object)filter[eachFilter.Name]))) == list.Count)
                            num3 += list.Sum<IPropertyType>(
                                (Func<IPropertyType, int>)(eachFilter => eachFilter.FilterScore));
                    }

                    scoredResourceList.Add(new ScoredResource()
                    {
                        Score = num3,
                        Resource = each
                    });
                }

                scoredResourceList.Sort((Comparison<ScoredResource>)((x, y) => y.Score.CompareTo(x.Score)));
                foreach (var scoredResource in scoredResourceList)
                    m_scoredResourceListView.Items.Add(new ListViewItem(new string[2]
                    {
                        scoredResource.Resource.StorageEntry.Name,
                        scoredResource.Score.ToString()
                    }));
            }
        }

        private static bool IsResourceEffective(IResource resource)
        {
            var now = DateTime.Now;
            var nullable1 = new DateTime?();
            if (resource["effective_date"] != null)
                nullable1 = (DateTime?)resource["effective_date"];
            var nullable2 = new DateTime?();
            if (resource["expiry_date"] != null)
                nullable2 = (DateTime?)resource["expiry_date"];
            DateTime? nullable3;
            if (nullable1.HasValue)
            {
                var dateTime = now;
                nullable3 = nullable1;
                if ((nullable3.HasValue ? dateTime < nullable3.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (nullable2.HasValue)
            {
                var dateTime = now;
                nullable3 = nullable2;
                if ((nullable3.HasValue ? dateTime > nullable3.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (resource["daypart_dayofweek_only"] != null &&
                !resource.PropertyEquals2("daypart_dayofweek_only", (object)now.DayOfWeek.ToString()))
                return false;
            var nullable4 = new DayOfWeek?();
            if (resource["daypart_effective_weekday"] != null)
                nullable4 = (DayOfWeek?)resource["daypart_effective_weekday"];
            var nullable5 = new DayOfWeek?();
            if (resource["daypart_expiry_weekday"] != null)
                nullable5 = (DayOfWeek?)resource["daypart_expiry_weekday"];
            DayOfWeek? nullable6;
            if (nullable4.HasValue)
            {
                var dayOfWeek = (int)now.DayOfWeek;
                nullable6 = nullable4;
                var valueOrDefault = (int)nullable6.GetValueOrDefault();
                if ((dayOfWeek < valueOrDefault) & nullable6.HasValue)
                    return false;
            }

            if (nullable5.HasValue)
            {
                var dayOfWeek = (int)now.DayOfWeek;
                nullable6 = nullable5;
                var valueOrDefault = (int)nullable6.GetValueOrDefault();
                if ((dayOfWeek > valueOrDefault) & nullable6.HasValue)
                    return false;
            }

            var nullable7 = new TimeSpan?();
            if (resource["daypart_effective_time"] != null)
                nullable7 = (TimeSpan?)resource["daypart_effective_time"];
            var nullable8 = new TimeSpan?();
            if (resource["daypart_expiry_time"] != null)
                nullable8 = (TimeSpan?)resource["daypart_expiry_time"];
            TimeSpan? nullable9;
            if (nullable7.HasValue)
            {
                var timeOfDay = now.TimeOfDay;
                nullable9 = nullable7;
                if ((nullable9.HasValue ? timeOfDay < nullable9.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (nullable8.HasValue)
            {
                var timeOfDay = now.TimeOfDay;
                nullable9 = nullable8;
                if ((nullable9.HasValue ? timeOfDay > nullable9.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = (IContainer)new Container();
            m_errorProvider = new ErrorProvider(components);
            m_resourceNameTextBox = new TextBox();
            m_ResourceToScoreLabel = new Label();
            m_resourceScoreButton = new Button();
            m_scoredResourceListView = new ListView();
            m_colFullName = new ColumnHeader();
            m_colScore = new ColumnHeader();
            m_resourceTypeCombo = new ComboBox();
            m_resourceRichTextBox = new RichTextBox();
            m_headerLabel = new Label();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_resourceNameTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            m_resourceNameTextBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            m_resourceNameTextBox.Location = new Point(34, 68);
            m_resourceNameTextBox.MaxLength = 1024;
            m_resourceNameTextBox.Name = "m_resourceNameTextBox";
            m_resourceNameTextBox.Size = new Size(216, 20);
            m_resourceNameTextBox.TabIndex = 5;
            m_ResourceToScoreLabel.AutoSize = true;
            m_ResourceToScoreLabel.Location = new Point(31, 52);
            m_ResourceToScoreLabel.Name = "m_ResourceToScoreLabel";
            m_ResourceToScoreLabel.Size = new Size(84, 13);
            m_ResourceToScoreLabel.TabIndex = 6;
            m_ResourceToScoreLabel.Text = "Resource Name";
            m_resourceScoreButton.Location = new Point(403, 65);
            m_resourceScoreButton.Name = "m_resourceScoreButton";
            m_resourceScoreButton.Size = new Size(94, 23);
            m_resourceScoreButton.TabIndex = 7;
            m_resourceScoreButton.Text = "Score Resource";
            m_resourceScoreButton.UseVisualStyleBackColor = true;
            m_resourceScoreButton.Click += new EventHandler(m_resourceScoreButton_Click);
            m_scoredResourceListView.Columns.AddRange(new ColumnHeader[2]
            {
                m_colFullName,
                m_colScore
            });
            m_scoredResourceListView.Location = new Point(37, 105);
            m_scoredResourceListView.Name = "m_scoredResourceListView";
            m_scoredResourceListView.Size = new Size(459, 217);
            m_scoredResourceListView.TabIndex = 8;
            m_scoredResourceListView.UseCompatibleStateImageBehavior = false;
            m_scoredResourceListView.View = View.Details;
            m_colFullName.Text = "Full Name";
            m_colFullName.Width = 373;
            m_colScore.Text = "Score";
            m_resourceTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            m_resourceTypeCombo.FormattingEnabled = true;
            m_resourceTypeCombo.Location = new Point(256, 67);
            m_resourceTypeCombo.MaxDropDownItems = 20;
            m_resourceTypeCombo.Name = "m_resourceTypeCombo";
            m_resourceTypeCombo.Size = new Size(141, 21);
            m_resourceTypeCombo.TabIndex = 9;
            m_resourceRichTextBox.Location = new Point(37, 331);
            m_resourceRichTextBox.Name = "m_resourceRichTextBox";
            m_resourceRichTextBox.Size = new Size(458, 138);
            m_resourceRichTextBox.TabIndex = 10;
            m_resourceRichTextBox.Text = "";
            m_headerLabel.Anchor = AnchorStyles.Top;
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(542, 38);
            m_headerLabel.TabIndex = 11;
            m_headerLabel.Text = "  Engine Core: Resource Scoring";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_headerLabel);
            Controls.Add((Control)m_resourceRichTextBox);
            Controls.Add((Control)m_resourceTypeCombo);
            Controls.Add((Control)m_scoredResourceListView);
            Controls.Add((Control)m_resourceScoreButton);
            Controls.Add((Control)m_ResourceToScoreLabel);
            Controls.Add((Control)m_resourceNameTextBox);
            Name = nameof(ResourcePreferencePage);
            Size = new Size(542, 506);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        internal sealed class ScoredResource
        {
            public int Score { get; set; }

            public IResource Resource { get; set; }
        }
    }
}