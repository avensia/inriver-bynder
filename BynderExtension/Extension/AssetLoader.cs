using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Bynder.Api;
using Bynder.Workers;
using inRiver.Remoting;
using inRiver.Remoting.Connect;
using inRiver.Remoting.Extension.Interface;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;
using inRiver.Remoting.Util;

namespace Bynder.Extension
{
    public class AssetLoader : Extension, IScheduledExtension
    {
        /// <summary>
        /// Get a list of all assetIds from Bynder using the configured filter Query
        /// which will be executed against api/v4/media/?-----
        /// for each found asset, process it using the worker implementation as if it would have been triggered by a notificaton message
        /// </summary>
        public void Execute(bool force)
        {
            if (!force)
            {
                if (!ScheduledTime()) return;
            }

            try
            {

                Context.Logger.Log(LogLevel.Information, "Start loading assets");

                var worker = Container.GetInstance<AssetUpdatedWorker>();
                var bynderClient = Container.GetInstance<BynderClient>();

                // get all assets ids
                // note: this is a paged result set, call next page until reaching end.
                var counter = 0;
                var assetCollection = bynderClient.GetAssetCollection(Context.Settings[Config.Settings.InitialAssetLoadUrlQuery]);
                Context.Logger.Log(LogLevel.Information, $"Start processing {assetCollection.GetTotal()} assets.");

                assetCollection.Media.ForEach(a => worker.Execute(a.Id));
                counter += assetCollection.Media.Count;
                while (!assetCollection.IsLastPage())
                {
                    // when not reached end get next group of assets
                    assetCollection = bynderClient.GetAssetCollection(
                        Context.Settings[Config.Settings.InitialAssetLoadUrlQuery],
                        assetCollection.GetNextPage());
                    assetCollection.Media.ForEach(a => worker.Execute(a.Id));
                    counter += assetCollection.Media.Count;
                    Context.Logger.Log(LogLevel.Information, $"Processed {counter} assets.");
                }
                Context.Logger.Log(LogLevel.Information, "Initial Import Successful!");

                SaveConnectorState(DateTime.Now);
            }
            catch (System.Exception ex)
            {
                Context.Log(LogLevel.Error, ex.GetBaseException().Message, ex);
            }
        }

        private bool ScheduledTime()
        {
            var currentDateTime = DateTime.Now;
            var connectorStates = Context.ExtensionManager.UtilityService.GetAllConnectorStatesForConnector(Context.ExtensionId);
            if (connectorStates.Count.Equals(0))
            {
                var newConnectorState = new ConnectorState
                {
                    ConnectorId = Context.ExtensionId,
                    Data = currentDateTime.ToString("g")
                };
                Context.ExtensionManager.UtilityService.AddConnectorState(newConnectorState);
                return true;
            }

            var connectorState = connectorStates.Count > 1 ? connectorStates.OrderBy(state => state.Created).Last() : connectorStates.Single();

            var lastRunTime = connectorState.Data;
            if (string.IsNullOrEmpty(lastRunTime)) return false;


            var (hours, minutes) = GetHoursAndMinutes();

            if (currentDateTime.Hour.Equals(hours) && currentDateTime.Minute >= minutes)
            {
                var oneDayTimeSpan = new TimeSpan(24, 0, 0);
                if (currentDateTime.Subtract(oneDayTimeSpan) >= DateTime.Parse(lastRunTime))
                {
                    DeleteConnectorStates(connectorStates);
                    return true;
                }
            }
            return false;
        }

        private void SaveConnectorState(DateTime dateTime)
        {
            var connectorState = new ConnectorState
            {
                ConnectorId = Context.ExtensionId,
                Data = dateTime.ToString("g")
            };
            Context.ExtensionManager.UtilityService.AddConnectorState(connectorState);
        }

        private void DeleteConnectorStates(List<ConnectorState> connectorStates)
        {
            Context.ExtensionManager.UtilityService.DeleteConnectorStates(connectorStates.Select(c => c.Id).ToList());
        }

        private (int, int) GetHoursAndMinutes()
        {
            var time = ScheduledRun.Split(':');
            if (int.TryParse(time[0], out var hours) && int.TryParse(time[1], out var minutes))
            {
                return (hours, minutes);
            }
            return (6, 0);
        }

        private string ScheduledRun => Context.Settings[Config.Settings.ScheduledTime];
    }
}