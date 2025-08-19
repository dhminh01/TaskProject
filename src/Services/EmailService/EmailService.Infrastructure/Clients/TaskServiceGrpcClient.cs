using EmailService.Application.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.Grpc;

namespace EmailService.Infrastructure.Clients;

public class TaskServiceGrpcClient : ITaskServiceClient
{
    private readonly TaskAckService.TaskAckServiceClient _client;
    private readonly ILogger<TaskServiceGrpcClient> _logger;

    public TaskServiceGrpcClient(TaskAckService.TaskAckServiceClient client, ILogger<TaskServiceGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> AcknowledgeTaskEmailSentAsync(
        Guid taskId,
        bool success,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email acknowledgment for Task {TaskId}, Success: {Success}",
                taskId, success);

            var request = new TaskEmailAckRequest
            {
                TaskId = taskId.ToString(),
                Success = success,
                ErrorMessage = errorMessage ?? string.Empty,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var response = await _client.AcknowledgeTaskEmailAsync(request,
                cancellationToken: cancellationToken);

            if (response.Acknowledged)
            {
                _logger.LogInformation("Successfully acknowledged email status for Task {TaskId}", taskId);
                return true;
            }
            else
            {
                _logger.LogWarning("TaskService did not acknowledge email status for Task {TaskId}: {Message}",
                    taskId, response.Message);
                return false;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while acknowledging email for Task {TaskId}", taskId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while acknowledging email for Task {TaskId}", taskId);
            return false;
        }
    }
}