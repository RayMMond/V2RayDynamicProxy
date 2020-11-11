using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using V2Ray.Core;
using V2Ray.Core.App.Proxyman.Command;
using V2Ray.Core.Common.Net;
using V2Ray.Core.Common.Protocol;
using V2Ray.Core.Common.Serial;
using V2Ray.Core.Proxy.Socks;
using YuXiang.Infrastructures.Http;

namespace DynamicProxy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            const int poolSize = 5;

            var proxySourceProvider = new HttpProxySourceProvider(
                new OptionsManager<HttpProxyOptions>(
                    new OptionsFactory<HttpProxyOptions>(new List<IConfigureOptions<HttpProxyOptions>>
                    {
                        new ConfigureOptions<HttpProxyOptions>(options =>
                        {
                            options.RequestUrl = InfrastructuresHttpModule.DefaultHttpProxyRequestUrl;
                            options.BatchSize = InfrastructuresHttpModule.DefaultBatchSize;
                            options.FormatArguments = new object[]
                            {
                                (int) ProxyType.积分套餐, InfrastructuresHttpModule.DefaultBatchSize,
                                (int) ProxyKind.Socks5, (int) AvailableTime.Fixed15Minutes
                            };
                            options.Retry = InfrastructuresHttpModule.DefaultRetry;
                            options.Kind = ProxyKind.Socks5;
                        })
                    }, new List<IPostConfigureOptions<HttpProxyOptions>>())), new ConsoleLoggerFactory());

            var source = proxySourceProvider.Create();
            using var channel = GrpcChannel.ForAddress("http://localhost:54672");
            var client = new HandlerService.HandlerServiceClient(channel);

            while (true)
            {
                Console.WriteLine($"updating proxy ...");
                var task = Task.Delay(TimeSpan.FromMinutes(14));
                var list = new List<ServerEndpoint>();
                for (var i = 0; i < poolSize; i++)
                {
                    var proxy = await source.TakeAsync();

                    list.Add(new ServerEndpoint
                    {
                        Address = new IPOrDomain
                        {
                            Ip = ByteString.CopyFrom(IPAddress.Parse(proxy.Ip).GetAddressBytes())
                        },
                        Port = (uint) proxy.Port,
                    });
                }


                var config = new ClientConfig
                {
                    Server =
                    {
                        list
                    }
                };


                var request = new AddOutboundRequest
                {
                    Outbound = new OutboundHandlerConfig
                    {
                        Tag = "dynamic_socks5",
                        ProxySettings = new TypedMessage
                        {
                            Type = "v2ray.core.proxy.socks.ClientConfig",
                            Value = config.ToByteString()
                        }
                    }
                };

                Console.Write($"Sending request ... ");
                _ = await client.AddOutboundAsync(request);
                Console.WriteLine("Success");
                Console.WriteLine($"wait for 14 minutes ...");
                await task;
            }
        }
    }
}