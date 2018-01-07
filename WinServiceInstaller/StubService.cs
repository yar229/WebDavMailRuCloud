using System;
using System.ServiceProcess;

namespace WinServiceInstaller
{
    class StubService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            FireStart?.Invoke();
        }

        protected override void OnStop()
        {
            FireStop?.Invoke();
        }

        public Action FireStart { get; set; }
        public Action FireStop { get; set; }
    }
}
