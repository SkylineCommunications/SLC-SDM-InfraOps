using System;
using System.Collections.Generic;
using System.Text;

using SharedMappers.DomIds;

using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
using Skyline.DataMiner.SDM.AssetManagement.Models;

namespace SharedCommonLibrary.AssetManagement.State_Management
{
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
        /// <returns>A list of transitions required to reach the target status, or null if no valid path exists.</returns>
        public static List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> GetTransitionPath(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum fromStatus, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toStatus)
        {
            if (AssetStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>(transitions);
            }

            return null;
        }

        /// <summary>
        /// Gets the required transition path (list of transition steps) to move from one status to another.
        /// </summary>
        /// <param name="fromStatus">The starting status.</param>
        /// <param name="toStatus">The target status.</param>
        /// <returns>A list of transitions required to reach the target status, or null if no valid path exists.</returns>
        public static List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum> GetTransitionPath(Asset asset)
        {
            bool isNew = String.IsNullOrWhiteSpace(asset.Identifier);
            var from = isNew == true ? SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable : asset.StateField.OriginalValue;
            var to = asset.State;

            if (to != from)
            {
                return GetTransitionPath(from, to);
            }

            return new List<SlcAsset_Management.Behaviors.Asset_Behavior.TransitionsEnum>();
        }
    }
}
