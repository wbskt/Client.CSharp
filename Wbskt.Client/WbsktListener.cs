using System;
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

        /// <summary>
        /// Delegate for handling received payloads.
        /// </summary>
        /// <param name="clientPayload">The payload received from the WebSocket server.</param>
        public delegate void TriggerActionHandler(ClientPayload clientPayload);

        /// <summary>
        /// Event triggered when a payload is received from the WebSocket server.
        /// Subscribers can attach their handlers to process the payload.
        /// </summary>
        public event TriggerActionHandler ReceivedPayloadEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="WbsktListener"/> class.
        /// </summary>
        /// <param name="wbsktConfiguration">The WebSocket client configuration.</param>
        /// <param name="logger">The logger instance for logging events and errors.</param>
        public WbsktListener(IWbsktConfiguration wbsktConfiguration, ILogger<WbsktListener> logger = null)
        {
            _wbsktConfiguration = wbsktConfiguration ?? throw new ArgumentNullException(nameof(wbsktConfiguration));
            _logger = logger;
        }

        /// <summary>
        /// Starts listening for WebSocket events.
        /// This method runs in a loop until the provided <see cref="CancellationToken"/> is canceled.
        /// It validates the configuration, establishes a WebSocket connection, and processes incoming messages.
        /// </summary>
        /// <param name="ct">The cancellation token to stop the listening process.</param>
        public async Task StartListening(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Validate the configuration before starting the WebSocket connection.
                if (!ConfigurationValidator.IsValid(_wbsktConfiguration, _logger))
                {
                    _logger?.LogError("Restart the service after configuring properly.");
                    await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, ct);
                    continue;
                }

                _logger?.LogInformation("Client running at: {time}", DateTimeOffset.Now);

                try
                {
                    // Start listening to WebSocket events.
                    await WebSocketHandler.ListenAsync(_logger, _wbsktConfiguration, OnReceivedPayload, ct);
                }
                catch (Exception ex)
                {
                    // Log unexpected errors during WebSocket communication.
                    _logger?.LogError(ex, "Unexpected error: {error}", ex.Message);
                }

                // Wait for the retry interval before attempting to reconnect.
                await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, ct);
            }
        }

        /// <summary>
        /// Invokes the <see cref="ReceivedPayloadEvent"/> event when a payload is received.
        /// This method is called by the WebSocket handler when a new message is received.
        /// </summary>
        /// <param name="clientPayload">The payload received from the WebSocket server.</param>
        private void OnReceivedPayload(ClientPayload clientPayload)
        {
            // Trigger the event to notify subscribers of the received payload.
            ReceivedPayloadEvent?.Invoke(clientPayload);
        }
    }
}
