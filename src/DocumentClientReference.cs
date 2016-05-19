using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace MassTransit.Extras.MessageData.DocumentDb
{
    public class DocumentClientReference : IDisposable
    {
        public DocumentClientReference()
        {
            IsOwned = true;
        }

        public DocumentClient Client { get; set; }
        public bool IsOwned { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && IsOwned)
            {
                Client.Dispose();
            }
        }
    }
}
