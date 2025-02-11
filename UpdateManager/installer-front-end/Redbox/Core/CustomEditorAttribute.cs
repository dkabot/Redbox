using System;

namespace Redbox.Core
{
    internal class CustomEditorAttribute : Attribute
    {
        public CustomEditorAttribute(string text) => this.Text = text;

        public string Text { get; private set; }

        public string GetMethodName { get; set; }

        public string SetMethodName { get; set; }
    }
}
