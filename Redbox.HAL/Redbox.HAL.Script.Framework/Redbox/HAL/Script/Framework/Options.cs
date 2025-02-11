using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal class Options
    {
        private readonly string m_file;
        private readonly IDictionary<string, object> m_options = new Dictionary<string, object>();

        internal Options(string file, bool read)
        {
            if (!File.Exists(file))
                return;
            m_file = file;
            if (!read)
                return;
            ReadInner();
        }

        private bool FileValid
        {
            get
            {
                if (!string.IsNullOrEmpty(m_file) && File.Exists(m_file))
                    return true;
                LogHelper.Instance.Log(string.Format("Options file {0} doesn't exist.", m_file), LogEntryType.Info);
                return false;
            }
        }

        internal T GetValue<T>(string name, T def)
        {
            if (!m_options.ContainsKey(name))
                return def;
            try
            {
                return (T)Convert.ChangeType(m_options[name], typeof(T));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Unable to convert option {0}", name), LogEntryType.Info);
                return def;
            }
        }

        internal void Read()
        {
            ReadInner();
        }

        private void ReadInner()
        {
            if (!FileValid)
                return;
            using (var textReader = (TextReader)new StreamReader(m_file))
            {
                while (true)
                {
                    string[] strArray;
                    do
                    {
                        var str = textReader.ReadLine();
                        if (str != null)
                            strArray = str.Split(',');
                        else
                            goto label_1;
                    } while (strArray.Length != 2);

                    m_options[strArray[0]] = strArray[1];
                }

                label_1: ;
            }
        }
    }
}