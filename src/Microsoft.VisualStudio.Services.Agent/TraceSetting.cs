// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Agent.Sdk.Knob;

namespace Microsoft.VisualStudio.Services.Agent
{
    [DataContract]
    public class TraceSetting
    {
        public TraceSetting() : this(HostType.Agent, null)
        {
        }

        public TraceSetting(HostType hostType, IKnobValueContext knobContext = null)
        {
            // Write debug info to a temp file since console might be redirected
            var debugFile = @"C:\temp\tracesetting_debug.log";
            try 
            {
                System.IO.File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] TraceSetting constructor called - HostType: {hostType}, knobContext: {(knobContext != null ? "not null" : "null")}\n");
            } catch { }

            if (hostType == HostType.Agent)
            {
                // Enable logs by default for listener
                DefaultTraceLevel = TraceLevel.Verbose;
                try { System.IO.File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Agent hostType - set to Verbose\n"); } catch { }
                return;
            }

            DefaultTraceLevel = TraceLevel.Info; // Default to Info for worker
            try { System.IO.File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Initial DefaultTraceLevel set to Info for Worker\n"); } catch { }

            if (hostType == HostType.Worker)
            {
                try { System.IO.File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Worker logging will be controlled dynamically during job execution via JobRunner\n"); } catch { }
                // Worker verbose logging is now handled dynamically in JobRunner.cs after ExecutionContext is available
                // This allows access to pipeline variables through the knob system
            }

#if DEBUG
            // Only enable verbose in debug for non-worker or when explicitly requested
            if (hostType != HostType.Worker)
            {
                DefaultTraceLevel = TraceLevel.Verbose; // Enable Verbose in Debug builds for non-worker
            }
#endif

            // Worker logging is now controlled by the WorkerLogs knob only
        }

        [DataMember(EmitDefaultValue = false)]
        public TraceLevel DefaultTraceLevel
        {
            get;
            set;
        }

        public Dictionary<String, TraceLevel> DetailTraceSetting
        {
            get
            {
                if (_detailTraceSetting == null)
                {
                    _detailTraceSetting = new Dictionary<String, TraceLevel>(StringComparer.OrdinalIgnoreCase);
                }
                return _detailTraceSetting;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "DetailTraceSetting")]
        private Dictionary<String, TraceLevel> _detailTraceSetting;
    }

    [DataContract]
    public enum TraceLevel
    {
        [EnumMember]
        Off = 0,

        [EnumMember]
        Critical = 1,

        [EnumMember]
        Error = 2,

        [EnumMember]
        Warning = 3,

        [EnumMember]
        Info = 4,

        [EnumMember]
        Verbose = 5,
    }

    public static class TraceLevelExtensions
    {
        public static SourceLevels ToSourceLevels(this TraceLevel traceLevel)
        {
            switch (traceLevel)
            {
                case TraceLevel.Off:
                    return SourceLevels.Off;
                case TraceLevel.Critical:
                    return SourceLevels.Critical;
                case TraceLevel.Error:
                    return SourceLevels.Error;
                case TraceLevel.Warning:
                    return SourceLevels.Warning;
                case TraceLevel.Info:
                    return SourceLevels.Information;
                case TraceLevel.Verbose:
                    return SourceLevels.Verbose;
                default:
                    return SourceLevels.Information;
            }
        }
    }
}