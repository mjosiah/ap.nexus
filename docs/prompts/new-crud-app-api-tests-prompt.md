
You are a senior .NET developer with extensive experience building layered, enterprise-grade applications. Your task is to generate the following artifacts based on the provided inputs. The **domain entity**, **infrastructure layer**, and **integration test fixture** are already in place.

### Input Parameters

1. **Top-level namespace:** `<TopLevelNamespace>`

2. **Entity class definition:**  
   \```csharp
   <EntityClassDefinition>
   \```
   *Assume the entity already exists in `<TopLevelNamespace>.Domain.Entities`.*

3. **Application Service Interface and Associated DTOs/Enums:**  
   \```csharp
   <ApplicationServiceInterfaceAndDTOs>
   \```
   *This includes the service contract (e.g., `IAgentService`), request/response DTOs (e.g., `CreateAgentRequest`, `AgentDto`, `PagedResultDto<T>`), and any related enums (e.g., `ScopeType`).*

4. **Optional Additional Context:** `<AdditionalContext>`

   *This may include extra business rules, naming conventions, logging/telemetry requirements, authorization attributes, or any other cross-cutting concerns. Incorporate these into the generated code as needed.*

---

## Requirements & Artifacts to Generate

You must generate the following artifacts in the **Application**, **API**, and **Tests** layers. If the `<AdditionalContext>` includes extra details (like logging, telemetry, or authorization), apply them accordingly.

### 1. Application Layer

**Artifact: Application Service**

- Place this in `<TopLevelNamespace>.Application.Services`.
- Implement the service interface provided (e.g., `IAgentService`) and fulfill the contract’s methods (e.g., `CreateAgentAsync`, `GetAllAgentsAsync`, etc.).
- Use a generic repository (from the Infrastructure layer) for data access.  
  *Example: `IGenericRepository<T>`.*
- Map data between DTOs and the Entity.  
  *If your environment uses something like AutoMapper, incorporate it if indicated.*
- **Business rules**:  
  - Throw a custom business exception (e.g., `FriendlyBusinessException`) if certain domain rules are violated (e.g., “Name cannot be ‘Invalid’”).
  - Validate incoming DTOs and throw a standard `FluentValidation.ValidationException` when validation fails.
- **Logging and Cross-Cutting Concerns**:  
  - If `<AdditionalContext>` specifies logging or telemetry, inject a logger or telemetry client, and log key actions (e.g., creation, update, or error scenarios).
- **Authorization** (Optional):  
  - If `<AdditionalContext>` indicates you need `[Authorize]` or role-based checks, ensure the relevant code or attributes are added.

#### Example: Application Service Code

```csharp
using <TopLevelNamespace>.Abstractions.Agents.DTOs;
using <TopLevelNamespace>.Abstractions.Agents.Interfaces;
using <TopLevelNamespace>.Application.Exceptions;
using <TopLevelNamespace>.Domain.Entities;
using <TopLevelNamespace>.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace <TopLevelNamespace>.Application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IGenericRepository<Agent> _agentRepository;

        // If the AdditionalContext indicates logging/telemetry, inject it here
        // private readonly ILogger<AgentService> _logger;

        public AgentService(IGenericRepository<Agent> agentRepository /*, ILogger<AgentService> logger*/)
        {
            _agentRepository = agentRepository;
            // _logger = logger;
        }

        public async Task<AgentDto> CreateAgentAsync(CreateAgentRequest request)
        {
            // Business rule example
            if (request.Name.Equals("Invalid", StringComparison.OrdinalIgnoreCase))
            {
                // _logger.LogWarning("Attempt to create an agent with name 'Invalid'");
                throw new FriendlyBusinessException("Agent name cannot be 'Invalid'.");
            }

            // Map the DTO to the Entity.
            var agent = new Agent
            {
                ExternalId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Model = request.Model,
                Instruction = request.Instructions,
                // Additional properties...
            };

            await _agentRepository.AddAsync(agent);
            await _agentRepository.SaveChangesAsync();

            // Map the Entity back to a DTO.
            return new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                // Additional properties...
            };
        }

        public async Task<PagedResultDto<AgentDto>> GetAllAgentsAsync(PagedAndSortedResultRequestDto input)
        {
            var query = _agentRepository.Query();

            if (!string.IsNullOrWhiteSpace(input.Sorting))
                query = query.OrderBy(a => a.Name); // simplistic sorting example
            else
                query = query.OrderBy(a => a.Id);

            var totalCount = await query.CountAsync();
            var agents = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var agentDtos = agents.Select(a => new AgentDto
            {
                ExternalId = a.ExternalId,
                Name = a.Name,
                Description = a.Description,
                Model = a.Model,
                Instruction = a.Instruction,
                // Additional properties...
            }).ToList();

            return new PagedResultDto<AgentDto>
            {
                TotalCount = totalCount,
                Items = agentDtos
            };
        }

        // Optionally implement UpdateAgentAsync, DeleteAgentAsync, etc. if the interface defines them.
    }
}
```

---

### 2. API Layer

#### A. FastEndpoints API Endpoints

**Artifacts:** Create one or more FastEndpoints classes for each operation.  
- **Create Endpoint**:  
  - Define HTTP method, route, and Swagger metadata in `Configure()`.
  - Accept the request DTO, validate automatically with FluentValidation.
  - Call the application service, return the response DTO.
  - Catch exceptions (`ValidationException`, `FriendlyBusinessException`), use `AddError`, then `ThrowIfAnyErrors()`.

**Example Endpoint**:

```csharp
using <TopLevelNamespace>.Abstractions.Agents.DTOs;
using <TopLevelNamespace>.Abstractions.Agents.Interfaces;
using <TopLevelNamespace>.Application.Exceptions;
using FastEndpoints;
using FluentValidation;

