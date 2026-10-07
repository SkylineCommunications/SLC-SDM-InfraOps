namespace SharedCommonLibrary.AssetManagement.State_Management
{
    using System;
    using System.Collections.Generic;
    using SharedMappers.DomIds;
    using AssetClassStatuses = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum;
    using AssetClassTransitions = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Class_Behavior.TransitionsEnum;
    using AssetStatuses = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum;
    using AssetTransitions = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum;

    internal static class StateMachine
    {

        private static readonly IDictionary<(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum startStatus, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum endStatus), List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>> AssetStatusToStatusTransitions = new Dictionary<(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum startStatus, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum endStatus), List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>>
        {
            #region NotAvailable To

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,

            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Inrepair },

            #endregion

            #region Available To

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Notavailable },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,

            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,

            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inplanning },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Inrepair },

            #endregion

            #region BuildPlanReady To

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,

            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Inplanning,
            },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Inrepair },

            #endregion

            #region Installed To

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,

            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Buildplanready,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Inplanning,
            },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inrepair },

            #endregion

            #region InService To

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Available,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Buildplanready },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Installed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Inplanning },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inservice_To_Inrepair },

            #endregion

            #region Disposed To

            // Dispose cannot transition.

            #endregion

            #region InPlanning To
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Notavailable,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Available },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Available,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Available_To_Notavailable,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Notavailable_To_Disposed,
            },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Buildplanready,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Buildplanready_To_Installed,
            },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Intransit },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inplanning_To_Inrepair },

            #endregion

            #region InTransit To
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Notavailable },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Available },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Buildplanready },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Installed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Disposed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Inplanning },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>
            {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Intransit_To_Inrepair },

            #endregion

            #region InRepair To
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Notavailable },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Available },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Buildplanready },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Installed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Disposed },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Inplanning },
            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> {
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Installed,
                SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Installed_To_Inservice,
            },

            [(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)] = new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> { SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum.Inrepair_To_Intransit },

            #endregion
        };

        private static readonly IReadOnlyDictionary<AssetTransitions, AssetStatuses> TransitionDestinationStates =
            new Dictionary<AssetTransitions, AssetStatuses>
            {
                [AssetTransitions.Notavailable_To_Available] = AssetStatuses.Available,
                [AssetTransitions.Notavailable_To_Disposed] = AssetStatuses.Disposed,
                [AssetTransitions.Available_To_Notavailable] = AssetStatuses.NotAvailable,
                [AssetTransitions.Buildplanready_To_Installed] = AssetStatuses.Installed,
                [AssetTransitions.Installed_To_Inservice] = AssetStatuses.InService,
                [AssetTransitions.Inservice_To_Notavailable] = AssetStatuses.NotAvailable,
                [AssetTransitions.Inservice_To_Buildplanready] = AssetStatuses.BuildPlanReady,
                [AssetTransitions.Inservice_To_Available] = AssetStatuses.Available,
                [AssetTransitions.Inservice_To_Installed] = AssetStatuses.Installed,
                [AssetTransitions.Inplanning_To_Available] = AssetStatuses.Available,
                [AssetTransitions.Inplanning_To_Buildplanready] = AssetStatuses.BuildPlanReady,
                [AssetTransitions.Buildplanready_To_Inplanning] = AssetStatuses.InPlanning,
                [AssetTransitions.Inservice_To_Inplanning] = AssetStatuses.InPlanning,
                [AssetTransitions.Available_To_Inplanning] = AssetStatuses.InPlanning,
                [AssetTransitions.Installed_To_Inplanning] = AssetStatuses.InPlanning,
                [AssetTransitions.Notavailable_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Available_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Buildplanready_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Installed_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Inservice_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Inplanning_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Inrepair_To_Intransit] = AssetStatuses.InTransit,
                [AssetTransitions.Notavailable_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Available_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Buildplanready_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Installed_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Inservice_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Inplanning_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Intransit_To_Inrepair] = AssetStatuses.InRepair,
                [AssetTransitions.Intransit_To_Notavailable] = AssetStatuses.NotAvailable,
                [AssetTransitions.Intransit_To_Available] = AssetStatuses.Available,
                [AssetTransitions.Intransit_To_Buildplanready] = AssetStatuses.BuildPlanReady,
                [AssetTransitions.Intransit_To_Installed] = AssetStatuses.Installed,
                [AssetTransitions.Intransit_To_Disposed] = AssetStatuses.Disposed,
                [AssetTransitions.Intransit_To_Inplanning] = AssetStatuses.InPlanning,
                [AssetTransitions.Inrepair_To_Notavailable] = AssetStatuses.NotAvailable,
                [AssetTransitions.Inrepair_To_Available] = AssetStatuses.Available,
                [AssetTransitions.Inrepair_To_Buildplanready] = AssetStatuses.BuildPlanReady,
                [AssetTransitions.Inrepair_To_Installed] = AssetStatuses.Installed,
                [AssetTransitions.Inrepair_To_Disposed] = AssetStatuses.Disposed,
                [AssetTransitions.Inrepair_To_Inplanning] = AssetStatuses.InPlanning,
            };

        private static readonly IDictionary<(AssetClassStatuses startStatus, AssetClassStatuses endStatus), List<AssetClassTransitions>> AssetClassStatusToStatusTransitions =
            new Dictionary<(AssetClassStatuses startStatus, AssetClassStatuses endStatus), List<AssetClassTransitions>>
            {
               [(AssetClassStatuses.Draft, AssetClassStatuses.Active)] = new List<AssetClassTransitions>
               {
                   AssetClassTransitions.Draft_Active,
               },
               [(AssetClassStatuses.Active, AssetClassStatuses.Deprecated)] = new List<AssetClassTransitions>
               {
                   AssetClassTransitions.Active_Deprecated,
               },
               [(AssetClassStatuses.Draft, AssetClassStatuses.Deprecated)] = new List<AssetClassTransitions>
               {
                   AssetClassTransitions.Draft_Active,
                   AssetClassTransitions.Active_Deprecated,
               },
            };

        /// <summary>
        /// Checks if a state transition from the specified start status to end status is allowed.
        /// </summary>
        /// <param name="fromStatus">The starting status.</param>
        /// <param name="toStatus">The target status.</param>
        /// <returns>True if the transition is allowed; otherwise, false.</returns>
        public static bool IsTransitionAllowed(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum fromStatus, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toStatus)
        {
            return AssetStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        /// <summary>
        /// Gets the required transition path (list of transition steps) to move from one status to another.
        /// </summary>
        /// <param name="fromStatus">The starting status.</param>
        /// <param name="toStatus">The target status.</param>
        /// <returns>A list of transitions required to reach the target status, or an empty list if no valid path exists.</returns>
        public static List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> GetTransitionPath(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum fromStatus, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toStatus)
        {
            if (AssetStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>(transitions);
            }

            return new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>();
        }

        public static bool IsTransitionAllowed(AssetClassStatuses fromStatus, AssetClassStatuses toStatus)
        {
            return AssetClassStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        public static List<AssetClassTransitions> GetTransitionPath(AssetClassStatuses fromStatus, AssetClassStatuses toStatus)
        {
            if (AssetClassStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<AssetClassTransitions>(transitions);
            }

            return new List<AssetClassTransitions>();
        }

        /// <summary>
        /// Gets the destination state entered by each transition in the path.
        /// </summary>
        public static List<SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum> GetTransitionDestinationStates(
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum fromStatus,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toStatus)
        {
            var transitions = GetTransitionPath(fromStatus, toStatus);
            var destinationStates = new List<SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum>();

            foreach (var transition in transitions)
            {
                if (!TransitionDestinationStates.TryGetValue(transition, out var destinationState))
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve destination state for transition '{transition}' from '{fromStatus}' to '{toStatus}'.");
                }

                destinationStates.Add(destinationState);
            }

            return destinationStates;
        }
    }
}