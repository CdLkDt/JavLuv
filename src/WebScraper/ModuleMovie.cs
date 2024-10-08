﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WebScraper
{
    abstract public class ModuleMovie : ModuleBase
    {
        #region Constructors

        public ModuleMovie(MovieMetadata metadata, Dispatcher dispatcher, WebBrowser webBrowser, LanguageType language) : base(dispatcher, webBrowser, language)
        {
            m_metadata = metadata;
        }

        #endregion

        #region Properties

        public MovieMetadata Metadata { get {  return m_metadata; } }

        #endregion

        #region Protected Functions

        override protected bool IsValidDataParsed()
        {
            if (m_parsingSuccessful) 
                return true;
            if (Metadata != null)
            {
                if (String.IsNullOrEmpty(Metadata.Title) == false)
                    return true;
            }
            return false;
        }

        protected string GetToken(Token token)
        {
            if (m_language == LanguageType.English)
            {
                switch (token)
                {
                    case Token.ReleaseDate:
                        return "Release Date:";
                    case Token.Length:
                        return "Length:";
                    case Token.Director:
                        return "Director:";
                    case Token.Series:
                        return "Series:";
                    case Token.Studio:
                        return "Studio:";
                    case Token.Maker:
                        return "Maker:";
                    case Token.Label:
                        return "Label:";
                }
            }
            else if (m_language == LanguageType.Japanese)
            {
                switch (token)
                {
                    case Token.ReleaseDate:
                        return "発売日:";
                    case Token.Length:
                        return "収録時間:";
                    case Token.Director:
                        return "監督:";
                    case Token.Series:
                        return "シリーズ:";
                    case Token.Studio:
                        return "メーカー:";
                    case Token.Maker:
                        return "メーカー:";
                    case Token.Label:
                        return "レーベル:";
                }
            }
            return String.Empty;
        }

        #endregion

        #region Protected Members

        protected enum Token
        {
            ReleaseDate,
            Length,
            Director,
            Series,
            Studio,
            Maker,
            Label,
        }

        protected MovieMetadata m_metadata;

        #endregion
    }
}
