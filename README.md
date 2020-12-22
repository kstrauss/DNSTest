# DNSTest

Some simple test scripts to test DNS failures.

### Where does all this come from?
I have the netgear Orbi router and satellite. There are cases where it seems that wifi clients have errors. The error from chrome says somethine like "Dns_Probe_Finished_No_Internet". Some websites have inferenced that the problem lies with a configuration error in the dnsmasq setup used on the router. Some say that it has to do with UDP flood protection:
https://community.netgear.com/t5/Nighthawk-WiFi-Routers/R7000P-DNS-PROBE-FINISHED-NXDOMAIN-Error/m-p/1632821
https://community.netgear.com/t5/Nighthawk-WiFi-Routers/Fatal-Flaw-in-NETGEAR-Routers-Read-Before-Buying/td-p/1134948

Before I try to fix things I wanted something that I could use to reproduce the problem as right now its' only when my wife tells me her phone is not working or a kid is trying to use their Ipad for school. SO this this is my attempt to try and create something that is reproducible...

### How to run
simple:

    dotnet run -- iterations:500 dictionaryFile:C:\temp\DNSTest\pwsh\words.txt topLevelDomains:ca com net resolver 192.168.1.1


