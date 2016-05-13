using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.MessageData;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public class DocumentDbRepository : IMessageDataRepository
    {
        private readonly Func<DocumentClient> _clientFactory;
        private readonly string _databaseId;
        private readonly string _collectionId;

        /// <summary>
        /// Create a new instance of the DocumentDbRepository
        /// </summary>
        /// <param name="clientFactory">
        /// A factory method used to create a DocumentDB client. A client will be created and disposed after each request.
        /// </param>
        /// <param name="databaseId"></param>
        /// <param name="collectionId"></param>
        public DocumentDbRepository(Func<DocumentClient> clientFactory, string databaseId, string collectionId)
        {
            _clientFactory = clientFactory;
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

        public async Task<Stream> Get(Uri address, CancellationToken cancellationToken = new CancellationToken())
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }

            using (var client = _clientFactory.Invoke())
            {
                var result = await client.ReadDocumentAsync(address).ConfigureAwait(false);

                var stream = new MemoryStream();
                var writer = new JsonTextWriter(new StreamWriter(stream));
                JsonSerializer.Create().Serialize(writer, result.Resource);
                writer.Flush();

                stream.Position = 0;
                return stream;
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

                var result = await client.CreateDocumentAsync(uri, JsonSerializable.LoadFrom<Document>(stream), options).ConfigureAwait(false);
                return new Uri(result.Resource.SelfLink);
            }
        }
    }
}
