#if NET48 || NET7_0_WINDOWS
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WinServiceInstaller
{
    class StubService : ServiceBase
    {
        private Task _runner;

        protected override void OnStart(string[] args)
        {
            _runner = Task.Factory.StartNew(() =>
            {
                FireStart?.Invoke();
            });
            
        }

        protected override void OnStop()
        {
            FireStop?.Invoke();
        }

        public Action FireStart { get; set; }
        public Action FireStop { get; set; }
    }
}
#endif
