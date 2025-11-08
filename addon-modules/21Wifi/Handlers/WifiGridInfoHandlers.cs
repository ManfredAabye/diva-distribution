/**
 * Copyright (c) Crista Lopes (aka Diva). All rights reserved.
 * 
 * Handlers for additional GridInfo pages
 */

using System;
using System.IO;
using System.Net;
using log4net;
using System.Reflection;

using OpenSim.Framework.Servers.HttpServer;
using Diva.Utils;
using Environment = Diva.Utils.Environment;

namespace Diva.Wifi
{
    #region About Handler
    public class WifiAboutGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiAboutGetHandler(WebApp webapp) :
            base("GET", "/wifi/about")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "text/html";
            return WebAppUtils.StringToBytes(m_WebApp.Services.AboutRequest(env));
        }
    }
    #endregion

    #region Help Handler
    public class WifiHelpGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiHelpGetHandler(WebApp webapp) :
            base("GET", "/wifi/help")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "text/html";
            return WebAppUtils.StringToBytes(m_WebApp.Services.HelpRequest(env));
        }
    }
    #endregion

    #region Account Handler
    public class WifiAccountGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiAccountGetHandler(WebApp webapp) :
            base("GET", "/wifi/account")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "text/html";
            return WebAppUtils.StringToBytes(m_WebApp.Services.AccountRequest(env));
        }
    }
    #endregion

    #region GridStatus Handler
    public class WifiGridStatusGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiGridStatusGetHandler(WebApp webapp) :
            base("GET", "/wifi/GridStatus")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "text/html";
            return WebAppUtils.StringToBytes(m_WebApp.Services.GridStatusRequest(env));
        }
    }
    #endregion

    #region Guide Handler
    public class WifiGuideGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiGuideGetHandler(WebApp webapp) :
            base("GET", "/wifi/guide")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "text/html";
            return WebAppUtils.StringToBytes(m_WebApp.Services.GuideRequest(env));
        }
    }
    #endregion

    #region RSS Handler
    public class WifiRSSGetHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private WebApp m_WebApp;

        public WifiRSSGetHandler(WebApp webapp) :
            base("GET", "/wifi/rss")
        {
            m_WebApp = webapp;
        }

        public override byte[] Handle(string path, Stream requestData,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            Request request = RequestFactory.CreateRequest(string.Empty, httpRequest, Localization.GetLanguageInfo(httpRequest.Headers.Get("accept-language")));
            Environment env = new Environment(request);

            httpResponse.ContentType = "application/rss+xml";
            return WebAppUtils.StringToBytes(m_WebApp.Services.RSSRequest(env));
        }
    }
    #endregion
}
