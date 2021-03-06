﻿using System;
using Amazon.SimpleNotificationService.Util;
using Newtonsoft.Json;

namespace Bynder.Workers
{
    public class NotificationWorker : IWorker
    {
        public class Result : WorkerResult
        {
            public string MediaId { get; set; }
        }

        public Result Execute(string requestBody)
        {
            var result = new Result();

            var snsMessage = Message.ParseMessage(requestBody);
            if (snsMessage == null) throw new ArgumentException("Cannot parse Request Body as AWS SNS message");

            // check if (initial) subscription type
            if (snsMessage.IsSubscriptionType && snsMessage.IsMessageSignatureValid())
            {
                result.Messages.Add("AWS SNS Subscription message received");
                snsMessage.SubscribeToTopic();
                result.Messages.Add("AWS SNS Subscription acknowleged");
                return result;
            }

            // check if notification & notification topic is expected
            if (snsMessage.IsNotificationType)
            {
                string[] allowedSubjects =
                {
                    "asset_bank.media.uploaded",
                    "asset_bank.media.pre_archived",
                    "asset_bank.media.upload",
                    "asset_bank.media.create"
                };

                if (!Array.Exists(allowedSubjects, s => s.Equals(snsMessage.Subject.ToString())))
                {
                    result.Messages.Add($"AWS SNS - Not acting on subject {snsMessage.Subject}");
                    return result;
                }

                dynamic innerMessage = JsonConvert.DeserializeObject(snsMessage.MessageText);
                if (!string.IsNullOrEmpty(innerMessage?.media_id?.ToString()))
                {
                    var mediaId = innerMessage.media_id.ToString();
                    result.Messages.Add($"AWS SNS - Media update for media_id '{mediaId}'");
                    result.MediaId = mediaId;
                    return result;
                }
            }

            return result;
        }
    }

}