using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.api.contracts;
using ap.nexus.agents.application.Exceptions;
using FastEndpoints;
using System.ComponentModel.DataAnnotations;

namespace ap.nexus.agents.api.Endpoints
{
    public class GetUserThreadsEndpoint : Endpoint<GetUserThreadsRequest, PagedResultDto<ChatThreadDto>>
    {
        private readonly IThreadService _threadService;

        public GetUserThreadsEndpoint(IThreadService threadService)
        {
            _threadService = threadService;
        }

        public override void Configure()
        {
            Get("/threads");
            AllowAnonymous(); // In production, you would replace this with proper authorization
            Options(x =>
            {
                x.WithSummary("Retrieves a paged list of chat threads for a user.")
                 .WithDescription("Returns a paged list of chat threads based on the provided user ID and paging parameters.");
            });
        }

        public override async Task HandleAsync(GetUserThreadsRequest req, CancellationToken ct)
        {
            try
            {
                var request = new PagedAndSortedResultRequest
                {
                    MaxResultCount = req.MaxResultCount,
                    SkipCount = req.SkipCount,
                    Sorting = req.Sorting
                };

                var pagedResult = await _threadService.GetAllThreadsAsync(request);

                // Filter by userId if provided
                if (!string.IsNullOrEmpty(req.UserId))
                {
                    var filteredThreads = pagedResult.Items
                        .Where(t => t.UserId == req.UserId)
                        .ToList();

                    var newPagedResult = new PagedResultDto<ChatThreadDto>
                    {
                        Items = filteredThreads,
                        TotalCount = filteredThreads.Count
                    };

                    await SendAsync(newPagedResult, cancellation: ct);
                    return;
                }

                // Return all threads if no userId filter
                var result = new PagedResultDto<ChatThreadDto>
                {
                    Items = pagedResult.Items.ToList(),
                    TotalCount = pagedResult.TotalCount
                };

                await SendAsync(result, cancellation: ct);
            }
            catch (ValidationException vex)
            {
                AddError(vex.Message);
                ThrowIfAnyErrors();
            }
            catch (FriendlyBusinessException cex)
            {
                AddError(cex.Message);
                ThrowIfAnyErrors();
            }
        }
    }

    public class GetUserThreadsRequest : PagedAndSortedResultRequestDto
    {
        public string? UserId { get; set; }
    }
}