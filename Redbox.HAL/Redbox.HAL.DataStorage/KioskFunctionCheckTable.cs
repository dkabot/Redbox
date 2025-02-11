using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class KioskFunctionCheckTable : AbstractVistaDBDataTable<IKioskFunctionCheckData>
    {
        internal KioskFunctionCheckTable(IDataTableDescriptor d)
            : base(d, "KioskFunctionCheckData")
        {
        }

        protected override string PrimaryKeyToken => "kfc_PK";

        protected override string PrimaryKeyColumns => "( SampleDate, UserToken )";

        protected override List<IKioskFunctionCheckData> OnLoadEntries()
        {
            var rv = new List<IKioskFunctionCheckData>();
            ExectuteSelectQuery(reader => rv.Add(new KioskFunctionCheckData
                {
                    Timestamp = reader.GetDateTime(0),
                    UserIdentifier = reader.GetString(1),
                    VerticalSlotTestResult = reader.GetString(2),
                    VendDoorTestResult = reader.GetString(3),
                    TrackTestResult = reader.GetString(4),
                    SnapDecodeTestResult = reader.GetString(5),
                    CameraDriverTestResult = reader.GetString(6),
                    TouchscreenDriverTestResult = reader.GetString(7),
                    InitTestResult = reader.GetString(8)
                }),
                string.Format(
                    "select SampleDate, UserToken, VerticalTestResult, VendDoorTestResult, TrackTestResult, SnapDecodeTestResult, CameraDriverTestResult, TouchScreenDriverTestResult, InitTestResult FROM [{0}]",
                    Name));
            return rv;
        }

        protected override string UpdateStatement(IKioskFunctionCheckData obj)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("UPDATE {0} SET", Name);
            stringBuilder.AppendFormat(" VerticalTestResult = '{0}'", obj.VerticalSlotTestResult);
            stringBuilder.AppendFormat(" VendDoorTestResult = '{0}'", obj.VendDoorTestResult);
            stringBuilder.AppendFormat(" TrackTestResult = '{0}'", obj.TrackTestResult);
            stringBuilder.AppendFormat(" SnapDecodeTestResult = '{0}'", obj.SnapDecodeTestResult);
            stringBuilder.AppendFormat(" CameraDriverTestResult = '{0}'", obj.CameraDriverTestResult);
            stringBuilder.AppendFormat(" TouchScreenDriverTestResult = '{0}'", obj.TouchscreenDriverTestResult);
            stringBuilder.AppendFormat(" InitTestResult = '{0}'", obj.InitTestResult);
            stringBuilder.AppendFormat(" where SampleDate = '{0}' and UserToken = '{1}'", obj.Timestamp.ToString(),
                obj.UserIdentifier);
            return stringBuilder.ToString();
        }

        protected override string InsertStatement(IKioskFunctionCheckData data)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("INSERT INTO [{0}]", Name);
            stringBuilder.Append(" ( SampleDate, UserToken, VerticalTestResult, VendDoorTestResult,");
            stringBuilder.Append(" TrackTestResult, SnapDecodeTestResult, CameraDriverTestResult,");
            stringBuilder.Append(" TouchScreenDriverTestResult, InitTestResult )");
            stringBuilder.AppendFormat(" VALUES ( '{0}', '{1}', '{2}', '{3}',", data.Timestamp.ToString(),
                data.UserIdentifier, data.VerticalSlotTestResult, data.VendDoorTestResult);
            stringBuilder.AppendFormat(" '{0}', '{1}', '{2}',", data.TrackTestResult, data.SnapDecodeTestResult,
                data.CameraDriverTestResult);
            stringBuilder.AppendFormat(" '{0}', '{1}' )", data.TouchscreenDriverTestResult, data.InitTestResult);
            return stringBuilder.ToString();
        }

        protected override string DeleteStatement(IKioskFunctionCheckData obj)
        {
            return string.Format("delete from {0} where UserToken = '{1}' and SampleDate = '{2}'", Name,
                obj.UserIdentifier, obj.Timestamp);
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [SampleDate] DateTime NOT NULL,");
            stringBuilder.Append(" [UserToken] VarChar(64) NOT NULL,");
            stringBuilder.Append(" [VerticalTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [VendDoorTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [TrackTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [SnapDecodeTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [CameraDriverTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [TouchScreenDriverTestResult] VarChar(16) NOT NULL,");
            stringBuilder.Append(" [InitTestResult] VarChar(16) NOT NULL,");
            stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
            return stringBuilder.ToString();
        }

        protected override string CleanStatement()
        {
            return string.Format("DELETE FROM [{0}] where SampleDate < '{1}'", Name,
                DateTime.Now.Subtract(TimeSpan.FromDays(14.0)));
        }
    }
}