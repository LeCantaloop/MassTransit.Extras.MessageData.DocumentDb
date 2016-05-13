using System;
using System.IO;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public interface IDocumentSerializer
    {
        Stream Serialize(Document document);
    }

    public class DocumentSerializer : IDocumentSerializer
    {
        public Stream Serialize(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            var stream = new MemoryStream();
            document.SaveTo(stream);
            stream.Position = 0;

            return stream;
        }
    }
}