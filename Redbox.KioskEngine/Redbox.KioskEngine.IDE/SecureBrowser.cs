using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class SecureBrowser : Form
    {
        private readonly NavigateUrlCallback m_navigateUrlCallback;
        private readonly IDictionary<string, string> m_approvedUrls;
        private IContainer components;
        private Label label1;
        private ComboBox m_urlComboBox;
        private Panel panel1;
        private WebBrowser m_browser;
        private Button btn_dropdown;
        private Button btn_close;
        private Panel panel2;
        private Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;

        public SecureBrowser(
            IDictionary<string, string> approvedUrls,
            NavigateUrlCallback navigateUrlCallback)
        {
            InitializeComponent();
            m_navigateUrlCallback = navigateUrlCallback;
            m_approvedUrls = approvedUrls;
            foreach (var approvedUrl in (IEnumerable<KeyValuePair<string, string>>)approvedUrls)
                m_urlComboBox.Items.Add((object)approvedUrl.Key);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Hide();
        }

        private void SecureBrowserComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            NavigateToUrl(m_urlComboBox.Text);
        }

        private void SecureBrowserComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r')
                return;
            NavigateToUrl(m_urlComboBox.Text);
        }

        private bool IsWebsiteApproved(string currentUrl)
        {
            foreach (var approvedUrl in (IEnumerable<KeyValuePair<string, string>>)m_approvedUrls)
                if (approvedUrl.Value.StartsWith(currentUrl))
                    return true;
            return false;
        }

        private static bool IsUrlAddressAllowed(string url)
        {
            return Regex.IsMatch(url, "((192.168.)|(172.16.))\\d{1,3}\\.\\d{1,3}$");
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (m_navigateUrlCallback == null)
                return;
            label2.Text = "Secure Browser - Loading...";
            m_navigateUrlCallback("Valid URL", e.Url.ToString());
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            label2.Text = "Secure Browser";
            m_browser.Focus();
        }

        private void NavigateToUrl(string urlName)
        {
            var service = ServiceLocator.Instance.GetService<IMacroService>();
            var input = m_approvedUrls.ContainsKey(urlName) ? m_approvedUrls[urlName] : string.Empty;
            if (!string.IsNullOrEmpty(input))
            {
                m_browser.Navigate(service.ExpandProperties(input));
            }
            else if (IsUrlAddressAllowed(urlName) || IsWebsiteApproved(urlName))
            {
                m_browser.Navigate(service.ExpandProperties(urlName));
            }
            else
            {
                m_browser.Navigate(string.Empty);
                m_navigateUrlCallback("Invalid URL", urlName);
            }
        }

        private void btn_dropdown_Click(object sender, EventArgs e)
        {
            m_urlComboBox.DroppedDown = true;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(SecureBrowser));
            m_urlComboBox = new ComboBox();
            label1 = new Label();
            panel1 = new Panel();
            btn_dropdown = new Button();
            btn_close = new Button();
            m_browser = new WebBrowser();
            panel2 = new Panel();
            label2 = new Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            m_urlComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_urlComboBox.FlatStyle = FlatStyle.Flat;
            m_urlComboBox.Font = new Font("Microsoft Sans Serif", 16f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            m_urlComboBox.Location = new Point(175, 8);
            m_urlComboBox.Name = "m_urlComboBox";
            m_urlComboBox.Size = new Size(823, 33);
            m_urlComboBox.TabIndex = 1;
            m_urlComboBox.SelectedIndexChanged += new EventHandler(SecureBrowserComboBox_SelectedValueChanged);
            m_urlComboBox.KeyPress += new KeyPressEventHandler(SecureBrowserComboBox_KeyPress);
            label1.Font = new Font("Tahoma", 16f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(5, 12);
            label1.Name = "label1";
            label1.Size = new Size(163, 24);
            label1.TabIndex = 2;
            label1.Text = "URL Address:";
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.DimGray;
            panel1.Controls.Add((Control)btn_dropdown);
            panel1.Controls.Add((Control)label1);
            panel1.Controls.Add((Control)m_urlComboBox);
            panel1.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            panel1.ForeColor = Color.White;
            panel1.Location = new Point(0, 50);
            panel1.Name = "panel1";
            panel1.Size = new Size(1024, 50);
            panel1.TabIndex = 3;
            btn_dropdown.BackColor = Color.White;
            btn_dropdown.BackgroundImage = (Image)componentResourceManager.GetObject("btn_dropdown.BackgroundImage");
            btn_dropdown.BackgroundImageLayout = ImageLayout.Stretch;
            btn_dropdown.FlatAppearance.BorderColor = Color.Maroon;
            btn_dropdown.FlatAppearance.BorderSize = 0;
            btn_dropdown.FlatStyle = FlatStyle.Flat;
            btn_dropdown.Font = new Font("Microsoft Sans Serif", 13f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            btn_dropdown.ForeColor = Color.Black;
            btn_dropdown.Location = new Point(940, 8);
            btn_dropdown.Name = "btn_dropdown";
            btn_dropdown.Size = new Size(84, 33);
            btn_dropdown.TabIndex = 3;
            btn_dropdown.UseVisualStyleBackColor = false;
            btn_dropdown.Click += new EventHandler(btn_dropdown_Click);
            btn_close.BackColor = Color.LightGray;
            btn_close.DialogResult = DialogResult.Cancel;
            btn_close.FlatAppearance.BorderSize = 0;
            btn_close.FlatStyle = FlatStyle.Flat;
            btn_close.Font = new Font("Tahoma", 24f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            btn_close.ForeColor = Color.White;
            btn_close.Location = new Point(940, 0);
            btn_close.Name = "btn_close";
            btn_close.Size = new Size(84, 50);
            btn_close.TabIndex = 4;
            btn_close.Text = "X";
            btn_close.UseVisualStyleBackColor = false;
            btn_close.Click += new EventHandler(btn_close_Click);
            m_browser.Location = new Point(26, 125);
            m_browser.MinimumSize = new Size(20, 20);
            m_browser.Name = "m_browser";
            m_browser.Size = new Size(972, 617);
            m_browser.TabIndex = 4;
            m_browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Browser_DocumentCompleted);
            m_browser.Navigating += new WebBrowserNavigatingEventHandler(Browser_Navigating);
            panel2.BackColor = Color.Maroon;
            panel2.Controls.Add((Control)label2);
            panel2.Controls.Add((Control)pictureBox1);
            panel2.Controls.Add((Control)btn_close);
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1024, 50);
            panel2.TabIndex = 5;
            label2.AutoSize = true;
            label2.Font = new Font("Tahoma", 24f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(55, 5);
            label2.Name = "label2";
            label2.Size = new Size(268, 39);
            label2.TabIndex = 1;
            label2.Text = "Secure Browser";
            pictureBox1.Image = (Image)componentResourceManager.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(45, 50);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            AutoScaleBaseSize = new Size(5, 13);
            CancelButton = (IButtonControl)btn_close;
            ClientSize = new Size(1024, 768);
            Controls.Add((Control)panel2);
            Controls.Add((Control)m_browser);
            Controls.Add((Control)panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MaximumSize = new Size(1024, 768);
            MinimizeBox = false;
            MinimumSize = new Size(1024, 768);
            Name = nameof(SecureBrowser);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Secure Browser";
            FormClosing += new FormClosingEventHandler(OnFormClosing);
            Load += new EventHandler(OnLoad);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }
    }
}