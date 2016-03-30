using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace HistoryPx
{
    public class ExtendedHistoryInfo
    {
        internal ExtendedHistoryInfo(long historyId, List<PSObject> output = null, int outputCount = 0, List<IScriptExtent> outputSources = null, List<PSObject> error = null, Nullable<bool> commandSuccessful = null)
        {
            Id = historyId;
            this.output = output;
            OutputCount = outputCount;
            this.outputSource = outputSources;
            this.error = error;
            CommandSuccessful = commandSuccessful;
        }

        private List<PSObject> output = new List<PSObject>();
        private List<PSObject> error = new List<PSObject>();
        private List<IScriptExtent> outputSource = new List<IScriptExtent>();

        public long Id { get; private set; }
        public Nullable<bool> CommandSuccessful { get; private set; }
        public PSObject[] Output
        {
            get
            {
                if ((output != null) && (output.Count > 0))
                {
                    return output.ToArray();
                }

                return null;
            }
        }
        public int OutputCount { get; private set; }
        public IScriptExtent[] OutputSource
        {
            get
            {
                if ((outputSource != null) && (outputSource.Count > 0))
                {
                    return outputSource.ToArray();
                }

                return null;
            }
        }
        public PSObject[] Error
        {
            get
            {
                if ((error != null) && (error.Count > 0))
                {
                    return error.ToArray();
                }

                return null;
            }
        }

        public void Update(List<PSObject> output, int outputCount, List<IScriptExtent> outputSource, List<PSObject> error)
        {
            if ((output != null) && (output.Count > 0))
            {
                if (this.output == null)
                {
                    this.output = new List<PSObject>();
                }
                this.output.AddRange(output);
            }
            OutputCount += outputCount;
            if ((outputSource != null) && (outputSource.Count > 0))
            {
                if (this.outputSource == null)
                {
                    this.outputSource = new List<IScriptExtent>();
                }
                this.outputSource.AddRange(outputSource);
            }
            if ((error != null) && (error.Count > 0))
            {
                if (this.error == null)
                {
                    this.error = new List<PSObject>();
                }
                this.error.AddRange(error);
            }
        }
    }
}