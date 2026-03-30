using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class ElevatorService : IElevatorService
    {
        private readonly List<Elevator> _elevators;
        private readonly Dictionary<int, int> _elevatorsStopTimer;
        private readonly Dictionary<int, int> _elevatorsMoveTimer;
        private const int MoveDuration = 10;
        private const int StopDuration = 10;
        private const double SameDirFactor = 0.8;
        private const double WrongDirFactor = 3.0;

        public ElevatorService(int elevatorCount)
        {
            _elevators = [.. Enumerable.Range(1, elevatorCount).Select(i => new Elevator(i))];
            _elevatorsStopTimer = _elevators.ToDictionary(c => c.Id, _ => 0);
            _elevatorsMoveTimer = _elevators.ToDictionary(c => c.Id, _ => 0);
        }

        public void HandlePassengerRequest(Passenger passenger, int noFloors, Action<string> log)
        {
            var elevator = GetElevator(passenger);

            log($"Elevator {elevator.Id} assigned " +
                $"({passenger.Origin} to {passenger.Destination})");

            elevator.Stops.Add(passenger.Origin);
            elevator.PassengersWaitingToBePicked.Add(passenger);

            if (elevator.CurrentFloor == passenger.Origin)
                HandleArrivals(elevator, noFloors, log);
        }

        public void Step(int noFloors, Action<string> log)
        {
            foreach (var elevator in _elevators)
                SetStep(elevator, noFloors, log);
        }

        private Elevator GetElevator(Passenger passenger)
        {
            Elevator bestElevator = new();
            double bestScore = double.MaxValue;

            foreach (var elevator in _elevators)
            {
                double score = Score(passenger, elevator);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestElevator = elevator;
                }
            }

            //if (bestElevator is not null)
            //    CommitAssignment(call, bestCar);

            return bestElevator;
            //return _elevators.FirstOrDefault();
            //    .Where()
            //.OrderBy(e => e.Score(passenger))
            //.First();
        }

        private void SetStep(Elevator elevator, int noFloors, Action<string> log)
        {
            if (_elevatorsStopTimer[elevator.Id] > 0)
            {
                _elevatorsStopTimer[elevator.Id]--;
                return;
            }

            if (elevator.Passengers.Count == 0 && elevator.Stops.Count == 0)
            {
                elevator.Direction = Direction.Idle;
                return;
            }

            EnsureDirection(elevator);

            _elevatorsMoveTimer[elevator.Id]++;
            if (_elevatorsMoveTimer[elevator.Id] < MoveDuration)
                return;

            _elevatorsMoveTimer[elevator.Id] = 0;
            MoveOneFloor(elevator, log);

            HandleArrivals(elevator, noFloors, log);
            UpdateDirection(elevator);
        }

        private void MoveOneFloor(Elevator elevator, Action<string> log)
        {
            elevator.CurrentFloor += elevator.Direction == Direction.Up ? 1 : -1;
            log($"[Elevator {elevator.Id}] moved to floor {elevator.CurrentFloor}");
        }

        private void HandleArrivals(Elevator elevator, int noFloors, Action<string> log)
        {
            bool stopped = false;
            var isLastFloor = noFloors == elevator.CurrentFloor;

            // Drop-offs
            var exiting = elevator.Passengers
                .Where(p => p.Destination == elevator.CurrentFloor)
                .ToList();

            foreach (var passenger in exiting)
            {
                elevator.Passengers.Remove(passenger);
                stopped = true;
                log($"[Elevator {elevator.Id}] passenger exited at {elevator.CurrentFloor}");
            }

            // Pickups
            if (elevator.Stops.Remove(elevator.CurrentFloor))
            {
                stopped = true;

                var passengers = elevator.PassengersWaitingToBePicked
                                .Where(x => x.Origin == elevator.CurrentFloor 
                                    && (isLastFloor || x.Direction == elevator.Direction))
                                .ToList();
                foreach (var passenger in passengers)
                {
                    elevator.Passengers.Add(passenger);
                    elevator.Stops.Add(passenger.Destination);
                    log($"[Elevator {elevator.Id}] picked passenger from {passenger.Origin} to destination {passenger.Destination}");
                    elevator.PassengersWaitingToBePicked.Remove(passenger);
                }
            }

            if (stopped)
                _elevatorsStopTimer[elevator.Id] = StopDuration;
        }

        private void EnsureDirection(Elevator elevator)
        {
            if (elevator.Direction != Direction.Idle) return;

            int next = elevator.Stops.First();
            elevator.Direction = next > elevator.CurrentFloor ? Direction.Up : Direction.Down;
        }

        private void UpdateDirection(Elevator elevator)
        {
            var allTargets = elevator.Stops
                .Concat(elevator.Passengers.Select(p => p.Destination))
                .ToList();

            if (allTargets.Count == 0)
            {
                elevator.Direction = Direction.Idle;
                return;
            }

            if (elevator.Direction == Direction.Up && allTargets.All(t => t < elevator.CurrentFloor))
                elevator.Direction = Direction.Down;

            if (elevator.Direction == Direction.Down && allTargets.All(t => t > elevator.CurrentFloor))
                elevator.Direction = Direction.Up;
        }

        private static double Score(Passenger passenger, Elevator elevator)
        {
            double distance = Math.Abs(elevator.CurrentFloor - passenger.Origin);

            if (elevator.Direction == Direction.Idle)
                return distance;

            bool isTravellingToward =
                (elevator.Direction == Direction.Up && passenger.Origin >= elevator.CurrentFloor) ||
                (elevator.Direction == Direction.Down && passenger.Origin <= elevator.CurrentFloor);

            return isTravellingToward
                ? distance * SameDirFactor
                : distance * WrongDirFactor;
        }
    }
}
