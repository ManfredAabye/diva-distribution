using System;

namespace MetaverseInk.Configuration
{
    /// <summary>
    /// OpenSim Architecture Types
    /// </summary>
    public enum ArchitectureType
    {
        /// <summary>
        /// Standalone mode without Hypergrid support
        /// </summary>
        Standalone,
        
        /// <summary>
        /// Standalone mode with Hypergrid support (default for Diva Distribution)
        /// </summary>
        StandaloneHypergrid,
        
        /// <summary>
        /// Grid mode without Hypergrid support
        /// </summary>
        Grid,
        
        /// <summary>
        /// Grid mode with Hypergrid support
        /// </summary>
        GridHypergrid
    }

    /// <summary>
    /// Region configuration status
    /// </summary>
    public enum RegionConfigStatus : uint
    {
        OK = 0,
        NeedsCreation = 1,
        NeedsEditing = 2
    }
}
