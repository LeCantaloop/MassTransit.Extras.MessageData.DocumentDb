using System.IO;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    internal class StreamMapper
    {
        public MessageWrapper Map(Stream stream)
        {
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                return new MessageWrapper
                {
                    Data = memory.ToArray()
                };
            }
        }
    }
}