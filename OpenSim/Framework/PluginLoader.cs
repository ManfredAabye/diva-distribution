/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Mono.Addins;


namespace OpenSim.Framework
{
    /// <summary>
    /// Exception thrown if an incorrect number of plugins are loaded
    /// </summary>
    public class PluginConstraintViolatedException : Exception
    {
        public PluginConstraintViolatedException () : base() {}
        public PluginConstraintViolatedException (string msg) : base(msg) {}
        public PluginConstraintViolatedException (string msg, Exception e) : base(msg, e) {}
    }

    /// <summary>
    /// Classes wishing to impose constraints on plugin loading must implement 
    /// this class and pass it to PluginLoader AddConstraint()
    /// </summary>
    public interface IPluginConstraint
    {
        string Message { get; }
        bool Apply (string extpoint);
    }

    /// <summary>
    /// Classes wishing to select specific plugins from a range of possible options
    /// must implement this class and pass it to PluginLoader Load()
    /// </summary>
    public interface IPluginFilter
    {
        bool Apply (ExtensionNode plugin);
    }

    /// <summary>
    /// Generic Plugin Loader
    /// </summary>
    public class PluginLoader <T> : IDisposable where T : IPlugin
    {
        private const int max_loadable_plugins = 10000;

        private List<T> loaded = new List<T>();
        private List<string> extpoints = new List<string>();
        private PluginInitialiserBase initialiser;
        
        private Dictionary<string,IPluginConstraint> constraints 
            = new Dictionary<string,IPluginConstraint>();

        private Dictionary<string,IPluginFilter> filters 
            = new Dictionary<string,IPluginFilter>();

        private static readonly ILog log 
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PluginInitialiserBase Initialiser
        { 
            set { initialiser = value; } 
            get { return initialiser; } 
        }

        public List<T> Plugins 
        { 
            get { return loaded; } 
        }

        public PluginLoader () 
        {
            Initialiser = new PluginInitialiserBase();
            initialise_plugin_dir_ (".");
        }

        public PluginLoader (PluginInitialiserBase init)
        {            
            Initialiser = init;
            initialise_plugin_dir_ (".");
        }

        public PluginLoader (PluginInitialiserBase init, string dir)
        {            
            Initialiser = init;
            initialise_plugin_dir_ (dir);
        }

        public void AddExtensionPoint (string extpoint)
        {
            extpoints.Add (extpoint);
        }

        public void AddConstraint (string extpoint, IPluginConstraint cons)
        {
            constraints.Add (extpoint, cons);
        }

        public void AddFilter (string extpoint, IPluginFilter filter)
        {
            filters.Add (extpoint, filter);
        }

        public void Load (string extpoint)
        {
            AddExtensionPoint (extpoint);
            Load();
        }

        public void Load ()
        {            
            foreach (string ext in extpoints)
            {
                log.Info("[PLUGINS]: Loading extension point " + ext);

                if (constraints.ContainsKey (ext))
                {
                    IPluginConstraint cons = constraints [ext];
                    if (cons.Apply (ext))
                        log.Error ("[PLUGINS]: " + ext + " failed constraint: " + cons.Message);
                }

                IPluginFilter filter = null;
                
                if (filters.ContainsKey (ext))
                    filter = filters [ext];

                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (ext))
                {
                    log.Info("[PLUGINS]: Trying plugin " + node.Path);
                    
                    if ((filter != null) && (filter.Apply (node) == false))
                        continue;
                        
                    T plugin = (T) node.CreateInstance();
                    Initialiser.Initialise (plugin);
                    Plugins.Add (plugin);
                }
            }
        }

        public void Dispose ()
        {
            foreach (T plugin in Plugins)
                plugin.Dispose ();
        }

        private void initialise_plugin_dir_ (string dir)
        {
            if (AddinManager.IsInitialized == true)
                return;

            log.Info("[PLUGINS]: Initialzing");

            AddinManager.AddinLoadError += on_addinloaderror_;
            AddinManager.AddinLoaded += on_addinloaded_;

            clear_registry_();

            suppress_console_output_ (true);
            AddinManager.Initialize (dir);
            AddinManager.Registry.Update (null);
            suppress_console_output_ (false);
        }

        private void on_addinloaded_(object sender, AddinEventArgs args)
        {
            log.Info ("[PLUGINS]: Plugin Loaded: " + args.AddinId);
        }

        private void on_addinloaderror_(object sender, AddinErrorEventArgs args)
        {
            log.Error ("[PLUGINS]: Plugin Error: " + args.Message 
                    + ": " + args.Exception.Message 
                    + "\n"+ args.Exception.StackTrace);
        }

        private void clear_registry_ ()
        {
            // The Mono addin manager (in Mono.Addins.dll version 0.2.0.0) 
            // occasionally seems to corrupt its addin cache
            // Hence, as a temporary solution we'll remove it before each startup
            if (Directory.Exists("addin-db-000"))
                Directory.Delete("addin-db-000", true);

            if (Directory.Exists("addin-db-001"))
                Directory.Delete("addin-db-001", true);
        }

        private static TextWriter prev_console_;        
        public void suppress_console_output_ (bool save)
        {
            if (save)
            {
                prev_console_ = System.Console.Out;
                System.Console.SetOut(new StreamWriter(Stream.Null));
            }
            else
            {
                if (prev_console_ != null) 
                    System.Console.SetOut(prev_console_);
            }
        }
    }

    /// <summary>
    /// Constraint that bounds the number of plugins to be loaded.
    /// </summary>
    public class PluginCountConstraint : IPluginConstraint
    { 
        private int min; 
        private int max; 

        public PluginCountConstraint (int exact)
        {
            min = exact; 
            max = exact; 
        }

        public PluginCountConstraint (int minimum, int maximum) 
        { 
            min = minimum; 
            max = maximum; 
        } 

        public string Message 
        { 
            get 
            { 
                return "The number of plugins is constrained to the interval [" 
                    + min + ", " + max + "]"; 
            } 
        }

        public bool Apply (string extpoint)
        {
            int count = AddinManager.GetExtensionNodes (extpoint).Count;

            if ((count < min) || (count > max))
                throw new PluginConstraintViolatedException (Message);

            return true;
        }
    }
    
    /// <summary>
    /// Filters out which plugin to load based on its "Id", which is name given by the namespace or by Mono.Addins.
    /// </summary>
    public class PluginIdFilter : IPluginFilter
    {
        private string id;

        public PluginIdFilter (string id) 
        {
            this.id = id;
        }

        public bool Apply (ExtensionNode plugin)
        {
            return (plugin.Id == id);
        }
    }
}
