namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using System;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    /// <summary>
    /// Allocates sequential, system-generated Job IDs backed by the singleton <see cref="PlanAndBuildAppSettings"/>
    /// instance. Mirrors the legacy InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.JobIdManager:
    /// composes "{JobIDPrefix}{JobIDNextSequence:D{JobIDMinimumDigits}}" and atomically advances the stored
    /// next-sequence counter by JobIDIncrement.
    /// </summary>
    internal static class JobIdAllocator
    {
        private static readonly object Lock = new object();

        /// <summary>
        /// Allocates the next Job ID using the singleton <see cref="PlanAndBuildAppSettings"/> instance and
        /// persists the advanced sequence.
        /// </summary>
        /// <param name="helper">The Plan &amp; Build API helper used to read/update the AppSettings singleton.</param>
        /// <exception cref="InvalidOperationException">
        /// No <see cref="PlanAndBuildAppSettings"/> instance exists yet (mirrors legacy's AppSettingsNotFoundException),
        /// or more than one exists (an invalid/corrupt state, since AppSettings is meant to be a singleton).
        /// </exception>
        internal static string AllocateNextJobId(IPlanAndBuildApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            lock (Lock)
            {
                PlanAndBuildAppSettings appSettings;
                try
                {
                    appSettings = helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>()).SingleOrDefault();
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException(
                        "Cannot allocate a Job ID: more than one PlanAndBuildAppSettings instance exists. " +
                        "Exactly one AppSettings instance is expected.");
                }

                if (appSettings == null)
                {
                    throw new InvalidOperationException(
                        "Cannot allocate a Job ID: no PlanAndBuildAppSettings instance was found. " +
                        "Create the Plan & Build AppSettings singleton before creating Jobs.");
                }

                string jobId = ComposeJobId(appSettings);

                appSettings.JobIDNextSequence += appSettings.JobIDIncrement;
                helper.AppSettings.Update(appSettings);

                return jobId;
            }
        }

        private static string ComposeJobId(PlanAndBuildAppSettings appSettings)
        {
            string sequence = appSettings.JobIDNextSequence.ToString($"D{appSettings.JobIDMinimumDigits}");
            return $"{appSettings.JobIDPrefix}{sequence}";
        }
    }
}
