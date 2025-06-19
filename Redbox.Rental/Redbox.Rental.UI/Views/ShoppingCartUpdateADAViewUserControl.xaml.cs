using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ShoppingCartUpdateADAView")]
    public partial class ShoppingCartUpdateADAViewUserControl : TextToSpeechUserControl
    {
        public ShoppingCartUpdateADAViewUserControl()
        {
            InitializeComponent();
        }

        private ShoppingCartUpdateADAViewModel Model => DataContext as ShoppingCartUpdateADAViewModel;

        private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOkButtonPress();
        }

        private void AddMovieCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessAddMovieButtonPress();
        }

        private void PopulateDisplayProductModels()
        {
            var list = new List<DisplayProductWithCommandsUserControl>();
            foreach (var obj in ShoppingCartStackPanel.Children)
                if (obj is DisplayProductWithCommandsUserControl)
                    list.Add(obj as DisplayProductWithCommandsUserControl);

            foreach (var displayProductWithCommandsUserControl in list)
                ShoppingCartStackPanel.Children.Remove(displayProductWithCommandsUserControl);
            var model = Model;
            if ((model != null ? model.DisplayProductModels : null) != null)
                foreach (var displayProductModel in Model.DisplayProductModels)
                {
                    var displayProductWithCommandsUserControl2 = new DisplayProductWithCommandsUserControl
                    {
                        DataContext = displayProductModel,
                        Width = 135.0,
                        Height = 166.3,
                        Margin = new Thickness(15.0, 0.0, 5.0, 0.0)
                    };
                    ShoppingCartStackPanel.Children.Insert(Model.DisplayProductModels.IndexOf(displayProductModel),
                        displayProductWithCommandsUserControl2);
                }
        }

        private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PopulateDisplayProductModels();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.ProcessGetSpeechControl();
        }

        private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var displayProductModel =
                (frameworkElement != null ? frameworkElement.DataContext : null) as DisplayProductModel;
            if (Model != null) Model.ProcessOnCancelBrowseItemModel(displayProductModel, e.Parameter);
            HandleWPFHit();
        }
    }
}