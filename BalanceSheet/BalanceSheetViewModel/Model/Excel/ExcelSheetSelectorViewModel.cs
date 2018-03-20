using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nachiappan.BalanceSheetViewModel.Model.Excel
{
    public class ExcelSheetSelectorViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public Action ValidityChanged;

        private bool _isValid = false;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    ValidityChanged?.Invoke();
                }
            }
        }

        private string _inputFileName = String.Empty;

        public string InputFileName
        {
            get { return _inputFileName; }
            set
            {
                if (_inputFileName != value)
                {
                    _inputFileName = value;
                    OnInputFileChanged();
                }
            }
        }

        private IList<string> _sheetNames = new List<string>();

        public IList<string> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                _sheetNames = value;
                OnPropertyChanged();
            }
        }

        private string _selectedSheet = String.Empty;
        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                if (_selectedSheet != value)
                {
                    _selectedSheet = value;
                    OnPropertyChanged();
                    OnSelectedSheetChanged();
                }
            }
        }

        private void OnSelectedSheetChanged()
        {
            if (string.IsNullOrEmpty(SelectedSheet))
            {
                IsValid = false;
            }
            if (!SheetNames.Contains(SelectedSheet))
            {
                IsValid = false;
            }
            IsValid = true;
        }

        private void OnInputFileChanged()
        {
            SelectedSheet = String.Empty;
            IsValid = false;
            try
            {
                ExcelSheetInfoProvider sheetInfoProvider = new ExcelSheetInfoProvider(InputFileName);
                SheetNames = sheetInfoProvider.GetSheetNames();
            }
            catch (Exception)
            {
                SheetNames = new List<string>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}