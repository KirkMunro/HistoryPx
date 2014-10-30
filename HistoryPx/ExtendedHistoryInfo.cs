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
        internal ExtendedHistoryInfo(long historyId, Nullable<bool> commandSuccessful = null, Collection<PSObject> output = null, int outputCount = 0, Collection<PSObject> error = null)
        {
            Id = historyId;
            CommandSuccessful = commandSuccessful;
            this.output = output != null ? output : null;
            OutputCount = outputCount;
            this.error = error != null ? error : null;
        }

        private Collection<PSObject> output = null;
        private Collection<PSObject> error = null;

        public long Id { get; private set; }
        public Nullable<bool> CommandSuccessful { get; private set; }
        public PSObject[] Output
        {
            get
            {
                if (output != null)
                {
                    return output.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }
        public int OutputCount { get; private set; }
        public PSObject[] Error
        {
            get
            {
                if (error != null)
                {
                    return error.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Update(Collection<PSObject> output, int outputCount, Collection<PSObject> error)
        {
            this.output = new Collection<PSObject>(this.output.Concat(output).ToList());
            OutputCount += outputCount;
            this.error = new Collection<PSObject>(this.error.Concat(error).ToList());
        }
    }
}