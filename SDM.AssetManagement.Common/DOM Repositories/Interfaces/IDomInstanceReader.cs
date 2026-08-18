namespace Skyline.DataMiner.SDM.AssetManagement.Models.Interafaces
{
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

    internal interface IDomInstanceReader<out T> where T : class
    {
        /// <summary>
        /// Maps a DOM instance of the definition to its model.
        /// </summary>
        /// <param name="instance">The DOM instance to map.</param>
        /// <returns>The mapped instance.</returns>
        T FromDomInstance(DomInstance instance);
    }
}