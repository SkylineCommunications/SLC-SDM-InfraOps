namespace Skyline.DataMiner.SDM.AssetManagement
{
    public static class SlcAssetManagement
    {
        public static class Enums
        {
            public enum HierarchyRole
            {
                Chassis,
                Card,
                SubCard,
                Module,
                Fan,
                PowerSupply,
                None,
            }

            public enum ConnectionType
            {
                Data = 0,
                Power = 1,
            }

            public enum TypeOfHistory
            {
                Add,
                Modification,
                Removal,
            }

            public enum Categories
            {
                Networking,
                Power,
                Audio,
                Storage,
                Peripheral,
                Video,
                Misc,
                Data,
                Broadcast,
            }

            public enum PowerConnectionExposure
            {
                Front,
                Back,
            }

            public enum Outputtype
            {
                Out = 0,
                In = 1,
                IO = 2,
            }

            public enum Jobactiononchange
            {
                DONOTCREATE,
                ASKUSER,
            }

            public enum TagOption
            {
                PowerProvider = 0,
                AcceptsDataConnection = 1,
                RackUnitConsumer = 2,
            }

            public enum PowerSupply
            {
                AC,
                DC,
            }

            public enum PortExposure
            {
                Back,
                Front,
            }

            public enum Operational
            {
                Faulty = 0,
            }

            public enum Side
            {
                Front,
                Back,
                Right,
                Left,
            }
        }
    }
}