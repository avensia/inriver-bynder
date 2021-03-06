using System;
using System.Linq;
using Bynder.Api;
using Bynder.Api.Model;
using Bynder.Workers;
using inRiver.Remoting.Extension.Interface;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;
using Newtonsoft.Json;

namespace Bynder.Extension
{
    public class AssetLoader : Extension, IScheduledExtension
    {
        private readonly Lazy<ConnectorState> _lazyConnectorState;
        private ConnectorState ConnectorState => _lazyConnectorState.Value;

        private readonly Lazy<DateTime> _lazyLastRunTime;
        private DateTime LastRunTime => _lazyLastRunTime.Value;

        private string ScheduledRun => Context.Settings[Config.Settings.FullSyncScheduledTime];

        public AssetLoader()
        {
            _lazyConnectorState = new Lazy<ConnectorState>(() =>
            {
                var connectorStates = Context.ExtensionManager.UtilityService.GetAllConnectorStatesForConnector(Context.ExtensionId);
                if (connectorStates.Any())
                {
                    return connectorStates.Select(state => (state.Modified, state: state)).Max().state;
                }

                var newConnectorState = new ConnectorState
                {
                    ConnectorId = Context.ExtensionId,
                    Data = null
                };
                return Context.ExtensionManager.UtilityService.AddConnectorState(newConnectorState);
            });

            _lazyLastRunTime = new Lazy<DateTime>(() =>
            {
                if (string.IsNullOrEmpty(ConnectorState.Data))
                {
                    return DateTime.MinValue;
                }

                try
                {
                    return JsonConvert.DeserializeObject<DateTime?>(ConnectorState.Data) ?? DateTime.MinValue;
                }
                catch (JsonException jsonException)
                {
                    Context.Log(LogLevel.Error,
                        $"Failed to deserialize connector state data as DateTime?. Data: {ConnectorState.Data}",
                        jsonException);
                }

                return DateTime.MinValue;
            });
        }

        /// <summary>
        /// Get a list of all assetIds from Bynder using the configured filter Query
        /// which will be executed against api/v4/media/?-----
        /// for each found asset, process it using the worker implementation as if it would have been triggered by a notificaton message
        /// </summary>
        public void Execute(bool force)
        {
            try
            {
                Context.Logger.Log(LogLevel.Information, "Start loading assets");

                var worker = Container.GetInstance<AssetUpdatedWorker>();
                var bynderClient = Container.GetInstance<BynderClient>();
                var lastRunTime = FullSync() ? DateTime.MinValue : LastRunTime;
                var startTime = DateTime.Now.AddDays(1); //Do sync every other day

                // get all assets ids
                // note: this is a paged result set, call next page until reaching end.
                var counter = 0;
                var assetCollection = bynderClient.GetAssetCollection(Context.Settings[Config.Settings.InitialAssetLoadUrlQuery], limit: 1000);
                Context.Logger.Log(LogLevel.Information, $"Start processing {assetCollection.GetTotal()} assets.");
                ProcessAssets(assetCollection, worker, lastRunTime, ref counter);

                while (!assetCollection.IsLastPage())
                {
                    // when not reached end get next group of assets
                    assetCollection = bynderClient.GetAssetCollection(
                        Context.Settings[Config.Settings.InitialAssetLoadUrlQuery],
                        assetCollection.GetNextPage(), assetCollection.Limit);
                    ProcessAssets(assetCollection, worker, lastRunTime, ref counter);
                }

                ConnectorState.Data = JsonConvert.SerializeObject(startTime);
                Context.ExtensionManager.UtilityService.UpdateConnectorState(ConnectorState);

                Context.Logger.Log(LogLevel.Information, "Initial Import Successful!");
            }
            catch (Exception ex)
            {
                Context.Log(LogLevel.Error, ex.GetBaseException().Message, ex);
            }
        }

        private void ProcessAssets(AssetCollection assetCollection, AssetUpdatedWorker worker, DateTime? lastRunTime,
            ref int counter)
        {
            assetCollection.Media.ForEach(a => worker.Execute(a.Id, lastRunTime));
            counter += assetCollection.Media.Count;
            Context.Logger.Log(LogLevel.Information, $"Processed {counter} assets.");
        }

        private bool FullSync()
        {
            if (!TryGetHoursAndMinutes(out var hours, out var minutes))
            {
                return false;
            }

            var now = DateTime.Now;
            var todaysRunTime = DateTime.Today.AddHours(hours).AddMinutes(minutes);
            return now >= todaysRunTime && !(LastRunTime >= todaysRunTime);

        }

        private bool TryGetHoursAndMinutes(out int hours, out int minutes)
        {
            hours = 0;
            minutes = 0;
            var time = ScheduledRun.Split(':');
            return time.Length > 1 && int.TryParse(time[0], out hours) && int.TryParse(time[1], out minutes);
        }
    }
}