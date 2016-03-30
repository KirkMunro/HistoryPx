using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections;
using System.Management.Automation.Language;

namespace HistoryPx
{
    public class ExtendedHistoryManager
    {
        private static readonly Lazy<ExtendedHistoryManager> lazyInstance = new Lazy<ExtendedHistoryManager>(() => new ExtendedHistoryManager());

        private static ExtendedHistoryManager Instance { get { return lazyInstance.Value; } }

        private Dictionary<Guid, ExtendedHistoryTable> runspaceExtendedHistory = new Dictionary<Guid, ExtendedHistoryTable>();

        private List<Guid> runspacesProcessingModuleRemoveEvent = new List<Guid>();

        private ExtendedHistoryManager()
        {
        }

        private void OnRunspaceStateChanged(object sender, RunspaceStateEventArgs e)
        {
            if (e.RunspaceStateInfo.State == RunspaceState.Closing)
            {
                var runspace = sender as Runspace;
                var runspaceId = runspace.InstanceId;
                if (runspaceExtendedHistory.ContainsKey(runspaceId))
                {
                    runspace.StateChanged -= OnRunspaceStateChanged;
                    runspaceExtendedHistory[runspaceId].Clear();
                    runspaceExtendedHistory.Remove(runspaceId);
                }
            }
        }

        private void OnRunspaceAvailabilityChanged(object sender, RunspaceAvailabilityEventArgs e)
        {
            if ((e.RunspaceAvailability == RunspaceAvailability.Available) ||
                (e.RunspaceAvailability == RunspaceAvailability.None))
            {
                var runspace = sender as Runspace;
                var runspaceId = runspace.InstanceId;
                if (runspacesProcessingModuleRemoveEvent.Contains(runspaceId))
                {
                    runspace.AvailabilityChanged -= OnRunspaceAvailabilityChanged;
                    runspacesProcessingModuleRemoveEvent.Remove(runspaceId);
                    if (runspaceExtendedHistory.ContainsKey(runspaceId))
                    {
                        runspaceExtendedHistory[runspaceId].Clear();
                        runspaceExtendedHistory.Remove(runspaceId);
                    }
                }
            }
        }

        private ExtendedHistoryTable GetTable()
        {
            var runspace = Runspace.DefaultRunspace;
            if (runspace == null)
            {
                return null;
            }

            var runspaceId = runspace.InstanceId;
            if (!runspaceExtendedHistory.ContainsKey(runspaceId))
            {
                runspaceExtendedHistory.Add(runspaceId, new ExtendedHistoryTable(MaximumEntryCount, MaximumItemCountPerEntry));
                runspace.StateChanged += OnRunspaceStateChanged;
            }

            return runspaceExtendedHistory[runspaceId];
        }

        private void RemoveTable()
        {
            var runspace = Runspace.DefaultRunspace;
            var runspaceId = runspace.InstanceId;
            if (runspacesProcessingModuleRemoveEvent.Contains(runspaceId))
            {
                return;
            }
            if (runspaceExtendedHistory.ContainsKey(runspaceId))
            {
                runspacesProcessingModuleRemoveEvent.Add(runspaceId);
                runspace.AvailabilityChanged += OnRunspaceAvailabilityChanged;
            }
        }

        private void AddEntry(long historyId, List<PSObject> output, int outputCount, List<IScriptExtent> outputSources, List<PSObject> error, bool commandSuccessful)
        {
            // If we don't have a default runspace, then just throw the extended history information away
            if (Runspace.DefaultRunspace == null)
            {
                return;
            }
            // Add the extended history information to the runspace extended history table
            var runspaceId = Runspace.DefaultRunspace.InstanceId;
            if (runspaceExtendedHistory.ContainsKey(runspaceId))
            {
                runspaceExtendedHistory[runspaceId].Add(historyId, output, outputCount, outputSources, error, commandSuccessful);
            }
        }

        internal static int MaximumEntryCount { get; set; } = 200;
        internal static int MaximumItemCountPerEntry { get; set; } = 1000;

