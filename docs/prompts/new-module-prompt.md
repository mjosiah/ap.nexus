You are an expert .NET developer and architect for the Nexus AI platform (with a top-level namespace of `AP.Nexus`). Based on the provided input parameters below, generate a complete module implementation that covers the following layers: Domain, Infrastructure, Application, API, and Testing. Do not generate the project structure itself; assume the directory layout already exists.

**Input Parameters:**  
- **Module Name:** `{ModuleName}`  
- **Entity Name:** `{EntityName}`  
- **Feature Context/Description:** `{FeatureContext}`  
- **Entity Properties:** `{Properties}`  
  (Properties are provided as a comma-separated list in the format `PropertyName: Type`.)

Your response must generate code examples for the following:

---

#### 1. Domain Layer

- **File:** `AP.Nexus.{ModuleName}.Domain/Entities/{EntityName}.cs`  
- **Task:**  
  Define a class `{EntityName}` in the namespace `AP.Nexus.{ModuleName}.Domain.Entities` using the provided properties.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Domain/Entities/{EntityName}.cs
using System;

namespace AP.Nexus.{ModuleName}.Domain.Entities
{
    public class {EntityName}
    {
        // Use the provided properties here:
        // Example: Id: Guid, Name: string, Description: string, etc.
        {PropertiesFormatted}
    }
}
```
*Note: Replace `{PropertiesFormatted}` with each property on its own line using the format:*  
`public <Type> <PropertyName> { get; set; }`

---

#### 2. Infrastructure Layer

**A. DbContext Setup**

- **File:** `AP.Nexus.{ModuleName}.Infrastructure/Data/{ModuleName}DbContext.cs`  
- **Task:**  
  Create a `DbContext` for the module. Include a `DbSet<{EntityName}>` and configure the entity (table name, keys, and any constraints).

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Infrastructure/Data/{ModuleName}DbContext.cs
using AP.Nexus.{ModuleName}.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AP.Nexus.{ModuleName}.Infrastructure.Data
{
    public class {ModuleName}DbContext : DbContext
    {
        public DbSet<{EntityName}> {ModuleName} { get; set; }

        public {ModuleName}DbContext(DbContextOptions<{ModuleName}DbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<{EntityName}>(entity =>
            {
                entity.ToTable("{ModuleName}", "{ModuleName}");
                entity.HasKey(e => e.Id);
                
                // Configure properties as needed. Example:
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                // Add further configuration based on the entity properties if needed.
            });
        }
    }
}
```

**B. Data Seeder**

- **File:** `AP.Nexus.{ModuleName}.Infrastructure/Data/{ModuleName}DataSeeder.cs`  
- **Task:**  
  Create a seeder that inserts a sample `{EntityName}` record if none exist.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Infrastructure/Data/{ModuleName}DataSeeder.cs
using AP.Nexus.{ModuleName}.Domain.Entities;
using System;
using System.Linq;

namespace AP.Nexus.{ModuleName}.Infrastructure.Data
{
    public static class {ModuleName}DataSeeder
    {
        public static void Seed({ModuleName}DbContext context)
        {
            if (!context.{ModuleName}.Any())
            {
                context.{ModuleName}.AddRange(
                    new {EntityName}
                    {
                        // Set sample values for the properties.
                        // Example: Id = Guid.NewGuid(), Name = "Sample {EntityName}", etc.
                        {SamplePropertyValues}
                    }
                );
                
                context.SaveChanges();
            }
        }
    }
}
```
*Note: Replace `{SamplePropertyValues}` with sample assignments for each property as appropriate.*

---

#### 3. Application Layer

**A. DTOs**

- **File:** `AP.Nexus.{ModuleName}.Application/DTOs/{EntityName}Dto.cs`  
- **Task:**  
  Define two classes: one for the output (`{EntityName}Dto`) and one for creation requests (`Create{EntityName}Request`).

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Application/DTOs/{EntityName}Dto.cs
namespace AP.Nexus.{ModuleName}.Application.DTOs
{
    public class {EntityName}Dto
    {
        // Include properties that should be returned to the client.
        {DtoProperties}
    }

    public class Create{EntityName}Request
    {
        // Include properties needed for creating a new entity.
        {RequestProperties}
    }
}
```
*Note: You can choose to include all or a subset of properties for the DTO and request classes as per the feature context.*

**B. Service Implementation**

- **File:** `AP.Nexus.{ModuleName}.Application/Services/{EntityName}Service.cs`  
- **Task:**  
  Implement a service that accepts a `Create{EntityName}Request`, validates it (using business rules implied by `{FeatureContext}` if needed), persists the new entity with an `IGenericRepository<{EntityName}>`, and returns a `{EntityName}Dto`.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Application/Services/{EntityName}Service.cs
