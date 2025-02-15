using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console
{
    internal class ListViewFactory
    {
        private ListViewFactory()
        {
        }

        internal static ListViewFactory Instance => Singleton<ListViewFactory>.Instance;

        internal void MakeTab(ListViewNames tab, bool enable)
        {
            switch (tab)
            {
                case ListViewNames.Errors:
                    CreateErrorListViewTab(enable);
                    break;
                case ListViewNames.Job:
                    CreateJobListViewTab(enable);
                    break;
                case ListViewNames.OutputWindow:
                    CreateOutputListViewTab(true);
                    break;
                case ListViewNames.ProgramEvents:
                    CreateProgramEventListViewTab(enable);
                    break;
                case ListViewNames.Results:
                    CreateResultListViewTab(enable);
                    break;
                case ListViewNames.Stack:
                    CreateStackListViewTab(enable);
                    break;
                case ListViewNames.Symbols:
                    CreateSymbolListViewTab(enable);
                    break;
            }
        }

        private void CreateJobListViewTab(bool enable)
        {
            ListViewTabControl.Instance.Add(ListViewNames.Job, new JobListView<HardwareJobWrapper>(new string[8, 2]
            {
                {
                    "Job ID",
                    "ID"
                },
                {
                    "Connection State",
                    "ConnectionState"
                },
                {
                    "Label",
                    "Label"
                },
                {
                    "Running Program",
                    "ProgramName"
                },
                {
                    "Priority",
                    "Priority"
                },
                {
                    "Status",
                    "Status"
                },
                {
                    "Start Time",
                    "StartTime"
                },
                {
                    "Execution Time",
                    "ExecutionTime"
                }
            }, JobHelper.JobList), null, enable);
        }

        private void CreateStackListViewTab(bool enable)
        {
            var control = new StackListView<StringWrapper>(new string[1, 2]
            {
                {
                    "Stack Values",
                    "Value"
                }
            }, JobHelper.StackList);
            ListViewTabControl.Instance.Add(ListViewNames.Stack, control, control.RefreshData, enable);
        }

        private void CreateSymbolListViewTab(bool enable)
        {
            var control = new SymbolListView<Symbol>(new string[2, 2]
            {
                {
                    "Name",
                    "Name"
                },
                {
                    "Value",
                    "Value"
                }
            }, JobHelper.SymbolList);
            ListViewTabControl.Instance.Add(ListViewNames.Symbols, control, control.RefreshData, enable);
        }

        private void CreateResultListViewTab(bool enable)
        {
            var control = new ResultListView<StringWrapper>(new string[1, 2]
            {
                {
                    "Results",
                    "Value"
                }
            }, JobHelper.ResultList);
            ListViewTabControl.Instance.Add(ListViewNames.Results, control, control.RefreshData, enable);
        }

        private void CreateOutputListViewTab(bool enable)
        {
            ListViewTabControl.Instance.Add(ListViewNames.OutputWindow, OutputWindow.Instance, null, true);
        }

        private void CreateErrorListViewTab(bool enable)
        {
            ListViewTabControl.Instance.Add(ListViewNames.Errors, ErrorListView.Instance, null, true);
        }

        private void CreateProgramEventListViewTab(bool enable)
        {
            var control = new ProgramEventListView<ProgramEvent>(new string[2, 2]
            {
                {
                    "Date",
                    "EventTime"
                },
                {
                    "Message",
                    "Message"
                }
            }, JobHelper.ProgramEventList);
            ListViewTabControl.Instance.Add(ListViewNames.ProgramEvents, control, control.RefreshData, enable);
        }
    }
}