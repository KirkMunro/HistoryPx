using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Microsoft.PowerShell.Commands;

namespace HistoryPx
{
    [Cmdlet(
        VerbsData.Out,
        "Default"
    )]
    [OutputType(typeof(void))]
    public class OutDefaultCommand : Microsoft.PowerShell.Commands.OutDefaultCommand
    {
        private static string writeErrorStream = "writeErrorStream";
        private static string writeWarningStream = "writeWarningStream";

        protected Collection<PSObject> output = new Collection<PSObject>();
        protected bool adjustHistoryId = false;
        protected int removedObjectCount = 0;
        protected int removedHistoryInfoCount = 0;
        protected long historyId = -1;

        protected override void BeginProcessing()
        {
            // Reset OutDefaultCommand helper variables
            output.Clear();
            adjustHistoryId = false;
            removedObjectCount = 0;
            removedHistoryInfoCount = 0;

            // Look up the history id for the current command
            historyId = MyInvocation.HistoryId;

            // Let the base class do its work
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // Process input objects according to the stream to which they are being sent
            if (MyInvocation.BoundParameters.ContainsKey("InputObject") && (InputObject != null))
            {
                // If the inputobject contains an error record, isolate the ErrorRecord instance
                ErrorRecord errorRecord = null;
                if (InputObject.BaseObject is ErrorRecord)
                {
                    errorRecord = (ErrorRecord)InputObject.BaseObject;
                }
                else if ((InputObject.BaseObject as IContainsErrorRecord) != null)
                {
                    errorRecord = (ErrorRecord)InputObject.Properties["ErrorRecord"].Value;
                }

                // If an error record that is being sent to the error stream was found, adjust
                // the history id according to the error record's history id value; otherwise, if
                // the object is not of type HistoryInfo, add it to the output collection
                if ((errorRecord != null) && (InputObject.Properties[writeErrorStream] != null))
                {
                    if (errorRecord.InvocationInfo != null)
                    {
                        // If history id is -1, and if the exception was thrown from a throw
                        // statement, then we need to remove one from our history id in the
                        // EndProcessing method; otherwise, if it is  less than our current
                        // history id, we can immediately adjust our history id accordingly
                        if (errorRecord.InvocationInfo.HistoryId == -1)
                        {
                            if (((errorRecord.Exception is RuntimeException) && ((RuntimeException)errorRecord.Exception).WasThrownFromThrowStatement) ||
                                (errorRecord.Exception is ParentContainsErrorRecordException))
                            {
                                adjustHistoryId = true;
                            }
                        }
                        else if (errorRecord.InvocationInfo.HistoryId < historyId)
                        {
                            historyId = errorRecord.InvocationInfo.HistoryId;
                        }
                    }
                }
                else if ((!(InputObject.BaseObject is HistoryInfo)) && (!(InputObject.BaseObject is ExtendedHistoryInfo)))
                {
                    // HistoryInfo and ExtendedHistoryInfo instances are omitted from extended
                    // history information data to keep the memory footprint under control
                    if (output.Count < ExtendedHistoryTable.MaximumItemCountPerEntry)
                    {
                        output.Add(InputObject);
                    }
                    else
                    {
                        removedObjectCount++;
                    }
                }
                else
                {
                    removedHistoryInfoCount++;
                }
            }

            // Let the base class do its work
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            // Get the current value of the $? variable
            bool commandSucceeded = (bool)SessionState.PSVariable.Get("?").Value;

            // If the history id needs to be updated (because we received error records
            // with no history id, meaning they come from the previous command), update
            // it
            if (adjustHistoryId)
            {
                historyId--;
            }

            // Calculate the actual number of items in the output
            int outputCount = output.Count + removedHistoryInfoCount + removedObjectCount;

            // Add warnings if appropriate
            if (removedHistoryInfoCount > 0)
            {
                PSObject warningRecord = new PSObject(new WarningRecord(string.Format("{0} history information objects removed.", removedHistoryInfoCount)));
                warningRecord.Properties.Add(new PSNoteProperty(writeWarningStream, true));
                output.Insert(0, warningRecord);
            }
            if (removedObjectCount > 0)
            {
                PSObject warningRecord = new PSObject(new WarningRecord(string.Format("Output truncated: {0} objects returned, {1} objects removed.", outputCount, removedObjectCount)));
                warningRecord.Properties.Add(new PSNoteProperty(writeWarningStream, true));
                output.Insert(0, warningRecord);
            }

            // Update the last output collection in the __ variable
            if (output.Count == 1)
            {
                SessionState.PSVariable.Set("__", output[0]);
            }
            else if (output.Count > 1)
            {
                SessionState.PSVariable.Set("__", output.ToArray());
            }
            else
            {
                SessionState.PSVariable.Set("__", null);
            }

            // Add the error log entries that have been added since the last change in runspace
            // availability to the error collection
            ArrayList errorLog = (ArrayList)SessionState.PSVariable.Get("Error").Value;
            Collection<PSObject> error = new Collection<PSObject>();
            foreach (var errorLogEntry in errorLog)
            {
                if ((ExtendedHistoryTable.Watermark != -1) && (errorLogEntry.GetHashCode() == ExtendedHistoryTable.Watermark))
                {
                    // Stop when we reach our error log watermark
                    break;
                }
                if (errorLogEntry is IncompleteParseException)
                {
                    // Skip all incomplete parse exceptions
                    continue;
                }
                if (errorLogEntry is ActionPreferenceStopException)
                {
                    // Skip all action preference stop exceptions
                    continue;
                }
                error.Add(new PSObject(errorLogEntry));
            }
            if (error.Count > 0)
            {
                // If we found some errors, update our watermark and reverse the error list
                ExtendedHistoryTable.Watermark = error[0].BaseObject.GetHashCode();
                error = new Collection<PSObject>(error.Reverse().ToList());
            }

            // If we have no output or errors, then mark the command a success
            if ((outputCount == 0) && (error.Count == 0))
            {
                commandSucceeded = true;
            }

            // Add a detailed history entry to the detailed history table
            ExtendedHistoryTable.Add(historyId, commandSucceeded, output, outputCount, error);

            // Let the base class do its work
            base.EndProcessing();
        }
    }
}