namespace Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences
{
    public interface IObjectReference<out T>
    {
        string Identifier { get; }
    }
}