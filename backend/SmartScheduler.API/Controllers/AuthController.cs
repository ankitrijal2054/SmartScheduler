using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartScheduler.Application.Services;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Authentication controller for user login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;
    private readonly IGeocodingService _geocodingService;

    public AuthController(
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        ApplicationDbContext dbContext,
        ILogger<AuthController> logger,
        IGeocodingService geocodingService)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _passwordHashingService = passwordHashingService ?? throw new ArgumentNullException(nameof(passwordHashingService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
    }

    /// <summary>
    /// Authenticates a user with email and password, returns JWT and refresh token.
    /// </summary>
    /// <param name="request">Login credentials (email, password).</param>
    /// <returns>200 OK with JWT and refresh token, or 401 Unauthorized if credentials invalid.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find user by email (case-insensitive)
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid email or password", statusCode = 401 } });
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt with invalid password for email: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid email or password", statusCode = 401 } });
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "ACCOUNT_INACTIVE", message = "Account is inactive", statusCode = 401 } });
        }

        // Generate JWT and refresh token
        var tokenResponse = _jwtTokenService.GenerateToken(user);

        // Save refresh token to database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenResponse.RefreshToken!,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Refreshes the JWT token using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <returns>200 OK with new JWT token, or 401 Unauthorized if refresh token invalid/revoked.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find refresh token in database
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh attempt with invalid token");
            return Unauthorized(new { error = new { code = "INVALID_TOKEN", message = "Invalid refresh token", statusCode = 401 } });
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Refresh attempt with revoked token for user: {UserId}", refreshToken.UserId);
            return Unauthorized(new { error = new { code = "TOKEN_REVOKED", message = "Refresh token has been revoked", statusCode = 401 } });
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh attempt with expired token for user: {UserId}", refreshToken.UserId);
            return Unauthorized(new { error = new { code = "TOKEN_EXPIRED", message = "Refresh token expired", statusCode = 401 } });
        }

        // Generate new JWT
        var tokenResponse = _jwtTokenService.RefreshToken(refreshToken.User);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", refreshToken.UserId);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Creates a new user account with email, password, role, and profile information.
    /// </summary>
    /// <param name="request">Signup credentials including profile data.</param>
    /// <returns>201 Created with JWT and refresh token, or 400/409 if validation fails or email exists.</returns>
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate role-specific requirements
        if (request.Role == UserRole.Contractor)
        {
            if (string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Location is required for contractors", statusCode = 400 } });
            }
            if (!request.TradeType.HasValue)
            {
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "TradeType is required for contractors", statusCode = 400 } });
            }
            if (string.IsNullOrWhiteSpace(request.WorkingHoursStart) || string.IsNullOrWhiteSpace(request.WorkingHoursEnd))
            {
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Working hours are required for contractors", statusCode = 400 } });
            }
        }

        // Check if email already exists (case-insensitive)
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            _logger.LogWarning("Signup attempt with duplicate email: {Email}", request.Email);
            return Conflict(new { error = new { code = "EMAIL_EXISTS", message = "An account with this email already exists", statusCode = 409 } });
        }

        // Hash password
        var passwordHash = _passwordHashingService.HashPassword(request.Password);

        // Create new user and role-specific entity in a transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Create new user
            var user = new User
            {
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Create role-specific entity with profile data
            if (request.Role == UserRole.Contractor)
            {
                // Parse working hours
                if (!TimeSpan.TryParse(request.WorkingHoursStart, out var startTime))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid WorkingHoursStart format. Use HH:mm", statusCode = 400 } });
                }
                if (!TimeSpan.TryParse(request.WorkingHoursEnd, out var endTime))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid WorkingHoursEnd format. Use HH:mm", statusCode = 400 } });
                }
                if (endTime <= startTime)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "WorkingHoursEnd must be after WorkingHoursStart", statusCode = 400 } });
                }

                // Geocode location with error handling
                decimal latitude = 0;
                decimal longitude = 0;
                try
                {
                    var (lat, lon) = await _geocodingService.GeocodeAddressAsync(request.Location!);
                    latitude = (decimal)lat;
                    longitude = (decimal)lon;
                    _logger.LogInformation("Geocoded location for contractor: {Location} -> ({Latitude}, {Longitude})", 
                        request.Location, latitude, longitude);
                }
                catch (Exception geocodeEx)
                {
                    _logger.LogWarning(geocodeEx, "Geocoding failed for location: {Location}. Using default coordinates (0, 0)", request.Location);
                    // Continue with default coordinates - don't fail signup
                }

                // Ensure PhoneNumber is not empty (required field)
                var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "000-000-0000" : request.PhoneNumber.Trim();

                var contractor = new Contractor
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Name = request.Name.Trim(),
                    PhoneNumber = phoneNumber,
                    Location = request.Location!.Trim(),
                    Latitude = latitude,
                    Longitude = longitude,
                    TradeType = request.TradeType!.Value,
                    WorkingHoursStart = startTime,
                    WorkingHoursEnd = endTime,
                    IsActive = true,
                    ReviewCount = 0,
                    TotalJobsCompleted = 0
                };
                _dbContext.Contractors.Add(contractor);
                _logger.LogInformation("Contractor profile created: UserId={UserId}, Name={Name}, TradeType={TradeType}, Location={Location}, PhoneNumber={PhoneNumber}", 
                    user.Id, contractor.Name, contractor.TradeType, contractor.Location, contractor.PhoneNumber);
            }
            else if (request.Role == UserRole.Customer)
            {
                // Ensure required fields are not empty
                var customerPhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "000-000-0000" : request.PhoneNumber.Trim();
                var customerLocation = string.IsNullOrWhiteSpace(request.Location) ? "Not provided" : request.Location.Trim();

                var customer = new Customer
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Name = request.Name.Trim(),
                    PhoneNumber = customerPhoneNumber,
                    Location = customerLocation
                };
                _dbContext.Customers.Add(customer);
                _logger.LogInformation("Customer profile created: UserId={UserId}, Name={Name}, PhoneNumber={PhoneNumber}, Location={Location}", 
                    user.Id, customer.Name, customer.PhoneNumber, customer.Location);
            }
            // Dispatcher role doesn't require a specific entity, just User record
            else if (request.Role == UserRole.Dispatcher)
            {
                _logger.LogInformation("Dispatcher account created: Name={Name}", request.Name);
            }

            await _dbContext.SaveChangesAsync();
            
            // Verify contractor was saved
            if (request.Role == UserRole.Contractor)
            {
                var savedContractor = await _dbContext.Contractors.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (savedContractor == null)
                {
                    _logger.LogError("Contractor profile was not saved after SaveChangesAsync. UserId={UserId}", user.Id);
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { error = new { code = "SIGNUP_FAILED", message = "Failed to create contractor profile. Please try again.", statusCode = 500 } });
                }
                _logger.LogInformation("Contractor profile verified in database: ContractorId={ContractorId}, UserId={UserId}", savedContractor.Id, user.Id);
            }
            
            await transaction.CommitAsync();

            // Generate JWT and refresh token
            var tokenResponse = _jwtTokenService.GenerateToken(user);

            // Save refresh token to database
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenResponse.RefreshToken!,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User signed up successfully: {UserId} with role {Role}", user.Id, user.Role);

            return CreatedAtAction(nameof(Signup), tokenResponse);
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            _logger.LogError(dbEx, "Database error during signup for email: {Email}. InnerException: {InnerException}", 
                request.Email, dbEx.InnerException?.Message);
            
            // Check for specific constraint violations
            if (dbEx.InnerException?.Message.Contains("duplicate") == true || 
                dbEx.InnerException?.Message.Contains("unique") == true)
            {
                return Conflict(new { error = new { code = "DUPLICATE_ENTRY", message = "A record with this information already exists", statusCode = 409 } });
            }
            
            return StatusCode(500, new { error = new { code = "SIGNUP_FAILED", message = "Database error occurred. Please try again.", statusCode = 500 } });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during signup for email: {Email}. ExceptionType: {ExceptionType}, Message: {Message}", 
                request.Email, ex.GetType().Name, ex.Message);
            return StatusCode(500, new { error = new { code = "SIGNUP_FAILED", message = "Failed to create account. Please try again.", statusCode = 500 } });
        }
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    /// <param name="request">Refresh token to revoke.</param>
    /// <returns>200 OK if logout successful.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find refresh token
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null)
        {
            // Idempotent response: treat missing token as already logged out
            _logger.LogWarning("Logout attempt with non-existent token");
            return Ok(new { message = "Logged out successfully" });
        }

        // Revoke token
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User logged out successfully: {UserId}", refreshToken.UserId);

        return Ok(new { message = "Logged out successfully" });
    }
}

