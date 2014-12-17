using ARSoft.Tools.Net.Dns;
using System;
using System.Net;
using System.Net.Sockets;

namespace DNSRootServerResolver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var server = new DnsServer(IPAddress.Any, 25, 25, DnsServerHandler))
            {
                server.Start();

                string command;
                while ((command = Console.ReadLine()) != "exit")
                {
                    if (command == "clear")
                    {
                        DNS.GlobalCache.Clear();
                    }
                }
            }
        }

        public static DnsMessageBase DnsServerHandler(DnsMessageBase dnsMessageBase, IPAddress clientAddress, ProtocolType protocolType)
        {
            DnsMessage query = dnsMessageBase as DnsMessage;

            foreach (var question in query.Questions)
            {
                switch (question.RecordType)
                {
                    case RecordType.A: 
                        query.AnswerRecords.AddRange(DNS.Resolve(question.Name)); break;

                    case RecordType.Ptr: 
                        {
                            if (question.Name == "1.0.0.127.in-addr.arpa")
                            {
                                query.AnswerRecords.Add(new PtrRecord("127.0.0.1", 172800 /*2 days*/, "localhost"));
                            }
                        }; break;
                }
            }

            return query;
        }
    }
}
