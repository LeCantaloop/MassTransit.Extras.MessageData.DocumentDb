using System.IO;
using System.Text;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb.Tests
{
    internal static class StringExtensions
    {
        public static Document ToDocument(this string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return JsonSerializable.LoadFrom<Document>(stream);
            }
        }
    }
}
