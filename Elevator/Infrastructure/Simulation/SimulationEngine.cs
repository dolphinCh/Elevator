using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Simulation
{
    public class SimulationEngine(IElevatorService elevatorService)
    {
        private readonly IElevatorService _elevatorService = elevatorService;

        public async Task RunAsync(CancellationToken token, int noFloors)
        {
            int tick = 0;

            while (!token.IsCancellationRequested)
            {
                tick++;

                if (tick % 10 == 0)
                {
                    var passenger = GeneratePassenger(noFloors);
                    Log($"Request from floor {passenger.Origin} to floor {passenger.Destination}");

                    _elevatorService.HandlePassengerRequest(passenger, noFloors, Log);
                }

                _elevatorService.Step(noFloors, Log);

                await Task.Delay(1000, token);
            }
        }

        private Passenger GeneratePassenger(int noFloors)
        {
            var random = Random.Shared;

            int origin = random.Next(1, noFloors + 1);
            int destination;

            do
            {
                destination = random.Next(1, noFloors + 1);
            } while (destination == origin);

            return new Passenger(origin, destination);
        }

        private static void Log(string msg)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} {msg}");
        }
    }
}
