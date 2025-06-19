using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class ShoppingCartUpdateADAViewModel : BaseModel<ShoppingCartUpdateADAViewModel>
    {
        public static readonly DependencyProperty AddMovieButtonVisibilityProperty =
            CreateDependencyProperty("AddMovieButtonVisibility", typeof(Visibility), Visibility.Visible);

        public static readonly DependencyProperty DisplayProductModelsProperty =
            CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayProductModel>));

        public Action OnAddMovieButtonPress;

        public Action<DisplayProductModel, object> OnCancelBrowseItemModel;

        public Action OnOkButtonPress;

        public string Title { get; set; }

        public string Message { get; set; }

        public string OkButtonText { get; set; }

        public string AddMovieButtonText { get; set; }

        public Visibility AddMovieButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AddMovieButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddMovieButtonVisibilityProperty, value); }); }
        }

        public List<DisplayProductModel> DisplayProductModels
        {
            get { return Dispatcher.Invoke(() => (List<DisplayProductModel>)GetValue(DisplayProductModelsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayProductModelsProperty, value); }); }
        }

        public void ProcessOkButtonPress()
        {
            var onOkButtonPress = OnOkButtonPress;
            if (onOkButtonPress == null) return;
            onOkButtonPress();
        }

        public void ProcessAddMovieButtonPress()
        {
            var onAddMovieButtonPress = OnAddMovieButtonPress;
            if (onAddMovieButtonPress == null) return;
            onAddMovieButtonPress();
        }

        public void ProcessOnCancelBrowseItemModel(DisplayProductModel displayProductModel, object parameter)
        {
            var onCancelBrowseItemModel = OnCancelBrowseItemModel;
            if (onCancelBrowseItemModel == null) return;
            onCancelBrowseItemModel(displayProductModel, parameter);
        }
    }
}