using System.IO;

namespace MassTransit.Extras.MessageData.DocumentDb.Tests
{
    internal static class StreamExtensions
    {
        public static string AsString(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}