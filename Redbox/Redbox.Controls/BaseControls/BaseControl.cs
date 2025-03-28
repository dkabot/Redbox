using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls.BaseControls
{
    [Browsable(false)]
    public class BaseControl : Control
    {
        private object[] objArray = new object[1]
        {
            (object)true
        };

        private object UpdateVisualStateInfo;

        public new static readonly DependencyProperty IsEnabledProperty = CreateDependencyProperty("IsEnabled",
            typeof(bool), (object)false, new PropertyChangedCallback(OnVisualStatePropertyChanged));

        public static readonly DependencyProperty IsMouseOverPropertyKey = CreateDependencyProperty("IsMouseOver",
            typeof(bool), (object)false, new PropertyChangedCallback(OnVisualStatePropertyChanged));

        public static DependencyProperty CreateDependencyProperty(
            string propertyName,
            Type propertyType,
            object defaultValue,
            PropertyChangedCallback callback = null)
        {
            FrameworkPropertyMetadata propertyMetadata;
            if (callback != null)
            {
                propertyMetadata =
                    new FrameworkPropertyMetadata(defaultValue, new PropertyChangedCallback(callback.Invoke))
                    {
                        AffectsRender = true
                    };
            }
            else
            {
                propertyMetadata = new FrameworkPropertyMetadata(defaultValue);
                propertyMetadata.AffectsRender = true;
            }

            var typeMetadata = propertyMetadata;
            return DependencyProperty.Register(propertyName, propertyType, typeof(BaseControl),
                (PropertyMetadata)typeMetadata);
        }

        public new static void OnVisualStatePropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is BaseControl baseControl))
                return;
            baseControl.UpdateVisualState();
        }

        public new void UpdateVisualState()
        {
            UpdateVisualState(true);
        }

        public new void UpdateVisualState(bool useTransitions)
        {
            if (UpdateVisualStateInfo == null)
                UpdateVisualStateInfo = (object)GetType().GetMethod(nameof(UpdateVisualState), new Type[1]
                {
                    typeof(bool)
                });
            objArray[0] = (object)useTransitions;
            ((MethodBase)UpdateVisualStateInfo).Invoke((object)this, objArray);
        }

        protected new virtual void ChangeVisualState(bool useTransitions)
        {
        }

        protected new void ChangeValidationVisualState(bool useTransitions)
        {
            if (Validation.GetHasError((DependencyObject)this))
            {
                if (IsKeyboardFocused)
                    VisualStateManager.GoToState((FrameworkElement)this, "InvalidFocused", useTransitions);
                else
                    VisualStateManager.GoToState((FrameworkElement)this, "InvalidUnfocused", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState((FrameworkElement)this, "Valid", useTransitions);
            }
        }
    }
}