        public static int Watermark
        {
            get
            {
                var extendedHistoryTable = RunspaceExtendedHistoryTable;
                return extendedHistoryTable != null ? extendedHistoryTable.Watermark : -1;
            }
            set
            {
                var extendedHistoryTable = RunspaceExtendedHistoryTable;
                if (extendedHistoryTable != null)
                {
                    extendedHistoryTable.Watermark = value;
                }
            }
        }

        private static ExtendedHistoryTable RunspaceExtendedHistoryTable
        {
            get
            {
                return Instance.GetTable();
            }
        }

        internal static void OnImportModule()
        {
            // Set the watermark to the hash code for the most recent error

            var errorList = Runspace.DefaultRunspace?.GetSessionState()?.PSVariable.GetValueAtScope("Error", "1") as IEnumerable;
            if (errorList != null)
            {
                IEnumerator enumerator = errorList.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    ExtendedHistoryManager.Watermark = enumerator.Current.GetHashCode();
                }
            }

            // Retrieve the table for the current runspace to initialize it
            Instance.GetTable();
        }

        internal static void OnRemoveModule()
        {
            // Get the current runspace
            var runspace = Runspace.DefaultRunspace;

            // Remove the last captured output variable from global scope when the module is unloaded
            runspace?.GetSessionState()?.PSVariable.Remove(CaptureOutputConfiguration.VariableName);

            // Remove the extended history table for the current runspace
            Instance.RemoveTable();
        }

        internal static void Add(long historyId, List<PSObject> output, int outputCount, List<IScriptExtent> outputSources, List<PSObject> error, bool commandSuccessful)
        {
            Instance.AddEntry(historyId, output, outputCount, outputSources, error, commandSuccessful);
        }

        internal static void SyncWithHistoryTable()
        {
            RunspaceExtendedHistoryTable.Sync();
        }

        internal static PSObject ExtendHistoryInfoObject(PSObject item)
        {
            // If the item we receive is not a history info object, pass it through as is
            if (!(item.BaseObject is HistoryInfo))
            {
                return item;
            }

            // Add a reference to the wrapped HistoryInfo object to make it easier
            HistoryInfo historyInfo = (HistoryInfo)item.BaseObject;

            // Add a custom type name to identify the extended object
            item.TypeNames.Insert(0, "Microsoft.PowerShell.Commands.HistoryInfo#Extended");

            // Add a duration property to the extended history information object
            item.Members.Add(new PSNoteProperty("Duration", historyInfo.EndExecutionTime != null ? historyInfo.EndExecutionTime - historyInfo.StartExecutionTime : (object)null));

            // Look up the extended history information in our table
            ExtendedHistoryInfo extendedHistoryInfo = RunspaceExtendedHistoryTable.ContainsKey(historyInfo.Id) ? RunspaceExtendedHistoryTable[historyInfo.Id] : null;

            // Add a success property to the extended history information object
            bool success = false;
            if (historyInfo.ExecutionStatus != PipelineState.Failed)
            {
                if (extendedHistoryInfo != null)
                {
                    success = (bool)extendedHistoryInfo.CommandSuccessful;
                }
                else
                {
                    success = historyInfo.ExecutionStatus == PipelineState.Completed;
                }
            }
            item.Members.Add(new PSNoteProperty("Success", success));

            // Add an output property to the extended history information object
            item.Members.Add(new PSNoteProperty("Output", extendedHistoryInfo?.Output));

            // Add an outputcount property to the extended history information object
            item.Members.Add(new PSNoteProperty("OutputCount", extendedHistoryInfo?.OutputCount > 0 ? extendedHistoryInfo.OutputCount : (object)null));

            // Add an outputsources property to the extended history information object
            item.Members.Add(new PSNoteProperty("OutputSource", extendedHistoryInfo?.OutputSource));

            // Add an outputsources property to the extended history information object
            item.Members.Add(new PSNoteProperty("OutputSourceCount", extendedHistoryInfo?.OutputSource?.Length));

            // Add an error property to the extended history information object
            item.Members.Add(new PSNoteProperty("Error", extendedHistoryInfo?.Error));

            // Add an errorcount property to the extended history information object
            item.Members.Add(new PSNoteProperty("ErrorCount", extendedHistoryInfo?.Error?.Length));

            return item;
        }

    }
}
