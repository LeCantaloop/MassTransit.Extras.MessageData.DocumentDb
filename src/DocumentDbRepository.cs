using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Internals.Extensions;
using MassTransit.MessageData;
using Microsoft.Azure.Documents.Client;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public class DocumentDbRepository : IMessageDataRepository
    {
        private readonly Func<DocumentClient> _clientFactory;
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly Func<TimeSpan?, RequestOptions> _requestOptionsBuilder;

        private static readonly UriBuilder UriBuilder = new UriBuilder();
        private static readonly DocumentMapper DocMapper = new DocumentMapper();
        private static readonly StreamMapper StreamMapper = new StreamMapper();

        public DocumentDbRepository(Func<DocumentClient> clientFactory, string databaseId, string collectionId)
            : this(
                clientFactory, databaseId, collectionId,
                timeToLive => new RequestOptionsBuilder().Build(timeToLive))
        {
        }

        /// <summary>
        /// Create a new instance of the DocumentDbRepository
        /// </summary>
        /// <param name="clientFactory">
        /// A factory method used to create a DocumentDB client. A client will be created and disposed after each request.
        /// </param>
        /// <param name="databaseId"></param>
        /// <param name="collectionId"></param>
        /// <param name="requestOptionsBuilder"></param>
        public DocumentDbRepository(Func<DocumentClient> clientFactory, string databaseId, string collectionId, Func<TimeSpan?, RequestOptions> requestOptionsBuilder)
        {
            if (clientFactory == null) { throw new ArgumentNullException(nameof(clientFactory)); }
            if (string.IsNullOrWhiteSpace(databaseId)) { throw new ArgumentNullException(nameof(databaseId)); }
            if (string.IsNullOrWhiteSpace(collectionId)) { throw new ArgumentNullException(nameof(collectionId)); }
            if (requestOptionsBuilder == null) { throw new ArgumentNullException(nameof(requestOptionsBuilder)); }

            _clientFactory = clientFactory;
            _databaseId = databaseId;
            _collectionId = collectionId;
            _requestOptionsBuilder = requestOptionsBuilder;
        }

        public async Task<Stream> Get(Uri address, CancellationToken cancellationToken = new CancellationToken())
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }

            using (var client = _clientFactory.Invoke())
            {
                var result =
                    await client.ReadDocumentAsync(address).WithCancellation(cancellationToken).ConfigureAwait(false);

                var wrapper = DocMapper.Map<MessageWrapper>(result.Resource);
                return new MemoryStream(wrapper.Data);
            }
        }

        public async Task<Uri> Put(Stream stream, TimeSpan? timeToLive = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

            using (var client = _clientFactory.Invoke())
            {
                var options = _requestOptionsBuilder.Invoke(timeToLive);
                var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var wrapper = StreamMapper.Map(stream);

                var result = await
                        client.CreateDocumentAsync(uri, wrapper, options).WithCancellation(cancellationToken).ConfigureAwait(false);

                return UriBuilder.Build(result.Resource);
            }
        }
    }
}
