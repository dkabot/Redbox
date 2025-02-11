using System.IO;

namespace Redbox.HAL.Script.Framework
{
    public class CFA
    {
        public enum CFAInternalsVersion
        {
            None,
            Mock
        }

        internal const string CFAErrorCode = "C008";
        internal const string CFAWarningCode = "C009";
        private const string DirectiveToken = "CFA:";

        public static bool DumpAfterFinalize { get; set; }

        public static bool DumpBeforeFinalize { get; set; }

        public static bool Debug { get; set; }

        public static bool DumpAfterDeadBlockRemoval { get; set; }

        public static bool DumpVariableLiveSearch { get; set; }

        public static void Configure(string optionsFile)
        {
            if (!File.Exists(optionsFile))
                return;
            var options = new Options(optionsFile, true);
            DumpAfterFinalize = options.GetValue("DumpAfterFinalize", DumpAfterFinalize);
            DumpBeforeFinalize = options.GetValue("DumpBeforeFinalize", DumpBeforeFinalize);
            Debug = options.GetValue("Debug", Debug);
            DumpAfterDeadBlockRemoval = options.GetValue("DumpAfterDeadBlockRemoval", DumpAfterDeadBlockRemoval);
            DumpVariableLiveSearch = options.GetValue("DumpVariableLiveSearch", DumpVariableLiveSearch);
        }

        internal static CFAInternals Initialize(CFAInternalsVersion version, ExecutionResult result)
        {
            var cfaInternalsMock = new CFAInternalsMock();
            cfaInternalsMock.Initialize(result);
            return cfaInternalsMock;
        }
    }
}