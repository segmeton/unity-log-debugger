using UnityEngine;

namespace Segmeton.UnityDebugger
{
    public class LogData
    {
        #region Variables
        private int count = 1;
        private LogType _logType;
        private string _condition;
        private string _stacktrace;
        #endregion

        #region Init
        public void Init(string condition, string stacktrace, LogType logType)
        {
            _condition = condition;
            _stacktrace = stacktrace;
            _logType = logType;
        }
        #endregion

        #region Setter Getter
        public int Count
        {
            set { count = value; }
            get { return count; }
        }

        public LogType LogType
        {
            set { _logType = value; }
            get { return _logType; }
        }

        public string Condition
        {
            set { _condition = value; }
            get { return _condition; }
        }

        public string StackTrace
        {
            set { _stacktrace = value; }
            get { return _stacktrace; }
        }
        #endregion
    }
}