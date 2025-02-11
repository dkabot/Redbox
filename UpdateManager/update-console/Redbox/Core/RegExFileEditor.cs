using System.IO;
using System.Text.RegularExpressions;

namespace Redbox.Core
{
    internal class RegExFileEditor
    {
        private readonly string m_filePath;

        public RegExFileEditor(string filePath, bool backup, string backupExtension)
        {
            this.m_filePath = filePath;
            if (!backup)
                return;
            FileHelper.CreateBackup(this.m_filePath, backupExtension);
        }

        public void Replace(string pattern, string token, string newValue)
        {
            this.Replace(new string[1] { pattern }, new string[1]
            {
        token
            }, new string[1] { newValue });
        }

        public void Replace(string[] patterns, string[] tokens, string[] newValues)
        {
            this.Update(this.ReplaceTokens(newValues, patterns, tokens));
        }

        internal static string Apply(string regEx, string originalText, string token)
        {
            Match match = new Regex(regEx).Match(originalText);
            return !match.Success ? (string)null : match.Groups[token].ToString();
        }

        internal string ReplaceTokens(string[] newValues, string[] patterns, string[] tokens)
        {
            using (TextReader textReader = (TextReader)new StreamReader(this.m_filePath))
                return RegExFileEditor.ReplaceTokens(textReader.ReadToEnd(), newValues, patterns, tokens);
        }

        private static string ReplaceTokens(
          string originalFileContents,
          string[] newValues,
          string[] patterns,
          string[] tokens)
        {
            for (int index = 0; index < newValues.Length; ++index)
                originalFileContents = originalFileContents.Replace(RegExFileEditor.Apply(patterns[index], originalFileContents, tokens[index]), newValues[index]);
            return originalFileContents;
        }

        private void Update(string newFileContents)
        {
            using (TextWriter textWriter = (TextWriter)new StreamWriter(this.m_filePath))
                textWriter.WriteLine(newFileContents);
        }
    }
}
