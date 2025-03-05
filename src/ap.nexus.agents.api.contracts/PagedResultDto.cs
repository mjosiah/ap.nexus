namespace ap.nexus.agents.api.contracts
{
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
