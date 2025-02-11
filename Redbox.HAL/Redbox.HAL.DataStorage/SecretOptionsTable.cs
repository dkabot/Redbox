using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class SecretOptionsTable : AbstractVistaDBDataTable<IPersistentOption>
    {
        internal SecretOptionsTable(IDataTableDescriptor d)
            : base(d, "SecretOptions_v2")
        {
        }

        protected override string PrimaryKeyToken => "secretOptions_PK";

        protected override string PrimaryKeyColumns => "( OptionName )";

        protected override List<IPersistentOption> OnLoadEntries()
        {
            var options = new List<IPersistentOption>();
            ExectuteSelectQuery(reader => options.Add(new PersistentOption
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1)
            }), string.Format("SELECT OptionName, OptionValue, OptionName FROM [{0}]", Name));
            return options;
        }

        protected override string UpdateStatement(IPersistentOption obj)
        {
            return string.Format("UPDATE [{0}] SET OptionValue = '{1}' WHERE OptionName = '{2}'", Name, obj.Value,
                obj.Key);
        }

        protected override string InsertStatement(IPersistentOption obj)
        {
            return string.Format(
                "INSERT INTO [{0}] ( OptionName, OptionValue, OptionEncrypted ) VALUES( '{1}','{2}', '0' )", Name,
                obj.Key, obj.Value);
        }

        protected override string DeleteStatement(IPersistentOption obj)
        {
            return string.Format("DELETE FROM [{0}] WHERE OptionName = '{1}' ", Name, obj.Key);
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [OptionName] VarChar(32) NOT NULL,");
            stringBuilder.Append(" [OptionValue] VarChar(64) NOT NULL,");
            stringBuilder.Append(" [OptionEncrypted] Bit NOT NULL,");
            stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
            return stringBuilder.ToString();
        }
    }
}