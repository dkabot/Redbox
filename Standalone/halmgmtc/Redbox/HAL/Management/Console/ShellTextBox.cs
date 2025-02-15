using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class ShellTextBox : TextBox
    {
        protected const int WM_PASTE = 770;
        protected const int WM_CUT = 768;
        protected const int WM_SETTEXT = 12;
        protected const int WM_CLEAR = 771;
        private readonly CommandHistory m_commandHistory = new CommandHistory();
        private string m_prompt = "> ";

        public ShellTextBox()
        {
            Initialize();
        }

        public string Prompt
        {
            get => m_prompt;
            set => SetPromptText(value);
        }

        public void Reset()
        {
            m_commandHistory.Clear();
            Clear();
            PrintPrompt();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 12:
                case 768:
                case 770:
                    if (!IsCaretAtWritablePosition()) MoveCaretToEndOfText();
                    break;
                case 771:
                    return;
            }

            base.WndProc(ref m);
        }

        private void Initialize()
        {
            Dock = DockStyle.Fill;
            MaxLength = 0;
            Multiline = true;
            AcceptsTab = true;
            AcceptsReturn = true;
            ScrollBars = ScrollBars.Both;
            Size = new Size(400, 176);
            TabIndex = 0;
            Text = string.Empty;
            KeyPress += OnKeyPress;
            KeyDown += OnKeyDown;
            Name = nameof(ShellTextBox);
            Size = new Size(400, 176);
            PrintPrompt();
        }

        private void PrintPrompt()
        {
            var text = Text;
            if (text.Length != 0)
            {
                var str = text;
                if (str[str.Length - 1] != '\n')
                    PrintLine();
            }

            AddText(m_prompt);
        }

        private void PrintLine()
        {
            AddText(Environment.NewLine);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b' && IsCaretJustBeforePrompt())
            {
                e.Handled = true;
            }
            else
            {
                if (!IsTerminatorKey(e.KeyChar))
                    return;
                e.Handled = true;
                var textAtPrompt = GetTextAtPrompt();
                if (textAtPrompt.Length != 0)
                {
                    PrintLine();
                    ((ShellControl)Parent).FireCommandEntered(textAtPrompt);
                    m_commandHistory.Add(textAtPrompt);
                }

                PrintPrompt();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsCaretAtWritablePosition() && !e.Control && !IsTerminatorKey(e.KeyCode))
                MoveCaretToEndOfText();
            if (e.KeyCode == Keys.Left && IsCaretJustBeforePrompt())
            {
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (m_commandHistory.DoesNextCommandExist())
                    ReplaceTextAtPrompt(m_commandHistory.GetNextCommand());
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (m_commandHistory.DoesPreviousCommandExist())
                    ReplaceTextAtPrompt(m_commandHistory.GetPreviousCommand());
                e.Handled = true;
            }
            else
            {
                if (e.KeyCode != Keys.Right)
                    return;
                var textAtPrompt = GetTextAtPrompt();
                var lastCommand = m_commandHistory.LastCommand;
                if (lastCommand == null || (textAtPrompt.Length != 0 && !lastCommand.StartsWith(textAtPrompt)) ||
                    lastCommand.Length <= textAtPrompt.Length)
                    return;
                AddText(lastCommand[textAtPrompt.Length].ToString());
            }
        }

        private string GetCurrentLine()
        {
            return Lines.Length != 0 ? (string)Lines.GetValue(Lines.GetLength(0) - 1) : string.Empty;
        }

        private string GetTextAtPrompt()
        {
            return GetCurrentLine().Substring(m_prompt.Length);
        }

        private void ReplaceTextAtPrompt(string text)
        {
            var length = GetCurrentLine().Length - m_prompt.Length;
            if (length == 0)
            {
                AddText(text);
            }
            else
            {
                Select(TextLength - length, length);
                SelectedText = text;
            }
        }

        private bool IsCaretAtCurrentLine()
        {
            return TextLength - SelectionStart <= GetCurrentLine().Length;
        }

        private void MoveCaretToEndOfText()
        {
            SelectionStart = TextLength;
            ScrollToCaret();
        }

        private bool IsCaretJustBeforePrompt()
        {
            return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() == m_prompt.Length;
        }

        private int GetCurrentCaretColumnPosition()
        {
            return SelectionStart - TextLength + GetCurrentLine().Length;
        }

        private bool IsCaretAtWritablePosition()
        {
            return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() >= m_prompt.Length;
        }

        private void SetPromptText(string val)
        {
            Select(0, m_prompt.Length);
            SelectedText = val;
            m_prompt = val;
        }

        public string[] GetCommandHistory()
        {
            return m_commandHistory.GetCommandHistory();
        }

        public void WriteText(string text)
        {
            AddText(text);
        }

        private static bool IsTerminatorKey(Keys key)
        {
            return key == Keys.Return;
        }

        private static bool IsTerminatorKey(char keyChar)
        {
            return keyChar == '\r';
        }

        private void AddText(string text)
        {
            Text += text;
            MoveCaretToEndOfText();
        }
    }
}