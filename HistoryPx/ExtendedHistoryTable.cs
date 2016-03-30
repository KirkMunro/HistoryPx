using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace HistoryPx
{
    public class ExtendedHistoryTable : QueuedDictionary<long, ExtendedHistoryInfo>
    {
        public int ItemsPerRecord { get; set; } = 1000;
        public int Watermark { get; set; } = -1;

        internal ExtendedHistoryTable(int capacity, int itemsPerRecord)
            : base(capacity)
        {
            ItemsPerRecord = itemsPerRecord;
        }

        internal void Sync()
        {
            // Retrieve up to Capacity history ids
            List<long> idsPresent = Runspace.DefaultRunspace.GetHistory(0, Capacity)?.Select(x => x.Id).ToList();
            // If the history is empty, clear the extended history table; otherwise,
            // remove any entries in the extended history table that are not in the
            // history id list
            if (idsPresent.Count == 0)
            {
                Clear();
            }
            else
            {
                foreach (long id in Keys.Where(x => !idsPresent.Contains(x)).ToArray())
                {
                    Remove(id);
                }
            }
        }

        internal void Add(long historyId, List<PSObject> output = null, int outputCount = 0, List<IScriptExtent> outputSource = null, List<PSObject> error = null, Nullable<bool> commandSuccessful = null)
        {
            // If the table does not contains an item, add it; otherwise, add to it
            // (adding to an item is necessary when multiple commands are generated
            // as part of a single command, and the output and error data comes from
            // both commands)
            if (!ContainsKey(historyId))
            {
                Add(historyId, new ExtendedHistoryInfo(historyId, output, outputCount, outputSource, error, commandSuccessful));
            }
            else
            {
                this[historyId].Update(output, outputCount, outputSource, error);
            }
        }
    }
}
