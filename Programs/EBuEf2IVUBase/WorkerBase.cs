using Common.Enums;
using Common.EventsArgs;
using Common.Interfaces;
using EBuEf2IVUBase.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EBuEf2IVUBase
{
    public abstract class WorkerBase
        : BackgroundService
    {
        #region Protected Fields

        protected readonly IConfiguration config;
        protected readonly IDatabaseConnector databaseConnector;
        protected readonly ILogger logger;
        protected readonly IStateHandler sessionStateHandler;

        protected SessionStates currentState;
        protected DateTime ebuefSessionStart = DateTime.Now;
        protected DateTime ivuSessionDate = DateTime.Now;

        #endregion Protected Fields

        #region Private Fields

        private TimeSpan ebuefTimeshift;
        private CancellationTokenSource sessionCancellationTokenSource;

        #endregion Private Fields

        #region Protected Constructors

        protected WorkerBase(IConfiguration config, IStateHandler sessionStateHandler,
            IDatabaseConnector databaseConnector, ILogger<WorkerBase> logger)
        {
            var assemblyInfo = Assembly.GetExecutingAssembly().GetName();
            logger.LogInformation(
                "{name} (Version {version}) wird gestartet.",
                assemblyInfo.Name,
                $"{assemblyInfo.Version.Major}.{assemblyInfo.Version.Minor}");

            this.config = config;
            this.logger = logger;

            this.databaseConnector = databaseConnector;

            this.sessionStateHandler = sessionStateHandler;
            this.sessionStateHandler.SessionChangedEvent += OnSessionChanged;
        }

        #endregion Protected Constructors

        #region Protected Methods

        protected CancellationToken GetSessionCancellationToken(CancellationToken workerCancellationToken)
        {
            sessionCancellationTokenSource = new CancellationTokenSource();
            workerCancellationToken.Register(() => sessionCancellationTokenSource.Cancel());

            return sessionCancellationTokenSource.Token;
        }

        protected DateTime GetSimTime()
        {
            return DateTime.Now.Add(ebuefTimeshift);
        }

        protected void InitializeConnector(CancellationToken sessionCancellationToken)
        {
            var connectorSettings = config
                .GetSection(nameof(EBuEfDBConnector))
                .Get<EBuEfDBConnector>();

            databaseConnector.Initialize(
                connectionString: connectorSettings.ConnectionString,
                retryTime: connectorSettings.RetryTime,
                cancellationToken: sessionCancellationToken);
        }

        protected void InitializeStateHandler()
        {
            var settings = config
                .GetSection(nameof(StatusReceiver))
                .Get<StatusReceiver>();

            sessionStateHandler.Initialize(
                ipAdress: settings.IpAddress,
                port: settings.ListenerPort,
                retryTime: settings.RetryTime,
                startPattern: settings.StartPattern,
                statusPattern: settings.StatusPattern);
        }

        protected async Task StartIVUSessionAsync()
        {
            var currentSession = await databaseConnector.GetEBuEfSessionAsync();

            ivuSessionDate = currentSession.IVUDatum;
            ebuefSessionStart = ivuSessionDate
                .Add(currentSession.SessionStart.TimeOfDay);

            ebuefTimeshift = currentSession.Verschiebung;

            logger.LogDebug(
                "Die IVU-Sitzung läuft am {sessionDate} um {sessionTime}.",
                ivuSessionDate.ToString("yyyy-MM-dd"),
                ebuefSessionStart.ToString("hh:mm"));
        }

        #endregion Protected Methods

        #region Private Methods

        private void OnSessionChanged(object sender, StateChangedArgs e)
        {
            if (e.State == SessionStates.IsRunning)
            {
                logger?.LogInformation("Sessionstart-Nachricht empfangen.");

                sessionCancellationTokenSource.Cancel();

                currentState = e.State;
            }
            else if (e.State == SessionStates.IsPaused)
            {
                logger?.LogInformation("Sessionpause-Nachricht empfangen.");

                currentState = e.State;
            }
            else if (e.State == SessionStates.IsEnded)
            {
                logger?.LogInformation("Sessionende-Nachricht empfangen.");

                currentState = e.State;
            }
        }

        #endregion Private Methods
    }
}