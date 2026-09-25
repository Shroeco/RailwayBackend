using Railway.Domain.Entities;

namespace Railway.Application.Exceptions;

public sealed class CarrierProviderException : Exception
{
    public CarrierProviderException(string carrierCode) : base($"The carrier provider '{carrierCode}' is currently unavailable")
    {
        CarrierCode = carrierCode;
    }

    public string CarrierCode { get; }
}