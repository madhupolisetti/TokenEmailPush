using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace TokenEmailPush
{
    public static class SharedClass
    {
        private static Dictionary<Environment, string> _connectionStrings = new Dictionary<Environment, string>();
        private static bool _pollStaging = false;
        private static bool _hasStopSignal = false;
        private static bool _isServiceCleaned = false;
        private static ILog _logger = null;
        private static string _tokenAttachmentsPath = string.Empty;
        public static QueueSlno QueueSlno = new QueueSlno();
        public static void InitializeLogger()
        {
            GlobalContext.Properties["LogName"] = DateTime.Now.ToString("yyyyMMdd");
            log4net.Config.XmlConfigurator.Configure();
            _logger = log4net.LogManager.GetLogger("Log");
        }
        public static void SetConnectionString(Environment environment, string connectionString)
        {
            if (_connectionStrings.ContainsKey(environment))
                _connectionStrings[environment] = connectionString;
            else
                _connectionStrings.Add(environment, connectionString);
        }
        public static string GetConnectionString(Environment environment)
        {
            if (!_connectionStrings.ContainsKey(environment))
                throw new KeyNotFoundException("Key " + environment.ToString() + " Not Found in the dictionary");
            else
                return _connectionStrings[environment];
        }
        #region PROPERTIES
        public static bool PollStaging
        {
            get { return _pollStaging; }
            set { _pollStaging = value; }
        }
        public static bool HasStopSignal
        {
            get { return _hasStopSignal; }
            set { _hasStopSignal = value; }
        }
        public static bool IsServiceCleaned
        {
            get { return _isServiceCleaned; }
            set { _isServiceCleaned = value; }
        }
        public static ILog Logger
        {
            get { return _logger; }
        }
        public static string TokenAttachmentsPath
        {
            get { return _tokenAttachmentsPath; }
            set { _tokenAttachmentsPath = value; }
        }
        #endregion
    }
}
