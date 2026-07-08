namespace SDM.PlanAndBuild.Tests
{
	using SDM.PlanAndBuild.Tests.Setup;
	using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	public static class RepositoryInitialize
	{
		public static IPlanAndBuildApiHelper InitializeEmptyRepositories()
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper();
		}

		/// <summary>
		/// Populates the Jobs repository with the provided <paramref name="jobs"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="jobs">Predefined collection of <see cref="PlanAndBuildJob"/> objects to create.</param>
		/// <returns><see cref="IPlanAndBuildApiHelper"/> API helper interface with populated data.</returns>
		public static IPlanAndBuildApiHelper PopulateJobs(this IPlanAndBuildApiHelper helper, IEnumerable<PlanAndBuildJob> jobs)
		{
			if (jobs is null || !jobs.Any())
			{
				return helper.PopulateJobs();
			}

			helper.Jobs.Create(jobs);

			return helper;
		}

		/// <summary>
		/// Populates the Jobs repository with default <see cref="PlanAndBuildJob"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IPlanAndBuildApiHelper"/> API helper interface with populated data.</returns>
		public static IPlanAndBuildApiHelper PopulateJobs(this IPlanAndBuildApiHelper helper)
		{
			helper.Jobs.Create(DemoData.Jobs);

			return helper;
		}

		/// <summary>
		/// Populates the JobTypes repository with the provided <paramref name="jobTypes"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="jobTypes">Predefined collection of <see cref="JobType"/> objects to create.</param>
		/// <returns><see cref="IPlanAndBuildApiHelper"/> API helper interface with populated data.</returns>
		public static IPlanAndBuildApiHelper PopulateJobTypes(this IPlanAndBuildApiHelper helper, IEnumerable<JobType> jobTypes)
		{
			if (jobTypes is null || !jobTypes.Any())
			{
				return helper.PopulateJobTypes();
			}

			helper.JobTypes.Create(jobTypes);

			return helper;
		}

		/// <summary>
		/// Populates the JobTypes repository with default <see cref="JobType"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IPlanAndBuildApiHelper"/> API helper interface with populated data.</returns>
		public static IPlanAndBuildApiHelper PopulateJobTypes(this IPlanAndBuildApiHelper helper)
		{
			helper.JobTypes.Create(DemoData.JobTypes);

			return helper;
		}

		/// <summary>
		/// Populates the AppSettings repository with default <see cref="PlanAndBuildAppSettings"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IPlanAndBuildApiHelper"/> API helper interface with populated data.</returns>
		public static IPlanAndBuildApiHelper PopulateAppSettings(this IPlanAndBuildApiHelper helper)
		{
			helper.AppSettings.Create(DemoData.AppSettingsList);

			return helper;
		}
	}
}