namespace <TopLevelNamespace>.Api.Endpoints
{
    public class CreateAgentEndpoint : Endpoint<CreateAgentRequest, AgentDto>
    {
        private readonly IAgentService _agentService;

        public CreateAgentEndpoint(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public override void Configure()
        {
            Post("/agents/create");
            AllowAnonymous(); // or use [Authorize] if needed
            Options(x =>
            {
                x.WithSummary("Creates a new agent.")
                 .WithDescription("Creates a new agent using the provided data and returns the created agent.");
            });
        }

        public override async Task HandleAsync(CreateAgentRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _agentService.CreateAgentAsync(req);
                await SendAsync(result, cancellation: ct);
            }
            catch (ValidationException vex)
            {
                AddError(vex.Message);
                ThrowIfAnyErrors();
            }
            catch (FriendlyBusinessException fex)
            {
                AddError(fex.Message);
                ThrowIfAnyErrors();
            }
        }
    }
}
```

- *(Similarly, generate endpoints for “GetAll”, “Update”, “Delete” if specified by the interface or `<AdditionalContext>`.)*

#### B. Validators

**Artifact:** FluentValidation validators for each request DTO.  
- Place them in `<TopLevelNamespace>.Api.Validators`.

**Example**:

```csharp
using <TopLevelNamespace>.Abstractions.Agents.DTOs;
using FastEndpoints;
using FluentValidation;

namespace <TopLevelNamespace>.Api.Validators
{
    public class CreateAgentRequestValidator : Validator<CreateAgentRequest>
    {
        public CreateAgentRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Agent name is required.");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Agent model is required.");
        }
    }
}
```

---

### 3. Tests

#### A. Unit Tests

- Create in `<TopLevelNamespace>.Tests.UnitTests`.
- Write a sample test demonstrating a failure scenario (e.g., business rule violation).

**Example**:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Xunit;
using FluentAssertions;
using <TopLevelNamespace>.Application.Exceptions;
using <TopLevelNamespace>.Application.Services;
using <TopLevelNamespace>.Domain.Entities;
using <TopLevelNamespace>.Abstractions.Agents.DTOs;
using <TopLevelNamespace>.Infrastructure.Data.Repositories;

namespace <TopLevelNamespace>.Tests.UnitTests
{
    public class AgentServiceTests
    {
        [Fact]
        public async Task CreateAgentAsync_InvalidName_ShouldThrowFriendlyBusinessException()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<Agent>>();
            var service = new AgentService(mockRepo.Object);
            var request = new CreateAgentRequest
            {
                Name = "Invalid", // Should trigger the business rule.
                Model = "TestModel"
                // ...other properties
            };

            // Act & Assert
            await Assert.ThrowsAsync<FriendlyBusinessException>(() => service.CreateAgentAsync(request));
        }
    }
}
```

- *(Optionally add more tests to cover each method, boundary conditions, etc. If `<AdditionalContext>` includes logging or other cross-cutting concerns, test them accordingly.)*

#### B. Integration Tests

- Create in `<TopLevelNamespace>.Tests.IntegrationTests`.
- Assume integration test fixture is already configured (e.g., in-memory DB, service collection, etc.).

**Example**:

```csharp
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using <TopLevelNamespace>.Abstractions.Agents.DTOs;
using <TopLevelNamespace>.Abstractions.Agents.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace <TopLevelNamespace>.Tests.IntegrationTests
{
    public class AgentIntegrationTests // Integration fixture is assumed
    {
        private readonly IAgentService _agentService;

        public AgentIntegrationTests()
        {
            // Typically, you'd resolve from your TestFixture’s ServiceProvider
            _agentService = TestFixture.ServiceProvider.GetRequiredService<IAgentService>();
        }

        [Fact]
        public async Task GetAllAgents_ShouldReturnSeededAgents()
        {
            // Arrange
            var input = new PagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "Name"
            };

            // Act
            var result = await _agentService.GetAllAgentsAsync(input);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty("because the DB was seeded with test data");
            result.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
```

---

### Optional Enhancements

- **Additional CRUD Endpoints**:  
  If the `<ApplicationServiceInterfaceAndDTOs>` includes methods like `UpdateAgentAsync` or `DeleteAgentAsync`, generate corresponding FastEndpoints classes and unit tests (and possibly integration tests).
  
- **Logging & Telemetry**:  
  If `<AdditionalContext>` mentions logging (e.g., Serilog, ILogger<T>) or telemetry (e.g., Application Insights), inject and use them in the service or endpoint methods.

- **Authorization**:  
  If `<AdditionalContext>` indicates security requirements, apply `[Authorize]` or role-based checks in your endpoints.

- **Code Style & Conventions**:  
  If you have specific style rules (e.g., use `var`, naming conventions, bracket style), ensure the generated code follows them.

- **Extended Test Coverage**:  
  If needed, request more robust test coverage (e.g., multiple success/failure tests, boundary tests, or scenario-based tests) to ensure each method is thoroughly validated.

---

## Output Requirements

Your output must include:

1. The complete **Application Service** code (with mapping, business rules, and repository usage).
2. **FastEndpoints** API endpoint(s) for at least Create and (optionally) GetAll/Update/Delete, depending on the interface.
3. A **FluentValidation** validator for the request DTO(s).
4. A sample **unit test** (showing at least one test case) for the Application Service.
5. A sample **integration test** (showing at least one test case) using the preconfigured fixture.

**All code** should:
- Reside under the `<TopLevelNamespace>` (or sub-namespaces) as demonstrated.
- Incorporate **any additional context** you provide (logging, telemetry, authorization, naming conventions, etc.).
- Use placeholders in the code or comments for anything that might differ depending on project structure, so as not to confuse generation.

---
