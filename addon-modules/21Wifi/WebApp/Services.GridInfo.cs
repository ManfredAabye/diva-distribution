/**
 * Copyright (c) Crista Lopes (aka Diva). All rights reserved.
 * 
 * Additional GridInfo pages for Wifi
 */

using System;
using Diva.Wifi.ScriptEngine;
using Diva.Utils;
using Environment = Diva.Utils.Environment;

namespace Diva.Wifi
{
    public partial class Services
    {
        public string AboutRequest(Environment env)
        {
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.TheRequest, out sinfo))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn;
            }

            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "about.html");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }

        public string HelpRequest(Environment env)
        {
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.TheRequest, out sinfo))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn;
            }

            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "help.html");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }

        public string AccountRequest(Environment env)
        {
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.TheRequest, out sinfo))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn;
            }

            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "account.html");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }

        public string GridStatusRequest(Environment env)
        {
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.TheRequest, out sinfo))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn;
            }

            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "gridstatus.html");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }

        public string GuideRequest(Environment env)
        {
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.TheRequest, out sinfo))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn;
            }

            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "guide.html");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }

        public string RSSRequest(Environment env)
        {
            env.State = State.Default;
            string resourcePath = Localization.LocalizePath(env, "rss.xml");
            Processor p = new Processor(m_WebApp.WifiScriptFace, env);
            
            // Content type wird im Handler gesetzt
            
            return p.Process(WebAppUtils.ReadTextResource(new string[] {resourcePath}, WebApp.MissingPage));
        }
    }
}
