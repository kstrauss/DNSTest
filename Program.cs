﻿using DnsClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DNSTest { 
    class Program
    {

        static void Main(string[] args)
        {
            var rootCommand = new RootCommand {
                new Option<int>("iterations", ()=>5, "Number of queries to send"){AllowMultipleArgumentsPerToken = false, IsRequired = false },
                new Option<FileSystemInfo>("dictionaryFile","Provide a text file with possible domain names"),
                new Option<string[]>("topLevelDomains", ()=> new []{".com",".net"}, "Top level domains to random pick from"),
                new Option<IPAddress>(alias:"resolver", parseArgument: ConvertIPAddress, description:"The dns server to run against")
                
                {
                    AllowMultipleArgumentsPerToken = false, IsRequired = true 
                    //Argument = new Argument<IPAddress>(parse: v=>{Console.Out.WriteLine("here");  return IPAddress.Parse(v.Tokens.Single().Value); })
                },
                new Option<bool>(alias:"UseUdp", getDefaultValue: ()=>false)
            };

            rootCommand.Handler = CommandHandler.Create<FileSystemInfo, int,string[],IPAddress,bool>(
                (dictionaryFile ,iterations, topLevelDomains, resolver, UseUdp) => new Program().RunLinqPad(dictionaryFile,iterations, topLevelDmains: topLevelDomains, dnsServer: resolver, UseUDPOnly:UseUdp));
            rootCommand.Invoke(args);
        }

        static IPAddress ConvertIPAddress(ArgumentResult result)
        {
            if (result.Tokens.Count == 1)
            {
                if (IPAddress.TryParse(result.Tokens[0].Value, out IPAddress outAddress))
                    return outAddress;
                result.ErrorMessage = $"Invalid IPAddress : {result.Tokens[0].Value}";
                return null;
            }
            result.ErrorMessage = $"Invalid IPAddress : {String.Join(",", result.Tokens)}";
            return null;
        }

        void RunLinqPad(FileSystemInfo domainNameList, int iterations, string[] topLevelDmains, IPAddress dnsServer = null, bool UseUDPOnly = false)
        {
            var settings = dnsServer == null ? new LookupClientOptions() : new LookupClientOptions(dnsServer);
            settings.EnableAuditTrail = true;
            settings.Timeout = TimeSpan.FromSeconds(10);
            settings.Recursion = true;
            settings.UseTcpFallback = UseUDPOnly ? false : true;
            
            var lc = new LookupClient(settings);

            var words = File.ReadAllLines(domainNameList.FullName);
            var random = new Random();
            var wordMaxLength = words.Length;

            var domains = new List<string>();

            for (int i = 0; i < iterations; i++)
            {
                var alleged = words[random.Next(0, wordMaxLength)];
                domains.Add($"{alleged}.{topLevelDmains[random.Next(0, topLevelDmains.Length)]}");
            }

            var queryResults = new ConcurrentDictionary<string, Task<TimedResponse>>();


            Parallel.ForEach(domains, domain =>
            {
                var sw = Stopwatch.StartNew();

                queryResults.TryAdd(domain, lc.QueryAsync(domain, QueryType.A).ContinueWith(v=> { sw.Stop(); return new TimedResponse { Response = v.Result, TimeTaken = sw.Elapsed }; }));
            });

            Console.WriteLine($"Waiting for {queryResults.Count} requests to complete");
            Task.WhenAll(queryResults.Values).Wait();


            var errors = queryResults
                .Where(e => e.Value.Result.Response.HasError)// && e.Value.Result.Header.ResponseCode != DnsClient.DnsHeaderResponseCode.NotExistentDomain)
                .ToList();
            foreach (var thing in errors
                .Select(e => new { Name = e.Key, Response = e.Value.Result }))
            {
                Console.WriteLine("lookup failure for {0} of {1}, but took {2} milliseconds",thing.Name, thing.Response.Response.ErrorMessage, thing.Response.TimeTaken.TotalMilliseconds);
            }

            // summary of errors
            foreach (var errorGroup in errors.GroupBy(e => e.Value.Result.Response.ErrorMessage))
            {
                Console.WriteLine($"{errorGroup.Key} has {errorGroup.Count()} instances");
            }

            var success = queryResults
                .Where(e => !e.Value.Result.Response.HasError).ToList();
            if (success.Any())
                Console.WriteLine($"Successfully processed {success.Count()}, in an average of {success.Average(v=>v.Value.Result.TimeTaken.TotalMilliseconds)} milliseconds");
        }
    }

    public class TimedResponse
    {
        public TimeSpan TimeTaken { get; set; }
        public IDnsQueryResponse Response { get; set; }
    }
}
