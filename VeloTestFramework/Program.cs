using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Velopack;

namespace VeloTestFramework
{
    internal class Program
    {
        public static MemoryLogger Log { get; private set; }

        [STAThread]
        public static void Main2(string[] args)
        {
            try
            {
                // Logging is essential for debugging! Ideally you should write it to a file.
                Log = new MemoryLogger();

                // It's important to Run() the VelopackApp as early as possible in app startup.
                VelopackApp.Build()
                    .WithFirstRun((v) => { /* Your first run code here */ })
                    .Run(Log);

                // We can now launch the WPF application as normal.
                var app = new App();
                app.InitializeComponent();
                app.Run();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unhandled exception: " + ex.ToString());
            }
        }
    }

    public class LogUpdatedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }

    public class MemoryLogger : ILogger
    {
        public event EventHandler<LogUpdatedEventArgs> LogUpdated;
        private readonly StringBuilder _sb = new StringBuilder();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (_sb)
            {
                var message = formatter(state, exception);
                if (exception != null) message += "\n" + exception.ToString();
                Console.WriteLine("log: " + message);
                _sb.AppendLine(message);
                LogUpdated?.Invoke(this, new LogUpdatedEventArgs { Text = _sb.ToString() });
            }
        }

        public override string ToString()
        {
            lock (_sb)
            {
                return _sb.ToString();
            }
        }
    }
}
