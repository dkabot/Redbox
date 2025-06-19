using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class MemberPointsLogic : BaseLogic
    {
        public static readonly RoutedEvent CartEvent = EventManager.RegisterRoutedEvent("Cart", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MemberPointsLogic));

        public MemberPointsLogic(MemberPointsModel pointsModel, PointsMemberInfo memberInfo)
        {
            Instructions = true;
            CancelAction = memberInfo.CancelAction;
            ContinueAction = memberInfo.ContinueAction;
            Info = memberInfo;
            Model = pointsModel;
            Model.PointsIncludeCommand = RegisterRoutedCommand(Commands.PointsIncludeCommand,
                new Action<IRentalShoppingCartTitleItem>(PointsIncludeCommand_Execute));
            Model.PointsExcludeCommand = RegisterRoutedCommand(Commands.PointsExcludeCommand,
                new Action<IRentalShoppingCartTitleItem>(PointsExcludeCommand_Execute));
            Model.CancelButtonCommand =
                RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
            Func<bool> func = () => Model.ContinueButtonEnabled;
            Model.ContinueButtonCommand =
                RegisterRoutedCommand(Commands.ContinueButtonCommand, ContinueButtonCommand_Execute, func);
        }

        public bool Instructions { get; set; }

        public Action<CartRoutedEventArgs> CartAction { get; set; }

        public Action CancelAction { get; set; }

        public Action ContinueAction { get; set; }

        public Action PointsAction { get; set; }

        public static MemberPointsModel Model { get; private set; }

        public static PointsMemberInfo Info { get; private set; }

        public MemberPointsModel GetModel()
        {
            return Model;
        }

        public PointsMemberInfo GetInfo()
        {
            return Info;
        }

        public void CancelButtonCommand_Execute()
        {
            var cancelAction = CancelAction;
            if (cancelAction != null) cancelAction();
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPointsLogicCancel");
        }

        public void ContinueButtonCommand_Execute()
        {
            var continueAction = ContinueAction;
            if (continueAction != null) continueAction();
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPointsLogicOkay");
        }

        public List<DisplayProductModel> GetSelectedCartItems()
        {
            var list = new List<DisplayProductModel>();
            var displayProductModels = Model.DisplayProductModels;
            var selectedProductIds = Model.SelectedProductIds;
            foreach (var displayProductModel in displayProductModels)
            {
                var shoppingCartItem = GetShoppingCartItem(displayProductModel);
                if (Model.SelectedProductIds.Contains(shoppingCartItem.ProductId)) list.Add(displayProductModel);
            }

            return list;
        }

        public List<DisplayProductModel> GetUnselectedCartItems()
        {
            var list = new List<DisplayProductModel>();
            var displayProductModels = Model.DisplayProductModels;
            var selectedProductIds = Model.SelectedProductIds;
            foreach (var displayProductModel in displayProductModels)
            {
                var shoppingCartItem = GetShoppingCartItem(displayProductModel);
                if (!Model.SelectedProductIds.Contains(shoppingCartItem.ProductId)) list.Add(displayProductModel);
            }

            return list;
        }

        public void DisplayErrorView()
        {
            var pointsAction = PointsAction;
            if (pointsAction == null) return;
            pointsAction();
        }

        public bool IsOverPointLimit(DisplayProductModel displayModel)
        {
            var pointsToRedeem = GetPointsToRedeem(displayModel);
            return Info.RemainingPoints - pointsToRedeem < 0;
        }

        private IRentalShoppingCartTitleItem GetShoppingCartItem(DisplayProductModel displayModel)
        {
            if (displayModel != null && displayModel.Data != null && displayModel.Data is IRentalShoppingCartTitleItem)
                return displayModel.Data as IRentalShoppingCartTitleItem;
            return null;
        }

        public int GetPointsToRedeem(DisplayProductModel displayModel)
        {
            return GetPointsToRedeem(GetShoppingCartItem(displayModel));
        }

        public int GetPointsToRedeem(IRentalShoppingCartTitleItem shoppingCartItem)
        {
            var num = 0;
            if ((shoppingCartItem != null ? shoppingCartItem.LoyaltyPointsRecord : null) != null)
                num = shoppingCartItem.LoyaltyPointsRecord.PointsToRedeem.Value;
            return num;
        }

        public void UpdatePointsInfo(int index, bool bAdd)
        {
            CartAction(new CartRoutedEventArgs(CartEvent)
            {
                ButtonName = index.ToString(),
                Parameter = bAdd
            });
        }

        private static void UpdatePointsInfo(IRentalShoppingCartTitleItem shoppingCartItem, int scalar)
        {
            var loyaltyPointsRecord = shoppingCartItem.LoyaltyPointsRecord;
            if (loyaltyPointsRecord != null && loyaltyPointsRecord.PointsToRedeem != null)
            {
                Info.RemainingPoints += loyaltyPointsRecord.PointsToRedeem.Value * scalar;
                Model.PointsMessage = string.Format(Resources.points_message, Info.RemainingPoints);
            }
        }

        public void PointsIncludeCommand_Execute(IRentalShoppingCartTitleItem shoppingCartItem)
        {
            if (shoppingCartItem != null)
            {
                UpdatePointsInfo(shoppingCartItem, -1);
                if (!Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
                {
                    Model.SelectedProductIds.Add(shoppingCartItem.ProductId);
                    var analyticsService = AnalyticsService;
                    if (analyticsService == null) return;
                    var analyticsEvent = analyticsService.AddButtonPressEvent("MemberPointsLogicRedeem");
                    if (analyticsEvent == null) return;
                    analyticsEvent.AddData(ProductData.ToProductData(shoppingCartItem));
                }
            }
        }

        public void PointsExcludeCommand_Execute(IRentalShoppingCartTitleItem shoppingCartItem)
        {
            if (shoppingCartItem != null)
            {
                UpdatePointsInfo(shoppingCartItem, 1);
                if (Model.SelectedProductIds.Contains(shoppingCartItem.ProductId))
                {
                    Model.SelectedProductIds.Remove(shoppingCartItem.ProductId);
                    var analyticsService = AnalyticsService;
                    if (analyticsService == null) return;
                    var analyticsEvent = analyticsService.AddButtonPressEvent("MemberPointsLogicCancelRedeem");
                    if (analyticsEvent == null) return;
                    analyticsEvent.AddData(ProductData.ToProductData(shoppingCartItem));
                }
            }
        }
    }
}