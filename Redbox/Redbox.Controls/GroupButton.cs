using Redbox.Controls.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls
{
    [Localizability(LocalizationCategory.Button)]
    public class GroupButton : RoundedButton
    {
        protected static List<GroupButton> groupButtons = new List<GroupButton>();

        public static readonly DependencyProperty ButtonStateProperty =
            Dependency<GroupButton>.CreateDependencyProperty(nameof(ButtonState), typeof(int), (object)0);

        public static readonly DependencyProperty DisposeProperty =
            Dependency<GroupButton>.CreateDependencyProperty(nameof(Dispose), typeof(bool), (object)false,
                new PropertyChangedCallback(OnDisposeChanged));

        public static readonly DependencyProperty GroupNameProperty =
            Dependency<GroupButton>.CreateDependencyProperty(nameof(GroupName), typeof(string), (object)string.Empty,
                new PropertyChangedCallback(OnGroupNameChanged));

        public static readonly DependencyProperty IsSelectedProperty =
            Dependency<GroupButton>.CreateDependencyProperty(nameof(IsSelected), typeof(bool), (object)false);

        static GroupButton()
        {
            Dependency<GroupButton>.DefaultOverrideMetadata(
                new Action<Type, PropertyMetadata>(DefaultStyleKeyProperty.OverrideMetadata));
        }

        public GroupButton()
        {
            Focusable = false;
            ClickMode = ClickMode.Release;
        }

        protected static void Register(GroupButton groupButton)
        {
            if (groupButtons.Contains(groupButton))
                return;
            groupButtons.Add(groupButton);
        }

        protected static void Unregister(GroupButton groupButton)
        {
            if (!groupButtons.Contains(groupButton))
                return;
            groupButtons.Remove(groupButton);
        }

        private static void OnDisposeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;
            Unregister(d as GroupButton);
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewValue as string))
                Unregister(d as GroupButton);
            else
                Register(d as GroupButton);
        }

        protected void UpdateSelection()
        {
            if (string.IsNullOrEmpty(GroupName))
                return;
            foreach (var groupButton in groupButtons)
                if (this != groupButton && GroupName.Equals(groupButton.GroupName))
                    groupButton.IsSelected = false;
        }

        [DefaultValue("")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, (object)value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                if (value)
                    UpdateSelection();
                SetValue(IsSelectedProperty, (object)value);
            }
        }

        public int ButtonState
        {
            get => (int)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, (object)value);
        }

        public bool Dispose
        {
            get => (bool)GetValue(DisposeProperty);
            set => SetValue(DisposeProperty, (object)value);
        }

        protected override void OnClick()
        {
            IsSelected = true;
            base.OnClick();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}