﻿using Common;
using MovieInfo;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JavLuv
{
    public class MovieItem : ObservableObject
    {
        #region Constructors

        public MovieItem(DetailViewModel parent, string movieFileName)
        {
            m_parent = parent;
            m_movieFileName = movieFileName;
        }

        #endregion

        #region Properties

        public Uri MovieURI 
        { 
            get 
            {
                UriBuilder builder = new UriBuilder("file://");
                builder.Path = m_movieFileName;
                return builder.Uri;
            } 
        }

        public string MovieName { get { return "Play " + Path.GetFileNameWithoutExtension(m_movieFileName); } }

        #endregion

        #region Commands

        #region Play Command

        private void PlayExecute()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = m_movieFileName;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, TextManager.GetString("Text.ErrorPlayingMovie"));
            }
        }

        private bool CanPlayExecute()
        {
            return true;
        }

        public ICommand PlayCommand { get { return new RelayCommand(PlayExecute, CanPlayExecute); } }

        #endregion

        #endregion

        #region Private Members

        private DetailViewModel m_parent;
        private string m_movieFileName;

        #endregion
    }

    public class DetailViewModel : ObservableObject
    {
        #region Constructors

        public DetailViewModel(BrowserViewModel parent, BrowserItemViewModel browserItem)
        {
            Logger.WriteInfo("Viewing details of movie " + browserItem.MovieData.Metadata.UniqueID.Value);
            m_parent = parent;
            m_browserItem = browserItem;
            m_movieData = m_browserItem.MovieData;
            if (String.IsNullOrEmpty(m_movieData.CoverFileName) == false)
            {
                m_loadImage = new CmdLoadImage(Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
                m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
            }
            if (String.IsNullOrEmpty(m_movieData.MovieResolution))
            {
                m_getResolution = new CmdGetResolution(Path.Combine(m_movieData.Path, m_movieData.MovieFileNames[0]));
                m_getResolution.FinishedScanning += GetResolution_FinishedScanning;
                CommandQueue.LongTask().Execute(m_getResolution, CommandOrder.First);
            }

            // We need to explicitly scrape the original title because previous versions of JavLuv did not do this when
            // scraping the metadata originally.  In addition, metadata may have been generated by another program, which
            // may not have retrieved this data.
            if (Settings.Get().Language != LanguageType.Japanese && String.IsNullOrEmpty(m_movieData.Metadata.OriginalTitle))
            {
                var getOrigTitle = new CmdGetOriginalTitle(ID, m_movieData);
                getOrigTitle.FinishedScraping += GetOriginalTitle_Finished;
                CommandQueue.LongTask().Execute(getOrigTitle, ShowOriginalTitle ? CommandOrder.First : CommandOrder.Last);
            }
        }

        #endregion

       #region Event Handlers

        private void LoadImage_FinishedLoading(object sender, System.EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
            m_loadImage = null;
        }

        private void GetResolution_FinishedScanning(object sender, EventArgs e)
        {
            m_movieData.MovieResolution = m_getResolution.Resolution;
            NotifyPropertyChanged("Resolution");
        }

        private void GetOriginalTitle_Finished(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Title");
        }

        #endregion

        #region Properties

        public BrowserViewModel Parent { get { return m_parent; } }

        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                if (value != m_image)
                {
                    m_image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        public ObservableCollection<MovieItem> Movies
        {
            get
            {
                var movies = new ObservableCollection<MovieItem>();
                foreach (var fileName in m_movieData.MovieFileNames)
                    movies.Add(new MovieItem(this, Path.Combine(m_movieData.Path, fileName)));
                foreach (var extraFileName in m_movieData.ExtraMovieFileNames)
                    movies.Add(new MovieItem(this, Path.Combine(m_movieData.Path, extraFileName)));
                return movies;
            }
        }

        public System.Windows.Visibility SubtitlesVisible
        {
            get
            {
                if (m_movieData.SubtitleFileNames.Count == 0)
                    return System.Windows.Visibility.Hidden;
                return System.Windows.Visibility.Visible;
            }
        }

        public bool ShowOriginalTitle
        {
            get 
            {
                if (Settings.Get().Language == LanguageType.Japanese)
                    return false;
                return Settings.Get().ShowOriginalTitle; 
            }
            set
            {
                if (value != Settings.Get().ShowOriginalTitle)
                {
                    Settings.Get().ShowOriginalTitle = value;
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("ShowOriginalTitle");
                }
            }
        }

        public System.Windows.Visibility ShowOriginalTitleVisible
        {
            get
            {
                if (Settings.Get().Language == LanguageType.Japanese)
                    return System.Windows.Visibility.Hidden;
                return System.Windows.Visibility.Visible;
            }
        }


        public BrowserItemViewModel BrowserItem { get { return m_browserItem; } }

        public MovieData MovieData { get { return m_movieData; } }

        public string Title
        {
            get 
            { 
                if (ShowOriginalTitle)
                    return m_movieData.Metadata.OriginalTitle; 
                else
                    return m_movieData.Metadata.Title;
            }
            set
            {
                if (ShowOriginalTitle)
                {
                    if (value != m_movieData.Metadata.OriginalTitle)
                    {
                        m_movieData.Metadata.OriginalTitle = value;
                        m_movieData.MetadataChanged = true;
                    }
                }
                else
                {
                    if (value != m_movieData.Metadata.Title)
                    {
                        m_movieData.Metadata.Title = value;
                        m_movieData.MetadataChanged = true;
                    }
                }
                NotifyPropertyChanged("Title");
            }
        }

        public string ID
        {
            get { return m_movieData.Metadata.UniqueID.Value; }
            set
            {
                if (value != m_movieData.Metadata.UniqueID.Value)
                {
                    m_movieData.Metadata.UniqueID.Value = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public string Released
        {
            get { return m_movieData.Metadata.Premiered; }
            set
            {
                if (value != m_movieData.Metadata.Premiered)
                {
                    m_movieData.Metadata.Premiered = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Released");
                }
            }
        }

        public string Runtime
        {
            get { return m_movieData.Metadata.Runtime.ToString() + " " + TextManager.GetString("Text.Minutes"); }
            set
            {
                int val = Utilities.ParseInitialDigits(value);
                if (val != -1 && val != m_movieData.Metadata.Runtime)
                {                
                    m_movieData.Metadata.Runtime = val;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Runtime");                 
                }
            }
        }

        public string Studio
        {
            get { return m_movieData.Metadata.Studio; }
            set
            {
                if (value != m_movieData.Metadata.Studio)
                {
                    m_movieData.Metadata.Studio = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Studio");
                }
            }
        }

        public string Label
        {
            get { return m_movieData.Metadata.Label; }
            set
            {
                if (value != m_movieData.Metadata.Label)
                {
                    m_movieData.Metadata.Label = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Label");
                }
            }
        }

        public string Director
        {
            get { return m_movieData.Metadata.Director; }
            set
            {
                if (value != m_movieData.Metadata.Director)
                {
                    m_movieData.Metadata.Director = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Director");
                }
            }
        }

        public int UserRating
        {
            get { return m_movieData.Metadata.UserRating; }
            set
            {
                if (value != m_movieData.Metadata.UserRating)
                {
                    m_movieData.Metadata.UserRating = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("UserRating");
                }
            }
        }

        public string Genres
        {
            get 
            {
                return MovieUtils.GenresToString(m_movieData);
            }
            set
            {
                if (MovieUtils.StringToGenres(m_movieData, value))
                {
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Genres");
                }
            }
        }

        public string Actors
        {
            get
            {
                return MovieUtils.ActorsToString(m_movieData.Metadata.Actors);
            }
            set
            {
                var actors = m_movieData.Metadata.Actors;
                if (MovieUtils.StringToActors(value, ref actors))
                {
                    m_movieData.Metadata.Actors = actors;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Actors");
                }
            }
        }

        public string Resolution
        {
            get { return m_movieData.MovieResolution; }
            set
            {
                if (value != m_movieData.MovieResolution)
                {
                    m_movieData.MovieResolution = value;
                    NotifyPropertyChanged("Resolution");
                }
            }
        }

        public string Notes
        {
            get { return m_movieData.Metadata.Plot; }
            set
            {
                if (value != m_movieData.Metadata.Plot)
                {
                    m_movieData.Metadata.Plot = value;
                    m_movieData.MetadataChanged = true;
                    NotifyPropertyChanged("Notes");
                }
            }
        }

        public string FolderPath
        {
            get { return m_movieData.Path; }
        }

        #endregion

        #region Commands

        #region Open Folder Command

        private void OpenFolderExecute()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = m_movieData.Path;
                psi.Verb = "open";
                System.Diagnostics.Process.Start(psi);
            }
            catch(Exception)
            {
            }
        }

        private bool CanOpenFolderExecute()
        {
            return true;
        }

        public ICommand OpenFolderCommand { get { return new RelayCommand(OpenFolderExecute, CanOpenFolderExecute); } }

        #endregion

        #region Import Cover Image Command

        private void ImportCoverImageExecute()
        {
            if (Parent.ImportCoverImage(m_movieData))
            {
                m_loadImage = new CmdLoadImage(Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
                m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage, CommandOrder.First);
            }
        }

        private bool CanImportCoverImageExecute()
        {
            return true;
        }

        public ICommand ImportCoverImageCommand { get { return new RelayCommand(ImportCoverImageExecute, CanImportCoverImageExecute); } }

        #endregion

        #region Copy Title And Metadata Command

        private void CopyTitleAndMetadataExecute()
        {
            string text = String.Empty;
            text += "[" + ID + "] ";
            text += Title;
            text += "\n\n";
            text += "ID: " + ID + "\n";
            text += "Released: " + Released + "\n";
            text += "Runtime: " + Runtime + "\n";
            text += "Studio: " + Studio + "\n";
            text += "Label: " + Label + "\n";
            text += "Director: " + Director + "\n";
            text += "Genres: " + Genres + "\n";
            text += "Actresses: " + Actors + "\n\n";

            Clipboard.SetText(text);
        }

        private bool CanCopyTitleAndMetadataExecute()
        {
            return true;
        }

        public ICommand CopyTitleAndMetadataCommand { get { return new RelayCommand(CopyTitleAndMetadataExecute, CanCopyTitleAndMetadataExecute); } }


        #endregion

        #endregion

        #region Private Members

        private BrowserViewModel m_parent;
        private MovieData m_movieData;
        private BrowserItemViewModel m_browserItem;
        private ImageSource m_image;
        private CmdLoadImage m_loadImage;
        private CmdGetResolution m_getResolution;

        #endregion
    }
}
