﻿#pragma warning disable CA1031 // Do not catch general exception types

using Common.Extensions;
using Common.Interfaces;
using Common.Models;
using CredentialChannelFactory;
using EnumerableExtensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrainPathImportService;
using TrainPathSender.Converters;

namespace TrainPathSender
{
    public class Sender
        : ITrainPathSender
    {
        #region Private Fields

        private readonly ConcurrentQueue<importTrainPaths> importsQueue = new ConcurrentQueue<importTrainPaths>();
        private readonly ILogger<Sender> logger;

        private Factory<TrainPathImportWebFacadeChannel> channelFactory;
        private Message2ImportPaths converter;
        private AsyncRetryPolicy retryPolicy;
        private string trainPathState;

        #endregion Private Fields

        #region Public Constructors

        public Sender(ILogger<Sender> logger)
        {
            this.logger = logger;
        }

        #endregion Public Constructors

        #region Public Methods

        public void AddMessages(IEnumerable<TrainPathMessage> messages)
        {
            if (messages.AnyItem())
            {
                var imports = converter.Get(
                    messages: messages,
                    trainPathState: trainPathState);

                if (imports != default)
                    importsQueue.Enqueue(imports);
            }
        }

        public void Initialize(string host, int port, string path, string username, string password, bool isHttps,
            int retryTime, DateTime sessionDate, string infrastructureManager, string orderingTransportationCompany,
            string trainPathState, string stoppingReasonStop, string stoppingReasonPass, string importProfile,
            bool preferPrognosis)
        {
            this.trainPathState = trainPathState;

            converter = new Message2ImportPaths(
                sessionDate: sessionDate,
                infrastructureManager: infrastructureManager,
                orderingTransportationCompany: orderingTransportationCompany,
                stoppingReasonStop: stoppingReasonStop,
                stoppingReasonPass: stoppingReasonPass,
                importProfile: importProfile,
                preferPrognosis: preferPrognosis);

            channelFactory = new Factory<TrainPathImportWebFacadeChannel>(
                host: host,
                port: port,
                path: path,
                userName: username,
                password: password,
                isHttps: isHttps,
                notIgnoreCertificateErrors: true);

            retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: (p) => TimeSpan.FromSeconds(retryTime),
                    onRetry: (exception, reconnection) => OnRetry(
                        exception: exception,
                        reconnection: reconnection));
        }

        public Task RunAsnc(CancellationToken cancellationToken)
        {
            var result = retryPolicy.ExecuteAsync(
                action: (token) => RunSenderAsync(token),
                cancellationToken: cancellationToken);

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnRetry(Exception exception, TimeSpan reconnection)
        {
            while (exception.InnerException != null) exception = exception.InnerException;

            logger.LogError(
                "Fehler beim Senden der Trassendaten an IVU.rail: {message}\r\n" +
                "Die Verbindung wird in {reconection} Sekunden wieder versucht.",
                exception.Message,
                reconnection.TotalSeconds);
        }

        private async Task RunSenderAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!importsQueue.IsEmpty)
                {
                    var currentImport = importsQueue.GetFirst().FirstOrDefault();

                    if (currentImport != default)
                    {
                        using var channel = channelFactory.Get();
                        var response = await channel.importTrainPathsAsync(currentImport);

                        if (response.trainPathImportResponse != default)
                        {
                            logger.LogDebug(
                                "Trassen wurden mit folgender ID an IVU.rail gesendet: {id}",
                                response.trainPathImportResponse.protocolTransactionId);
                        }

                        importsQueue.TryDequeue(out importTrainPaths _);
                    }
                }
            }
        }

        #endregion Private Methods
    }
}

#pragma warning disable CA1031 // Do not catch general exception types