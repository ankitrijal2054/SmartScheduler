using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SmartScheduler.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time notifications to contractors, customers, and dispatchers.
/// Handles contractor job notifications, customer job updates, and dispatcher alerts.
/// </summary>
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to NotificationHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected from NotificationHub. Exception: {ExceptionMessage}",
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join contractor group for receiving contractor-specific notifications.
    /// </summary>
    /// <param name="contractorId">The contractor ID</param>
    public async Task JoinContractorGroup(int contractorId)
    {
        var groupName = $"contractor-{contractorId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Join customer group for receiving customer-specific notifications.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    public async Task JoinCustomerGroup(int customerId)
    {
        var groupName = $"customer-{customerId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Join dispatcher group for receiving dispatcher-specific notifications.
    /// </summary>
    /// <param name="dispatcherId">The dispatcher ID</param>
    public async Task JoinDispatcherGroup(int dispatcherId)
    {
        var groupName = $"dispatcher-{dispatcherId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave contractor group.
    /// </summary>
    /// <param name="contractorId">The contractor ID</param>
    public async Task LeaveContractorGroup(int contractorId)
    {
        var groupName = $"contractor-{contractorId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave customer group.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    public async Task LeaveCustomerGroup(int customerId)
    {
        var groupName = $"customer-{customerId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave dispatcher group.
    /// </summary>
    /// <param name="dispatcherId">The dispatcher ID</param>
    public async Task LeaveDispatcherGroup(int dispatcherId)
    {
        var groupName = $"dispatcher-{dispatcherId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }
}

