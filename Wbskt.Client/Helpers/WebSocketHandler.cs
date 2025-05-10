using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Wbskt.Client.Configurations;
using Wbskt.Client.Contracts;

namespace Wbskt.Client.Helpers
{
    /// <summary>
    /// Handles WebSocket communication.
    /// </summary>
    internal static class WebSocketHandler
    {
        public static async Task ListenAsync(ILogger logger, IWbsktConfiguration configuration, Action<UserClientPayload> onReceivedPayload, Action<bool> updateStatus, CancellationToken cancellationToken)
        {
            updateStatus(false);
            var token = await TokenProvider.GetTokenAsync(configuration, logger);
            if (string.IsNullOrWhiteSpace(token)) return;

            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
            var socketServerAddress = jwt.Claims.GetSocketServerAddress();
            var tokenId = jwt.Claims.GetTokenId();
            logger?.LogDebug("Token with ID: {tokenId} received", tokenId);

            var ws = new ClientWebSocket();
            try
            {
                ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");
#if NET
                ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
#endif

                var wsUri = new Uri($"ws://{socketServerAddress}/ws");
                logger?.LogInformation("Trying to connect: {wsUri}", wsUri);
                await ws.ConnectAsync(wsUri, CancellationToken.None);
                updateStatus(true);
                logger?.LogInformation("Connection established to: {wsUri}", wsUri);

                await ws.WriteAsync(configuration.ClientDetails.Name, CancellationToken.None);

                cancellationToken.Register(() => CloseClientConnection(logger, ws, updateStatus));
                while (!cancellationToken.IsCancellationRequested && ws.State == WebSocketState.Open)
                {
                    var (receiveResult, message) = await ws.ReadAsync(CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.CloseSent))
                    {
                        logger?.LogInformation("Closing connection ({closeStatus})", receiveResult.CloseStatusDescription);
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection (socket server ack)", CancellationToken.None);
                        updateStatus(false);
                        break;
                    }

                    HandleMessage(message, logger, onReceivedPayload);
                }
            }
            catch (Exception ex)
            {
                updateStatus(false);
                logger?.LogError(ex, "Unexpected error during WebSocket communication: {error}", ex.Message);
            }
        }

        private static void CloseClientConnection(ILogger logger, ClientWebSocket ws, Action<bool> updateStatus)
        {
            logger?.LogInformation("Closing connection ({closeStatus})", "Closing connection (client initiated)");
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection (client initiated)", CancellationToken.None);
            updateStatus(false);
        }

        private static void HandleMessage(string message, ILogger logger, Action<UserClientPayload> onReceivedPayload)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                logger?.LogWarning("Received message is empty.");
                return;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<UserClientPayload>(message) ?? throw new JsonException("Serialized payload is null.");
                logger?.LogDebug("Payload received: {payload}", payload.Data);
                onReceivedPayload(payload);
            }
            catch (JsonException ex)
            {
                logger?.LogError(ex, "Error while deserializing payload: {message}, error: {error}", message, ex.Message);
            }
        }
    }
}
