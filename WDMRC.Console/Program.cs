using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;

namespace YaR.CloudMailRu.Console
{
    class SampleService :
        ServiceControl
    {
        readonly bool _throwOnStart;
        readonly bool _throwOnStop;
        readonly bool _throwUnhandled;
        static readonly LogWriter _log = HostLogger.Get<SampleService>();

        public SampleService(bool throwOnStart, bool throwOnStop, bool throwUnhandled)
        {
            _throwOnStart = throwOnStart;
            _throwOnStop = throwOnStop;
            _throwUnhandled = throwUnhandled;
        }

        public bool Start(HostControl hostControl)
        {
            _log.Info("SampleService Starting...");

            hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

            Thread.Sleep(1000);

            if (_throwOnStart)
            {
                _log.Info("Throwing as requested");
                throw new InvalidOperationException("Throw on Start Requested");
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                Thread.Sleep(3000);

                if (_throwUnhandled)
                    throw new InvalidOperationException("Throw Unhandled In Random Thread");

                _log.Info("Requesting stop");

                hostControl.Stop();
            });
            _log.Info("SampleService Started");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("SampleService Stopped");

            if (_throwOnStop)
                throw new InvalidOperationException("Throw on Stop Requested!");

            return true;
        }

        public bool Pause(HostControl hostControl)
        {
            _log.Info("SampleService Paused");

            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            _log.Info("SampleService Continued");

            return true;
        }
    }


    public class Program 
    {
        static void Main(string[] args)
        {
            Payload.Run(args);
        }
    }
}
