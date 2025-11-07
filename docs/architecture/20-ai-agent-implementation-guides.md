# 20. AI Agent Implementation Guides

This section provides step-by-step implementation guidance specifically designed for AI agents. Each guide includes concrete examples, file paths, and implementation checklists to ensure consistent, correct implementation across all features.

## 20.1 CQRS Command Implementation Guide

**When to use:** Any operation that modifies data (create, update, delete, assign, etc.)

**Step-by-Step Process:**

### Step 1: Create Command Class

**File:** `backend/SmartScheduler.Application/Commands/{Feature}/{Action}Command.cs`

**Template:**

```csharp
using MediatR;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Commands.Jobs;

public record AssignJobCommand(
    Guid JobId,
    Guid ContractorId,
    Guid DispatcherId
) : IRequest<Result<AssignmentDto>>;

public class AssignJobCommandValidator : AbstractValidator<AssignJobCommand>
{
    public AssignJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("Job ID is required");

        RuleFor(x => x.ContractorId)
            .NotEmpty()
            .WithMessage("Contractor ID is required");

        RuleFor(x => x.DispatcherId)
            .NotEmpty()
            .WithMessage("Dispatcher ID is required");
    }
}
```

### Step 2: Create Command Handler

**File:** `backend/SmartScheduler.Application/Commands/{Feature}/{Action}CommandHandler.cs`

**Template:**

```csharp
using MediatR;
using SmartScheduler.Application.Events;
using SmartScheduler.Domain.Interfaces;

namespace SmartScheduler.Application.Commands.Jobs;

public class AssignJobCommandHandler : IRequestHandler<AssignJobCommand, Result<AssignmentDto>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IContractorRepository _contractorRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AssignJobCommandHandler> _logger;

    public AssignJobCommandHandler(
        IJobRepository jobRepository,
        IContractorRepository contractorRepository,
        IEventPublisher eventPublisher,
        ILogger<AssignJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _contractorRepository = contractorRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Result<AssignmentDto>> Handle(
        AssignJobCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate job exists and is pending
            var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
            if (job == null)
            {
                return Result<AssignmentDto>.Failure("JOB_NOT_FOUND", "Job not found");
            }

            if (job.Status != JobStatus.Pending)
            {
                return Result<AssignmentDto>.Failure("JOB_ALREADY_ASSIGNED", "Job is already assigned");
            }

            // 2. Validate contractor exists and is active
            var contractor = await _contractorRepository.GetByIdAsync(request.ContractorId, cancellationToken);
            if (contractor == null || !contractor.IsActive)
            {
                return Result<AssignmentDto>.Failure("CONTRACTOR_UNAVAILABLE", "Contractor is not available");
            }

            // 3. Create assignment (domain logic)
            var assignment = job.AssignToContractor(contractor.Id, request.DispatcherId);

            // 4. Persist changes
            await _jobRepository.UpdateAsync(job, cancellationToken);
            await _jobRepository.SaveChangesAsync(cancellationToken);

            // 5. Publish domain event (for SignalR + Email notifications)
            await _eventPublisher.PublishAsync(new JobAssignedEvent
            {
                JobId = job.Id,
                ContractorId = contractor.Id,
                CustomerId = job.CustomerId,
                AssignmentId = assignment.Id,
                OccurredAt = DateTime.UtcNow
            }, cancellationToken);

            // 6. Return success result
            _logger.LogInformation("Job {JobId} assigned to contractor {ContractorId}", job.Id, contractor.Id);

            return Result<AssignmentDto>.Success(new AssignmentDto
            {
                Id = assignment.Id,
                JobId = job.Id,
                ContractorId = contractor.Id,
                AssignedAt = assignment.AssignedAt,
                Status = assignment.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning job {JobId} to contractor {ContractorId}",
                request.JobId, request.ContractorId);
            return Result<AssignmentDto>.Failure("INTERNAL_ERROR", "An error occurred while assigning the job");
        }
    }
}
```

### Step 3: Add Controller Endpoint

**File:** `backend/SmartScheduler.API/Controllers/DispatcherController.cs`

**Template:**

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.Commands.Jobs;

namespace SmartScheduler.API.Controllers;

