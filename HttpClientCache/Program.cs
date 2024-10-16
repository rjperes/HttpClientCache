using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;

namespace HttpClientCache
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(options =>
            {
                options.AddConsole();
            });

            services.AddMemoryCache();

            services.AddOptions<CachingHandlerOptions>().Configure(options =>
            {
                options.CacheDuration = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient("Todos", client =>
            {
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/todos/");
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
                client.DefaultRequestHeaders.Add(HeaderNames.CacheControl, "public, max-age=360");
            }).AddHttpMessageHandler<CachingHandler>();

            services.AddTransient<CachingHandler>();

            var serviceProvider = services.BuildServiceProvider();

            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient("Todos");

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json1 = await httpClient.GetStringAsync("1");
            //var todo1 = JsonSerializer.Deserialize<Todo>(json1, jsonSerializerOptions);
            var todo1 = await httpClient.GetFromJsonAsync<Todo>("1");

            httpClient = httpClientFactory.CreateClient("Todos");
            var json2 = await httpClient.GetStringAsync("1");
            //var todo2 = JsonSerializer.Deserialize<Todo>(json2, jsonSerializerOptions);
            var todo2 = await httpClient.GetFromJsonAsync<Todo>("1");

            httpClient = httpClientFactory.CreateClient("Todos");
            var json3 = await httpClient.GetStringAsync("1");
            //var todo3 = JsonSerializer.Deserialize<Todo>(json3, jsonSerializerOptions);
            var todo3 = await httpClient.GetFromJsonAsync<Todo>("1");
        }
    }

    public record Todo(int Id, int UserId, string Title, bool Completed);
}
