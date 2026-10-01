namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Mocked <see cref="IPeopleAndOrganizationsApi"/> whose Organizations repository is backed by a seeded
    /// in-memory list, so existence checks depend on which organizations were seeded.
    /// </summary>
    internal static class PeopleApiMock
    {
        internal static PnoObjectReference<Organization> NewManufacturer() => new PnoObjectReference<Organization>(Guid.NewGuid());

        /// <summary>Every organization "exists" (lenient default for tests using arbitrary manufacturer Guids).</summary>
        internal static IPeopleAndOrganizationsApi CreateDefault() => Create(exists: true);

        internal static IPeopleAndOrganizationsApi Create(bool exists)
        {
            var organizationsMock = new Mock<IOrganizationsRepository>();
            organizationsMock
                .Setup(r => r.Count(It.IsAny<FilterElement<Organization>>()))
                .Returns(exists ? 1 : 0);

            return Wrap(organizationsMock.Object);
        }

        /// <summary>Only the given organization ids exist.</summary>
        internal static IPeopleAndOrganizationsApi WithOrganizations(params Guid[] existingIds)
        {
            var ids = existingIds.ToList();

            var organizationsMock = new Mock<IOrganizationsRepository>();
            organizationsMock
                .Setup(r => r.Count(It.IsAny<FilterElement<Organization>>()))
                .Returns((FilterElement<Organization> filter) => (long)ids.Count(id => Describes(filter, id)));

            return Wrap(organizationsMock.Object);
        }

        // FilterElement has no public in-memory evaluation; the exposer-based Equal(id) filter renders the id in its text.
        private static bool Describes(FilterElement<Organization> filter, Guid id)
            => filter.ToString().IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;

        private static IPeopleAndOrganizationsApi Wrap(IOrganizationsRepository organizations)
        {
            var apiMock = new Mock<IPeopleAndOrganizationsApi>();
            apiMock.Setup(a => a.Organizations).Returns(organizations);
            return apiMock.Object;
        }
    }
}