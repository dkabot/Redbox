using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Health
{
    public static class AlertTypes
    {
        public const string Qlm = "Qlm";
        public const string ReturnRejected = "ReturnRejected";
        public const string ApplicationCrash = "ApplicationCrash";
        public const string HardwareError = "HardwareError";
        public const string HeartbeatNotReceived = "HeartbeatNotReceived";
        public const string MachineNearingFull = "MachineNearingFull";
        public const string MachineFull = "MachineFull";
        public const string TitleEmpty = "TitleEmpty";

        private static readonly IDictionary<string, int> AlertMappings =
            (IDictionary<string, int>)new Dictionary<string, int>(
                (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
            {
                {
                    nameof(Qlm),
                    1
                },
                {
                    nameof(ReturnRejected),
                    2
                },
                {
                    nameof(ApplicationCrash),
                    3
                },
                {
                    nameof(HardwareError),
                    4
                },
                {
                    nameof(HeartbeatNotReceived),
                    5
                },
                {
                    nameof(MachineNearingFull),
                    6
                },
                {
                    nameof(MachineFull),
                    7
                },
                {
                    nameof(TitleEmpty),
                    8
                }
            };

        public static int ToAlertTypeNumeric(this string key)
        {
            return !AlertMappings.ContainsKey(key) ? 0 : AlertMappings[key];
        }
    }
}