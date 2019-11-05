// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NotSoSimpleJSON;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNet.SignalR.Client
{
    [DebuggerDisplay("{ConnectionId}")]
    public class NegotiationResponse
    {
        public string ConnectionId { get; set; }
        public string[] Transports { get; set; }
        //public string ConnectionToken { get; set; }
        //public string Url { get; set; }
        //public string ProtocolVersion { get; set; }
        //public double DisconnectTimeout { get; set; }
        //public bool TryWebSockets { get; set; }
        //public double? KeepAliveTimeout { get; set; }
        //public double TransportConnectTimeout { get; set; }

        public static NegotiationResponse FromJson(JSONNode json)
        {
            if (json == null)
                return null;

            var Result = new NegotiationResponse();

            Result.ConnectionId = json["connectionId"].AsString;
            Result.Transports = json["availableTransports"].AsArray.Select(el => el.AsObject["transport"].AsString).ToArray();
            //Result.ConnectionToken = json["ConnectionToken"].AsString;
            //Result.Url = json["Url"].AsString;
            //Result.ProtocolVersion = json["ProtocolVersion"].AsString;
            //Result.DisconnectTimeout = json["DisconnectTimeout"].AsDouble.GetValueOrDefault();
            //Result.TryWebSockets = json["TryWebSockets"].AsBool.GetValueOrDefault();
            //Result.KeepAliveTimeout = json["KeepAliveTimeout"].AsDouble;
            //Result.TransportConnectTimeout = json["TransportConnectTimeout"].AsDouble.GetValueOrDefault();

            return Result;
        }
    }
}
