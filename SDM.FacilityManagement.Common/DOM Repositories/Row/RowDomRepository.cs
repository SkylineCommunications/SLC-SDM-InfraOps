namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IRowRepository : IBulkRepository<Row>
    {
        Row TransitionTo(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState);

        Row UpdateAndTransitionTo(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState);

        Row TransitionAndUpdate(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState);
    }

    internal partial class RowDomRepository : IRowRepository
    {
        public Row TransitionTo(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            if (!RowStateMachine.IsTransitionAllowed(row.State, newState))
            {
                throw new InvalidOperationException($"State transition from {row.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(row, newState);
        }

        public Row UpdateAndTransitionTo(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            if (!RowStateMachine.IsTransitionAllowed(row.State, newState))
            {
                throw new InvalidOperationException($"State transition from {row.State} to {newState} is not allowed.");
            }

            var updated = Update(row);

            return ExecuteStateTransition(updated, newState);
        }

        public Row TransitionAndUpdate(Row row, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum newState)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            if (!RowStateMachine.IsTransitionAllowed(row.State, newState))
            {
                throw new InvalidOperationException($"State transition from {row.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(row, newState);

            return Update(transitioned);
        }

        private Row ExecuteStateTransition(
            Row row,
            SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum toState)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            try
            {
                var transitions = RowStateMachine.GetTransitionPath(row.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {row.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(row.Identifier))
                {
                    ModuleId = RowDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Row_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for row '{row.Identifier}' to {toState}.");
                }

                row.State = SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return row;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition row '{row.Identifier}' from {row.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
