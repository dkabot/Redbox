using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "UpsellView")]
    public partial class UpsellViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public UpsellViewUserControl()
        {
            InitializeComponent();
        }

        private UpsellViewModel Model => DataContext as UpsellViewModel;

        private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnBackButtonClicked();
        }

        private void NoThanksCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnNoThanksButtonClicked();
        }

        private void UpgradeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnUpgradeButtonClicked();
        }

        private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PopulateDisplayProductModels();
        }

        private void PopulateDisplayProductModels()
        {
            var list = new List<DisplayProductUpsellUserControl>();
            foreach (var obj in UpsellStackPanel.Children)
                if (obj is DisplayProductUpsellUserControl)
                    list.Add(obj as DisplayProductUpsellUserControl);

            foreach (var displayProductUpsellUserControl in list)
                UpsellStackPanel.Children.Remove(displayProductUpsellUserControl);
            var model = Model;
            if ((model != null ? model.DisplayProductModels : null) != null)
                foreach (var displayUpsellProductModel in Model.DisplayProductModels)
                {
                    var displayProductUpsellUserControl2 = new DisplayProductUpsellUserControl
                    {
                        DataContext = displayUpsellProductModel,
                        Width = 167.0,
                        Margin = new Thickness(-3.0, 0.0, -3.0, 0.0)
                    };
                    UpsellStackPanel.Children.Insert(Model.DisplayProductModels.IndexOf(displayUpsellProductModel),
                        displayProductUpsellUserControl2);
                }
        }

        private void ToggleUpsellCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dependencyObject = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dependencyObject != null && !(dependencyObject is UserControl))
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            var displayProductUpsellUserControl = dependencyObject as DisplayProductUpsellUserControl;
            if (displayProductUpsellUserControl != null)
            {
                var displayUpsellProductModel =
                    displayProductUpsellUserControl.DataContext as DisplayUpsellProductModel;
                if (displayUpsellProductModel != null) Model.ProcessOnCheckBoxClick(displayUpsellProductModel);
            }
        }

        private void UpgradeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var model = Model;
            e.CanExecute = model != null && model.ProcessOnCanExecuteUpgradeButton();
        }
    }
}