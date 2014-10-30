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
        static public int MaximumEntryCount = 200;
        static public int MaximumItemCountPerEntry = 1000;
        static public int Watermark = -1;

        static public ExtendedHistoryInfo Item(long historyId)
        {
            return (ExtendedHistoryInfo)extendedHistoryTable[(object)historyId];
        }

        static private OrderedDictionary extendedHistoryTable = new OrderedDictionary();

        static internal void Add(long historyId, Nullable<bool> commandSuccessful, Collection<PSObject> output = null, int outputCount = 0, Collection<PSObject> error = null)
        {
            if (!extendedHistoryTable.Contains(historyId))
            {
                extendedHistoryTable.Add(historyId, new ExtendedHistoryInfo(historyId, commandSuccessful, output, outputCount, error));
            }
            else
            {
                ((ExtendedHistoryInfo)extendedHistoryTable[historyId]).Update(output, outputCount, error);
            }
            while (extendedHistoryTable.Count > MaximumEntryCount)
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
                    if (historyIds.Any(x => (long)x.BaseObject == historyId))
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