using AP.Nexus.{ModuleName}.Application.DTOs;
using AP.Nexus.{ModuleName}.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace AP.Nexus.{ModuleName}.Application.Services
{
    public class {EntityName}Service : I{EntityName}Service
    {
        private readonly IGenericRepository<{EntityName}> _repository;
        private readonly ISettingManager _settingManager;

        public {EntityName}Service(IGenericRepository<{EntityName}> repository, ISettingManager settingManager)
        {
            _repository = repository;
            _settingManager = settingManager;
        }

        public async Task<{EntityName}Dto> Create{EntityName}Async(Create{EntityName}Request request)
        {
            // Optional: Validate the request based on {FeatureContext}.
            // Example: Validate price limits or other business rules using settings.
            var maxValue = await _settingManager.GetSettingValueAsync<decimal>("{ModuleName}:Max{EntityName}Value");
            // Add validation logic here if applicable.
            
            var entity = new {EntityName}
            {
                // Map request properties to the entity. Example:
                // Id = Guid.NewGuid(),
                // Name = request.Name,
                // ...
                {EntityMapping}
            };

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return new {EntityName}Dto
            {
                // Map the entity to the DTO. Example:
                // Id = entity.Id,
                // Name = entity.Name,
                // ...
                {DtoMapping}
            };
        }
    }
}
```
*Note: Replace `{EntityMapping}` and `{DtoMapping}` with the appropriate property assignments.*

---

#### 4. API Layer

- **File:** `AP.Nexus.{ModuleName}.Api/Endpoints/Create{EntityName}Endpoint.cs`  
- **Task:**  
  Implement an HTTP POST endpoint using FastEndpoints. The endpoint should accept a `Create{EntityName}Request`, call the service, and return a `{EntityName}Dto` with proper error handling.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Api/Endpoints/Create{EntityName}Endpoint.cs
using AP.Nexus.{ModuleName}.Application.DTOs;
using AP.Nexus.{ModuleName}.Application.Services;
using FastEndpoints;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AP.Nexus.{ModuleName}.Api.Endpoints
{
    public class Create{EntityName}Endpoint : Endpoint<Create{EntityName}Request, {EntityName}Dto>
    {
        private readonly I{EntityName}Service _service;

        public Create{EntityName}Endpoint(I{EntityName}Service service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/{ModuleName.ToLower()}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Create{EntityName}Request req, CancellationToken ct)
        {
            try
            {
                var result = await _service.Create{EntityName}Async(req);
                await SendAsync(result, cancellation: ct);
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                ThrowIfAnyErrors();
            }
        }
    }
}
```

---

#### 5. Testing

**A. Unit Tests**

- **File:** `AP.Nexus.{ModuleName}.Tests/{EntityName}ServiceTests.cs`  
- **Task:**  
  Create unit tests for the service that verify a valid creation request returns the expected DTO.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.Tests/{EntityName}ServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using AP.Nexus.{ModuleName}.Application.DTOs;
using AP.Nexus.{ModuleName}.Application.Services;
using AP.Nexus.{ModuleName}.Domain.Entities;
using System;
using System.Threading.Tasks;

public class {EntityName}ServiceTests
{
    private readonly Mock<IGenericRepository<{EntityName}>> _mockRepository;
    private readonly I{EntityName}Service _service;

    public {EntityName}ServiceTests()
    {
        _mockRepository = new Mock<IGenericRepository<{EntityName}>>();
        var mockSettingManager = new Mock<ISettingManager>();
        mockSettingManager.Setup(s => s.GetSettingValueAsync<decimal>(It.IsAny<string>()))
                          .ReturnsAsync(1000.00m);

        _service = new {EntityName}Service(_mockRepository.Object, mockSettingManager.Object);
    }

    [Fact]
    public async Task Create{EntityName}_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new Create{EntityName}Request
        {
            // Set test property values
            {TestRequestProperties}
        };

        // Act
        var result = await _service.Create{EntityName}Async(request);

        // Assert
        result.Should().NotBeNull();
        // Add further assertions as needed, for example:
        // result.Name.Should().Be("Expected Value");
    }
}
```
*Note: Replace `{TestRequestProperties}` with assignments for the properties used in the create request.*

**B. Integration Tests**

- **File:** `AP.Nexus.{ModuleName}.IntegrationTests/IntegrationTestFixture.cs`  
- **Task:**  
  Set up an in-memory or SQLite database, register services, ensure the database is created, and seed initial data. Then include an integration test class that confirms a new `{EntityName}` is persisted when created through the service.

**Example Output:**
```csharp
// AP.Nexus.{ModuleName}.IntegrationTests/IntegrationTestFixture.cs
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using AP.Nexus.{ModuleName}.Infrastructure.Data;
using AP.Nexus.{ModuleName}.Application.Services;

public class IntegrationTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public ServiceProvider ServiceProvider { get; }
    public {ModuleName}DbContext DbContext { get; }

    public IntegrationTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<{ModuleName}DbContext>(options =>
            options.UseSqlite(_connection));
            
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<I{EntityName}Service, {EntityName}Service>();

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<{ModuleName}DbContext>();
        DbContext.Database.EnsureCreated();
        
        // Seed initial data
        {ModuleName}DataSeeder.Seed(DbContext);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
```

And an integration test class:

```csharp
// AP.Nexus.{ModuleName}.IntegrationTests/{EntityName}IntegrationTests.cs
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class {EntityName}IntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly I{EntityName}Service _service;

    public {EntityName}IntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _service = fixture.ServiceProvider.GetRequiredService<I{EntityName}Service>();
    }

    [Fact]
    public async Task Create{EntityName}_ValidRequest_ShouldPersistInDatabase()
    {
        // Arrange
        var request = new Create{EntityName}Request
        {
            // Set integration test property values
            {IntegrationTestProperties}
        };

        // Act
        var result = await _service.Create{EntityName}Async(request);

        // Assert
        result.Should().NotBeNull();
        var savedEntity = await _fixture.DbContext.{ModuleName}.FirstOrDefaultAsync(e => e.Id == result.Id);
        savedEntity.Should().NotBeNull();
        // Further assertions can be added here.
    }
}
```
*Note: Replace `{IntegrationTestProperties}` with appropriate assignments.*

---

**Final Note to the LLM:**  
If any input details (for instance, the property list) are missing or unclear, please ask clarifying questions before proceeding. Otherwise, use the provided inputs to replace all placeholders (such as `{ModuleName}`, `{EntityName}`, `{PropertiesFormatted}`, `{EntityMapping}`, etc.) to generate the complete code boilerplate for the new module.