[ApiController]
[Route("api/v1/dispatcher")]
[Authorize(Roles = "Dispatcher")]
public class DispatcherController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DispatcherController> _logger;

    public DispatcherController(IMediator mediator, ILogger<DispatcherController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Assign a job to a contractor
    /// </summary>
    [HttpPost("jobs/{jobId}/assign")]
    [ProducesResponseType(typeof(AssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignJob(
        Guid jobId,
        [FromBody] AssignJobRequest request,
        CancellationToken cancellationToken)
    {
        var dispatcherId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());

        var command = new AssignJobCommand(jobId, request.ContractorId, dispatcherId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorCode switch
        {
            "JOB_NOT_FOUND" => NotFound(new ErrorResponse
            {
                Code = result.ErrorCode,
                Message = result.ErrorMessage,
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            }),
            "JOB_ALREADY_ASSIGNED" or "CONTRACTOR_UNAVAILABLE" => BadRequest(new ErrorResponse
            {
                Code = result.ErrorCode,
                Message = result.ErrorMessage,
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            }),
            _ => StatusCode(500, new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred",
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            })
        };
    }
}

public record AssignJobRequest(Guid ContractorId);
```

### Step 4: Add Frontend Service Method

**File:** `frontend/src/services/dispatcherService.ts`

**Template:**

```typescript
import { apiClient } from "./api";
import { Assignment } from "../types/assignment";

export const dispatcherService = {
  async assignJob(jobId: string, contractorId: string): Promise<Assignment> {
    try {
      const response = await apiClient.post<Assignment>(
        `/dispatcher/jobs/${jobId}/assign`,
        { contractorId }
      );
      return response.data;
    } catch (error) {
      // Error is already handled by Axios interceptor
      throw error;
    }
  },

  // ... other methods
};
```

### Step 5: Add Frontend UI Component

**File:** `frontend/src/features/dispatcher/JobAssignment.tsx`

**Template:**

```typescript
import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { toast } from "react-hot-toast";
import { dispatcherService } from "@/services/dispatcherService";
import type { ContractorRecommendation } from "@/types/contractor";

interface JobAssignmentProps {
  jobId: string;
  recommendations: ContractorRecommendation[];
  onAssignSuccess: () => void;
}

export const JobAssignment: React.FC<JobAssignmentProps> = ({
  jobId,
  recommendations,
  onAssignSuccess,
}) => {
  const [assigningContractorId, setAssigningContractorId] = useState<
    string | null
  >(null);

  const handleAssign = async (contractorId: string, contractorName: string) => {
    if (!window.confirm(`Assign this job to ${contractorName}?`)) {
      return;
    }

    setAssigningContractorId(contractorId);

    try {
      await dispatcherService.assignJob(jobId, contractorId);
      toast.success(`Job assigned to ${contractorName}`);
      onAssignSuccess();
    } catch (error: any) {
      // Error already logged by interceptor
      toast.error(error.response?.data?.message || "Failed to assign job");
    } finally {
      setAssigningContractorId(null);
    }
  };

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">Top Recommendations</h3>

      <div className="grid gap-4">
        {recommendations.map((rec, index) => (
          <div key={rec.contractor.id} className="border rounded-lg p-4">
            <div className="flex justify-between items-start">
              <div>
                <div className="flex items-center gap-2">
                  <span className="font-semibold">#{index + 1}</span>
                  <span className="text-lg">{rec.contractor.name}</span>
                  <span className="text-sm text-gray-600">
                    {rec.contractor.tradeType}
                  </span>
                </div>

                <div className="mt-2 text-sm text-gray-600">
                  <div>
                    Rating: {rec.contractor.averageRating?.toFixed(1) || "N/A"}{" "}
                    ⭐ ({rec.contractor.reviewCount} reviews)
                  </div>
                  <div>
                    Distance: {rec.distanceMiles.toFixed(1)} miles (
                    {rec.travelTimeMinutes} min travel)
                  </div>
                  <div>Match Score: {(rec.score * 100).toFixed(0)}%</div>
                </div>
              </div>

              <Button
                onClick={() =>
                  handleAssign(rec.contractor.id, rec.contractor.name)
                }
                disabled={assigningContractorId !== null}
                loading={assigningContractorId === rec.contractor.id}
              >
                {assigningContractorId === rec.contractor.id
                  ? "Assigning..."
                  : "Assign"}
              </Button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
```

### Step 6: Add Unit Tests

**File:** `backend/SmartScheduler.Tests/Application/Commands/AssignJobCommandHandlerTests.cs`

**Template:**

```csharp
using FluentAssertions;
using Moq;
using SmartScheduler.Application.Commands.Jobs;
using Xunit;

namespace SmartScheduler.Tests.Application.Commands;

public class AssignJobCommandHandlerTests
{
    private readonly Mock<IJobRepository> _jobRepositoryMock;
    private readonly Mock<IContractorRepository> _contractorRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<AssignJobCommandHandler>> _loggerMock;
    private readonly AssignJobCommandHandler _handler;

    public AssignJobCommandHandlerTests()
    {
        _jobRepositoryMock = new Mock<IJobRepository>();
        _contractorRepositoryMock = new Mock<IContractorRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<AssignJobCommandHandler>>();

        _handler = new AssignJobCommandHandler(
            _jobRepositoryMock.Object,
            _contractorRepositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();
        var dispatcherId = Guid.NewGuid();

        var job = new Job
        {
            Id = jobId,
            Status = JobStatus.Pending,
            CustomerId = Guid.NewGuid()
        };

        var contractor = new Contractor
        {
            Id = contractorId,
            IsActive = true,
            Name = "John Doe"
        };

        _jobRepositoryMock.Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _contractorRepositoryMock.Setup(x => x.GetByIdAsync(contractorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractor);

        var command = new AssignJobCommand(jobId, contractorId, dispatcherId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.ContractorId.Should().Be(contractorId);

        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<JobAssignedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_JobNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new AssignJobCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _jobRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("JOB_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ContractorInactive_ReturnsFailure()
    {
        // Arrange
        var job = new Job { Id = Guid.NewGuid(), Status = JobStatus.Pending };
        var contractor = new Contractor { Id = Guid.NewGuid(), IsActive = false };

        _jobRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _contractorRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractor);

        var command = new AssignJobCommand(job.Id, contractor.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONTRACTOR_UNAVAILABLE");
    }
}
```

**Implementation Checklist:**

- [ ] Create Command class with validation in `Application/Commands/{Feature}/`
- [ ] Create CommandHandler with business logic in same folder
- [ ] Add controller endpoint in `API/Controllers/{Role}Controller.cs`
- [ ] Add authorization attribute `[Authorize(Roles = "...")]`
- [ ] Add frontend service method in `services/{role}Service.ts`
- [ ] Create/update frontend component in `features/{role}/`
- [ ] Add unit tests in `Tests/Application/Commands/`
- [ ] Verify domain event is published if needed
- [ ] Test end-to-end flow in development environment

---

## 20.2 CQRS Query Implementation Guide

**When to use:** Any operation that reads data without modifying it (list, get by ID, search, filter)

**Step-by-Step Process:**

### Step 1: Create Query Class

**File:** `backend/SmartScheduler.Application/Queries/{Feature}/{Action}Query.cs`

**Template:**

```csharp
using MediatR;

namespace SmartScheduler.Application.Queries.Contractors;

public record GetContractorRecommendationsQuery(
    Guid JobId,
    string JobType,
    decimal Latitude,
    decimal Longitude,
    DateTime DesiredDateTime,
    bool ContractorListOnly = false,
    Guid? DispatcherId = null
) : IRequest<Result<List<ContractorRecommendationDto>>>;
```

### Step 2: Create Query Handler

**File:** `backend/SmartScheduler.Application/Queries/{Feature}/{Action}QueryHandler.cs`

**Template:**

```csharp
using MediatR;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Interfaces;

namespace SmartScheduler.Application.Queries.Contractors;

public class GetContractorRecommendationsQueryHandler
    : IRequestHandler<GetContractorRecommendationsQuery, Result<List<ContractorRecommendationDto>>>
{
    private readonly IContractorRepository _contractorRepository;
    private readonly IScoringEngine _scoringEngine;
    private readonly IDistanceService _distanceService;
    private readonly ILogger<GetContractorRecommendationsQueryHandler> _logger;

    public GetContractorRecommendationsQueryHandler(
        IContractorRepository contractorRepository,
        IScoringEngine scoringEngine,
        IDistanceService distanceService,
        ILogger<GetContractorRecommendationsQueryHandler> logger)
    {
        _contractorRepository = contractorRepository;
        _scoringEngine = scoringEngine;
        _distanceService = distanceService;
        _logger = logger;
    }

    public async Task<Result<List<ContractorRecommendationDto>>> Handle(
        GetContractorRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get contractors (filtered by dispatcher list if requested)
            var contractors = await _contractorRepository.GetActiveContractorsAsync(
                request.JobType,
                request.ContractorListOnly ? request.DispatcherId : null,
                cancellationToken
            );

            if (!contractors.Any())
            {
                return Result<List<ContractorRecommendationDto>>.Success(new List<ContractorRecommendationDto>());
            }

            // 2. Calculate scores for each contractor
            var recommendations = new List<ContractorRecommendationDto>();

            foreach (var contractor in contractors)
            {
                var score = await _scoringEngine.CalculateScoreAsync(
                    contractor,
                    request.Latitude,
                    request.Longitude,
                    request.DesiredDateTime,
                    cancellationToken
                );

                var distance = await _distanceService.GetDistanceAsync(
                    request.Latitude,
                    request.Longitude,
                    contractor.Latitude,
                    contractor.Longitude,
                    cancellationToken
                );

                recommendations.Add(new ContractorRecommendationDto
                {
                    Contractor = contractor.ToDto(),
                    Score = score.TotalScore,
                    AvailabilityScore = score.AvailabilityScore,
                    RatingScore = score.RatingScore,
                    DistanceScore = score.DistanceScore,
                    DistanceMiles = distance.DistanceMiles,
                    TravelTimeMinutes = distance.TravelTimeMinutes,
                    SuggestedTimeSlot = request.DesiredDateTime
                });
            }

            // 3. Return top 5 recommendations sorted by score
            var topRecommendations = recommendations
                .OrderByDescending(r => r.Score)
                .Take(5)
                .ToList();

            _logger.LogInformation(
                "Generated {Count} recommendations for job {JobId}",
                topRecommendations.Count,
                request.JobId
            );

            return Result<List<ContractorRecommendationDto>>.Success(topRecommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for job {JobId}", request.JobId);
            return Result<List<ContractorRecommendationDto>>.Failure(
                "RECOMMENDATION_ERROR",
                "Failed to generate recommendations"
            );
        }
    }
}
```

### Step 3: Add Controller Endpoint

**File:** `backend/SmartScheduler.API/Controllers/DispatcherController.cs`

```csharp
/// <summary>
/// Get contractor recommendations for a job
/// </summary>
[HttpPost("recommendations")]
[ProducesResponseType(typeof(List<ContractorRecommendationDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetRecommendations(
    [FromBody] GetRecommendationsRequest request,
    CancellationToken cancellationToken)
{
    var dispatcherId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());

    var query = new GetContractorRecommendationsQuery(
        request.JobId,
        request.JobType,
        request.Latitude,
        request.Longitude,
        request.DesiredDateTime,
        request.ContractorListOnly,
        dispatcherId
    );

    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
        return Ok(result.Data);
    }

    return StatusCode(500, new ErrorResponse
    {
        Code = result.ErrorCode,
        Message = result.ErrorMessage,
        Timestamp = DateTime.UtcNow,
        RequestId = HttpContext.TraceIdentifier
    });
}

public record GetRecommendationsRequest(
    Guid JobId,
    string JobType,
    decimal Latitude,
    decimal Longitude,
    DateTime DesiredDateTime,
    bool ContractorListOnly = false
);
```

**Implementation Checklist:**

- [ ] Create Query class in `Application/Queries/{Feature}/`
- [ ] Create QueryHandler with read logic (no data modification)
- [ ] Add controller endpoint (GET or POST depending on complexity)
- [ ] Add frontend service method
- [ ] Ensure query is performant (use indexes, caching if needed)
- [ ] Add unit tests for query handler
- [ ] Test with realistic data volumes

---

## 20.3 Repository Implementation Guide

**When to use:** Every domain entity needs a repository for data access

**Step-by-Step Process:**

### Step 1: Define Repository Interface

**File:** `backend/SmartScheduler.Domain/Interfaces/I{Entity}Repository.cs`

**Template:**

```csharp
namespace SmartScheduler.Domain.Interfaces;

public interface IContractorRepository
{
    Task<Contractor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Contractor>> GetActiveContractorsAsync(
        string? jobType = null,
        Guid? dispatcherId = null,
        CancellationToken cancellationToken = default
    );
    Task<Contractor> CreateAsync(Contractor contractor, CancellationToken cancellationToken = default);
    Task UpdateAsync(Contractor contractor, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Step 2: Implement Repository

**File:** `backend/SmartScheduler.Infrastructure/Repositories/{Entity}Repository.cs`

**Template:**

```csharp
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Interfaces;
using SmartScheduler.Infrastructure.Data;

namespace SmartScheduler.Infrastructure.Repositories;

public class ContractorRepository : IContractorRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContractorRepository> _logger;

    public ContractorRepository(
        ApplicationDbContext context,
        ILogger<ContractorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Contractor?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Contractors
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Contractor>> GetActiveContractorsAsync(
        string? jobType = null,
        Guid? dispatcherId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contractors
            .Include(c => c.User)
            .Where(c => c.IsActive);

        // Filter by job type if specified
        if (!string.IsNullOrEmpty(jobType))
        {
            query = query.Where(c => c.TradeType == jobType);
        }

        // Filter by dispatcher's contractor list if specified
        if (dispatcherId.HasValue)
        {
            query = query.Where(c => _context.DispatcherContractorLists
                .Any(dcl => dcl.DispatcherId == dispatcherId.Value && dcl.ContractorId == c.Id));
        }

        return await query
            .OrderByDescending(c => c.AverageRating)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contractor> CreateAsync(
        Contractor contractor,
        CancellationToken cancellationToken = default)
    {
        var entry = await _context.Contractors.AddAsync(contractor, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task UpdateAsync(
        Contractor contractor,
        CancellationToken cancellationToken = default)
    {
        _context.Contractors.Update(contractor);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Contractors
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Step 3: Register Repository in DI

**File:** `backend/SmartScheduler.API/Program.cs` or `Infrastructure/DependencyInjection.cs`

```csharp
// Register repositories
builder.Services.AddScoped<IContractorRepository, ContractorRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
// ... other repositories
```

**Implementation Checklist:**

- [ ] Define interface in `Domain/Interfaces/`
- [ ] Implement repository in `Infrastructure/Repositories/`
- [ ] Use `Include()` for related entities (avoid N+1 queries)
- [ ] All methods use `async`/`await`
- [ ] All methods accept `CancellationToken`
- [ ] Use proper indexes (defined in DbContext)
- [ ] Register in DI container
- [ ] Write unit tests with in-memory database

**Common Pitfalls:**

- ❌ **Don't** call `SaveChanges()` in every method—let the handler control transactions
- ❌ **Don't** use `.ToList()` on large datasets—use pagination
- ❌ **Don't** load entire related entities if only IDs needed
- ✅ **Do** use `.AsNoTracking()` for read-only queries (performance)

---

## 20.4 SignalR Event Handler Implementation Guide

**When to use:** Real-time notifications to users (job assigned, contractor accepted, status updates)

**Step-by-Step Process:**

### Step 1: Define Domain Event

**File:** `backend/SmartScheduler.Domain/Events/{EventName}.cs`

**Template:**

```csharp
namespace SmartScheduler.Domain.Events;

public record JobAssignedEvent : IDomainEvent
{
    public Guid JobId { get; init; }
    public Guid ContractorId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid AssignmentId { get; init; }
    public DateTime OccurredAt { get; init; }
}
```

### Step 2: Create Event Handler

**File:** `backend/SmartScheduler.Application/EventHandlers/{EventName}Handler.cs`

**Template:**

```csharp
using MediatR;
using Microsoft.AspNetCore.SignalR;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.SignalR;

namespace SmartScheduler.Application.EventHandlers;

public class JobAssignedEventHandler : INotificationHandler<JobAssignedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IContractorRepository _contractorRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobAssignedEventHandler> _logger;

    public JobAssignedEventHandler(
        IHubContext<NotificationHub> hubContext,
        IContractorRepository contractorRepository,
        IJobRepository jobRepository,
        ILogger<JobAssignedEventHandler> logger)
    {
        _hubContext = hubContext;
        _contractorRepository = contractorRepository;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task Handle(JobAssignedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get full job and contractor details for notification
            var job = await _jobRepository.GetByIdAsync(notification.JobId, cancellationToken);
            var contractor = await _contractorRepository.GetByIdAsync(notification.ContractorId, cancellationToken);

            if (job == null || contractor == null)
            {
                _logger.LogWarning("Job or contractor not found for JobAssignedEvent");
                return;
            }

            // Notify contractor
            await _hubContext.Clients
                .Group($"contractor-{notification.ContractorId}")
                .SendAsync("JobAssigned", new
                {
                    jobId = notification.JobId,
                    assignmentId = notification.AssignmentId,
                    jobType = job.JobType,
                    location = job.Location,
                    desiredDateTime = job.DesiredDateTime,
                    estimatedDurationHours = job.EstimatedDurationHours,
                    message = $"New job assigned: {job.JobType} at {job.Location}"
                }, cancellationToken);

            _logger.LogInformation("Notified contractor {ContractorId} of job assignment", notification.ContractorId);

            // Notify customer
            await _hubContext.Clients
                .Group($"customer-{notification.CustomerId}")
                .SendAsync("ContractorAssigned", new
                {
                    jobId = notification.JobId,
                    contractorId = notification.ContractorId,
                    contractorName = contractor.Name,
                    contractorRating = contractor.AverageRating,
                    contractorPhone = contractor.PhoneNumber,
                    message = $"{contractor.Name} has been assigned to your job"
                }, cancellationToken);

            _logger.LogInformation("Notified customer {CustomerId} of contractor assignment", notification.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobAssignedEvent for job {JobId}", notification.JobId);
            // Don't throw - SignalR notifications are non-critical
        }
    }
}
```

### Step 3: Define SignalR Hub

**File:** `backend/SmartScheduler.Infrastructure/SignalR/NotificationHub.cs`

**Template:**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartScheduler.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        var role = Context.User?.FindFirst("role")?.Value;

        if (userId != null && role != null)
        {
            // Add user to their role-specific group
            var groupName = $"{role.ToLower()}-{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "User {UserId} ({Role}) connected to SignalR with connection {ConnectionId}",
                userId,
                role,
                Context.ConnectionId
            );
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;

        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "User {UserId} disconnected from SignalR with error",
                userId
            );
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from SignalR", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
```

### Step 4: Configure SignalR in Program.cs

**File:** `backend/SmartScheduler.API/Program.cs`

```csharp
// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ... after app is built ...

// Map SignalR hub
app.MapHub<NotificationHub>("/hubs/notifications");
```

### Step 5: Frontend SignalR Client Setup

**File:** `frontend/src/hooks/useSignalR.ts`

**Template:**

```typescript
import { useEffect, useState, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { useAuth } from "./useAuth";

export const useSignalR = () => {
  const { accessToken } = useAuth();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    if (!accessToken) return;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_SIGNALR_HUB_URL}`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, then 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    newConnection.onreconnecting((error) => {
      console.warn("SignalR reconnecting...", error);
      setIsConnected(false);
    });

    newConnection.onreconnected((connectionId) => {
      console.log("SignalR reconnected:", connectionId);
      setIsConnected(true);
    });

    newConnection.onclose((error) => {
      console.error("SignalR connection closed:", error);
      setIsConnected(false);
    });

    newConnection
      .start()
      .then(() => {
        console.log("SignalR connected");
        setIsConnected(true);
        setConnection(newConnection);
      })
      .catch((error) => {
        console.error("SignalR connection error:", error);
      });

    return () => {
      newConnection.stop();
    };
  }, [accessToken]);

  const subscribe = useCallback(
    (eventName: string, callback: (...args: any[]) => void) => {
      if (connection) {
        connection.on(eventName, callback);
        return () => {
          connection.off(eventName, callback);
        };
      }
      return () => {};
    },
    [connection]
  );

  return { connection, isConnected, subscribe };
};
```

### Step 6: Use SignalR in Component

**File:** `frontend/src/features/contractor/AssignmentNotifications.tsx`

**Template:**

```typescript
import React, { useEffect } from "react";
import { toast } from "react-hot-toast";
import { useSignalR } from "@/hooks/useSignalR";
import { useQueryClient } from "@tanstack/react-query";

export const AssignmentNotifications: React.FC = () => {
  const { subscribe, isConnected } = useSignalR();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!isConnected) return;

    // Subscribe to job assignment notifications
    const unsubscribe = subscribe("JobAssigned", (data: any) => {
      toast.success(data.message, {
        duration: 10000,
        icon: "📋",
      });

      // Invalidate assignments query to refresh the list
      queryClient.invalidateQueries(["assignments"]);
    });

    return unsubscribe;
  }, [subscribe, isConnected, queryClient]);

  return null; // This is a notification handler component
};
```

**Implementation Checklist:**

- [ ] Define domain event in `Domain/Events/`
- [ ] Create event handler in `Application/EventHandlers/`
- [ ] Implement `INotificationHandler<TEvent>`
- [ ] Use `IHubContext<NotificationHub>` to send messages
- [ ] Send to specific groups: `contractor-{id}`, `customer-{id}`, `dispatcher-{id}`
- [ ] Configure SignalR in `Program.cs`
- [ ] Create frontend hook `useSignalR.ts`
- [ ] Subscribe to events in components
- [ ] Handle reconnection gracefully
- [ ] Test with multiple connected clients

**Common Pitfalls:**

- ❌ **Don't** throw exceptions in event handlers (non-critical, log instead)
- ❌ **Don't** send large payloads via SignalR (send IDs, fetch details via API)
- ❌ **Don't** forget to unsubscribe in `useEffect` cleanup
- ✅ **Do** add users to groups on connection
- ✅ **Do** implement automatic reconnection with exponential backoff

---

## 20.5 Complete Feature Implementation Checklist

**Feature:** Job Cancellation (End-to-End Example)

**Estimated Time:** 70 minutes (30 backend, 20 frontend, 10 notifications, 10 testing)

### Backend (30 minutes)

**1. Update Domain Entity (5 min)**

File: `backend/SmartScheduler.Domain/Entities/Job.cs`

```csharp
public class Job
{
    // ... existing properties ...

    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public void Cancel(string reason)
    {
        if (Status == JobStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed job");
        }

        Status = JobStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        // Raise domain event
        AddDomainEvent(new JobCancelledEvent
        {
            JobId = Id,
            CustomerId = CustomerId,
            ContractorId = AssignedContractorId,
            CancelledAt = CancelledAt.Value,
            Reason = reason
        });
    }
}
```

**2. Create Migration (2 min)**

```bash
cd backend/SmartScheduler.Infrastructure
dotnet ef migrations add AddJobCancellation --startup-project ../SmartScheduler.API
dotnet ef database update --startup-project ../SmartScheduler.API
```

**3. Create Command (5 min)**

File: `backend/SmartScheduler.Application/Commands/Jobs/CancelJobCommand.cs`

```csharp
public record CancelJobCommand(
    Guid JobId,
    string Reason,
    Guid RequesterId
) : IRequest<Result<JobDto>>;

public class CancelJobCommandValidator : AbstractValidator<CancelJobCommand>
{
    public CancelJobCommandValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RequesterId).NotEmpty();
    }
}
```

**4. Create Command Handler (8 min)**

File: `backend/SmartScheduler.Application/Commands/Jobs/CancelJobCommandHandler.cs`

```csharp
public class CancelJobCommandHandler : IRequestHandler<CancelJobCommand, Result<JobDto>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IEventPublisher _eventPublisher;

    public async Task<Result<JobDto>> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            return Result<JobDto>.Failure("JOB_NOT_FOUND", "Job not found");
        }

        // Authorize: Only dispatcher or customer can cancel
        if (job.CustomerId != request.RequesterId && /* check if requester is dispatcher */)
        {
            return Result<JobDto>.Failure("UNAUTHORIZED", "You are not authorized to cancel this job");
        }

        job.Cancel(request.Reason);
        await _jobRepository.UpdateAsync(job, cancellationToken);

        return Result<JobDto>.Success(job.ToDto());
    }
}
```

**5. Add Controller Endpoint (5 min)**

File: `backend/SmartScheduler.API/Controllers/DispatcherController.cs`

```csharp
[HttpPost("jobs/{jobId}/cancel")]
[Authorize(Roles = "Dispatcher")]
public async Task<IActionResult> CancelJob(
    Guid jobId,
    [FromBody] CancelJobRequest request,
    CancellationToken cancellationToken)
{
    var dispatcherId = Guid.Parse(User.FindFirst("sub")!.Value);
    var command = new CancelJobCommand(jobId, request.Reason, dispatcherId);

    var result = await _mediator.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result);
}
```

**6. Add Unit Tests (5 min)**

File: `backend/SmartScheduler.Tests/Application/Commands/CancelJobCommandHandlerTests.cs`

```csharp
[Fact]
public async Task Handle_ValidCommand_CancelsJob()
{
    // Arrange
    var job = new Job { Id = Guid.NewGuid(), Status = JobStatus.Assigned };
    _jobRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(job);

    var command = new CancelJobCommand(job.Id, "Customer request", Guid.NewGuid());

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    job.Status.Should().Be(JobStatus.Cancelled);
}
```

### Frontend (20 minutes)

**7. Update Job Interface (2 min)**

File: `frontend/src/types/job.ts`

```typescript
export interface Job {
  // ... existing fields ...
  cancelledAt: string | null;
  cancellationReason: string | null;
}
```

**8. Add Service Method (3 min)**

File: `frontend/src/services/dispatcherService.ts`

```typescript
async cancelJob(jobId: string, reason: string): Promise<Job> {
  const response = await apiClient.post<Job>(
    `/dispatcher/jobs/${jobId}/cancel`,
    { reason }
  );
  return response.data;
}
```

**9. Add Cancel Button Component (10 min)**

File: `frontend/src/features/dispatcher/JobCancelButton.tsx`

```typescript
import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { toast } from "react-hot-toast";
import { dispatcherService } from "@/services/dispatcherService";

interface JobCancelButtonProps {
  jobId: string;
  onCancelSuccess: () => void;
}

export const JobCancelButton: React.FC<JobCancelButtonProps> = ({
  jobId,
  onCancelSuccess,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [reason, setReason] = useState("");
  const [isCancelling, setIsCancelling] = useState(false);

  const handleCancel = async () => {
    if (!reason.trim()) {
      toast.error("Please provide a cancellation reason");
      return;
    }

    setIsCancelling(true);
    try {
      await dispatcherService.cancelJob(jobId, reason);
      toast.success("Job cancelled successfully");
      setIsOpen(false);
      onCancelSuccess();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Failed to cancel job");
    } finally {
      setIsCancelling(false);
    }
  };

  return (
    <>
      <Button variant="destructive" onClick={() => setIsOpen(true)}>
        Cancel Job
      </Button>

      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel Job</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <p className="text-sm text-gray-600">
              Please provide a reason for cancellation. This will be shared with
              the customer and contractor.
            </p>

            <Textarea
              placeholder="Cancellation reason..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={4}
            />

            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => setIsOpen(false)}
                disabled={isCancelling}
              >
                Nevermind
              </Button>
              <Button
                variant="destructive"
                onClick={handleCancel}
                loading={isCancelling}
              >
                {isCancelling ? "Cancelling..." : "Confirm Cancellation"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
```

**10. Update Job List to Show Cancelled Status (5 min)**

File: `frontend/src/features/dispatcher/JobList.tsx`

```typescript
// Add cancelled status badge
{
  job.status === "Cancelled" && (
    <Badge variant="destructive">Cancelled: {job.cancellationReason}</Badge>
  );
}
```

### Notifications (10 minutes)

**11. Define Domain Event (2 min)**

File: `backend/SmartScheduler.Domain/Events/JobCancelledEvent.cs`

```csharp
public record JobCancelledEvent : IDomainEvent
{
    public Guid JobId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? ContractorId { get; init; }
    public DateTime CancelledAt { get; init; }
    public string Reason { get; init; } = string.Empty;
}
```

**12. Create SignalR Event Handler (4 min)**

File: `backend/SmartScheduler.Application/EventHandlers/JobCancelledEventHandler.cs`

```csharp
public class JobCancelledEventHandler : INotificationHandler<JobCancelledEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task Handle(JobCancelledEvent notification, CancellationToken cancellationToken)
    {
        // Notify customer
        await _hubContext.Clients
            .Group($"customer-{notification.CustomerId}")
            .SendAsync("JobCancelled", new
            {
                jobId = notification.JobId,
                reason = notification.Reason,
                message = $"Your job has been cancelled: {notification.Reason}"
            }, cancellationToken);

        // Notify contractor if assigned
        if (notification.ContractorId.HasValue)
        {
            await _hubContext.Clients
                .Group($"contractor-{notification.ContractorId.Value}")
                .SendAsync("JobCancelled", new
                {
                    jobId = notification.JobId,
                    reason = notification.Reason,
                    message = $"Job has been cancelled: {notification.Reason}"
                }, cancellationToken);
        }
    }
}
```

**13. Create Email Event Handler (4 min)**

File: `backend/SmartScheduler.Application/EventHandlers/JobCancelledEmailHandler.cs`

```csharp
public class JobCancelledEmailHandler : INotificationHandler<JobCancelledEvent>
{
    private readonly IEmailService _emailService;

    public async Task Handle(JobCancelledEvent notification, CancellationToken cancellationToken)
    {
        // Send email to customer
        await _emailService.SendEmailAsync(
            notification.CustomerId,
            "JobCancelled",
            new { jobId = notification.JobId, reason = notification.Reason },
            cancellationToken
        );

        // Send email to contractor if assigned
        if (notification.ContractorId.HasValue)
        {
            await _emailService.SendEmailAsync(
                notification.ContractorId.Value,
                "JobCancelled",
                new { jobId = notification.JobId, reason = notification.Reason },
                cancellationToken
            );
        }
    }
}
```

### Testing (10 minutes)

**14. Integration Test (5 min)**

File: `backend/SmartScheduler.Tests/Integration/JobCancellationTests.cs`

```csharp
[Fact]
public async Task CancelJob_NotifiesAllParties()
{
    // Arrange
    var job = await CreateTestJobAsync();
    var contractor = await CreateTestContractorAsync();
    await AssignJobAsync(job.Id, contractor.Id);

    // Act
    var response = await _client.PostAsync(
        $"/api/v1/dispatcher/jobs/{job.Id}/cancel",
        JsonContent.Create(new { reason = "Customer request" })
    );

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var updatedJob = await GetJobAsync(job.Id);
    updatedJob.Status.Should().Be(JobStatus.Cancelled);
    updatedJob.CancellationReason.Should().Be("Customer request");

    // Verify SignalR notification was sent (mock verification)
    // Verify email was sent (mock verification)
}
```

**15. E2E Test (5 min)**

File: `frontend/e2e/job-cancellation.spec.ts`

```typescript
import { test, expect } from "@playwright/test";

test("dispatcher can cancel job", async ({ page }) => {
  // Login as dispatcher
  await page.goto("/login");
  await page.fill('[name="email"]', "dispatcher@test.com");
  await page.fill('[name="password"]', "password123");
  await page.click('button[type="submit"]');

  // Navigate to job list
  await page.goto("/dispatcher/jobs");

  // Click cancel button on first job
  await page.click(
    '[data-testid="job-card"]:first-child button:has-text("Cancel Job")'
  );

  // Fill cancellation reason
  await page.fill(
    'textarea[placeholder="Cancellation reason..."]',
    "Customer changed plans"
  );

  // Confirm cancellation
  await page.click('button:has-text("Confirm Cancellation")');

  // Verify success toast
  await expect(page.locator("text=Job cancelled successfully")).toBeVisible();

  // Verify job shows cancelled status
  await expect(
    page.locator('[data-testid="job-card"]:first-child')
  ).toContainText("Cancelled");
});
```

**Complete Feature Checklist:**

- [x] Update Domain Entity with new properties and business logic
- [x] Create database migration
- [x] Create CQRS Command and Validator
- [x] Create Command Handler with authorization
- [x] Add Controller endpoint with proper authorization
- [x] Add unit tests for command handler
- [x] Update TypeScript interfaces
- [x] Add frontend service method
- [x] Create UI component with confirmation dialog
- [x] Update existing components to show new status
- [x] Define domain event
- [x] Create SignalR event handler for real-time notifications
- [x] Create email event handler
- [x] Write integration test (backend)
- [x] Write E2E test (Playwright)
- [x] Test in development environment

---

## 20.6 Common Pitfalls and Best Practices

### Backend Pitfalls

**❌ DON'T:**

1. **Access DbContext directly from Controllers**

   ```csharp
   // BAD
   public class DispatcherController : ControllerBase
   {
       private readonly ApplicationDbContext _context;

       public IActionResult GetJobs()
       {
           var jobs = _context.Jobs.ToList(); // ❌ Violates Clean Architecture
       }
   }
   ```

   **✅ DO:**

   ```csharp
   // GOOD
   public class DispatcherController : ControllerBase
   {
       private readonly IMediator _mediator;

       public async Task<IActionResult> GetJobs()
       {
           var query = new GetJobsQuery();
           var result = await _mediator.Send(query); // ✅ Uses CQRS
       }
   }
   ```

2. **Forget to publish domain events**

   ```csharp
   // BAD
   public async Task Handle(AssignJobCommand request, CancellationToken cancellationToken)
   {
       var job = await _jobRepository.GetByIdAsync(request.JobId);
       job.AssignToContractor(request.ContractorId);
       await _jobRepository.UpdateAsync(job);
       // ❌ Forgot to publish JobAssignedEvent - no notifications sent!
   }
   ```

   **✅ DO:**

   ```csharp
   // GOOD
   public async Task Handle(AssignJobCommand request, CancellationToken cancellationToken)
   {
       var job = await _jobRepository.GetByIdAsync(request.JobId);
       job.AssignToContractor(request.ContractorId);
       await _jobRepository.UpdateAsync(job);
       await _eventPublisher.PublishAsync(new JobAssignedEvent { ... }); // ✅
   }
   ```

3. **Use local time instead of UTC**

   ```csharp
   // BAD
   var job = new Job { CreatedAt = DateTime.Now }; // ❌ Uses local time
   ```

   **✅ DO:**

   ```csharp
   // GOOD
   var job = new Job { CreatedAt = DateTime.UtcNow }; // ✅ Always UTC
   ```

4. **Load entire related entities when only IDs needed**

   ```csharp
   // BAD
   var jobs = await _context.Jobs
       .Include(j => j.Customer) // ❌ Loads entire Customer entity
       .Include(j => j.Contractor) // ❌ Loads entire Contractor entity
       .ToListAsync();
   ```

   **✅ DO:**

   ```csharp
   // GOOD - Only load what you need
   var jobs = await _context.Jobs
       .Select(j => new JobDto
       {
           Id = j.Id,
           CustomerName = j.Customer.Name, // ✅ Only name, not entire entity
           ContractorName = j.Contractor.Name
       })
       .ToListAsync();
   ```

5. **Forget cache invalidation after updates**

   ```csharp
   // BAD
   public async Task UpdateContractorRating(Guid contractorId, decimal newRating)
   {
       contractor.AverageRating = newRating;
       await _contractorRepository.UpdateAsync(contractor);
       // ❌ Forgot to invalidate cache - recommendations will use old rating!
   }
   ```

   **✅ DO:**

   ```csharp
   // GOOD
   public async Task UpdateContractorRating(Guid contractorId, decimal newRating)
   {
       contractor.AverageRating = newRating;
       await _contractorRepository.UpdateAsync(contractor);
       await _cache.RemoveAsync($"contractor-list"); // ✅ Invalidate cache
   }
   ```

### Frontend Pitfalls

**❌ DON'T:**

1. **Call API directly from components**

   ```typescript
   // BAD
   const JobList = () => {
     const fetchJobs = async () => {
       const response = await axios.get("/api/v1/dispatcher/jobs"); // ❌ Direct API call
     };
   };
   ```

   **✅ DO:**

   ```typescript
   // GOOD
   const JobList = () => {
     const { jobs, loading, error } = useJobs(); // ✅ Uses custom hook + service
   };
   ```

2. **Mutate state directly**

   ```typescript
   // BAD
   const [jobs, setJobs] = useState<Job[]>([]);

   const updateJob = (jobId: string) => {
     const job = jobs.find((j) => j.id === jobId);
     job.status = "Assigned"; // ❌ Direct mutation
     setJobs(jobs); // ❌ React won't detect change
   };
   ```

   **✅ DO:**

   ```typescript
   // GOOD
   const updateJob = (jobId: string) => {
     setJobs(
       jobs.map(
         (j) => (j.id === jobId ? { ...j, status: "Assigned" } : j) // ✅ Immutable update
       )
     );
   };
   ```

3. **Forget to unsubscribe from SignalR**

   ```typescript
   // BAD
   useEffect(() => {
     connection.on("JobAssigned", handleJobAssigned); // ❌ No cleanup
   }, [connection]);
   ```

   **✅ DO:**

   ```typescript
   // GOOD
   useEffect(() => {
     const unsubscribe = subscribe("JobAssigned", handleJobAssigned);
     return unsubscribe; // ✅ Cleanup on unmount
   }, [subscribe]);
   ```

4. **Don't handle loading states**

   ```typescript
   // BAD
   return (
     <div>
       {jobs.map((job) => (
         <JobCard key={job.id} job={job} />
       ))}
       {/* ❌ No loading state - blank screen while fetching */}
     </div>
   );
   ```

   **✅ DO:**

   ```typescript
   // GOOD
   if (loading) return <LoadingSpinner />;
   if (error) return <ErrorMessage message={error} />;

   return (
     <div>
       {jobs.map((job) => (
         <JobCard key={job.id} job={job} />
       ))}
     </div>
   );
   ```

5. **Forget ARIA attributes for accessibility**

   ```typescript
   // BAD
   <button onClick={handleClick}>X</button> // ❌ Screen readers hear "X button"
   ```

   **✅ DO:**

   ```typescript
   // GOOD
   <button onClick={handleClick} aria-label="Cancel job">
     X
   </button>
   ```

### Database Pitfalls

**❌ DON'T:**

1. **Forget indexes on foreign keys**

   ```sql
   -- BAD
   CREATE TABLE "Assignments" (
       "JobId" UUID NOT NULL REFERENCES "Jobs"("Id"),
       "ContractorId" UUID NOT NULL -- ❌ No index - slow queries
   );
   ```

   **✅ DO:**

   ```sql
   -- GOOD
   CREATE INDEX "IX_Assignments_ContractorId" ON "Assignments" ("ContractorId");
   CREATE INDEX "IX_Assignments_JobId" ON "Assignments" ("JobId");
   ```

2. **Use SELECT \* in production**

   ```csharp
   // BAD
   var contractors = await _context.Contractors.ToListAsync(); // ❌ Loads all columns
   ```

   **✅ DO:**

   ```csharp
   // GOOD
   var contractors = await _context.Contractors
       .Select(c => new ContractorListDto { Id = c.Id, Name = c.Name, Rating = c.AverageRating })
       .ToListAsync(); // ✅ Only necessary columns
   ```

---

## 20.7 Implementation Priority Guide

**Order of Implementation for New Features:**

1. **Domain Layer** (Foundation)

   - Update domain entities with new properties/methods
   - Add business logic to entity methods
   - Define domain events

2. **Database Layer** (Persistence)

   - Create EF Core migration
   - Run migration in development
   - Verify schema changes

3. **Application Layer** (Use Cases)

   - Create CQRS commands/queries
   - Implement handlers
   - Add validation rules

4. **Infrastructure Layer** (External Concerns)

   - Update repositories if needed
   - Create event handlers (SignalR, Email)

5. **API Layer** (HTTP Interface)

   - Add controller endpoints
   - Apply authorization attributes
   - Add Swagger documentation

6. **Frontend Services** (Data Access)

   - Add service methods
   - Handle errors gracefully

7. **Frontend UI** (User Interface)

   - Create/update components
   - Add forms, modals, dialogs
   - Handle loading/error states

8. **Testing** (Quality Assurance)
   - Unit tests (backend handlers)
   - Component tests (frontend)
   - Integration tests (API)
   - E2E tests (critical flows)

**Why This Order?**

- **Bottom-up approach:** Foundation (domain) before implementation details (UI)
- **Fail fast:** Domain logic errors caught early, not during UI development
- **Testability:** Each layer can be tested independently as it's built
- **Clear dependencies:** Each layer depends only on layers below it

---
