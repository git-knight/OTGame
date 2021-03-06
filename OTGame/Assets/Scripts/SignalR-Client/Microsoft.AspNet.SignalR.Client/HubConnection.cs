﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NotSoSimpleJSON;

namespace Microsoft.AspNet.SignalR.Client
{
    enum HubMessageType
    {
        INVOCATION = 1,
        STREAM_ITEM = 2,
        COMPLETION = 3,
        STREAM_INVOCATION = 4,
        CANCEL_INVOCATION = 5,
        PING = 6,
        CLOSE = 7,
        INVOCATION_BINDING_FAILURE = -1
    }
    /// <summary>
    /// A <see cref="Connection"/> for interacting with Hubs.
    /// </summary>
    public class HubConnection<T> : Connection, IHubConnection
    {
        private readonly Dictionary<string, HubProxy> _hubs = new Dictionary<string, HubProxy>(StringComparer.OrdinalIgnoreCase);
        internal readonly Dictionary<string, Action<HubResult>> _callbacks = new Dictionary<string, Action<HubResult>>();
        private int _callbackId;

        private readonly T Hub;

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        public HubConnection(string url)
            : this(null, url, useDefaultUrl: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        /// <param name="useDefaultUrl">Determines if the default "/signalr" path should be appended to the specified url.</param>
        public HubConnection(T hub, string url, bool useDefaultUrl)
            : base(GetUrl(url, useDefaultUrl))
        {
            Hub = hub;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        /// <param name="queryString">The query string data to pass to the server.</param>
        public HubConnection(string url, string queryString)
            : this(url, queryString, useDefaultUrl: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        /// <param name="queryString">The query string data to pass to the server.</param>
        /// <param name="useDefaultUrl">Determines if the default "/signalr" path should be appended to the specified url.</param>
        public HubConnection(string url, string queryString, bool useDefaultUrl)
            : base(GetUrl(url, useDefaultUrl), queryString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        /// <param name="queryString">The query string data to pass to the server.</param>
        public HubConnection(string url, IDictionary<string, string> queryString)
            : this(url, queryString, useDefaultUrl: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HubConnection"/> class.
        /// </summary>
        /// <param name="url">The url to connect to.</param>
        /// <param name="queryString">The query string data to pass to the server.</param>
        /// <param name="useDefaultUrl">Determines if the default "/signalr" path should be appended to the specified url.</param>
        public HubConnection(string url, IDictionary<string, string> queryString, bool useDefaultUrl)
            : base(GetUrl(url, useDefaultUrl), queryString)
        {
        }

        internal override void OnReconnecting()
        {
            ClearInvocationCallbacks("Resources.Message_Reconnecting");
            base.OnReconnecting();
        }

        public void Invoke(string methodName, object[] args)
        {
            var invocation = new
            {
                type = 1,
                target = methodName,
                arguments = args
            };

            var value = JSON.FromData(invocation).Serialize() + '\x1e';

            Send(value).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    //_connection.RemoveCallback(callbackId);
                    //tcs.TrySetCanceled();
                }
                else if (task.IsFaulted)
                {
                    //_connection.RemoveCallback(callbackId);
                    //tcs.TrySetUnwrappedException(task.Exception);
                }
            },
            TaskContinuationOptions.NotOnRanToCompletion);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "")]
        protected override void OnMessageReceived(JSONNode message)
        {
            var type = message["type"].AsInt;
            switch (type)
            {
                case 1:
                    var targetName = message["target"].AsString;
                    var arguments = message["arguments"].AsArray;

                    if (targetName == "InvokeMethod")
                        (Hub as GameHub).MethodInvoked(arguments[0].AsString, arguments.Skip(1).ToArray());
                    else
                    {
                        var target = Hub.GetType().GetMethod(targetName);
                        if (target != null)
                            target.Invoke(Hub, arguments.Zip(target.GetParameters(), (a, b) => b.ParameterType == typeof(string) ? a.AsString : b.ParameterType == typeof(int) ? (object)a.AsInt : a.AsObject).ToArray());
                    }

                    break;

                case 6:
                    // ping
                    break;

                default:
                    break;
            }

            // We have to handle progress updates first in order to ensure old clients that receive
            // progress updates enter the return value branch and then no-op when they can't find
            // the callback in the map (because the message["I"[ value will not be a valid callback ID)
            /*
            if (message["P"] != null)
            {
                var result = HubResult.FromJson(message);
                Action<HubResult> callback;

                lock (_callbacks)
                {
                    if (!_callbacks.TryGetValue(result.ProgressUpdate.Id, out callback))
                    {
                        Trace(TraceLevels.Messages, "Callback with id " + result.ProgressUpdate.Id + " not found!");
                    }
                }

                callback?.Invoke(result);
            }
            else if (message["I"] != null)
            {
                var result = HubResult.FromJson(message);
                Action<HubResult> callback;

                lock (_callbacks)
                {
                    if (_callbacks.TryGetValue(result.Id, out callback))
                    {
                        _callbacks.Remove(result.Id);
                    }
                    else
                    {
                        Trace(TraceLevels.Messages, "Callback with id " + result.Id + " not found!");
                    }
                }

                if (callback != null)
                {
                    callback(result);
                }
            }
            else
            {
                var invocation = HubInvocation.FromJson(message);
                HubProxy hubProxy;
                if (_hubs.TryGetValue(invocation.Hub, out hubProxy))
                {
                    if (invocation.State != null)
                    {
                        foreach (var state in invocation.State)
                        {
                            hubProxy[state.Key] = state.Value;
                        }
                    }

                    hubProxy.InvokeEvent(invocation.Method, invocation.Args);
                }

                base.OnMessageReceived(message);
            }*/
        }

        protected override string OnSending()
        {
            var data = _hubs.Select(p => new HubRegistrationData
            {
                Name = p.Key
            });

            return JSON.FromData(data).Serialize();
        }

        protected override void OnClosed()
        {
            ClearInvocationCallbacks("Resources.Message_ConnectionClosed");
            base.OnClosed();
        }

        /// <summary>
        /// Creates an <see cref="IHubProxy"/> for the hub with the specified name.
        /// </summary>
        /// <param name="hubName">The name of the hub.</param>
        /// <returns>A <see cref="IHubProxy"/></returns>
        public IHubProxy CreateHubProxy(string hubName)
        {
            if (State != ConnectionState.Disconnected)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Resources.Error_ProxiesCannotBeAddedConnectionStarted"));
            }

            HubProxy hubProxy;
            if (!_hubs.TryGetValue(hubName, out hubProxy))
            {
                hubProxy = new HubProxy(this, hubName);
                _hubs[hubName] = hubProxy;
            }
            return hubProxy;
        }

