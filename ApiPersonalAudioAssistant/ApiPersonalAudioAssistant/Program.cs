using ApiPersonalAudioAssistant;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseStartup<Startup>()
                    .UseUrls("http://0.0.0.0:8080", "http://0.0.0.0:8090"); // слухати на всіх IP
            });
}
