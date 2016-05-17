using System;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    internal class UriBuilder
    {
        public Uri Build(Document document)
        {
            return new Uri(document.SelfLink, UriKind.Relative);
        }
    }
}
