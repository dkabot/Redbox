using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    public class Compiler
    {
        private const string DirectiveToken = "@";
        private const string IncludeDirective = "INC";
        private const string PragmaDirective = "PRAGMA";
        private static readonly Dictionary<string, bool> m_warningTable;
        private static readonly Deque<string> m_configScriptPaths = new Deque<string>();

        static Compiler()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            m_configScriptPaths.AddToBack(service.InstallPath("Scripts"));
            m_warningTable = new Dictionary<string, bool>();
            m_warningTable["W008"] = false;
        }

        public Compiler(Stream stream, string programName)
        {
            Stream = stream;
            ProgramName = programName;
            CFAVersion = CFA.CFAInternalsVersion.Mock;
        }

        public Dictionary<string, bool> ConstTokens { get; set; }

        public Stream Stream { get; set; }

        public string ProgramName { get; set; }

        public CFA.CFAInternalsVersion CFAVersion { get; set; }

        private CFAInternals ControlFlowAnalyzer { get; set; }

        public ExecutionResult Compile(out ProgramInstruction compiledProgram)
        {
            var num1 = 0;
            using (var executionTimer = new ExecutionTimer())
            {
                compiledProgram = null;
                var executionResult = new ExecutionResult();
                var instructionStack = new Stack<Instruction>();
                instructionStack.Push(ProgramInstruction.CreateEmpty(ProgramName));
                var lineNumber = 0;
                var results = new Deque<string>();
                ConstTokens = new Dictionary<string, bool>();
                var map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
                var flag = map.GetValue("Compiler.EnablePreprocessor", true);
                ControlFlowAnalyzer = CFA.Initialize(CFAVersion, executionResult);
                var instruction1 = (Instruction)null;
                using (var textReader = (TextReader)new StreamReader(Stream))
                {
                    do
                    {
                        string str1;
                        Tokenizer tokenizer = null;
                        do
                        {
                            do
                            {
                                str1 = results.Count >= 1 ? results.PopTop() : textReader.ReadLine();
                                if (str1 == null)
                                {
                                    if (instruction1 != null && !instruction1.IsBranch())
                                        if (!instruction1.EndsContext)
                                            goto label_43;
                                    var controlFlowAnalyzer = ControlFlowAnalyzer;
                                    var nopInstruction = new NopInstruction();
                                    int num2;
                                    var num3 = num2 = lineNumber + 1;
                                    nopInstruction.LineNumber = num2;
                                    var result = executionResult;
                                    controlFlowAnalyzer.ProcessInstruction(nopInstruction, null, result);
                                    goto label_43;
                                }

                                string directiveName;
                                if (flag && IsDirective(str1, out directiveName))
                                {
                                    ProcessDirective(directiveName, str1, executionResult, results);
                                    if (executionResult.Errors.ContainsError())
                                        goto label_43;
                                }
                                else
                                {
                                    tokenizer = new Tokenizer(++lineNumber, str1, ConstTokens);
                                    tokenizer.Tokenize();
                                    if (tokenizer.Errors.Count != 0)
                                    {
                                        executionResult.Errors.AddRange(tokenizer.Errors);
                                        goto label_43;
                                    }
                                }
                            } while (tokenizer.Tokens.HasOnlyComments() || tokenizer.Tokens.Count == 0);

                            if (tokenizer.Tokens.HasOnlyLabel())
                            {
                                var str2 = tokenizer.Tokens.GetLabel().Value;
                                instruction1 = Instruction.Parse(new TokenList(new Token[1]
                                {
                                    new Token(TokenType.Mnemonic, "NOP", false)
                                }), lineNumber, executionResult);
                                instruction1.LabelName = str2;
                            }
                            else
                            {
                                instruction1 = Instruction.Parse(tokenizer.Tokens, lineNumber, executionResult);
                            }

                            if (instruction1 == null)
                            {
                                executionResult.Errors.Add(Error.NewError("C004",
                                    "The statement contained an unknown mnemonic: " + str1,
                                    "Correct your source and recompile."));
                                goto label_43;
                            }

                            var label = tokenizer.Tokens.GetLabel();
                            if (label != null)
                            {
                                ControlFlowAnalyzer.AddLabel(label.Value, instruction1, executionResult);
                                if (!executionResult.Errors.ContainsError())
                                    instruction1.IsLabel = true;
                                else
                                    goto label_43;
                            }

                            var symbols = tokenizer.Tokens.GetSymbols();
                            foreach (var symbol1 in symbols)
                            {
                                var symbol2 = TranslateKeyValuePairToSymbol(symbol1);
                                if (!string.IsNullOrEmpty(symbol2) && !instruction1.OperandRecognized(symbol2) &&
                                    instruction1.BranchType != Instruction.Branch.Unconditional)
                                {
                                    if (instruction1.CreatesVariableDef)
                                        ControlFlowAnalyzer.AddVariableDef(symbol2, instruction1.LineNumber,
                                            executionResult);
                                    else
                                        ControlFlowAnalyzer.AddVariableUse(symbol2, instruction1.LineNumber,
                                            executionResult);
                                }

                                if (symbol1.Value.StartsWith("__"))
                                {
                                    executionResult.Errors.Add(Error.NewError("C005",
                                        "Symbols may not start with '__'; they are reserved. ",
                                        "Correct your source and recompile."));
                                    break;
                                }
                            }

                            ControlFlowAnalyzer.ProcessInstruction(instruction1, symbols, executionResult);
                            if (!executionResult.Errors.ContainsError())
                            {
                                if (instruction1.IsCooperativeMultitask)
                                    ++num1;
                                instructionStack.Peek().Instructions.Add(instruction1);
                                if (instruction1.BeginsContext)
                                    instructionStack.Push(instruction1);
                            }
                            else
                            {
                                goto label_43;
                            }
                        } while (!instruction1.EndsContext);

                        var endContextMnemonic = instructionStack.Peek().EndContextMnemonic;
                        if (instructionStack.Count > 1)
                        {
                            var begin = instructionStack.Pop();
                            if (string.Compare(endContextMnemonic, tokenizer.Tokens.GetMnemonic().Value, true) != 0)
                            {
                                executionResult.Errors.Add(Error.NewError("C004",
                                    "Expected context block closing mnemonic: " + endContextMnemonic,
                                    "Correct your source and recompile."));
                                break;
                            }

                            ControlFlowAnalyzer.ProcessControlBlock(begin, instruction1, executionResult);
                        }
                        else
                        {
                            executionResult.Errors.Add(Error.NewError("C004",
                                "An unmatched context block detected: " + str1, "Correct your source and recompile."));
                            break;
                        }
                    } while (!executionResult.Errors.ContainsError());
                }

                label_43:
                if (instructionStack.Count > 1)
                    executionResult.Errors.Add(Error.NewError("C005",
                        "A context block was not properly closed: " + instructionStack.Peek() +
                        ", expected closing mnemonic " + instructionStack.Peek().EndContextMnemonic,
                        "Correct your source and recompile."));
                if (!executionResult.Errors.ContainsError())
                {
                    ControlFlowAnalyzer.FixupUnconditionalBranches(executionResult);
                    if (!executionResult.Errors.ContainsError())
                    {
                        if (CFA.DumpBeforeFinalize)
                            ControlFlowAnalyzer.Dump(ProgramName + "-prefinalize");
                        ControlFlowAnalyzer.RemoveDeadBlocks(executionResult);
                        if (CFA.DumpAfterDeadBlockRemoval)
                            ControlFlowAnalyzer.Dump(ProgramName + "-afterdeadremove");
                        if (!executionResult.Errors.ContainsError())
                        {
                            ControlFlowAnalyzer.FindReferencedUndefinedVariables(executionResult);
                            if (!executionResult.Errors.ContainsError())
                            {
                                ControlFlowAnalyzer.Finalize(executionResult);
                                if (CFA.DumpAfterFinalize)
                                    ControlFlowAnalyzer.Dump(ProgramName + "-postfinalize");
                            }
                        }
                    }
                }

                if (num1 == 0 && m_warningTable["W008"])
                    executionResult.Errors.Add(Error.NewWarning("W008",
                        "Script does not contain cooperative multi-tasking instruction",
                        "The script doesn't contain an instruction, such as YIELD, that allows another script to run. Is this the expected behavior?"));
                if (!executionResult.Errors.ContainsError())
                    compiledProgram = (ProgramInstruction)instructionStack.Peek();
                executionResult.ExecutionTime = new TimeSpan(executionTimer.ElapsedTicks);
                if (compiledProgram != null)
                    compiledProgram.IsCooperative = num1 > 0;
                if (map.GetValue("Compiler.DumpCompiledProgram", false) && compiledProgram != null)
                {
                    var instructions = compiledProgram.Instructions;
                    using (var textWriter = (TextWriter)new StreamWriter("C:\\temp\\" + ProgramName + ".preprocessed"))
                    {
                        for (var index = 0; index < instructions.Count; ++index)
                        {
                            var instruction2 = instructions[index];
                            textWriter.WriteLine(instruction2);
                        }
                    }
                }

                return executionResult;
            }
        }

        public static void DisableWarning(string warning)
        {
            if (!warning.StartsWith("w", StringComparison.CurrentCultureIgnoreCase))
                warning = "W" + warning;
            if (!m_warningTable.ContainsKey(warning))
                return;
            m_warningTable[warning] = false;
        }

        public static void EnableWarning(string warning)
        {
            if (!warning.StartsWith("w", StringComparison.CurrentCultureIgnoreCase))
                warning = "W" + warning;
            if (!m_warningTable.ContainsKey(warning))
                return;
            m_warningTable[warning] = true;
        }

        public static void AppendSearchPath(string wholePath)
        {
            var delimitedPaths = ParseDelimitedPaths(wholePath);
            if (delimitedPaths.Length == 0)
                return;
            m_configScriptPaths.AddManyToFront(delimitedPaths);
        }

        private bool IsDirective(string maybeDirective, out string directiveName)
        {
            var indexA = maybeDirective.IndexOf("@") + 1;
            if (indexA == 0)
            {
                directiveName = string.Empty;
                return false;
            }

            if (string.Compare(maybeDirective, indexA, "INC", 0, "INC".Length) == 0)
            {
                directiveName = "INC";
                return true;
            }

            directiveName = string.Empty;
            return false;
        }

        private void ProcessDirective(
            string directive,
            string line,
            ExecutionResult result,
            Deque<string> results)
        {
            var directive1 = GetDirective(line);
            if (directive.Equals("INC"))
                ProcessInclude(directive1, result, results);
            else
                LogHelper.Instance.Log("PREPROCESSOR: invalid directive type " + directive, LogEntryType.Debug);
        }

        private string TranslateKeyValuePairToSymbol(Token symbol)
        {
            if (symbol.Type == TokenType.StringLiteral || symbol.Type == TokenType.NumericLiteral)
                return null;
            if (!symbol.IsKeyValuePair)
                return symbol.Value;
            if (symbol.Value.StartsWith("ENTRYTYPE") || symbol.Value.StartsWith("MODE") ||
                symbol.Value.StartsWith("TYPE") || symbol.Value.StartsWith("END"))
                return null;
            if (symbol.Value.Contains("TRUE") || symbol.Value.Contains("FALSE"))
                return null;
            return symbol.Value.Split(new string[1]
            {
                symbol.PairSeparator
            }, StringSplitOptions.RemoveEmptyEntries)[1];
        }

        private string GetDirective(string line)
        {
            int startIndex;
            if ((startIndex = line.IndexOf('"') + 1) == 0)
                return string.Empty;
            var num = line.LastIndexOf('"');
            return -1 != num ? line.Substring(startIndex, num - startIndex) : string.Empty;
        }

        private void GenExitHandler()
        {
        }

        private void ProcessInclude(string fileName, ExecutionResult result, Deque<string> results)
        {
            try
            {
                var flag = false;
                if (!File.Exists(fileName))
                {
                    if (-1 != fileName.IndexOf(Path.DirectorySeparatorChar))
                    {
                        result.Errors.Add(Error.NewError("C006",
                            "PREPROCESSOR Error processing @INC file: " + fileName + " doesn't exist.",
                            "Correct your source and recompile."));
                        return;
                    }

                    foreach (var configScriptPath in m_configScriptPaths)
                    {
                        var path = Path.Combine(configScriptPath, fileName);
                        if (File.Exists(path))
                        {
                            fileName = path;
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        result.Errors.Add(Error.NewError("C006",
                            "PREPROCESSOR Error processing @INC file " + fileName + ": unable to locate.",
                            "Correct your source and recompile."));
                        return;
                    }
                }

                var deque = new Deque<string>();
                using (var textReader = (TextReader)new StreamReader(fileName))
                {
                    while (true)
                    {
                        var str = textReader.ReadLine();
                        if (str != null)
                            deque.AddToBack(str);
                        else
                            break;
                    }
                }

                var count = deque.Count;
                for (var index = 0; index < count; ++index)
                {
                    var instance = deque.PopBottom();
                    if (!string.IsNullOrEmpty(instance))
                        results.PushTop(instance);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(Error.NewError("C006",
                    "PREPROCESSOR Error streaming include text (file " + fileName + "):" + ex.Message + ";",
                    "Check the target file for existence and accessibility."));
            }
        }

        private static string[] ParseDelimitedPaths(string wholeLot)
        {
            var rv = new List<string>();
            Array.ForEach(wholeLot.Split(Path.PathSeparator), path =>
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return;
                rv.Add(path);
            });
            return rv.ToArray();
        }
    }
}