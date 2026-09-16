namespace SDM.FacilityManagement.Tests.Floors
{
	using System;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.FacilityManagement.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.Extensions;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	[TestClass]
	public class FloorDomRepositoryTests : BaseRepositoryTest
	{
		[TestMethod]
		public void FloorDomRepository_ReadFilter_FloorsWithoutFacility()
		{
			var facility = Helper.Facilities.Create(new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = $"FAC-{Guid.NewGuid():N}",
				Name = "Floor Facility",
			});

			var floors = new[]
			{
				new Floor
				{
					Identifier = Guid.NewGuid().ToString(),
					FloorId = "FLR-001",
					Name = "Floor Without Facility",
				},
				new Floor
				{
					Identifier = Guid.NewGuid().ToString(),
					FloorId = "FLR-002",
					Name = "Floor With Facility",
					FacilityFk =
					{
						Facility = new SdmObjectReference<Facility>(facility.Identifier),
					},
				},
			};

			Helper.Floors.Create(floors);

			var filter = FloorExposers.FacilityFk.Facility.HasNoValue();
			var expected = floors.Where(f => (!f.FacilityFk?.Facility.HasValue()) ?? false).ToArray();

			var floorsRetrieved = Helper.Floors.Read(filter);

			using (new AssertionScope())
			{
				floorsRetrieved.Should().NotBeNull();
				floorsRetrieved.Should().NotBeEmpty();
				floorsRetrieved.Should().HaveCount(expected.Length);
				floorsRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FloorDomRepository_ReadFilter_NonExistentFacility()
		{
			var filter = FloorExposers.FacilityFk.Facility.Equal(new SdmObjectReference<Facility>(Guid.NewGuid().ToString()));

			var floorsRetrieved = Helper.Floors.Read(filter);

			using (new AssertionScope())
			{
				floorsRetrieved.Should().NotBeNull();
				floorsRetrieved.Should().BeEmpty();
				floorsRetrieved.Should().HaveCount(0);
			}
		}

		[TestMethod]
		public void FloorDomRepository_ReadFilter_FloorsWithFacility()
		{
			var firstFacility = Helper.Facilities.Create(new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = $"FAC-{Guid.NewGuid():N}",
				Name = "Floor Facility A",
			});
			var secondFacility = Helper.Facilities.Create(new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = $"FAC-{Guid.NewGuid():N}",
				Name = "Floor Facility B",
			});

			var floors = new[]
			{
				new Floor
				{
					Identifier = Guid.NewGuid().ToString(),
					FloorId = "FLR-003",
					Name = "Floor Without Facility B",
				},
				new Floor
				{
					Identifier = Guid.NewGuid().ToString(),
					FloorId = "FLR-004",
					Name = "Floor With Facility A",
					FacilityFk =
					{
						Facility = new SdmObjectReference<Facility>(firstFacility.Identifier),
					},
				},
				new Floor
				{
					Identifier = Guid.NewGuid().ToString(),
					FloorId = "FLR-005",
					Name = "Floor With Facility B",
					FacilityFk =
					{
						Facility = new SdmObjectReference<Facility>(secondFacility.Identifier),
					},
				},
			};

			Helper.Floors.Create(floors);

			var filter = FloorExposers.FacilityFk.Facility.HasValue();
			var expected = floors.Where(f => (f.FacilityFk?.Facility.HasValue()) ?? false).ToArray();

			var floorsRetrieved = Helper.Floors.Read(filter);

			using (new AssertionScope())
			{
				floorsRetrieved.Should().NotBeNull();
				floorsRetrieved.Should().NotBeEmpty();
				floorsRetrieved.Should().HaveCount(expected.Length);
				floorsRetrieved.Should().BeEquivalentTo(expected);
			}
		}
	}
}