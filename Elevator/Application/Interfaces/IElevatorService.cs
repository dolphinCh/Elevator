using Domain.Entities;

namespace Application.Interfaces
{
    public interface IElevatorService
    {
        public void HandlePassengerRequest(Passenger passenger, int noFloors, Action<string> log);
        public void Step(int noFloors, Action<string> log);
    }
}
