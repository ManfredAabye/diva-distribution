using System;
using System.Text.Json.Serialization;

namespace MetaverseInk.Configuration
{
    /// <summary>
    /// Configuration settings for OpenSim
    /// </summary>
    public class ConfigurationSettings
    {
        public string WorldName { get; set; }
        public string DbType { get; set; } // "MySQL" or "SQLite"
        public string DbHost { get; set; }
        public string DbSchema { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public string AdminFirstName { get; set; }
        public string AdminLastName { get; set; }
        public string AdminPassword { get; set; }
        public string AdminEmail { get; set; }
        public string IpAddress { get; set; }
        public int HttpPort { get; set; }
        public int BaseLocationX { get; set; }
        public int BaseLocationY { get; set; }
        public int RegionSizeX { get; set; }
        public int RegionSizeY { get; set; }
        public int RegionSizeZ { get; set; }
        public string GmailAccount { get; set; }
        public string GmailPassword { get; set; }
        public bool AutoBackup { get; set; }
        public bool Debug { get; set; }
        public string Region1Name { get; set; }
        
        /// <summary>
        /// Architecture type (Standalone, StandaloneHypergrid, Grid, GridHypergrid)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ArchitectureType Architecture { get; set; }
    }

    /// <summary>
    /// Backup metadata
    /// </summary>
    public class BackupMetadata
    {
        public string Timestamp { get; set; }
        public string WorldName { get; set; }
        public int ItemCount { get; set; }
    }
}
