using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Infrastructure.Persistence;

/// <summary>
/// Database seeder for populating test data.
/// Provides idempotent seed method to populate the database with sample data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with sample data.
    /// Safe to call multiple times - checks for existing data first.
    /// </summary>
    public static void Seed(ApplicationDbContext context)
    {
        // Check if database already has data (idempotent check)
        if (context.Users.Any())
        {
            return; // Database already seeded
        }

        // Create users
        var dispatcherUser = new User
        {
            Email = "dispatcher@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dispatcher123!"),
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var customerUser = new User
        {
            Email = "customer@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor1User = new User
        {
            Email = "contractor1@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor1!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor2User = new User
        {
            Email = "contractor2@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor2!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor3User = new User
        {
            Email = "contractor3@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor3!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        context.Users.AddRange(dispatcherUser, customerUser, contractor1User, contractor2User, contractor3User);
        context.SaveChanges();

        // Create contractor profiles
        var contractor1 = new Contractor
        {
            UserId = contractor1User.Id,
            Name = "John Plumber",
            PhoneNumber = "(555) 123-4567",
            Location = "123 Main St, Springfield, IL",
            Latitude = 39.7817m,
            Longitude = -89.6501m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00:00"),
            AverageRating = 4.5m,
            ReviewCount = 12,
            TotalJobsCompleted = 45,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var contractor2 = new Contractor
        {
            UserId = contractor2User.Id,
            Name = "Sarah Electrician",
            PhoneNumber = "(555) 234-5678",
            Location = "456 Oak Ave, Springfield, IL",
            Latitude = 39.7850m,
            Longitude = -89.6400m,
            TradeType = TradeType.Electrical,
            WorkingHoursStart = TimeSpan.Parse("07:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("16:00:00"),
            AverageRating = 4.8m,
            ReviewCount = 28,
            TotalJobsCompleted = 67,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var contractor3 = new Contractor
        {
            UserId = contractor3User.Id,
            Name = "Mike HVAC",
            PhoneNumber = "(555) 345-6789",
            Location = "789 Elm Rd, Springfield, IL",
            Latitude = 39.7700m,
            Longitude = -89.6550m,
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.Parse("06:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("15:00:00"),
            AverageRating = 4.2m,
            ReviewCount = 18,
            TotalJobsCompleted = 52,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Contractors.AddRange(contractor1, contractor2, contractor3);
        context.SaveChanges();

        // Create customer profile
        var customer = new Customer
        {
            UserId = customerUser.Id,
            Name = "Jane Homeowner",
            PhoneNumber = "(555) 987-6543",
            Location = "321 Pine St, Springfield, IL",
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        // Create dispatcher contractor list (favorites)
        var dispatcherList1 = new DispatcherContractorList
        {
            DispatcherId = dispatcherUser.Id,
            ContractorId = contractor1.Id,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var dispatcherList2 = new DispatcherContractorList
        {
            DispatcherId = dispatcherUser.Id,
            ContractorId = contractor2.Id,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.DispatcherContractorLists.AddRange(dispatcherList1, dispatcherList2);
        context.SaveChanges();

        // Create sample jobs
        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(2),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leaking kitchen faucet",
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job2 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(3),
            EstimatedDurationHours = 3m,
            Description = "Install new ceiling light fixture",
            Status = JobStatus.Assigned,
            AssignedContractorId = contractor2.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job3 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.HVAC,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 1.5m,
            Description = "HVAC system inspection and maintenance",
            Status = JobStatus.InProgress,
            AssignedContractorId = contractor3.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job4 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(-2),
            EstimatedDurationHours = 4m,
            Description = "Replace bathroom plumbing fixtures",
            Status = JobStatus.Completed,
            AssignedContractorId = contractor1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        var job5 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(-5),
            EstimatedDurationHours = 2m,
            Description = "Rewire outlets in home office",
            Status = JobStatus.Cancelled,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        context.Jobs.AddRange(job1, job2, job3, job4, job5);
        context.SaveChanges();

        // Create assignments
        var assignment1 = new Assignment
        {
            JobId = job2.Id,
            ContractorId = contractor2.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Status = AssignmentStatus.Accepted,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assignment2 = new Assignment
        {
            JobId = job3.Id,
            ContractorId = contractor3.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
            StartedAt = DateTime.UtcNow,
            Status = AssignmentStatus.InProgress,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assignment3 = new Assignment
        {
            JobId = job4.Id,
            ContractorId = contractor1.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-2),
            AcceptedAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
            StartedAt = DateTime.UtcNow.AddDays(-2).AddHours(2),
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            Status = AssignmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Assignments.AddRange(assignment1, assignment2, assignment3);
        context.SaveChanges();

        // Create reviews
        var review1 = new Review
        {
            JobId = job4.Id,
            ContractorId = contractor1.Id,
            CustomerId = customer.Id,
            Rating = 5,
            Comment = "Excellent work! Very professional and on time.",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var review2 = new Review
        {
            JobId = job2.Id,
            ContractorId = contractor2.Id,
            CustomerId = customer.Id,
            Rating = 4,
            Comment = "Great service, would hire again.",
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.AddRange(review1, review2);
        context.SaveChanges();
    }

    /// <summary>
    /// Seeds 50 contractor profiles with Austin, TX addresses.
    /// Creates users with email format: testcontractor1@testemail.com through testcontractor50@testemail.com
    /// Password for all: "test1234"
    /// </summary>
    public static void Seed50Contractors(ApplicationDbContext context)
    {
        var random = new Random();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("test1234");
        var tradeTypes = Enum.GetValues<TradeType>();

        // Austin, TX center coordinates
        const decimal austinLat = 30.2672m;
        const decimal austinLon = -97.7431m;

        // Street names in Austin area
        var streetNames = new[]
        {
            "Congress Ave", "Lamar Blvd", "Guadalupe St", "South 1st St", "East 6th St",
            "Barton Springs Rd", "Riverside Dr", "Burnet Rd", "North Loop Blvd", "South Lamar Blvd",
            "Manor Rd", "East Cesar Chavez St", "South Congress Ave", "Red River St", "East 7th St",
            "West 6th St", "East 5th St", "Cesar Chavez St", "East Riverside Dr", "South 1st St",
            "Airport Blvd", "Parmer Ln", "Research Blvd", "Mopac Expy", "I-35 Frontage Rd",
            "William Cannon Dr", "Slaughter Ln", "Ben White Blvd", "Oltorf St", "Riverside Dr"
        };

        // First names for contractors
        var firstNames = new[]
        {
            "James", "Michael", "Robert", "David", "William", "Richard", "Joseph", "Thomas",
            "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven",
            "Paul", "Andrew", "Joshua", "Kenneth", "Kevin", "Brian", "George", "Timothy",
            "Ronald", "Jason", "Edward", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas",
            "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon", "Benjamin",
            "Samuel", "Frank", "Gregory", "Raymond", "Alexander", "Patrick", "Jack", "Dennis",
            "Jerry", "Tyler", "Aaron", "Jose", "Henry", "Adam", "Douglas", "Nathan", "Zachary",
            "Peter", "Kyle", "Noah", "Ethan", "Jeremy", "Walter", "Christian", "Keith",
            "Roger", "Terry", "Austin", "Sean", "Gerald", "Carl", "Harold", "Dylan",
            "Lawrence", "Jordan", "Wayne", "Bryan", "Joe", "Billy", "Bruce", "Gabriel",
            "Alan", "Juan", "Logan", "Wayne", "Ralph", "Roy", "Randy", "Eugene", "Vincent",
            "Russell", "Louis", "Philip", "Bobby", "Johnny", "Willie"
        };

        // Last names for contractors
        var lastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Wilson", "Anderson", "Thomas",
            "Taylor", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White", "Harris",
            "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen",
            "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green",
            "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter",
            "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", "Cruz",
            "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy", "Cook",
            "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey", "Reed",
            "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson", "Watson",
            "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza", "Ruiz"
        };

        // Working hours variations
        var workingHours = new[]
        {
            (TimeSpan.Parse("06:00:00"), TimeSpan.Parse("15:00:00")),
            (TimeSpan.Parse("07:00:00"), TimeSpan.Parse("16:00:00")),
            (TimeSpan.Parse("08:00:00"), TimeSpan.Parse("17:00:00")),
            (TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00")),
            (TimeSpan.Parse("07:30:00"), TimeSpan.Parse("16:30:00")),
            (TimeSpan.Parse("08:30:00"), TimeSpan.Parse("17:30:00"))
        };

        var usersToAdd = new List<User>();
        var contractorsToAdd = new List<Contractor>();

        for (int i = 1; i <= 50; i++)
        {
            // Generate email
            var email = $"testcontractor{i}@testemail.com";

            // Check if user already exists
            if (context.Users.Any(u => u.Email == email))
            {
                continue; // Skip if already exists
            }

            // Determine trade type (distribute evenly: 10 each of 5 types)
            var tradeType = tradeTypes[(i - 1) % tradeTypes.Length];

            // Generate random name
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var fullName = $"{firstName} {lastName}";

            // Generate phone number (Austin area code: 512)
            var phoneArea = "512";
            var phonePrefix = random.Next(200, 999).ToString();
            var phoneSuffix = random.Next(1000, 9999).ToString();
            var phoneNumber = $"({phoneArea}) {phonePrefix}-{phoneSuffix}";

            // Ensure phone number is unique
            while (context.Contractors.Any(c => c.PhoneNumber == phoneNumber))
            {
                phonePrefix = random.Next(200, 999).ToString();
                phoneSuffix = random.Next(1000, 9999).ToString();
                phoneNumber = $"({phoneArea}) {phonePrefix}-{phoneSuffix}";
            }

            // Generate random address in Austin area
            var streetNumber = random.Next(100, 9999);
            var streetName = streetNames[random.Next(streetNames.Length)];
            var address = $"{streetNumber} {streetName}, Austin, TX 78701";

            // Generate random coordinates around Austin (within ~20 miles radius)
            // Roughly: 1 degree latitude ≈ 69 miles, 1 degree longitude ≈ 69 miles at this latitude
            var latOffset = (decimal)(random.NextDouble() * 0.3 - 0.15); // ~±10 miles
            var lonOffset = (decimal)(random.NextDouble() * 0.3 - 0.15); // ~±10 miles
            var latitude = austinLat + latOffset;
            var longitude = austinLon + lonOffset;

            // Select random working hours
            var (startTime, endTime) = workingHours[random.Next(workingHours.Length)];

            // Generate random rating (3.5 to 5.0) and review count
            var reviewCount = random.Next(0, 50);
            var averageRating = reviewCount > 0
                ? (decimal?)(Math.Round(random.NextDouble() * 1.5 + 3.5, 1))
                : null;

            var totalJobsCompleted = random.Next(0, 100);

            // Create user
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                Role = UserRole.Contractor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null
            };

            usersToAdd.Add(user);
        }

        // Add users in batch
        if (usersToAdd.Any())
        {
            context.Users.AddRange(usersToAdd);
            context.SaveChanges();
        }

        // Now create contractor profiles for the newly added users
        var addedUsers = context.Users
            .Where(u => u.Email.StartsWith("testcontractor") && u.Email.EndsWith("@testemail.com"))
            .OrderBy(u => u.Email)
            .ToList();

        for (int i = 0; i < addedUsers.Count && i < 50; i++)
        {
            var user = addedUsers[i];

            // Check if contractor already exists for this user
            if (context.Contractors.Any(c => c.UserId == user.Id))
            {
                continue;
            }

            // Determine trade type (distribute evenly: 10 each of 5 types)
            var tradeType = tradeTypes[i % tradeTypes.Length];

            // Generate random name
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var fullName = $"{firstName} {lastName}";

            // Generate phone number (Austin area code: 512)
            var phoneArea = "512";
            var phonePrefix = random.Next(200, 999).ToString();
            var phoneSuffix = random.Next(1000, 9999).ToString();
            var phoneNumber = $"({phoneArea}) {phonePrefix}-{phoneSuffix}";

            // Ensure phone number is unique
            while (context.Contractors.Any(c => c.PhoneNumber == phoneNumber))
            {
                phonePrefix = random.Next(200, 999).ToString();
                phoneSuffix = random.Next(1000, 9999).ToString();
                phoneNumber = $"({phoneArea}) {phonePrefix}-{phoneSuffix}";
            }

            // Generate random address in Austin area
            var streetNumber = random.Next(100, 9999);
            var streetName = streetNames[random.Next(streetNames.Length)];
            var address = $"{streetNumber} {streetName}, Austin, TX 78701";

            // Generate random coordinates around Austin
            var latOffset = (decimal)(random.NextDouble() * 0.3 - 0.15);
            var lonOffset = (decimal)(random.NextDouble() * 0.3 - 0.15);
            var latitude = austinLat + latOffset;
            var longitude = austinLon + lonOffset;

            // Select random working hours
            var (startTime, endTime) = workingHours[random.Next(workingHours.Length)];

            // Generate random rating and review count
            var reviewCount = random.Next(0, 50);
            var averageRating = reviewCount > 0
                ? (decimal?)(Math.Round(random.NextDouble() * 1.5 + 3.5, 1))
                : null;

            var totalJobsCompleted = random.Next(0, 100);

            var contractor = new Contractor
            {
                UserId = user.Id,
                Name = fullName,
                PhoneNumber = phoneNumber,
                Location = address,
                Latitude = latitude,
                Longitude = longitude,
                TradeType = tradeType,
                WorkingHoursStart = startTime,
                WorkingHoursEnd = endTime,
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                TotalJobsCompleted = totalJobsCompleted,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            contractorsToAdd.Add(contractor);
        }

        // Add contractors in batch
        if (contractorsToAdd.Any())
        {
            context.Contractors.AddRange(contractorsToAdd);
            context.SaveChanges();
        }
    }
}

