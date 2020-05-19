using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WindowsDesktop;

namespace Win10VirtualDesktopDeskBand
{
    public partial class DeskBandUserControl : UserControl, INotifyPropertyChanged
    {
        private List<VirtualDesktop> _desktopObjects;
        public ObservableCollection<string> DesktopNames { get; set; } = new ObservableCollection<string>();
        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                if (value != -1) _desktopObjects[value].Switch();
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsRemoveEnabled");
            }

        }

        private bool _isNotAddMode = true;
        public bool IsNotAddMode
        {
            get => _isNotAddMode;
            set
            {
                _isNotAddMode = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsRemoveEnabled");
            }

        }

        private string _newName;
        public string NewName
        {
            get => _newName;
            set
            {
                _newName = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsRemoveEnabled => IsNotAddMode && (SelectedIndex != -1) && DesktopNames.Count > 1;

        public string settingsFilePath = Environment.GetEnvironmentVariable("APPDATA") + "/Win10VirtualDesktopDeskBand.xml";
        public DeskBandUserControl()
        {
            InitializeComponent();
            VirtualDesktop.CurrentChanged += VirtualDesktopOnCurrentChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void VirtualDesktopOnCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
        {
            //var newIndex = _desktopObjects.FindIndex(d => d.Id == e.NewDesktop.Id);
            //Debug.WriteLine("new index: " + newIndex+ToString() + "\r\n");
            SelectedIndex = _desktopObjects.FindIndex(d => d.Id == e.NewDesktop.Id);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            IsNotAddMode = false;
            TxtName.Focus();
        }

        private void BtnRemove_OnClick(object sender, RoutedEventArgs e)
        {
            DesktopNames.RemoveAt(SelectedIndex);
        }

        private void LoadList()
        {
            try
            {
                _desktopObjects = VirtualDesktop.GetDesktops().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to initialize virtual desktop library");
                return;
            }

            //we create on save
            if (File.Exists(settingsFilePath))
            {
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
                using (var fs = new FileStream(settingsFilePath, FileMode.Open))
                {
                    var list = xs.Deserialize(fs) as List<string>;
                    foreach (var name in list) DesktopNames.Add(name);
                }
            }

            //sync up the windows list with our internal list in case anything has changes since last run
            //until can figure out how to get the vDesktop names directly from Windows...
            //https://github.com/Grabacr07/VirtualDesktop/issues/17#issuecomment-629850667
            //make the basic assumption that the count/index is all we can rely on to keep the native OS desktops in sync with our names list
            if (_desktopObjects.Count > DesktopNames.Count)
                for (var i = DesktopNames.Count; i < _desktopObjects.Count; i++)
                    DesktopNames.Add("Desktop " + (DesktopNames.Count - 1 + i));
            else if (_desktopObjects.Count < DesktopNames.Count)
                for (var i = 0; i < (DesktopNames.Count - _desktopObjects.Count); i++)
                    DesktopNames.RemoveAt(DesktopNames.Count - 1);

            if (DesktopNames.Count > 0) SelectedIndex = 0;
            DesktopNames.CollectionChanged += DesktopNamesOnCollectionChanged;
            SaveList();
        }

        //keeps the ObservableCollection of names in sync with the list of desk objects
        private void DesktopNamesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newdesk = VirtualDesktop.Create();
                _desktopObjects.Add(newdesk);
                newdesk.Switch();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _desktopObjects[e.OldStartingIndex].Remove();
                _desktopObjects.RemoveAt(e.OldStartingIndex);
            }
            SaveList();

        }

        private void SaveList()
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            using (FileStream fs = new FileStream(settingsFilePath, FileMode.Create))
            {
                xs.Serialize(fs, DesktopNames.ToList());
            }
        }


        private void DeskBandUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadList();
        }

        private void TxtName_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DesktopNames.Add(NewName);
                NewName = "";
                IsNotAddMode = true;
                SaveList();
            }
        }
    }

    //https://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter/2427307#2427307
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolVisConverter : IValueConverter
    {
        enum Parameters
        {
            NORMAL, REVERSE
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)(value ?? false);
            var direction = (Parameters)Enum.Parse(typeof(Parameters), (string)(parameter ?? Parameters.NORMAL.ToString()));

            if (direction == Parameters.REVERSE)
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
