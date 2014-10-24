using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;


namespace HistoryPx
{
    public class ExtendedHistoryTable
    {
        static public int MaximumExtendedHistoryCount = 200;

        static public ExtendedHistoryInfo Item(long historyId)
        {
            return (ExtendedHistoryInfo)extendedHistoryTable[(object)historyId];
        }

        static private OrderedDictionary extendedHistoryTable = new OrderedDictionary();

        static internal void Add(long historyId, bool commandSuccessful, Collection<PSObject> output = null, Collection<ErrorRecord> errors = null)
        {
            extendedHistoryTable.Add(historyId, new ExtendedHistoryInfo(historyId, commandSuccessful, output, errors));
            while (extendedHistoryTable.Count > MaximumExtendedHistoryCount)
            {
                extendedHistoryTable.RemoveAt(0);
            }
        }

        static internal void Clear()
        {
            extendedHistoryTable.Clear();
        }

        static internal void Remove(long historyId)
        {
            extendedHistoryTable.Remove(historyId);
        }

        static internal void RemoveAllExcept(Collection<PSObject> historyIds)
        {
            if ((historyIds == null) || (historyIds.Count == 0))
            {
                // If there are no history ids to keep, clear the table
                extendedHistoryTable.Clear();
            }
            else
            {
                // Create a collection to store the ids to remove
                Collection<long> idsToRemove = new Collection<long>();
                // Identify which ids to remove from the extended history table
                foreach (long historyId in extendedHistoryTable.Keys)
                {
                    if (historyIds.First(x => (long)x.BaseObject == historyId) != null)
                    {
                        idsToRemove.Add(historyId);
                    }
                }
                // Remove the ids from the extended history table
                foreach (long historyId in idsToRemove)
                {
                    extendedHistoryTable.Remove(historyId);
                }
            }
        }
    }
}
