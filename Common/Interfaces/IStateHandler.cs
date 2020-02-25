﻿using Common.EventsArgs;
using System;
using System.Threading;

namespace Common.Interfaces
{
    public interface IStateHandler
    {
        #region Public Events

        event EventHandler AllocationSetEvent;

        event EventHandler<StateChangedArgs> SessionChangedEvent;

        #endregion Public Events

        #region Public Methods

        void Initialize(string ipAdress, int port, int retryTime, string startPattern, string statusPattern);

        void Run(CancellationToken cancellationToken);

        #endregion Public Methods
    }
}