﻿namespace ap.nexus.abstractions.Agents.DTOs
{
    public class PagedAndSortedResultRequest
    {
        public int MaxResultCount { get; set; } = 10;
        public int SkipCount { get; set; } = 0;
        public string? Sorting { get; set; }
    }
}
