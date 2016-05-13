using System.IO;
using Machine.Specifications;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb.Tests
{
    [Subject(typeof (DocumentSerializer))]
    public class When_serializing_a_document_to_a_stream
    {
        Establish context = () =>
        {
            Subject = new DocumentSerializer();
            Document = new Document();

            Document.SetPropertyValue(Key, Value);
        };

        Because of = () => Stream = Subject.Serialize(Document);

        It should_be_at_position_0 = () => Stream.Position.ShouldEqual(0);

        It should_be_the_string_representation_of_the_document = () =>
        {
            Stream.AsString().ShouldEqual("{\"" + Key + "\":\"" + Value + "\"}");
        };

        static IDocumentSerializer Subject;
        static Stream Stream;
        static Document Document;

        static string Key = "MyKey";
        static string Value = "MyValue";
    }
}
