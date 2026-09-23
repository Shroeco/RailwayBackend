namespace Railway.Domain.Entities;

public sealed class JourneyLeg
{
    public Guid Id { get; private set; }
    public Guid JourneyId { get; private set; }
    public Guid OriginStationId { get; private set; }
    public Guid DestinationStationId { get; private set; }
    public DateTimeOffset DepartureTime { get; private set; }
    public DateTimeOffset ArrivalTime { get; private set; }
    public Guid CarrierId { get; private set; }

    public JourneyLeg(Guid id, Guid journeyId, Guid originStationId, Guid destinationStationId, DateTimeOffset departureTime, DateTimeOffset arrivalTime, Guid carrierId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Journey leg ID cannot be empty", nameof(id));
        
        if (journeyId == Guid.Empty)
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));
        
        if (originStationId == Guid.Empty)
            throw new ArgumentException("Origin station ID cannot be empty", nameof(originStationId));
        
        if (destinationStationId == Guid.Empty)
            throw new ArgumentException("Destination station ID cannot be empty", nameof(destinationStationId));
        
        if (originStationId == destinationStationId)
            throw new ArgumentException("Origin and destination stations must be different.");
        
        if (arrivalTime <= departureTime)
            throw new ArgumentException("Arrival time must be later than departure time.");
        
        if (carrierId == Guid.Empty)
            throw new ArgumentException("Carrier ID cannot be empty", nameof(carrierId));

        Id = id;
        JourneyId = journeyId;
        OriginStationId = originStationId;
        DestinationStationId = destinationStationId;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        CarrierId = carrierId;
    }
}