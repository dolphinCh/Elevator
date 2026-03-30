using Domain.Enums;

namespace Domain.Entities
{
    public class Passenger
    {
        public Passenger(int origin, int destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public int Origin { get; }
        public int Destination { get; }

        public Direction Direction => Destination > Origin ? Direction.Up : Direction.Down;
    }
}
