namespace ap.nexus.agents.api.contracts
{
    public class PagedAndSortedResultRequestDto
    {
        public int MaxResultCount { get; set; } = 10;
        public int SkipCount { get; set; } = 0;
        public string? Sorting { get; set; }
    }
}
