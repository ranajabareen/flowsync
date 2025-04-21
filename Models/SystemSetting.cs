﻿namespace WebApplicationFlowSync.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    }
}
