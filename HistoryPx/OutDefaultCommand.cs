using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
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
        protected Collection<PSObject> output = new Collection<PSObject>();

        protected override void BeginProcessing()
        {
            // Remove all data from the output property
            output.Clear();

            // Let the base class do its work
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // Add any input objects to the output collection
            if (MyInvocation.BoundParameters.ContainsKey("InputObject"))
            {
                output.Add(InputObject);
            }

            // Let the base class do its work
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            // Get the current value of the $? variable
            bool lastCommandSucceeded = (bool)SessionState.PSVariable.Get("?").Value;

            // Look up any errors that this command produced
            Collection<ErrorRecord> errors = new Collection<ErrorRecord>();
            foreach (object entry in (ArrayList)SessionState.PSVariable.Get("Error").Value)
            {
                if (entry is ErrorRecord)
                {
                    ErrorRecord errorRecord = (ErrorRecord)entry;
                    if (errorRecord.InvocationInfo.HistoryId == MyInvocation.HistoryId)
                    {
                        errors.Add(errorRecord);
                    }
                }
            }

            // Remove any errors on this command from the output collection
            if (errors.Count > 0)
            {
                for (int index = output.Count - 1; index >= 0; index--)
                {
                    PSObject item = output[index];
                    if ((item.BaseObject is ErrorRecord) && (errors.Contains(item.BaseObject)))
                    {
                        output.RemoveAt(index);
                    }
                }
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

            // Add a detailed history entry to the detailed history table
            ExtendedHistoryTable.Add(MyInvocation.HistoryId, lastCommandSucceeded, output, errors);

            // Let the base class do its work
            base.EndProcessing();
        }
    }
}