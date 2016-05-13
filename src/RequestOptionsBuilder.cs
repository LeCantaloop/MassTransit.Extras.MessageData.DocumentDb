using System;
using Microsoft.Azure.Documents.Client;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    internal class RequestOptionsBuilder
    {
        public RequestOptions Build(TimeSpan? timeToLive)
        {
            var options = new RequestOptions();
            if (timeToLive.HasValue)
            {
                options.ResourceTokenExpirySeconds = Convert.ToInt32(timeToLive.Value.TotalSeconds);
            }
            return options;
        }
    }
}