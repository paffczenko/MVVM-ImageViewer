using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WinForms = System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Threading;


namespace MVVM
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        System.Windows.Threading.DispatcherTimer theShowTimer = new System.Windows.Threading.DispatcherTimer();
        AbortableBackgroundWorker backgroundWorker = new AbortableBackgroundWorker();
        public event PropertyChangedEventHandler PropertyChanged;
        private FileInfo[] Files;
        private int progressValue = 0;

        #region ViewProperties
        private int _mySelectedIndex;
        private string directoryPath = null;
        private ObservableCollection<String> _ListOfFiles;
        private string _mySelectedItem;
        private string _MyImage;
        private string _StartButton;
        private string _PreviousButtonEnabled;
        private string _NextButtonEnabled;
        private string _StartButtonEnabled;
        private string _ProgressBarVisibility;
        private int _ProgressBarValue;
        #endregion

        public MainWindowViewModel()
        {
            StartButton = "START";
            ProgressBarVisibility = "Hidden";
            theShowTimer.Interval = TimeSpan.FromSeconds(4);
            theShowTimer.Tick += new EventHandler(delegate (Object o, EventArgs a)
            {
                NextButtonPressedExecute();
            });

            backgroundWorker_Configure();
        }

        #region Show and Timer functions

        private void backgroundWorker_Configure()
        {
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        private void backgroundWorker_ProgressChanged(object sernder, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!backgroundWorker.CancellationPending)
            {
                while (theShowTimer.IsEnabled != false)
                {
                    for (progressValue = 0; progressValue <= 100; progressValue++)
                    {
                        backgroundWorker.ReportProgress(progressValue);
                        if (progressValue != 100) Thread.Sleep(40);
                    }
                }
            }
        }

        private void StartShow()
        {
            theShowTimer.Start();
            ProgressBarVisibility = "Visible";
            backgroundWorker.RunWorkerAsync();
            NextButtonEnabled = "false";
            PreviousButtonEnabled = "false";
            StartButton = "STOP";
        }
        private void StopShow()
        {
            ProgressBarVisibility = "Hidden";
            theShowTimer.Stop();
            progressValue = 0;
            backgroundWorker.Abort();
            backgroundWorker.Dispose();
            StartButton = "START";
            NextButtonEnabled = "true";
            PreviousButtonEnabled = "true";
        }

        private void ResetProgressValue()
        {
            progressValue = 0;
            theShowTimer.Stop();
            theShowTimer.Start();
        }

        private void StartButtonPressedExecute()
        {
            if (StartButton == "START")
            {
                StartShow();
            }
            else
            {
                StopShow();
            }

        }
        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Button binding functions

        public ICommand DirectoryBrowse {
            get {
                return new RelayCommand(DirectoryBrowseExecute, CanDirectoryBrowseExecute);
            }
        }

        public ICommand NextButtonPressed
        {
            get
            {
                return new RelayCommand(NextButtonPressedExecute, CanNextButtonPressedExecute);
            }
        }

        public ICommand PreviousButtonPressed
        {
            get
            {
                return new RelayCommand(PreviousButtonPressedExecute, CanPreviousButtonPressedExecute);
            }
        }

        public ICommand StartButtonPressed
        {
            get
            {
                return new RelayCommand(StartButtonPressedExecute, CanStartButtonPressedExecute);
            }
        }

        #endregion

        private void NextButtonPressedExecute()
        {
            if (MySelectedIndex == ListOfItems.Count - 1) MySelectedIndex = 0;
            else MySelectedIndex += 1;
        }

        private void PreviousButtonPressedExecute()
        {
            if (MySelectedIndex == 0) MySelectedIndex = ListOfItems.Count - 1;
            else MySelectedIndex -= 1;
        }

        #region Properties from View setters

        public string PreviousButtonEnabled
        {
            get
            {
                return _PreviousButtonEnabled;
            }
            set
            {
                _PreviousButtonEnabled = value;
                OnPropertyChanged("PreviousButtonEnabled");
            }
        }

        public string NextButtonEnabled
        {
            get
            {
                return _NextButtonEnabled;
            }
            set
            {
                _NextButtonEnabled = value;
                OnPropertyChanged("NextButtonEnabled");
            }
        }

        public string StartButtonEnabled
        {
            get
            {
                return _StartButtonEnabled;
            }
            set
            {
                _StartButtonEnabled = value;
                OnPropertyChanged("StartButtonEnabled");
            }
        }

        public string StartButton
        {
            get
            {
                return _StartButton;
            }
            set
            {
                _StartButton = value;
                OnPropertyChanged("StartButton");
            }
        }

        public string ProgressBarVisibility
        {
            get
            {
                return _ProgressBarVisibility;
            }
            set
            {
                _ProgressBarVisibility = value;
                OnPropertyChanged("ProgressBarVisibility");
            }
        }

        public int ProgressBarValue
        {
            get
            {
                return _ProgressBarValue;
            }
            set
            {
                _ProgressBarValue = value;
                OnPropertyChanged("ProgressBarValue");
            }
        }

        public string MySelectedItem
        {
            get { return _mySelectedItem; }
            set
            {
                _mySelectedItem = directoryPath + @"\" + value;
                MyImage = _mySelectedItem;
                if (backgroundWorker.IsBusy)
                {
                    ResetProgressValue();
                }
                OnPropertyChanged("MySelectedItem");

            }
        }

        public int MySelectedIndex
        {
            get { return _mySelectedIndex; }
            set
            {
                _mySelectedIndex = value;
                OnPropertyChanged("MySelectedIndex");
            }
        }

        public string MyImage
        {
            get
            {
                return _MyImage;
            }
            set
            {
                _MyImage = value;
                OnPropertyChanged("MyImage");
            }
        }


        public ObservableCollection<String> ListOfItems
        {
            get
            {
                if (_ListOfFiles == null)
                {
                    _ListOfFiles = new ObservableCollection<String>();
                }
                return _ListOfFiles;
            }
            set
            {
                _ListOfFiles = value;
                OnPropertyChanged("ListOfItems");
            }
        }

        #endregion

        private void DirectoryBrowseExecute()
        {
            if (theShowTimer.IsEnabled == true) StopShow();
            WinForms.FolderBrowserDialog myFolders = new WinForms.FolderBrowserDialog();
            myFolders.ShowNewFolderButton = false;

            if (myFolders.ShowDialog() == WinForms.DialogResult.OK)
            {
                directoryPath = myFolders.SelectedPath;
                ListOfItems.Clear();
                AddItemsToListBox();
                PreviousButtonEnabled = NextButtonEnabled = StartButtonEnabled = "true";
                if (ListOfItems.Count == 0)
                {
                    MessageBox.Show("Selected directory doesn't contains images");
                    PreviousButtonEnabled = NextButtonEnabled = StartButtonEnabled = "false";
                }
            }
        }

        private void AddItemsToListBox()
        {
            string[] extensions = new[] { ".jpg", ".jpeg", ".bmp", ".tiff", ".png" };
            DirectoryInfo dinfo = new DirectoryInfo(directoryPath);
            Files = dinfo.EnumerateFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();

            foreach (FileInfo file in Files)
            {
                _ListOfFiles.Add(file.Name);
            }
        }
        #region CanExecute functions
        private bool CanDirectoryBrowseExecute()
        {
            return true;
        }

        private bool CanNextButtonPressedExecute()
        {
            return true;
        }

        private bool CanPreviousButtonPressedExecute()
        {
            return true;
        }

        private bool CanStartButtonPressedExecute()
        {
            return true;
        }
        #endregion

    }
}
