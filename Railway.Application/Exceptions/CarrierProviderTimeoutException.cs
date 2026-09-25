namespace Railway.Application.Exceptions;

public sealed class CarrierProviderTimeoutException : Exception
{
    public CarrierProviderTimeoutException(string carrierCode) : base($"The carrier provider '{carrierCode}' did not respond within the allowed time.")
    {
        CarrierCode = carrierCode;
    }

    public string CarrierCode { get; }
}