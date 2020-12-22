# DNSTest

Some simple test scripts to test DNS failures.

### Where does all this come from?
I have the netgear Orbi router and satellite. There are cases where it seems that wifi clients have errors. The error from chrome says somethine like "Dns_Probe_Finished_No_Internet". Some websites have inferenced that the problem lies with a configuration error in the dnsmasq setup used on the router.

Before I try to fix things I wanted something that I could use to reproduce the problem as right now its' only when my wife tells me her phone is not working or a kid is trying to use their Ipad for school. SO this this is my attempt to try and create something that is reproducible...

### How to run
simple:

    dotnet run -- iterations:500 dictionaryFile:C:\temp\DNSTest\pwsh\words.txt topLevelDomains:ca com net resolver 192.168.1.1


