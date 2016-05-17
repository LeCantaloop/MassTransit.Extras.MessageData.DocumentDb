﻿using System;
using System.IO;
using System.Text;
using Machine.Specifications;
using Microsoft.Azure.Documents;

namespace MassTransit.Extras.MessageData.DocumentDb.Tests
{
    [Subject(typeof(UriBuilder))]
    public class When_a_Uri_is_built
    {
        Establish context = () =>
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"_self\": \"" + Link + "\"}")))
            {
                Document = JsonSerializable.LoadFrom<Document>(stream);
            }
            Subject = new UriBuilder();
        };

        Because of = () =>
        {
            Uri = Subject.Build(Document);
        };

        It should_be_a_relative_Uri = () =>
        {
            Uri.IsAbsoluteUri.ShouldBeFalse();
        };

        It should_be_the_self_link = () =>
        {
            Uri.OriginalString.ShouldEqual(Link);
        };

        static string Link = "/link/to/doc";
        static UriBuilder Subject;
        static Document Document;
        static Uri Uri;
    }
}
