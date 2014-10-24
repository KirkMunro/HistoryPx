using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace HistoryPx
{
    public class ExtendedHistoryInfo
    {
        internal ExtendedHistoryInfo(long historyId, bool commandSuccessful, Collection<PSObject> output = null, Collection<ErrorRecord> errors = null)
        {
            Id = historyId;
            CommandSuccessful = commandSuccessful;
            Output = output != null ? output.ToArray() : null;
            Errors = errors != null ? errors.ToArray() : null;
        }

        public long Id { get; private set; }
        public bool CommandSuccessful { get; private set; }
        public PSObject[] Output { get; private set; }
        public ErrorRecord[] Errors { get; private set; }
    }
}