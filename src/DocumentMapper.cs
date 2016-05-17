using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    internal class DocumentMapper
    {
        public T Map<T>(Document document)
        {
            return (T) (dynamic) document;
        }
    }
}
