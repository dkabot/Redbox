using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel
{
    public class ErrorList : List<Error>, ICloneable
    {
        public int ErrorCount => FindAll(each => !each.IsWarning).Count;

        public int WarningCount => FindAll(each => each.IsWarning).Count;

        public object Clone()
        {
            var errorList = new ErrorList();
            errorList.AddRange(this);
            return errorList;
        }

        public bool ContainsCode(string code)
        {
            return Find(each => each.Code == code) != null;
        }

        public bool ContainsError()
        {
            return Find(each => !each.IsWarning) != null;
        }

        public int RemoveCode(string code)
        {
            return RemoveAll(each => each.Code == code);
        }

        public void ToLogHelper()
        {
            if (Count <= 0)
                return;
            var stringBuilder = new StringBuilder();
            foreach (var error in this)
            {
                stringBuilder.AppendLine(error.ToString());
                stringBuilder.AppendLine(error.Details);
            }

            LogHelper.Instance.Log(stringBuilder.ToString());
        }
    }
}