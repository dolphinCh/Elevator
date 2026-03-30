using Domain.Enums;

namespace Domain.Entities
{
    public class Elevator
    {
        public Elevator(){ }
        public Elevator(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public int CurrentFloor { get; set; } = 1;
        public Direction Direction { get; set; } = Direction.Idle;

        public List<Passenger> Passengers = [];
        public List<Passenger> PassengersWaitingToBePicked = [];
        public SortedSet<int> Stops = [];
    }
}
