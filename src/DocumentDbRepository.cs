using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Internals.Extensions;
using MassTransit.MessageData;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public class DocumentDbRepository : IMessageDataRepository
    {
        private readonly Func<DocumentClient> _clientFactory;
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly IDocumentSerializer _serializer;

        /// <summary>
        /// Create a new instance of the DocumentDbRepository
        /// </summary>
        /// <param name="clientFactory">
        /// A factory method used to create a DocumentDB client. A client will be created and disposed after each request.
        /// </param>
        /// <param name="databaseId"></param>
        /// <param name="collectionId"></param>
        /// <param name="serializer"></param>
        public DocumentDbRepository(Func<DocumentClient> clientFactory, string databaseId, string collectionId, IDocumentSerializer serializer)
        {
            if (clientFactory == null) { throw new ArgumentNullException(nameof(clientFactory)); }
            if (string.IsNullOrWhiteSpace(databaseId)) { throw new ArgumentNullException(nameof(databaseId)); }
            if (string.IsNullOrWhiteSpace(collectionId)) { throw new ArgumentNullException(nameof(collectionId)); }
            if (serializer == null) { throw new ArgumentNullException(nameof(serializer)); }

            _clientFactory = clientFactory;
            _databaseId = databaseId;
            _collectionId = collectionId;
            _serializer = serializer;
        }

        public async Task<Stream> Get(Uri address, CancellationToken cancellationToken = new CancellationToken())
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }

            using (var client = _clientFactory.Invoke())
            {
                var result =
                    await client.ReadDocumentAsync(address).WithCancellation(cancellationToken).ConfigureAwait(false);
                return _serializer.Serialize(result.Resource);
            }
        }

        public async Task<Uri> Put(Stream stream, TimeSpan? timeToLive = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

            using (var client = _clientFactory.Invoke())
            {
                var options = new RequestOptions();
                if (timeToLive.HasValue)
                {
                    options.ResourceTokenExpirySeconds = Convert.ToInt32(timeToLive.Value.TotalSeconds);
                }

                var uri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, Guid.NewGuid().ToString());

                var result =
                    await
                        client.CreateDocumentAsync(uri, JsonSerializable.LoadFrom<Document>(stream), options)
                            .WithCancellation(cancellationToken)
                            .ConfigureAwait(false);
                return new Uri(result.Resource.SelfLink);
            }
        }
    }
}