        string IHubConnection.RegisterCallback(Action<HubResult> callback)
        {
            lock (_callbacks)
            {
                string id = _callbackId.ToString(CultureInfo.InvariantCulture);
                _callbacks[id] = callback;
                _callbackId++;
                return id;
            }
        }

        void IHubConnection.RemoveCallback(string callbackId)
        {
            lock (_callbacks)
            {
                _callbacks.Remove(callbackId);
            }
        }

        private static string GetUrl(string url, bool useDefaultUrl)
        {
            if (!url.EndsWith("/", StringComparison.Ordinal))
            {
                url += "/";
            }

            if (useDefaultUrl)
            {
                return url + "signalr";
            }

            return url;
        }

        private void ClearInvocationCallbacks(string error)
        {
            // Copy the callbacks then clear the list so if any of them happen to do an Invoke again
            // they can safely register their own callback into the global list again.
            // Once the global list is clear, dispatch the callbacks on their own threads (BUG #3101)

            Action<HubResult>[] callbacks;

            lock (_callbacks)
            {
                callbacks = _callbacks.Values.ToArray();
                _callbacks.Clear();
            }

            foreach (var callback in callbacks)
            {
                // Create a new HubResult each time as it's mutable and we don't want callbacks
                // changing it during their parallel invocation
                Task.Factory.StartNew(() => callback(new HubResult { Error = error }))
                    .Catch(connection: this);
            }
        }
    }
}
