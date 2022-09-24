﻿using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace JavLuv
{
    public class ActressBrowserViewModel : ObservableObject
    {
        #region Constructors

        public ActressBrowserViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
            m_parent.Collection.ActressesDisplayedChanged += Collection_ActressesDisplayedChanged;
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        #endregion

        #region Public Functions

        public void NotifyAllProperties()
        {
            NotifyAllPropertiesChanged();
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public ObservableCollection<ActressBrowserItemViewModel> Actresses
        {
            get { return m_actresses; }
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                if (value != m_isEnabled)
                {
                    m_isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public ObservableCollection<ActressBrowserItemViewModel> SelectedItems
        {
            get { return m_selectedItems; }
            set
            {
                if (value != m_selectedItems)
                {
                    m_selectedItems = value;
                    NotifyPropertyChanged("SelectedItems");
                }
            }
        }

        public string SelectedDescription
        {
            get { return m_selectedDescription; }
            set
            {
                if (value != m_selectedDescription)
                {
                    m_selectedDescription = value;
                    NotifyPropertyChanged("SelectedDescription");
                }
            }
        }

        public System.Windows.Visibility EnableMoveRenameVisibility
        {
            get
            {
                if (Settings.Get().EnableMoveRename)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility ShowAdvancedOptionsVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility FindSubtitlesVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions && String.IsNullOrEmpty(Settings.Get().Subtitles) == false)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility PlayMovieVisibility { get; set; }

        public System.Windows.Visibility PlayRandomMovieVisibility { get; set; }

        #endregion

        #region Event Handlers

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void Collection_ActressesDisplayedChanged(object sender, EventArgs e)
        {
            Actresses.Clear();
            foreach (var movie in Parent.Collection.ActressesDisplayed)
                Actresses.Add(new ActressBrowserItemViewModel(this, movie));
        }

        #endregion

        #region Commands
/*
        #region Navigate Left Command

        private void NavigateLeftExecute()
        {
            MovieDetailViewModel current = Parent.Overlay as MovieDetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new ActressDetailViewModel(this, Actresses[Actresses.IndexOf(current.BrowserItem) - 1]);
        }

        private bool CanNavigateLeftExecute()
        {
            MovieDetailViewModel current = Parent.Overlay as MovieDetailViewModel;
            if (current == null)
                return false;
            if (Actresses.IndexOf(current.BrowserItem) == 0)
                return false;
            return true;
        }

        public ICommand NavigateLeftCommand { get { return new RelayCommand(NavigateLeftExecute, CanNavigateLeftExecute); } }

        #endregion

        #region Navigate Right Command

        private void NavigateRightExecute()
        {
            MovieDetailViewModel current = Parent.Overlay as MovieDetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new MovieDetailViewModel(this, Movies[Movies.IndexOf(current.BrowserItem) + 1]);
        }

        private bool CanNavigateRightExecute()
        {
            MovieDetailViewModel current = Parent.Overlay as MovieDetailViewModel;
            if (current == null)
                return false;
            if (Movies.IndexOf(current.BrowserItem) >= Movies.Count - 1)
                return false;
            return true;
        }

        public ICommand NavigateRightCommand { get { return new RelayCommand(NavigateRightExecute, CanNavigateRightExecute); } }

        #endregion
*/
        #endregion

        #region Public Functions

        public void OpenDetailView(ActressBrowserItemViewModel browserItem)
        {
            //Parent.Overlay = new ActressDetailViewModel(this, browserItem);
        }

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private ObservableCollection<ActressBrowserItemViewModel> m_actresses = new ObservableCollection<ActressBrowserItemViewModel>();
        private ObservableCollection<ActressBrowserItemViewModel> m_selectedItems = new ObservableCollection<ActressBrowserItemViewModel>();
        private string m_selectedDescription = String.Empty;
        private bool m_isEnabled = true;

        #endregion
    }
}

