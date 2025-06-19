using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class RedboxClassicProductCarouselUserControl : UserControl
    {
        public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating",
            typeof(bool), typeof(RedboxClassicProductCarouselUserControl), new FrameworkPropertyMetadata(false));

        private readonly int _carouselImagePadding = 30;

        private readonly int _carouselPositionCount = 7;

        private readonly CarouselPositionList _carouselPositions = new CarouselPositionList();

        private readonly List<DisplayProductUserControl> _displayProductUserControlList =
            new List<DisplayProductUserControl>();

        private readonly double _verticalCenterOfCenterImage = 384.0;

        private readonly double originalHorizontalCenterOfCenterImage = 562.0;

        private CarouselDirection _carouselDirection;

        private Point _carouselImageLargeSize;

        private Point _carouselImageSmallSize;

        private CarouselModel _carouselModel;

        private List<DisplayProductModel> _carouselProducts;

        private Storyboard _carouselRotationStoryboard;

        private bool _forceRotation;

        private double _horizontalCenterOfCenterImage;

        private CarouselDirection _nextCarouselDirection;

        private List<DisplayProductModel> _rotatingCarouselProducts;

        public RedboxClassicProductCarouselUserControl()
        {
            InitializeComponent();
        }

        public bool IsRotating
        {
            get => (bool)GetValue(IsRotatingProperty);
            set => SetValue(IsRotatingProperty, value);
        }

        private bool IsCarouselProperlyInitialized => _rotatingCarouselProducts != null &&
                                                      _rotatingCarouselProducts.Count > 0 &&
                                                      _carouselPositions != null && _carouselPositions.Count > 0 &&
                                                      _displayProductUserControlList != null &&
                                                      _displayProductUserControlList.Count == _carouselPositions.Count;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ConfigureCarousel();
        }

        private void ConfigureCarousel()
        {
            _displayProductUserControlList.Clear();
            CarouselContainer.Children.Clear();
            _rotatingCarouselProducts = new List<DisplayProductModel>();
            InitializeCarouselPositions();
            _carouselModel = DataContext as CarouselModel;
            if (_carouselModel != null)
            {
                _carouselModel.OnIsAnimationOnChanged += _carouselModel_OnIsAnimationOnChanged;
                _carouselProducts = _carouselModel.CarouselDisplayProductModels;
                _rotatingCarouselProducts.AddRange(_carouselProducts);
                _nextCarouselDirection = CarouselDirection.Left;
                foreach (var displayProductModel in _rotatingCarouselProducts)
                {
                    var num = _rotatingCarouselProducts.IndexOf(displayProductModel);
                    if (num < _carouselPositions.Count)
                    {
                        var carouselPosition = _carouselPositions[num];
                        var displayProductUserControl = new DisplayProductUserControl
                        {
                            DataContext = displayProductModel,
                            Margin = new Thickness(carouselPosition.Left, carouselPosition.Top, 0.0, 0.0),
                            Width = carouselPosition.Width,
                            Height = carouselPosition.Height
                        };
                        if (num < _carouselPositions.CenterCarouselPositionIndex)
                            displayProductUserControl.Tag = CarouselDirection.Right;
                        else if (num > _carouselPositions.CenterCarouselPositionIndex)
                            displayProductUserControl.Tag = CarouselDirection.Left;
                        _displayProductUserControlList.Add(displayProductUserControl);
                        CarouselContainer.Children.Add(displayProductUserControl);
                        displayProductUserControl.ApplyStyle();
                    }
                }

                InitializeTitleRollupOverlay();
                CommandManager.InvalidateRequerySuggested();
                if (_carouselModel.IsAnimationOn) AnimateCarouselRotation();
            }
        }

        private void SetDisplayProductModelsAddButtonVisibility()
        {
            if (IsCarouselProperlyInitialized)
                foreach (var displayProductModel in _rotatingCarouselProducts)
                {
                    if (displayProductModel ==
                        _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex])
                    {
                        var carouselModel = _carouselModel;
                        if (carouselModel != null && carouselModel.ShowAddButton)
                        {
                            displayProductModel.AddButtonVisibility = Visibility.Visible;
                            continue;
                        }
                    }

                    displayProductModel.AddButtonVisibility = Visibility.Collapsed;
                }
        }

        private void _carouselModel_OnIsAnimationOnChanged(CarouselModel carouselModel)
        {
            if (carouselModel != null && carouselModel.IsAnimationOn)
            {
                AnimateCarouselRotation();
                return;
            }

            _carouselRotationStoryboard.Stop();
            if (IsCarouselProperlyInitialized)
            {
                var displayProductUserControl =
                    _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex];
                if (displayProductUserControl != null)
                {
                    var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
                    if (displayProductModel != null) displayProductModel.AddButtonVisibility = Visibility.Collapsed;
                }
            }
        }

        private void InitializeTitleRollupOverlay()
        {
            TitleRollupOverlay.ApplyStyle();
            if (IsCarouselProperlyInitialized)
            {
                TitleRollupOverlay.Width = _carouselPositions.CenterCarouselPosition.Width;
                TitleRollupOverlay.Height = _carouselPositions.CenterCarouselPosition.Height;
                TitleRollupOverlay.Margin = new Thickness(_carouselPositions.CenterCarouselPosition.Left,
                    _carouselPositions.CenterCarouselPosition.Top, 0.0, 0.0);
                if (_carouselModel != null)
                    _carouselModel.TitleRollupOverlayModel.BrowseItemModel =
                        _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex];
            }
        }

        private void AnimateCarouselRotation()
        {
            SetDisplayProductModelsAddButtonVisibility();
            _carouselDirection = _nextCarouselDirection;
            var timeSpan = TimeSpan.FromSeconds(0.0);
            var timeSpan2 = TimeSpan.FromSeconds(_forceRotation ? 0.0 : 1.7);
            var timeSpan3 = timeSpan + timeSpan2;
            var timeSpan4 = TimeSpan.FromSeconds(0.0);
            var timeSpan5 = timeSpan3 + timeSpan4;
            var timeSpan6 = TimeSpan.FromSeconds(0.8);
            var timeSpan7 = timeSpan5 + timeSpan6;
            var timeSpan8 = timeSpan7;
            _forceRotation = false;
            var carouselRotationStoryboard = _carouselRotationStoryboard;
            if (carouselRotationStoryboard != null) carouselRotationStoryboard.Stop();
            _carouselRotationStoryboard = new Storyboard();
            _carouselRotationStoryboard.Duration = timeSpan8;
            _carouselRotationStoryboard.Completed += carousel_animation_storyBoard_Completed;
            if (IsCarouselProperlyInitialized)
                for (var i = 0; i < _carouselPositionCount; i++)
                    if ((_carouselDirection == CarouselDirection.Left && i > 0) ||
                        (_carouselDirection == CarouselDirection.Right && i < _carouselPositionCount - 1))
                    {
                        var carouselPosition = _carouselPositions[i];
                        var num = i + (_carouselDirection == CarouselDirection.Left ? -1 : 1);
                        var carouselPosition2 = _carouselPositions[num];
                        var displayProductUserControl = _displayProductUserControlList[i];
                        var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
                        var translateTransform = new TranslateTransform();
                        var scaleTransform = new ScaleTransform();
                        displayProductUserControl.RenderTransform = new TransformGroup
                        {
                            Children = { scaleTransform, translateTransform }
                        };
                        var thickness = new Thickness(3.0);
                        var thickness2 = new Thickness(3.0);
                        var num2 = thickness2.Bottom - thickness.Bottom;
                        var flag = carouselPosition2 == _carouselPositions.CenterCarouselPosition;
                        var flag2 = carouselPosition == _carouselPositions.CenterCarouselPosition;
                        var num3 = carouselPosition.Width - (flag2 ? num2 : 0.0);
                        var num4 = num3 / carouselPosition.Width;
                        var num5 = (carouselPosition2.Width - (flag ? num2 : 0.0)) / num3;
                        var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = num4
                        });
                        if (flag2)
                        {
                            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = 1.0
                            });
                            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = 1.0
                            });
                        }

                        doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                            Value = num4
                        });
                        doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num5
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames, displayProductUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames,
                            new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleX)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
                        var num6 = carouselPosition.Height - (flag2 ? num2 : 0.0);
                        var num7 = num6 / carouselPosition.Height;
                        var num8 = (carouselPosition2.Height - (flag ? num2 : 0.0)) / num6;
                        var doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = num7
                        });
                        if (flag2)
                        {
                            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = 1.0
                            });
                            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = 1.0
                            });
                        }

                        doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                            Value = num7
                        });
                        doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num8
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, displayProductUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames2);
                        var num9 = carouselPosition2.HorizontalCenter - carouselPosition.HorizontalCenter;
                        var doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = 0.0
                        });
                        doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                            Value = 0.0
                        });
                        doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num9
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, displayProductUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames3);
                        var num10 = carouselPosition2.VerticalCenter - carouselPosition.VerticalCenter;
                        var doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = 0.0
                        });
                        doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                            Value = 0.0
                        });
                        doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num10
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, displayProductUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames4);
                        var doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = 0.0
                        });
                        if (flag2)
                        {
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = 1.0
                            });
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = 1.0
                            });
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.0
                            });
                        }

                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, displayProductModel);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5,
                            new PropertyPath(DisplayProductModel.AddButtonOpacityProperty));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames5);
                        var colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
                        colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = Color.FromRgb(155, 153, 156)
                        });
                        if (flag2)
                        {
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = Color.FromRgb(155, 153, 156)
                            });
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = Color.FromRgb(155, 153, 156)
                            });
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = Color.FromRgb(155, 153, 156)
                            });
                        }

                        Storyboard.SetTarget(colorAnimationUsingKeyFrames, displayProductModel);
                        Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames,
                            new PropertyPath(DisplayProductModel.ImageBorderColorProperty));
                        _carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames);
                        var thicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames();
                        thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = thickness
                        });
                        if (flag2)
                        {
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = thickness2
                            });
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = thickness2
                            });
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = thickness
                            });
                        }

                        Storyboard.SetTarget(thicknessAnimationUsingKeyFrames, displayProductModel);
                        Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames,
                            new PropertyPath(DisplayProductModel.ImageBorderThicknessProperty));
                        _carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames);
                        var doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = 0.0
                        });
                        if (flag2)
                        {
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                                Value = 2.0
                            });
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                                Value = 2.0
                            });
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.0
                            });
                        }

                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, displayProductModel);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6,
                            new PropertyPath(DisplayProductModel.ImageBorderCornerRadiusProperty));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames6);
                        var booleanAnimationUsingKeyFrames = new BooleanAnimationUsingKeyFrames();
                        booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = false
                        });
                        booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                            Value = true
                        });
                        booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = true
                        });
                        Storyboard.SetTarget(booleanAnimationUsingKeyFrames, this);
                        Storyboard.SetTargetProperty(booleanAnimationUsingKeyFrames,
                            new PropertyPath("(local:RedboxClassicProductCarouselUserControl.IsRotating)"));
                        _carouselRotationStoryboard.Children.Add(booleanAnimationUsingKeyFrames);
                    }

            _carouselRotationStoryboard.Begin();
        }

        private void carousel_animation_storyBoard_Completed(object sender, EventArgs e)
        {
            if (_carouselModel.IsAnimationOn)
            {
                RotateCarouselPositions();
                AnimateCarouselRotation();
            }
        }

        private void RotateCarouselPositions()
        {
            if (IsCarouselProperlyInitialized)
            {
                var list = new List<DisplayProductModel>();
                var num = _carouselDirection == CarouselDirection.Left ? 1 : _rotatingCarouselProducts.Count - 1;
                list.AddRange(_rotatingCarouselProducts.Skip(num));
                list.AddRange(_rotatingCarouselProducts.Take(num));
                _rotatingCarouselProducts = list;
                foreach (var displayProductUserControl in _displayProductUserControlList)
                {
                    var num2 = _displayProductUserControlList.IndexOf(displayProductUserControl);
                    displayProductUserControl.RenderTransform = null;
                    displayProductUserControl.DataContext = _rotatingCarouselProducts[num2];
                }

                _carouselModel.TitleRollupOverlayModel.BrowseItemModel =
                    _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex];
                _carouselModel.ProcessCarouselRotation(
                    _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex], null);
            }
        }

        private void InitializeCarouselPositions()
        {
            _carouselPositions.Clear();
            var num = _carouselPositionCount / 2;
            InitializeCarouselImageSizes();
            _horizontalCenterOfCenterImage = originalHorizontalCenterOfCenterImage;
            var num2 = _horizontalCenterOfCenterImage - (_carouselImageLargeSize.X / 2.0 +
                num * (_carouselImagePadding + _carouselImageSmallSize.X) - _carouselImageSmallSize.X / 2.0);
            var verticalCenterOfCenterImage = _verticalCenterOfCenterImage;
            for (var i = 0; i < _carouselPositionCount; i++)
            {
                var flag = i == _carouselPositionCount / 2;
                var carouselPosition = new CarouselPosition();
                carouselPosition.Width = flag ? _carouselImageLargeSize.X : _carouselImageSmallSize.X;
                carouselPosition.Height = flag ? _carouselImageLargeSize.Y : _carouselImageSmallSize.Y;
                carouselPosition.VerticalCenter = _verticalCenterOfCenterImage;
                carouselPosition.HorizontalCenter = num2 + _carouselPositions.Sum(x => x.Width) +
                                                    _carouselPositions.Count() * _carouselImagePadding -
                                                    _carouselImageSmallSize.X / 2.0 +
                                                    carouselPosition.Width / 2.0;
                _carouselPositions.Add(carouselPosition);
            }
        }

        private void InitializeCarouselImageSizes()
        {
            _carouselImageSmallSize = new Point(216.0, 324.0);
            _carouselImageLargeSize = new Point(280.0, 420.0);
        }

        private void DisplayProductSelectedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DisplayProductSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsRotating && IsCarouselProperlyInitialized)
            {
                var displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
                if (displayProductUserControl != null)
                {
                    if (displayProductUserControl ==
                        _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex])
                    {
                        var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
                        _carouselModel.ProcessDisplayProductModelSelected(displayProductModel, e.Parameter);
                        return;
                    }

                    if (displayProductUserControl.Tag != null)
                    {
                        if (_carouselModel != null && _carouselModel.TitleRollupOverlayModel != null)
                            _carouselModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
                        _nextCarouselDirection = (CarouselDirection)displayProductUserControl.Tag;
                        _forceRotation = true;
                        _carouselModel.IsAnimationOn = true;
                        AnimateCarouselRotation();
                    }
                }
            }
        }

        private void DisplayProductAddCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsRotating && IsCarouselProperlyInitialized)
            {
                var displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
                if (displayProductUserControl != null)
                {
                    var displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
                    _carouselModel.ProcessAddDisplayProductModel(displayProductModel, e.Parameter);
                }
            }
        }

        private DisplayProductUserControl GetDisplayProductUserControl(object source)
        {
            var frameworkElement = source as FrameworkElement;
            while (frameworkElement != null && !(frameworkElement is DisplayProductUserControl))
                frameworkElement = frameworkElement.Parent as FrameworkElement;
            return frameworkElement as DisplayProductUserControl;
        }

        private class CarouselPositionList : List<CarouselPosition>
        {
            public int CenterCarouselPositionIndex => NumberOfPositionsLeftOfCenter;

            public CarouselPosition CenterCarouselPosition => base[NumberOfPositionsLeftOfCenter];

            private int NumberOfPositionsLeftOfCenter => Count / 2;

            public int NextCenterCarouselPositionIndex(CarouselDirection direction)
            {
                return NumberOfPositionsLeftOfCenter + (direction == CarouselDirection.Left ? 1 : -1);
            }
        }

        private class CarouselPosition
        {
            public double Width { get; set; }

            public double Height { get; set; }

            public double HorizontalCenter { get; set; }

            public double VerticalCenter { get; set; }

            public double Left => HorizontalCenter - Width / 2.0;

            public double Top => VerticalCenter - Height / 2.0;
        }

        private enum CarouselDirection
        {
            Left,
            Right
        }
    }
}