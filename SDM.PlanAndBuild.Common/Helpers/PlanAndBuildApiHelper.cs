namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.PlanAndBuild.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;

    public class PlanAndBuildApiHelper : IPlanAndBuildApiHelper
    {
        public PlanAndBuildApiHelper(IConnection connection)
        {
            Connection = connection;

            // Raw repositories - used internally by validators to query other entities (e.g. uniqueness/in-use checks).
            var jobRepository = new PlanAndBuildJobDomRepository(connection);
            var jobTypeRepository = new JobTypeDomRepository(connection);
            var appSettingsRepository = new PlanAndBuildAppSettingsDomRepository(connection);

            var jobValidator = new PlanAndBuildJobValidator(this);
            var jobTypeValidator = new JobTypeValidator(this);
            var appSettingsValidator = new PlanAndBuildAppSettingsValidator();

            Jobs = jobRepository
                .WithMiddleware(new PlanAndBuildJobValidationMiddleware(jobValidator))
                .WithMiddleware(new IdentifierMiddleware<PlanAndBuildJob>());

            JobTypes = jobTypeRepository
                .WithMiddleware(new JobTypeValidationMiddleware(jobTypeValidator))
                .WithMiddleware(new IdentifierMiddleware<JobType>());

            AppSettings = appSettingsRepository;

            JobValidator = jobValidator;
            JobTypeValidator = jobTypeValidator;
            AppSettingsValidator = appSettingsValidator;
        }

        public IConnection Connection { get; }

        public IPlanAndBuildJobRepository Jobs { get; }

        public IBulkRepository<JobType> JobTypes { get; }

        public IBulkRepository<PlanAndBuildAppSettings> AppSettings { get; }

        public PlanAndBuildJobValidator JobValidator { get; }

        public JobTypeValidator JobTypeValidator { get; }

        public PlanAndBuildAppSettingsValidator AppSettingsValidator { get; }
    }
}
