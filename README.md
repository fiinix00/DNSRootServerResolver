DNSRootServerResolver
=====================

Instead of going through the internet providers DNS cache and getting logged, this resolves domains from the root of the world (root-servers) and then down to the websites dns.

```csharp
var client = new DnsClient(IPAddress.Loopback, 2000);
var result = client.Resolve("google.com");
```

1. Connects to b.root-server (192.228.79.201) and asks for "com"
2. The result is a *root server* that knows about "com" {a.gtld-servers.net - 192.5.6.30}
3. Connects and asks for "google.com" 
4. The result is googles *nameserver* {ns2.google.com - 216.239.34.10}
5. Connects and asks for "google.com"
6. The result is googles *webserver* {google.com - 216.58.209.128}
7. The answer is sent back to the user.
