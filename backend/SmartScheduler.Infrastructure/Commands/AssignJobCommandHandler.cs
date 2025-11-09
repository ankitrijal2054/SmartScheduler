using MediatR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Events;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Commands;

/// <summary>
/// Handler for AssignJobCommand.
/// Creates an assignment for a job and publishes a JobAssignedEvent.
/// </summary>
public class AssignJobCommandHandler : IRequestHandler<AssignJobCommand, int>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<AssignJobCommandHandler> _logger;

    public AssignJobCommandHandler(
        ApplicationDbContext dbContext,
        IAssignmentRepository assignmentRepository,
        IMediator mediator,
        ILogger<AssignJobCommandHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(AssignJobCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing AssignJobCommand: JobId={JobId}, ContractorId={ContractorId}",
            request.JobId, request.ContractorId);

        // Validate job exists and is pending
        var job = await _dbContext.Jobs.FindAsync(new object[] { request.JobId }, cancellationToken: cancellationToken);
        if (job == null)
        {
            throw new NotFoundException($"Job {request.JobId} not found");
        }

        if (job.Status != JobStatus.Pending)
        {
            throw new ValidationException($"Job {request.JobId} is already assigned or completed. Current status: {job.Status}");
        }

        // Validate contractor exists
        var contractor = await _dbContext.Contractors.FindAsync(new object[] { request.ContractorId }, cancellationToken: cancellationToken);
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor {request.ContractorId} not found");
        }

        // Check if contractor is active
        if (!contractor.IsActive)
        {
            throw new ValidationException($"Contractor {request.ContractorId} is not active");
        }

        // Check if job is already assigned
        if (job.AssignedContractorId.HasValue)
        {
            throw new ValidationException($"Job {request.JobId} is already assigned to contractor {job.AssignedContractorId}");
        }

        // Create assignment
        var assignment = new Assignment
        {
            JobId = request.JobId,
            ContractorId = request.ContractorId,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Assignments.Add(assignment);

        // Update job status and assigned contractor
        job.Status = JobStatus.Assigned;
        job.AssignedContractorId = request.ContractorId;
        job.UpdatedAt = DateTime.UtcNow;
        _dbContext.Jobs.Update(job);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Job {JobId} assigned to Contractor {ContractorId}. Assignment ID: {AssignmentId}",
            request.JobId, request.ContractorId, assignment.Id);

        // Publish JobAssignedEvent
        var jobAssignedEvent = new JobAssignedEvent(
            jobId: request.JobId,
            assignmentId: assignment.Id,
            contractorId: request.ContractorId,
            customerId: job.CustomerId);

        await _mediator.Publish(jobAssignedEvent, cancellationToken);

        return assignment.Id;
    }
}

