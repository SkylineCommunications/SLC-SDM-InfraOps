namespace SDM.AssetManagement.Tests.InfraopsReservations
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    [TestClass]
    public class InfraopsReservationDomStorageProvider_FilterTests : BaseRepositoryTest
    {
        [TestMethod]
        public void ReadFilter_Rack_HasNoValue_ShouldReturnReservationsWithoutRack()
        {
            var rack = CreateRack("Reservation Rack A", "RCK-RES-001");

            var reservations = new[]
            {
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation without rack",
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with rack",
                    RackFk =
                    {
                        Rack = new SdmObjectReference<Rack>(rack.Identifier),
                    },
                },
            };

            Helper.AssetManagement.Reservations.Create(reservations);

            var filter = InfraopsReservationExposers.RackFk.Rack.HasNoValue();
            var expected = reservations.Where(r => !r.RackFk.Rack.HasValue()).ToArray();

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void ReadFilter_Rack_NonExistent_ShouldReturnNoReservations()
        {
            var filter = InfraopsReservationExposers.RackFk.Rack.Equal(new SdmObjectReference<Rack>(Guid.NewGuid().ToString()));

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().BeEmpty();
                results.Should().HaveCount(0);
            }
        }

        [TestMethod]
        public void ReadFilter_Rack_HasValue_ShouldReturnReservationsWithRack()
        {
            var firstRack = CreateRack("Reservation Rack B1", "RCK-RES-002");
            var secondRack = CreateRack("Reservation Rack B2", "RCK-RES-003");

            var reservations = new[]
            {
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation without rack B",
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with first rack",
                    RackFk =
                    {
                        Rack = new SdmObjectReference<Rack>(firstRack.Identifier),
                    },
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with second rack",
                    RackFk =
                    {
                        Rack = new SdmObjectReference<Rack>(secondRack.Identifier),
                    },
                },
            };

            Helper.AssetManagement.Reservations.Create(reservations);

            var filter = InfraopsReservationExposers.RackFk.Rack.HasValue();
            var expected = reservations.Where(r => r.RackFk.Rack.HasValue()).ToArray();

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void ReadFilter_Job_HasNoValue_ShouldReturnReservationsWithoutJob()
        {
            var firstJob = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());

            var reservations = new[]
            {
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation without job",
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with job",
                    JobFk =
                    {
                        Job = firstJob,
                    },
                },
            };

            Helper.AssetManagement.Reservations.Create(reservations);

            var filter = InfraopsReservationExposers.JobFk.Job.HasNoValue();
            var expected = reservations.Where(r => !r.JobFk.Job.HasValue()).ToArray();

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void ReadFilter_Job_NonExistent_ShouldReturnNoReservations()
        {
            var filter = InfraopsReservationExposers.JobFk.Job.Equal(new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString()));

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().BeEmpty();
                results.Should().HaveCount(0);
            }
        }

        [TestMethod]
        public void ReadFilter_Job_HasValue_ShouldReturnReservationsWithJob()
        {
            var firstJob = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());
            var secondJob = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());

            var reservations = new[]
            {
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation without job B",
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with first job",
                    JobFk =
                    {
                        Job = firstJob,
                    },
                },
                new InfraopsReservation
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Description = "Reservation with second job",
                    JobFk =
                    {
                        Job = secondJob,
                    },
                },
            };

            Helper.AssetManagement.Reservations.Create(reservations);

            var filter = InfraopsReservationExposers.JobFk.Job.HasValue();
            var expected = reservations.Where(r => r.JobFk.Job.HasValue()).ToArray();

            var results = Helper.AssetManagement.Reservations.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        private Rack CreateRack(string name, string rackId)
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                RackId = rackId,
                Name = name,
            };
            rack.Capacity.MaximumRackCapacity = 42;

            return Helper.FacilityManagement.Racks.Create(rack);
        }
    }
}