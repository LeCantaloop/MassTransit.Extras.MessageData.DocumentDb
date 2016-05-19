using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Internals.Extensions;
using MassTransit.MessageData;
using Microsoft.Azure.Documents.Client;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public class DocumentDbRepository : IMessageDataRepository, IDisposable
    {
        private readonly DocumentClientReference _client;
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly Func<TimeSpan?, RequestOptions> _requestOptionsBuilder;

        private static readonly UriBuilder UriBuilder = new UriBuilder();
        private static readonly DocumentMapper DocMapper = new DocumentMapper();
        private static readonly StreamMapper StreamMapper = new StreamMapper();

        public DocumentDbRepository(DocumentClient client, string databaseId, string collectionId)
            : this(
                new DocumentClientReference { Client = client }, databaseId, collectionId,
                timeToLive => new RequestOptionsBuilder().Build(timeToLive))
        {
        }

        public DocumentDbRepository(DocumentClientReference client, string databaseId, string collectionId, Func<TimeSpan?, RequestOptions> requestOptionsBuilder)
        {
            if (client == null) { throw new ArgumentNullException(nameof(client)); }
            if (client.Client == null) { throw new ArgumentNullException(nameof(client)); }
            if (string.IsNullOrWhiteSpace(databaseId)) { throw new ArgumentNullException(nameof(databaseId)); }
            if (string.IsNullOrWhiteSpace(collectionId)) { throw new ArgumentNullException(nameof(collectionId)); }
            if (requestOptionsBuilder == null) { throw new ArgumentNullException(nameof(requestOptionsBuilder)); }

            _client = client;
            _databaseId = databaseId;
            _collectionId = collectionId;
            _requestOptionsBuilder = requestOptionsBuilder;
        }

        public async Task<Stream> Get(Uri address, CancellationToken cancellationToken = new CancellationToken())
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }

            var result =
                await _client.Client.ReadDocumentAsync(address).WithCancellation(cancellationToken).ConfigureAwait(false);

            var wrapper = DocMapper.Map<MessageWrapper>(result.Resource);
            return new MemoryStream(wrapper.Data);
        }

        public async Task<Uri> Put(Stream stream, TimeSpan? timeToLive = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

            var options = _requestOptionsBuilder.Invoke(timeToLive);
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            var wrapper = StreamMapper.Map(stream);

            var result = await
                    _client.Client.CreateDocumentAsync(uri, wrapper, options).WithCancellation(cancellationToken).ConfigureAwait(false);

            return UriBuilder.Build(result.Resource);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }
    }
}
