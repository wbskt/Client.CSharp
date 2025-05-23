﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wbskt.Client.Configurations;
using Wbskt.Client.Contracts;
using Wbskt.Client.Helpers;

namespace Wbskt.Client
{
    /// <summary>
    /// Listens for WebSocket events and processes payloads.
    /// This class is responsible for managing the WebSocket connection lifecycle,
    /// validating configuration, and handling incoming messages.
    /// </summary>
    public class WbsktListener
    {
        private readonly IWbsktConfiguration _wbsktConfiguration;
        private readonly ILogger<WbsktListener> _logger;

        private Task _worker;
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// Delegate for handling received payloads.
        /// </summary>
        /// <param name="clientPayload">The payload received from the WebSocket server.</param>
        public delegate void TriggerActionHandler(UserClientPayload clientPayload);

        /// <summary>
        /// Event triggered when a payload is received from the WebSocket server.
        /// Subscribers can attach their handlers to process the payload.
        /// </summary>
        public event TriggerActionHandler ReceivedPayloadEvent;

        /// <summary>
        /// Event triggered when the WebSocket client successfully establishes a connection.
        /// Subscribers can attach their handlers to perform actions upon connection.
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Event triggered when the WebSocket client disconnects from the server.
        /// Subscribers can attach their handlers to perform actions upon disconnection.
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Gets the connection status of the WebSocket client.
        /// True if the connection is active, otherwise false.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WbsktListener"/> class.
        /// </summary>
        /// <param name="wbsktConfiguration">The WebSocket client configuration.</param>
        /// <param name="logger">The logger instance for logging events and errors. (may )</param>
        public WbsktListener(IWbsktConfiguration wbsktConfiguration, ILogger<WbsktListener> logger)
        {
            _wbsktConfiguration = wbsktConfiguration ?? throw new ArgumentNullException(nameof(wbsktConfiguration));
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts listening for WebSocket events.
        /// This method runs in a loop until the provided <see cref="CancellationToken"/> is canceled.
        /// It validates the configuration, establishes a WebSocket connection, and processes incoming messages.
        /// </summary>
        /// <param name="ct">The cancellation token to stop the listening process.</param>
        public void StartListening(CancellationToken ct)
        {
            _worker = StartListeningAsync(ct);
        }

        /// <summary>
        /// Starts listening for WebSocket events.
        /// This method runs in a loop until the provided <see cref="CancellationToken"/> is canceled.
        /// It validates the configuration, establishes a WebSocket connection, and processes incoming messages.
        /// </summary>
        /// <param name="ct">The cancellation token to stop the listening process.</param>
        public async Task StartListeningAsync(CancellationToken ct)
        {
            ct.Register(_cts.Cancel);
            while (!_cts.Token.IsCancellationRequested)
            {
                // Validate the configuration before starting the WebSocket connection.
                if (!ConfigurationValidator.IsValid(_wbsktConfiguration, _logger))
                {
                    _logger?.LogError("Please configure the listener");
                    if (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, _cts.Token);
                    }
                    continue;
                }

                _logger?.LogInformation("Client running at: {time}", DateTimeOffset.Now);

                try
                {
                    // Start listening to WebSocket events.
                    await WebSocketHandler.ListenAsync(_logger, _wbsktConfiguration, OnReceivedPayload, UpdateStatus, _cts.Token);
                }
                catch (Exception ex)
                {
                    // Log unexpected errors during WebSocket communication.
                    _logger?.LogError(ex, "Unexpected error: {error}", ex.Message);
                }

                UpdateStatus(false);
                // Wait for the retry interval before attempting to reconnect.
                if (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, _cts.Token);
                }
            }
        }

        /// <summary>
        /// Stops the WebSocket listener gracefully.
        /// This method cancels the listening process by signaling the associated <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public void StopListening()
        {
            StopListeningAsync().Wait();
        }

        /// <summary>
        /// Stops the WebSocket listener gracefully.
        /// This method cancels the listening process by signaling the associated <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task StopListeningAsync()
        {
            if (_worker != null && !_worker.GetAwaiter().IsCompleted)
            {
#if NETSTANDARD
                _cts.Cancel();
#elif NET
                await _cts.CancelAsync();
#endif

                await _worker;
            }

            IsConnected = false;
            _cts.Dispose();
        }


        /// <summary>
        /// Invokes the <see cref="ReceivedPayloadEvent"/> event when a payload is received.
        /// This method is called by the WebSocket handler when a new message is received.
        /// </summary>
        /// <param name="clientPayload">The payload received from the WebSocket server.</param>
        private void OnReceivedPayload(UserClientPayload clientPayload)
        {
            // Trigger the event to notify subscribers of the received payload.
            ReceivedPayloadEvent?.Invoke(clientPayload);
        }

        /// <summary>
        /// Updates the connection status of the WebSocket client.
        /// Invokes the <see cref="OnConnected"/> event when the client connects
        /// and the <see cref="OnDisconnected"/> event when the client disconnects.
        /// </summary>
        /// <param name="status">The new connection status. True if connected, false if disconnected.</param>
        private void UpdateStatus(bool status)
        {
            if (status)
            {
                OnConnected?.Invoke();
            }

            if (IsConnected && status == false)
            {
                OnDisconnected?.Invoke();
            }

            IsConnected = status;
        }
    }
}
