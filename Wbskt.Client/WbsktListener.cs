using System;
using System.Net.Http;
#if NETSTANDARD
using System.Text;
#elif NET
using System.Net.Http.Json;
#endif
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Wbskt.Client
{
    public class WbsktListener
    {
        private readonly IWbsktConfiguration _wbsktConfiguration;
        private readonly ILogger<WbsktListener> _logger;

        public delegate void TriggerActionHandler(ClientPayload clientPayload);

        public event TriggerActionHandler ReceivedPayload;

        public WbsktListener(IWbsktConfiguration wbsktConfiguration, ILogger<WbsktListener> logger)
        {
            _wbsktConfiguration = wbsktConfiguration;
            _logger = logger;
        }

        public async Task StartListening(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (!IsConfigValid(_wbsktConfiguration))
                {
                    _logger.LogError("restart the service after configuring properly");
                    await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, ct);
                    continue;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Client running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    await Listen(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "unexpected error: {error}", ex.Message);
                }

                try
                {
                    await Task.Delay(_wbsktConfiguration.ClientDetails.RetryIntervalInSeconds * 1000, ct);
                }
                catch (TaskCanceledException tcex)
                {
                    _logger.LogError(tcex, "unexpected error: {error}", tcex.Message);
                }
            }
        }

        private async Task Listen(CancellationToken ct)
        {
            var token = await GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
            var socketServerAddress = jwt.Claims.GetSocketServerAddress();
            var tokenId = jwt.Claims.GetTokenId();
            _logger.LogInformation("token with id: {tokenId} received", tokenId);

            var ws = new ClientWebSocket();
            try
            {
                ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");
#if NET
                ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
#endif

                var wsUri = new Uri($"ws://{socketServerAddress}/ws");

                _logger.LogInformation("trying to connect: {wsUri}", wsUri);
                await ws.ConnectAsync(wsUri, ct);
                _logger.LogInformation("connection established to: {wsUri}", wsUri);

                await ws.WriteAsync("ping", ct);

                while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
                {
                    var (receiveResult, message) = await ws.ReadAsync(ct);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("closing connection ({closeStatus})", receiveResult.CloseStatusDescription);
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing connection (socket server ack)", CancellationToken.None);
                        break;
                    }

                    HandleMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation token is cancelled.
                _logger.LogInformation("cancellation requested, closing socket connection.");
            }
            catch (WebSocketException ex) when (ct.IsCancellationRequested)
            {
                // Sometimes WebSocket throws when cancelled mid-await
                _logger.LogInformation("webSocket cancelled, closing gracefully. {error}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unexpected error occured during socket communication, error: {error}", ex.Message);
            }
            finally
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cancellation requested", CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("error while closing websocket: {error}", ex.Message);
                    }
                }

                ws.Dispose();
                _logger.LogInformation("disposed connection");
            }
        }

        private async Task<string> GetToken()
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"http://{_wbsktConfiguration.CoreServerAddress}"),
            };
            try
            {
                var clientConnReq = new ClientConnectionRequest()
                {
                    ChannelSecret = _wbsktConfiguration.ChannelDetails.Secret,
                    ClientName = _wbsktConfiguration.ClientDetails.Name,
                    ClientUniqueId = _wbsktConfiguration.ClientDetails.UniqueId,
                    ChannelSubscriberId = _wbsktConfiguration.ChannelDetails.SubscriberId
                };
#if NETSTANDARD
                var json = JsonSerializer.Serialize(clientConnReq);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync("/api/channels/client", content);
#elif NET

                var result = await httpClient.PostAsync("/api/channels/client", JsonContent.Create(clientConnReq));
#endif

                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }

                _logger.LogError("response from token request is: {message}, statusCode: {code}", result.ReasonPhrase, result.StatusCode);
                return string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "cannot reach server:{baseAddress}, error: {error}", httpClient.BaseAddress, ex.Message);
                throw;
            }
        }

        private void HandleMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("received message is empty");
                return;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<ClientPayload>(message) ?? throw new JsonException("Serialized payload is null");
                _logger.LogInformation("payload received: {payload}", payload.Data);
                OnReceivedPayload(payload);
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "error while deserializing payload: {message}, error: {error}", message, jex.Message);
            }
        }

        private bool IsConfigValid(IWbsktConfiguration configuration)
        {
            if (configuration == null)
            {
                _logger.LogError("configuration is not set");
                return false;
            }

            if (configuration.ChannelDetails.SubscriberId == default)
            {
                _logger.LogError("subscriberId is not set");
                return false;
            }

            if (string.IsNullOrWhiteSpace(configuration.ChannelDetails.Secret))
            {
                _logger.LogError("secret is not set");
                return false;
            }

            if (configuration.ClientDetails.UniqueId == default)
            {
                _logger.LogError("uniqueId is not set");
                return false;
            }

            return true;
        }

        protected virtual void OnReceivedPayload(ClientPayload clientPayload)
        {
            ReceivedPayload.Invoke(clientPayload);
        }
    }
}
