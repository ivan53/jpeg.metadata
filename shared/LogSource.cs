using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class LogSource : TraceSource
    {
        private static Dictionary<string, SourceLevels> sourceLevelsMap = new Dictionary<string, SourceLevels> 
        { 
            { string.Empty, SourceLevels.Information },
            { "DataReader", SourceLevels.Information },
        };

        private static List<TraceListener> defaultListeners = new List<TraceListener>();

        public LogSource(string sourceName) : base (sourceName)
        {
            this.Listeners.AddRange(Trace.Listeners);
            foreach (TraceListener listener in defaultListeners)
            {
                this.Listeners.Add(listener);
            }
        }

        public LogSource(string sourceName, SourceLevels sourceLevels)
            : base(sourceName, sourceLevels)
        {
            this.Listeners.AddRange(Trace.Listeners);
            foreach (TraceListener listener in defaultListeners)
            {
                this.Listeners.Add(listener);
            }
        }

        public static SourceLevels SourceLevel(string sourceName)
        {
            SourceLevels sourceLevels;
            if (!sourceLevelsMap.TryGetValue(sourceName, out sourceLevels))
            {
                sourceLevels = sourceLevelsMap[string.Empty];
            }
            return sourceLevels;
        }

        public static void SetSourceLevel(string sourceName, SourceLevels sourceLevels)
        {
            sourceLevelsMap[sourceName] = sourceLevels;
        }

        public static void AddDefaultListener(TraceListener listener)
        {
            defaultListeners.Add(listener);
        }

        public Exception TraceException(Exception e)
        {
            this.TraceEvent(TraceEventType.Error, 0, "Exception thrown: {0}", e.ToString());
            return e;
        }

        public new void TraceEvent(TraceEventType eventType, int id, string message)
        {
            base.TraceEvent(eventType, id, message);
        }

        public new void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            base.TraceEvent(eventType, id, format, args);
        }
    }
}
