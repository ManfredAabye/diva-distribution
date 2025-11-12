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
        
        /// <summary>
        /// Physics Engine Configuration
        /// </summary>
        public PhysicsEngineSettings PhysicsEngine { get; set; }
        
        /// <summary>
        /// Script Engine Configuration
        /// </summary>
        public ScriptEngineSettings ScriptEngine { get; set; }
        
        /// <summary>
        /// Network Configuration
        /// </summary>
        public NetworkSettings Network { get; set; }
        
        /// <summary>
        /// OSSL Configuration
        /// </summary>
        public OsslSettings OSSL { get; set; }
        
        /// <summary>
        /// Grid Service Configuration
        /// </summary>
        public GridServiceSettings GridService { get; set; }
        
        /// <summary>
        /// Hypergrid Configuration
        /// </summary>
        public HypergridSettings Hypergrid { get; set; }
        
        /// <summary>
        /// Robust Configuration
        /// </summary>
        public RobustSettings Robust { get; set; }
    }
    
    /// <summary>
    /// Physics Engine Settings
    /// </summary>
    public class PhysicsEngineSettings
    {
        public string Engine { get; set; } // "BulletSim", "ubODE", etc.
        public string Meshing { get; set; } // "Meshmerizer", "ubODEMeshmerizer"
        public bool DisableUbODE { get; set; }
    }
    
    /// <summary>
    /// Script Engine Settings
    /// </summary>
    public class ScriptEngineSettings
    {
        public string DefaultEngine { get; set; } // "YEngine", "XEngine"
        public bool YEngineEnabled { get; set; }
        public bool XEngineEnabled { get; set; }
        public double MinTimerInterval { get; set; }
        public int ScriptDistanceLimitFactor { get; set; }
        public bool DeleteScriptsOnStartup { get; set; }
        public string Priority { get; set; } // "BelowNormal", "Normal", etc.
        public int MaxScriptEventQueue { get; set; }
    }
    
    /// <summary>
    /// Network Settings
    /// </summary>
    public class NetworkSettings
    {
        public string OutboundDisallowForUserScriptsExcept { get; set; }
        public int HttpBodyMaxLenMAX { get; set; }
        public string ExternalHostNameForLSL { get; set; }
    }
    
    /// <summary>
    /// OSSL Settings
    /// </summary>
    public class OsslSettings
    {
        public bool Enabled { get; set; }
        public string AllowOsslFunctions { get; set; } // "true", "false", or specific functions
        public bool AllowMODFunctions { get; set; }
        public bool AllowLightShareFunctions { get; set; }
        public string OsslThreatLevel { get; set; } // "VeryLow", "Low", "Moderate", "High", "VeryHigh", "Severe"
        public bool PermissionErrorToOwner { get; set; }
    }
    
    /// <summary>
    /// Grid Service Settings
    /// </summary>
    public class GridServiceSettings
    {
        public string DefaultRegionFlags { get; set; } // "DefaultRegion, FallbackRegion"
    }
    
    /// <summary>
    /// Hypergrid Settings
    /// </summary>
    public class HypergridSettings
    {
        public bool Enabled { get; set; }
        public string WorldMapModule { get; set; }
    }
    
    /// <summary>
    /// Robust Settings
    /// </summary>
    public class RobustSettings
    {
        public bool Enabled { get; set; }
        public int Port { get; set; }
        public bool HypergridEnabled { get; set; }
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
