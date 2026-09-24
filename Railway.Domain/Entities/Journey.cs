namespace Railway.Domain.Entities;

public sealed class Journey
{
    public Guid Id { get; private set; }
    public Guid OriginStationId { get; private set; }
    public Guid DestinationStationId { get; private set; }
    public DateTimeOffset DepartureTime { get; private set; }
    public DateTimeOffset ArrivalTime { get; private set; }
    public int Capacity { get; private set; }
    public int AvailableSeats { get; private set; }

    public ICollection<JourneyLeg> JourneyLegs { get; private set; } = new List<JourneyLeg>();
    public ICollection<Fare> Fares { get; private set; } = new List<Fare>();

    public Journey(Guid id, Guid originStationId, Guid destinationStationId, DateTimeOffset departureTime, DateTimeOffset arrivalTime, int capacity)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Journey ID cannot be empty", nameof(id));
        
        if (originStationId == Guid.Empty)
            throw new ArgumentException("Origin station ID cannot be empty", nameof(originStationId));
    
        if (destinationStationId == Guid.Empty)
            throw new ArgumentException("Destination station ID cannot be empty", nameof(destinationStationId));
        
        if (originStationId == destinationStationId)
            throw new ArgumentException("Origin and destination stations must be different");

        if (arrivalTime <= departureTime)
            throw new ArgumentException("Arrival time must be later than departure time");
        
        if (capacity < 1)
            throw new ArgumentException("Journey capacity must be at least 1", nameof(capacity));

        Id = id;
        OriginStationId = originStationId;
        DestinationStationId = destinationStationId;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Capacity = capacity;
        AvailableSeats = capacity;
    }

    public void ReserveSeat()
    {
        if (AvailableSeats <= 0)
        throw new InvalidOperationException("No seats are available");

        AvailableSeats--;
    }
}