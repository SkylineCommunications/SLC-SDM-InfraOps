namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Read-only access to ports across the DataPort and PowerPort DOM definitions.
    /// Ports are looked up by their identifier (DOM instance id) in a single query
    /// spanning both definitions, and the results are split per definition.
    /// </summary>
    public interface IPortRepository
    {
        ///// <summary>
        ///// Reads the ports with the given DOM instance ids from both the DataPort and PowerPort definitions.
        ///// </summary>
        ///// <param name="domInstanceIds">The DOM instance ids of the ports to read.</param>
        ///// <returns>The matching ports, split per definition. Ids that do not match any port are ignored.</returns>
        //PortReadResult Read(IEnumerable<Guid> domInstanceIds);

        ///// <summary>
        ///// Reads the ports with the given identifiers from both the DataPort and PowerPort definitions.
        ///// </summary>
        ///// <param name="identifiers">The identifiers (DOM instance ids) of the ports to read.</param>
        ///// <returns>The matching ports, split per definition. Identifiers that do not match any port are ignored.</returns>
        //PortReadResult Read(IEnumerable<string> identifiers);

        ///// <summary>
        ///// Reads all ports from both the DataPort and PowerPort definitions, one page at a time.
        ///// </summary>
        ///// <param name="pageSize">The maximum number of ports (across both definitions) per page.</param>
        ///// <returns>A lazy sequence of pages, each split per definition.</returns>
        //IEnumerable<PortReadResult> ReadPaged(int pageSize = 500);

        ///// <summary>
        ///// Reads the ports with the given DOM instance ids, one page at a time.
        ///// </summary>
        ///// <param name="domInstanceIds">The DOM instance ids of the ports to read.</param>
        ///// <param name="pageSize">The maximum number of ids resolved per page.</param>
        ///// <returns>A lazy sequence of pages, each split per definition. Ids that do not match any port are ignored.</returns>
        //IEnumerable<PortReadResult> ReadPaged(IEnumerable<Guid> domInstanceIds, int pageSize = 500);

        ///// <summary>
        ///// Reads the ports with the given identifiers, one page at a time.
        ///// </summary>
        ///// <param name="identifiers">The identifiers (DOM instance ids) of the ports to read.</param>
        ///// <param name="pageSize">The maximum number of identifiers resolved per page.</param>
        ///// <returns>A lazy sequence of pages, each split per definition. Identifiers that do not match any port are ignored.</returns>
        //IEnumerable<PortReadResult> ReadPaged(IEnumerable<string> identifiers, int pageSize = 500);
    }
}
