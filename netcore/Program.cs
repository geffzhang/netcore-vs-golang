﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
        var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();
 
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConfiguration(config)
            .UseUrls("http://*:5000")
            .UseStartup<Startup>();

        return host;
    }
}

class Response
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long Time { get; set; }
}

class Startup
{
    private static readonly HttpMessageHandler _httpHandler = new HttpClientHandler
    {
        MaxConnectionsPerServer = 4000
    };

    private static readonly HttpClient _http = new HttpClient(_httpHandler)
    {
        BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("HOST")}:5002")
    };

    private static void HandleTest(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            using (var rsp = await _http.GetAsync("/data"))
            {
                var str = await rsp.Content.ReadAsStringAsync();

                // deserialize
                var obj = JsonSerializer.Deserialize<Response>(str);

                // serialize
                var json = JsonSerializer.Serialize<Response>(obj);

                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(json);
            }
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Map("/test", HandleTest);

        app.Run(async ctx =>
        {
            await ctx.Response.WriteAsync($"Hello, {ctx.Request.Path}");
        });
    }
}
