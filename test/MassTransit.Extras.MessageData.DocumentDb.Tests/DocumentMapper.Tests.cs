using System;
using System.Text;
using Machine.Specifications;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb.Tests
{
    [Subject(typeof (DocumentMapper))]
    public class When_a_Document_is_mapped_to_the_MessageWrapper_type
    {
        Establish context = () =>
        {
            Document = ("{\"Data\": \"" + EncodedData + "\"}").ToDocument();
            Subject = new DocumentMapper();
        };

        Because of = () =>
        {
            Wrapper = Subject.Map<MessageWrapper>(Document);
        };

        It should_have_the_data_as_a_string = () =>
        {
            Wrapper.Data.ShouldEqual(ByteArray);
        };

        static DocumentMapper Subject;
        static Document Document;
        static MessageWrapper Wrapper;

        static byte[] ByteArray = Encoding.UTF8.GetBytes("This is a test string");
        static string EncodedData = Convert.ToBase64String(ByteArray);
    }
}
