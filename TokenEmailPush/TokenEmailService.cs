using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TokenEmailPush
{
    public partial class TokenEmailService : ServiceBase
    {
        Thread startThread = null;
        ApplicationController applicationController = new ApplicationController();
        public TokenEmailService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            startThread = new Thread(new ThreadStart(applicationController.Start));
            startThread.Start();
        }

        protected override void OnStop()
        {
            applicationController.StopService();
            startThread.Abort();
        }
    }
}
