using System;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Xunit.Abstractions;

namespace Researcher.Bot.Tests.Helpers
{
    public class XUnitLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _helper;
        private readonly ILogger _logger;

        public XUnitLogger(ITestOutputHelper helper)
        {
            _helper = helper;
            var factory = new LoggerFactory();
            _logger = factory.CreateLogger("TEST");
            factory.AddProvider(new FileLoggerProvider(@"C:\Dvlp\git\ResearcherBot\Researcher.Bot.Integration.ElasticSearch.Test\app.log", true));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var s = formatter(state, exception);
            _helper.WriteLine(s);
            if (logLevel == LogLevel.Error)
                _logger.LogError(s);
            else
                _logger.LogInformation(s);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}