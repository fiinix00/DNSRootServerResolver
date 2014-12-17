using ARSoft.Tools.Net.Dns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DNSRootServerResolver
{
    public static class DNS
    {
        public static readonly SortedList<String, Entry> GlobalCache = new SortedList<String, Entry>();

        public static readonly DnsClient EarthRoot = new DnsClient(IPAddress.Parse("192.228.79.201"), 2000);
        public static readonly DnsClient GoogleDns = new DnsClient(IPAddress.Parse("8.8.8.8"), 2000);

        public class Entry : Expirable<DnsMessage>
        {
            public Entry(Func<DnsMessage> renewer, Func<DnsMessage, DateTime> reexpirator) : base(renewer, reexpirator) { }

            public long RequestedTimes;
        }

        public static List<DnsRecordBase> Resolve(string domain, bool useGoogleLookup = false)
        {
            Entry domainCache;
            if (GlobalCache.TryGetValue(domain, out domainCache))
            {
                domainCache.RequestedTimes++;

                return domainCache.RefreshIfExpired().AnswerRecords;
            }

            if (useGoogleLookup)
            {
                var google = CacheResolver(GoogleDns, domain, domain);

                if (google != null)
                {
                    return google.AnswerRecords;
                }
            }
            else
            {
                var tld = domain.Split('.').Last();

                var countryServers = CacheResolver(EarthRoot, domain, "192.228.79.201." + tld);
                if (countryServers != null)
                {
                    foreach (var countryServer in countryServers.AdditionalRecords.OfType<ARecord>())
                    {
                        var countryServerDns = new DnsClient(countryServer.Address, 2000);
                        
                        var providerServers = CacheResolver(countryServerDns, domain, countryServer.Address + "." + domain);
                        if (providerServers != null)
                        {
                            foreach (var providerServer in providerServers.AdditionalRecords.OfType<ARecord>())
                            {
                                var providerServerDns = new DnsClient(providerServer.Address, 2000);
                                
                                var hostServers = CacheResolver(providerServerDns, domain, domain);
                                if (hostServers != null)
                                {
                                    return hostServers.AnswerRecords;
                                }
                            }
                        }
                    }
                }
            }

            return new List<DnsRecordBase>();
        }

        private static DnsMessage CacheResolver(DnsClient client, string domain, string cacheName)
        {
            Entry domainCache;
            if (GlobalCache.TryGetValue(cacheName, out domainCache))
            {
                domainCache.RefreshIfExpired();
            }
            else
            {
                Func<DnsMessage> renewer = () => client.Resolve(domain, RecordType.A);

                Func<DnsMessage, DateTime> reexpirator = (dns) =>
                {
                    if (dns != null)
                    {
                        if (dns.AnswerRecords.Count != 0)
                        {
                            return DateTime.Now.AddSeconds(dns.AnswerRecords.First().TimeToLive);
                        }
                        else if (dns.AuthorityRecords.Count != 0)
                        {
                            return DateTime.Now.AddSeconds(dns.AuthorityRecords.First().TimeToLive);
                        }
                        else if (dns.AdditionalRecords.Count != 0)
                        {
                            return DateTime.Now.AddSeconds(dns.AdditionalRecords.First().TimeToLive);
                        }
                    }

                    return DateTime.Now.AddMinutes(1);
                };

                domainCache = new Entry(renewer, reexpirator);

                GlobalCache.Add(cacheName, domainCache);
            }

            domainCache.RequestedTimes++;

            return domainCache.Value;
        }
    }
}
