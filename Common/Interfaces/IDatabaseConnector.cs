using Common.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IDatabaseConnector
    {
        #region Public Methods

        Task AddRealtimeAsync(TrainLeg leg);

        Task<EBuEfSession> GetEBuEfSessionAsync();

        Task<IEnumerable<string>> GetLocationShortnamesAsync(IEnumerable<string> locationTypes);

        Task<IEnumerable<TrainRun>> GetTrainRunsDispoAsync(int trainId, bool preferPrognosis = false);

        Task<IEnumerable<TrainRun>> GetTrainRunsDispoAsync(TimeSpan minTime, TimeSpan maxTime);

        Task<IEnumerable<TrainRun>> GetTrainRunsPlanAsync(int timetableId, DayOfWeek weekday,
            bool preferPrognosis = false);

        Task<int?> GetTrainTypeIdAsync(string zuggattung);

        Task<IEnumerable<VehicleAllocation>> GetVehicleAllocationsAsync();

        void Initialize(string connectionString, int retryTime, CancellationToken sessionCancellationToken);

        Task SetCrewingsAsync(IEnumerable<CrewingElement> crewingElements);

        #endregion Public Methods
    }
}