using System;
using System.Threading;

namespace AFSM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
		static Mutex mutex = new Mutex(true, "{8f7112b5-18a2-4152-896c-97e0fb647681}");
		public App()
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				mutex.ReleaseMutex();
			}
			else
			{
				Current.Shutdown();
			}
		}
    }
}
