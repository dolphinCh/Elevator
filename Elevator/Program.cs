using Application.Interfaces;
using Application.Services;
using Infrastructure.Simulation;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.Write("Enter number of elevators: \n");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int noElevators))
            Console.WriteLine($"Elevators: {noElevators}");
        else
            Console.WriteLine("Invalid number");

        Console.Write("Enter how many flors the building has: \n");
        input = Console.ReadLine();

        if (int.TryParse(input, out int noFloors))
            Console.WriteLine($"Floors: {noFloors}");
        else
            Console.WriteLine("Invalid number");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var services = new ServiceCollection();
        services.AddScoped<IElevatorService, ElevatorService>(provider =>
            new ElevatorService(noElevators));
        services.AddScoped<SimulationEngine>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var simulation = scope.ServiceProvider.GetRequiredService<SimulationEngine>();

        Console.WriteLine("Elevator Simulation Running (Ctrl+C to stop)\n");
        await simulation.RunAsync(cts.Token, noFloors);
    }
}