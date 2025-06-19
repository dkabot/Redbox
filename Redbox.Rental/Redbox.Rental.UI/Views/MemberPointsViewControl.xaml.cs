using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPointsView")]
    public partial class MemberPointsViewControl : ViewUserControl
    {
        private Brush RbPurpleBrush;

        public MemberPointsViewControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            CartRoutedEventHandler += CartEventHandler;
            RbPurpleBrush = (Brush)FindResource("RbPurpleBrush");
        }

        private MemberPointsModel Model => DataContext as MemberPointsModel;

        public event RoutedEventHandler CartRoutedEventHandler
        {
            add => AddHandler(MemberPointsLogic.CartEvent, value);
            remove => RemoveHandler(MemberPointsLogic.CartEvent, value);
        }

        private void CartEventHandler(object sender, RoutedEventArgs e)
        {
            var cartRoutedEventArgs = e as CartRoutedEventArgs;
            var displayProductWithCommandsUserControl =
                (DisplayProductWithCommandsUserControl)CartStackPanelElem.Children[
                    int.Parse(cartRoutedEventArgs.ButtonName)];
            if (displayProductWithCommandsUserControl != null)
            {
                //if (MemberPointsViewControl.<>o__7.<>p__0 == null)
                //{
                //	MemberPointsViewControl.<>o__7.<>p__0 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(MemberPointsViewControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                //}
                //if (MemberPointsViewControl.<>o__7.<>p__0.Target(MemberPointsViewControl.<>o__7.<>p__0, cartRoutedEventArgs.Parameter))
                //{
                //	RoundedButton cornerIncludeButtonElem = displayProductWithCommandsUserControl.CornerIncludeButtonElem;
                //	if (cornerIncludeButtonElem.IsVisible)
                //	{
                //		this.CornerIncludeButton_Pressed(cornerIncludeButtonElem, e);
                //		return;
                //	}
                //}
                //else
                //{
                //	RoundedButton cornerExcludeButtonElem = displayProductWithCommandsUserControl.CornerExcludeButtonElem;
                //	if (cornerExcludeButtonElem.IsVisible)
                //	{
                //		this.CornerExcludeButton_Pressed(cornerExcludeButtonElem, e);
                //	}
                //}
            }
        }

        private long GetProductId(DisplayProductWithCommandsUserControl displayProductUserControl)
        {
            if ((displayProductUserControl != null ? displayProductUserControl.DataContext : null) is
                DisplayProductModel)
            {
                var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
                if ((displayProductModel != null ? displayProductModel.Data : null) is IRentalShoppingCartTitleItem)
                {
                    var rentalShoppingCartTitleItem = displayProductModel.Data as IRentalShoppingCartTitleItem;
                    if (rentalShoppingCartTitleItem != null) return rentalShoppingCartTitleItem.ProductId;
                }
            }

            return -1L;
        }

        private void PopulateDisplayProductModels()
        {
            var list = new List<DisplayProductWithCommandsUserControl>();
            foreach (var obj in CartStackPanelElem.Children)
                if (obj is DisplayProductWithCommandsUserControl)
                    list.Add(obj as DisplayProductWithCommandsUserControl);

            foreach (var displayProductWithCommandsUserControl in list)
                CartStackPanelElem.Children.Remove(displayProductWithCommandsUserControl);
            var model = Model;
            if ((model != null ? model.DisplayProductModels : null) != null)
            {
                foreach (var displayProductModel in Model.DisplayProductModels)
                {
                    var displayProductWithCommandsUserControl2 = new DisplayProductWithCommandsUserControl
                    {
                        DataContext = displayProductModel,
                        Height = 180.0,
                        Margin = new Thickness(15.0, 0.0, 15.0, 0.0)
                    };
                    var num = Model.DisplayProductModels.IndexOf(displayProductModel);
                    displayProductWithCommandsUserControl2.CornerIncludeButtonElem.Click += CornerIncludeButton_Pressed;
                    displayProductWithCommandsUserControl2.CornerExcludeButtonElem.Click += CornerExcludeButton_Pressed;
                    displayProductWithCommandsUserControl2.Name = string.Format("Points_Item_{0}", num);
                    CartStackPanelElem.Children.Insert(num, displayProductWithCommandsUserControl2);
                }

                var selectedProductIds = Model.SelectedProductIds;
                if (selectedProductIds != null && selectedProductIds.Count > 0)
                    foreach (var num2 in selectedProductIds)
                    foreach (var obj2 in CartStackPanelElem.Children)
                    {
                        var displayProductWithCommandsUserControl3 = obj2 as DisplayProductWithCommandsUserControl;
                        if (num2 == GetProductId(displayProductWithCommandsUserControl3))
                        {
                            var displayProductModel2 =
                                ChangeControlLook(displayProductWithCommandsUserControl3.CornerIncludeButtonElem,
                                    "FlagGrayColor", "FlagGrayShadow", true);
                            InvokeDisplayProductUserControl(Model.PointsIncludeCommand, displayProductModel2);
                        }
                    }
            }
        }

        protected override void DataContextHasChanged()
        {
            PopulateDisplayProductModels();
        }

        private void InvokeDisplayProductUserControl(DynamicRoutedCommand routeCommand,
            DisplayProductModel displayModel)
        {
            if (displayModel != null && displayModel != null)
                InvokeRoutedCommand(routeCommand, displayModel.Data as IRentalShoppingCartTitleItem);
        }

        private DisplayProductModel ChangeControlLook(RoundedButton buttonToHide, string flagColor, string shadowColor,
            bool includeOpacity = false)
        {
            var flag = buttonToHide.Name.Equals("CornerExcludeButtonElem");
            var grid = buttonToHide.Parent as Grid;
            var displayProductUserControl = (grid.Parent as Grid).Children[0] as DisplayProductUserControl;
            var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
            displayProductModel.ImageOpacity = includeOpacity ? 0.8 : 1.0;
            var flag2 = displayProductUserControl.Flag;
            if (grid.Children.Count > 1)
            {
                UIElement uielement = grid.Children[!flag ? 1 : 0] as RoundedButton;
                var color = (Color)FindResource(flagColor);
                var color2 = (Color)FindResource(shadowColor);
                flag2.FlagColor = color;
                flag2.FlagGradientColor = color;
                flag2.FlagShadowColor = color2;
                buttonToHide.Visibility = Visibility.Collapsed;
                uielement.Visibility = Visibility.Visible;
            }

            return displayProductModel;
        }

        private void CornerIncludeButton_Pressed(object sender, RoutedEventArgs e)
        {
            if (!IsMultiClickBlocked && Model != null)
            {
                IsMultiClickBlocked = true;
                var roundedButton = sender as RoundedButton;
                var logic = Model.Logic;
                if (logic.IsOverPointLimit(roundedButton.DataContext as DisplayProductModel))
                {
                    logic.DisplayErrorView();
                }
                else
                {
                    var displayProductModel = ChangeControlLook(roundedButton, "FlagGrayColor", "FlagGrayShadow", true);
                    InvokeDisplayProductUserControl(Model.PointsIncludeCommand, displayProductModel);
                }

                UpdateContinueButton();
                HandleWPFHit();
            }
        }

        private void CornerExcludeButton_Pressed(object sender, RoutedEventArgs e)
        {
            if (!IsMultiClickBlocked && Model != null)
            {
                IsMultiClickBlocked = true;
                var displayProductModel =
                    ChangeControlLook(sender as RoundedButton, "FlagRubineColor", "FlagRubineShadow");
                InvokeDisplayProductUserControl(Model.PointsExcludeCommand, displayProductModel);
                UpdateContinueButton();
                HandleWPFHit();
            }
        }

        private void UpdateContinueButton()
        {
            var flag = !Model.EntryShoppingProductIds.IsEqualTo(Model.SelectedProductIds);
            if (Model != null) Model.ContinueButtonEnabled = flag;
        }
    }
}