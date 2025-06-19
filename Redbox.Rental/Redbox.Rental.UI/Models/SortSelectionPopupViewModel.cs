using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class SortSelectionPopupViewModel : DependencyObject
    {
        public string HeaderText { get; set; }

        public string DescriptionText { get; set; }

        public string CancelButtonText { get; set; }

        public string ApplyButtonText { get; set; }

        public List<SortButtonModel> SortButtonModels { get; set; }

        public event Action OnCancelButtonClicked;

        public void ProcessOnCancelButtonClicked()
        {
            var onCancelButtonClicked = OnCancelButtonClicked;
            if (onCancelButtonClicked == null) return;
            onCancelButtonClicked();
        }

        public event Action OnApplyButtonClicked;

        public void ProcessOnApplyButtonClicked()
        {
            var onApplyButtonClicked = OnApplyButtonClicked;
            if (onApplyButtonClicked == null) return;
            onApplyButtonClicked();
        }
    }
}