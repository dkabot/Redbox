using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class NewCartConfirmADAActionUpdateViewModel : BaseModel<NewCartConfirmADAActionUpdateViewModel>
    {
        public static readonly DependencyProperty DisplayProductModelsProperty =
            CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayCheckoutProductModel>));

        public DynamicRoutedCommand OkButtonCommand { get; set; }

        public string Title { get; set; }

        public string OkButtonText { get; set; }

        public List<DisplayCheckoutProductModel> DisplayProductModels
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (List<DisplayCheckoutProductModel>)GetValue(DisplayProductModelsProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayProductModelsProperty, value); }); }
        }
    }
}