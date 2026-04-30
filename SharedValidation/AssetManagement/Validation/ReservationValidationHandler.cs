namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;

	public static class ReservationValidationHandler
	{
		public enum ReservationValidationField
		{
		}

		public static ValidationResult ValidateReservation(ReservationWrapper reservation, ValidatorContext<ReservationWrapper> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
			};

			ValidationResult result = new ValidationResult();
			foreach (var validation in validations)
			{
				result.CombineResults(validation());

				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
		}

		// TODO: Add validation methods here for positions already deserverd or in use.
	}
}