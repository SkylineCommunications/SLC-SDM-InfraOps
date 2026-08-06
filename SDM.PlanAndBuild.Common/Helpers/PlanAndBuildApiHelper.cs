namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.PlanAndBuild.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    public class PlanAndBuildApiHelper : IPlanAndBuildApiHelper
    {
        public PlanAndBuildApiHelper(IConnection connection)
            : this(connection, connection.GetPeopleAndOrganizationsApi())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildApiHelper"/> class with an explicit
        /// <see cref="IPeopleAndOrganizationsApi"/> instance. Mainly intended for unit tests, where a mocked
        /// People &amp; Organizations API is supplied instead of the real one resolved from <paramref name="connection"/>.
        /// </summary>
        public PlanAndBuildApiHelper(
            IConnection connection,
            IPeopleAndOrganizationsApi peopleApi,
            IPlanAndBuildExternalReferenceChecker externalReferenceChecker = null)
        {
            Connection = connection;
            PandOApiHelper = peopleApi;

            // Raw repositories - used internally by validators to query other entities (e.g. uniqueness/in-use checks).
            var jobRepository = new PlanAndBuildJobDomRepository(connection);
            var jobTypeRepository = new JobTypeDomRepository(connection);
            var appSettingsRepository = new PlanAndBuildAppSettingsDomRepository(connection);

            var jobValidator = new PlanAndBuildJobValidator(this, PandOApiHelper, externalReferenceChecker);
            var jobTypeValidator = new JobTypeValidator(this);
            var appSettingsValidator = new PlanAndBuildAppSettingsValidator();

            // Wired so UpdateAndTransitionTo/TransitionAndUpdate (which call this repository's own internal
            // Update() directly, bypassing PlanAndBuildJobValidationMiddleware) still enforce business-rule
            // validation on the field updates they persist.
            jobRepository.Validator = jobValidator;

            Jobs = jobRepository
                .WithMiddleware(new PlanAndBuildJobValidationMiddleware(jobValidator))
                .WithMiddleware(new JobIdAllocationMiddleware(this))
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

        internal IPeopleAndOrganizationsApi PandOApiHelper { get; }

        public IPlanAndBuildJobRepository Jobs { get; }

        public IJobTypeRepository JobTypes { get; }

        public IBulkRepository<PlanAndBuildAppSettings> AppSettings { get; }

        public PlanAndBuildJobValidator JobValidator { get; }

        public JobTypeValidator JobTypeValidator { get; }

        public PlanAndBuildAppSettingsValidator AppSettingsValidator { get; }
    }
}
