﻿using Common;
using MovieInfo;
using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace JavLuv
{
    public class BrowserItemViewModel : ObservableObject
    {
        #region Constructors

        public BrowserItemViewModel(BrowserViewModel parent, MovieData movieData)
        {
            Parent = parent;
            m_movieData = movieData;
            CreateDisplayTitle();
        }

        #endregion

        #region Properties

        public BrowserViewModel Parent { get; private set; }

        public MovieData MovieData { get { return m_movieData; } }

        public ImageSource Image
        {
            get
            {
                return m_image;
            }
            private set
            {
                if (value != m_image)
                {
                    m_image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        public string ID { get { return m_movieData.Metadata.UniqueID.Value; } }

        public string DisplayTitle { get { return m_displayTitle; } }

        #endregion

        #region Commands

        #region Detail View Command

        private void DetailViewExecute()
        {
            Parent.OpenDetailView(this);
        }

        private bool CanDetailViewExecute()
        {
            return true;
        }

        public ICommand DetailViewCommand { get { return new RelayCommand(DetailViewExecute, CanDetailViewExecute); } }

        #endregion

        #endregion

        #region Public Functions

        public void ImportCoverImage()
        {
            if (Parent.ImportCoverImage(m_movieData))
            {
                BeginLoadImage();
            }
        }

        #endregion

        #region Protected Functions

        protected override void OnShow()
        {
            base.OnShow();
            if (Image == null)
                BeginLoadImage();
        }

        protected override void OnHide()
        {
            base.OnHide();
            Image = null;
            if (m_loadImage != null)
            {
                m_loadImage.Cancel = true;
                m_loadImage.FinishedLoading -= LoadImage_FinishedLoading;
                m_loadImage = null;
            }
        }

        #endregion

        #region Private Functions

        private string GetOptionalIdAndTitle()
        {
            if (Settings.Get().ShowID)
                return String.Format("[{0}] {1}", m_movieData.Metadata.UniqueID.Value, m_movieData.Metadata.Title);
            else
                return m_movieData.Metadata.Title;
        }

        private void CreateDisplayTitle()
        {
            switch (Settings.Get().SortBy)
            {
                case SortBy.Title:
                    m_displayTitle = GetOptionalIdAndTitle();
                    break;
                case SortBy.ID:
                    m_displayTitle = String.Format("[{0}] {1}", m_movieData.Metadata.UniqueID.Value, m_movieData.Metadata.Title);
                    break;
                case SortBy.Actress:
                    if (m_movieData.Metadata.Actors.Count == 0)
                        m_displayTitle = String.Format("({0}) {1}", "???", GetOptionalIdAndTitle());
                    else if (m_movieData.Metadata.Actors.Count == 1)
                        m_displayTitle = String.Format("({0}) {1}", m_movieData.Metadata.Actors[0].Name, GetOptionalIdAndTitle());
                    else if (m_movieData.Metadata.Actors.Count == 2)
                        m_displayTitle = String.Format("({0} & {1}) {2}", m_movieData.Metadata.Actors[0].Name, m_movieData.Metadata.Actors[1].Name, GetOptionalIdAndTitle());
                    else
                        m_displayTitle = String.Format("({0}, {1}, & more...) {2}", m_movieData.Metadata.Actors[0].Name, m_movieData.Metadata.Actors[1].Name, GetOptionalIdAndTitle());
                    break;
                case SortBy.Date_Newest:
                    goto case SortBy.Date_Oldest;
                case SortBy.Date_Oldest:
                    string date = m_movieData.Metadata.Premiered;
                    if (String.IsNullOrEmpty(date))
                        date = "Unknown";
                    m_displayTitle = String.Format("({0}) {1}", date, GetOptionalIdAndTitle());
                    break;
                case SortBy.UserRating:
                    m_displayTitle = String.Format("({0}) {1}", UserRatingToStars(m_movieData.Metadata.UserRating), GetOptionalIdAndTitle());
                    break;
            }
        }

        private void BeginLoadImage()
        {
            if (m_loadImage != null)
                return;
            if (String.IsNullOrEmpty(m_movieData.Path) || String.IsNullOrEmpty(m_movieData.CoverFileName))
                return;
            string path = Path.Combine(m_movieData.Path, m_movieData.CoverFileName);
            m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
            m_loadImage.FinishedLoading += LoadImage_FinishedLoading;
            CommandQueue.ShortTask().Execute(m_loadImage);
        }

        private void LoadImage_FinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(m_movieData.Path, m_movieData.CoverFileName));
            m_loadImage = null;
        }

        private string UserRatingToStars(int userRating)
        {
            if (userRating == 0)
                return "unrated";
            StringBuilder sb = new StringBuilder(10);
            while (userRating >= 2)
            {
                sb.Append("\u2605");
                userRating -= 2;
            }
            if (userRating != 0)
                sb.Append("½");
            return sb.ToString();
        }

        #endregion

        #region Private Members

        private ImageSource m_image;
        private CmdLoadImage m_loadImage;
        private MovieData m_movieData;
        private string m_displayTitle;

        #endregion
    }
}
