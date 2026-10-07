namespace Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences
{
    public interface IObjectReference<in T>
    {
        string Identifier { get; }

        bool Equals(T obj);
    }
}