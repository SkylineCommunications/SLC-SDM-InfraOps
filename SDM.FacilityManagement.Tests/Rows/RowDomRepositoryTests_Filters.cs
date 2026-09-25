namespace SDM.FacilityManagement.Tests.Rows
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Filter tests for <see cref="RowDomRepository"/>.
    /// These tests verify that every <see cref="RowExposers"/> field used in a filter is
    /// correctly routed through <c>RowDomRepository.CreateFilter</c>.
    /// The latent production bug: all <c>RowProperties.*</c> switch cases were written
    /// prefixed (<c>"RowProperties.Name"</c>, <c>"RowProperties.Label"</c>, …) but the
    /// <see cref="RowExposers.RowProperties"/> exposers supply the bare field name
    /// (<c>"Name"</c>, <c>"Label"</c>, …), so every <c>RowProperties</c> filter fell through
    /// to <c>default: throw new NotImplementedException()</c>.
    /// </summary>
    [TestClass]
    public partial class RowDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // RowProperties.Name  ← primary regression test for the prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact row name returns exactly one row whose properties
        /// round-trip correctly. Primary regression test for the <c>"Name"</c> switch case.
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_Name_Equals()
        {
            Helper.PopulateRows();

            string rowName = "Row C";
            var nameFilter = RowExposers.RowProperties.Name.Equal(rowName);
            var expected = DemoData.Rows.Single(r => r.Name == rowName);

            var rowsRetrieved = Helper.Rows.Read(nameFilter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(1);

                var row = rowsRetrieved.First();
                row.Name.Should().Be(expected.Name);
                row.Plan.Should().Be(expected.Plan);
                row.Description.Should().Be(expected.Description);
                row.Label.Should().Be(expected.Label);
                row.RowId.Should().Be(expected.RowId);
            }
        }

        // ---------------------------------------------------------------------------
        // RowProperties.Description
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a substring of the description returns all matching rows.
        /// Exercises the <c>"Description"</c> switch case (previously <c>"RowProperties.Description"</c>).
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_Description_Contains()
        {
            Helper.PopulateRows();

            // "First equipment row", "Second equipment row", "Third equipment row".
            var descriptionFilter = RowExposers.RowProperties.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Rows
                .Where(r => r.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var rowsRetrieved = Helper.Rows.Read(descriptionFilter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(expected.Length);
                rowsRetrieved.Select(r => r.RowId).Should().BeEquivalentTo(expected.Select(r => r.RowId));
            }
        }

        // ---------------------------------------------------------------------------
        // RowProperties.Label  ← regression for the "RowProperties.Label" prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by label returns exactly the row with that label.
        /// Direct regression test for the <c>"Label"</c> switch case, previously
        /// <c>"RowProperties.Label"</c> which never matched the exposer's bare <c>"Label"</c>.
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_Label_Equal()
        {
            Helper.PopulateRows();

            string label = "D";
            var labelFilter = RowExposers.RowProperties.Label.Equal(label);
            var expected = DemoData.Rows.Single(r => r.Label == label);

            var rowsRetrieved = Helper.Rows.Read(labelFilter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(1);

                var row = rowsRetrieved.First();
                row.Label.Should().Be(expected.Label);
                row.Name.Should().Be(expected.Name);
                row.RowId.Should().Be(expected.RowId);
            }
        }

        // ---------------------------------------------------------------------------
        // RowProperties.YPosition  (double)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a minimum Y position (≥ threshold) returns all rows at or above it.
        /// Exercises the <c>"YPosition"</c> switch case (double cast).
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_YPosition_GreaterThanOrEqual()
        {
            Helper.PopulateRows();

            double yThreshold = 8.0;
            var yPositionFilter = RowExposers.RowProperties.YPosition.GreaterThanOrEqual(yThreshold);
            // "Row E" (8.0) and "Row F" (10.0).
            var expected = DemoData.Rows.Where(r => r.YPosition >= yThreshold).ToArray();

            var rowsRetrieved = Helper.Rows.Read(yPositionFilter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(expected.Length);
                rowsRetrieved.Select(r => r.RowId).Should().BeEquivalentTo(expected.Select(r => r.RowId));
            }
        }

        // ---------------------------------------------------------------------------
        // RowProperties.RowId  (exposer + switch both prefixed → always worked; baseline)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the business <c>RowId</c> field returns exactly one row.
        /// The <c>RowId</c> exposer and switch case are both prefixed
        /// (<c>"RowProperties.RowId"</c>), so this path was unaffected by the fix.
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_RowId_Equal()
        {
            Helper.PopulateRows();

            string rowId = "RW-005";
            var rowIdFilter = RowExposers.RowProperties.RowId.Equal(rowId);
            var expected = DemoData.Rows.Single(r => r.RowId == rowId);

            var rowsRetrieved = Helper.Rows.Read(rowIdFilter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(1);
                rowsRetrieved.First().Name.Should().Be(expected.Name);
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under RowProperties — always worked)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one row that owns
        /// that identifier. Baseline test confirming the fix did not regress it.
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateRows();

            var rowIdentifier = DemoData.Rows[3].Identifier;
            var filter = RowExposers.Identifier.Equal(rowIdentifier);
            var expected = DemoData.Rows.Single(filter.getLambda());

            var rowsRetrieved = Helper.Rows.Read(filter);

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(1);

                var row = rowsRetrieved.First();
                row.Identifier.Should().Be(expected.Identifier);
                row.Name.Should().Be(expected.Name);
                row.RowId.Should().Be(expected.RowId);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: Plan AND YPosition
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a Plan filter with a YPosition lower bound using AND returns only
        /// rows that satisfy both conditions simultaneously.
        /// </summary>
        [TestMethod]
        public void RowDomRepository_ReadFilter_PlanAndYPosition_Combined()
        {
            Helper.PopulateRows();

            // Plan "R-2": "Row C" (Y=4) and "Row D" (Y=6). YPosition ≥ 6 keeps only "Row D".
            var combinedFilter = RowExposers.RowProperties.Plan.Equal("R-2")
                .AND(RowExposers.RowProperties.YPosition.GreaterThanOrEqual(6.0));

            var rowsRetrieved = Helper.Rows.Read(combinedFilter);
            var expected = DemoData.Rows
                .Where(r => r.Plan == "R-2" && r.YPosition >= 6.0)
                .ToArray();

            using (new AssertionScope())
            {
                rowsRetrieved.Should().NotBeNull();
                rowsRetrieved.Should().HaveCount(expected.Length);
                rowsRetrieved.Select(r => r.RowId).Should().BeEquivalentTo(expected.Select(r => r.RowId));
            }
        }
    }
}
