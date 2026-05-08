/// <summary>
/// Loads a PortType by its identifier.
/// </summary>
/// <param name="portTypeRef">The PortType reference.</param>
/// <returns>The PortType instance, or null if not found.</returns>
public PortType LoadPortType(SdmObjectReference<PortType> portTypeRef)
{
    if (portTypeRef == null || !portTypeRef.HasValue())
    {
        return null;
    }

    return LoadPortType(portTypeRef.Identifier);
}

/// <summary>
/// Loads a PortType by its identifier string.
/// </summary>
/// <param name="portTypeId">The PortType identifier.</param>
/// <returns>The PortType instance, or null if not found.</returns>
public PortType LoadPortType(string portTypeId)
{
    if (string.IsNullOrWhiteSpace(portTypeId))
    {
        return null;
    }

    try
    {
        var filter = DomInstanceExposers.Id.Equal(new DomInstanceId(Guid.Parse(portTypeId)));
        var portTypeInstances = _domHelper.DomInstances.Read(filter);
        var portTypeInstance = portTypeInstances.FirstOrDefault();

        if (portTypeInstance == null)
        {
            return null;
        }

        return _mapper.MapToPortType(portTypeInstance);
    }
    catch (Exception ex)
    {
        // Log or handle exception as needed
        throw new InvalidOperationException($"Error loading PortType '{portTypeId}'.", ex);
    }
}