using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class RedboxProductCarouselUserControl : BaseUserControl
    {
        public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating",
            typeof(bool), typeof(RedboxProductCarouselUserControl), new FrameworkPropertyMetadata(false));

        private readonly CarouselPositionList _carouselPositions = new CarouselPositionList();

        private readonly List<DisplayProductCarouselUserControl> _displayProductUserControlList =
            new List<DisplayProductCarouselUserControl>();

        private CarouselDirection _carouselDirection = CarouselDirection.Right;

        private Point _carouselImageLargeSize;

        private int _carouselImagePadding;

        private Point _carouselImageSmallSize;

        private int _carouselPositionCount;

        private List<DisplayProductModel> _carouselProducts;

        private Storyboard _carouselRotationStoryboard;

        private bool _forceRotation;

        private double _horizontalCenterOfCenterImage;

        private Color _imageBorderColor = Colors.Gray;

        private bool _isDarkMode;

        private CarouselDirection _nextCarouselDirection = CarouselDirection.Right;

        private List<DisplayProductModel> _rotatingCarouselProducts;

        private DispatcherTimer _timer;

        private bool _timerStarted;

        private double _verticalCenterOfCenterImage;

        public RedboxProductCarouselUserControl()
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

        private CarouselTestModel CarouselTestModel => DataContext as CarouselTestModel;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ConfigureCarousel();
        }

        private void ConfigureCarousel()
        {
            _displayProductUserControlList.Clear();
            CarouselContainer.Children.Clear();
            _rotatingCarouselProducts = new List<DisplayProductModel>();
            ConfigureTimer();
            InitializeCarouselPositions();
            if (CarouselTestModel != null)
            {
                CarouselTestModel.OnIsAnimationOnChanged += _carouselModel_OnIsAnimationOnChanged;
                _carouselProducts = CarouselTestModel.CarouselDisplayProductModels;
                _rotatingCarouselProducts.AddRange(_carouselProducts);
                _nextCarouselDirection = CarouselDirection.Right;
                foreach (var displayProductModel in _rotatingCarouselProducts)
                {
                    var num = _rotatingCarouselProducts.IndexOf(displayProductModel);
                    if (num < _carouselPositions.Count)
                    {
                        var carouselPosition = _carouselPositions[num];
                        var displayProductCarouselUserControl = new DisplayProductCarouselUserControl
                        {
                            DataContext = displayProductModel,
                            Margin = new Thickness(carouselPosition.Left, carouselPosition.Top, 0.0, 0.0),
                            Width = carouselPosition.Width,
                            Height = carouselPosition.Height,
                            BorderThickness = new Thickness(0.0),
                            ShaderOpacity = 0.5
                        };
                        if (num < _carouselPositions.CenterCarouselPositionIndex)
                        {
                            displayProductCarouselUserControl.Tag = CarouselDirection.Right;
                            Panel.SetZIndex(displayProductCarouselUserControl, num);
                        }
                        else if (num > _carouselPositions.CenterCarouselPositionIndex)
                        {
                            displayProductCarouselUserControl.Tag = CarouselDirection.Left;
                            Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count - num);
                        }
                        else
                        {
                            Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count);
                            displayProductCarouselUserControl.ShaderOpacity = 0.0;
                            displayProductCarouselUserControl.ShadowDepth = 10.0;
                            displayProductCarouselUserControl.ShadowOpacity = 0.75;
                            displayProductCarouselUserControl.ImageBorderThickness = new Thickness(3.0);
                            displayProductCarouselUserControl.ImageBorderColor = _imageBorderColor;
                        }

                        _displayProductUserControlList.Add(displayProductCarouselUserControl);
                        CarouselContainer.Children.Add(displayProductCarouselUserControl);
                    }
                }

                CommandManager.InvalidateRequerySuggested();
                StartTimer();
            }
        }

        private void _carouselModel_OnIsAnimationOnChanged(CarouselTestModel carouselModel)
        {
            if (carouselModel != null && carouselModel.IsAnimationOn)
            {
                StartTimer();
                return;
            }

            var carouselRotationStoryboard = _carouselRotationStoryboard;
            if (carouselRotationStoryboard != null) carouselRotationStoryboard.Stop();
            StopTimer();
            if (IsCarouselProperlyInitialized)
            {
                var displayProductCarouselUserControl =
                    _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex];
                if (displayProductCarouselUserControl != null)
                {
                    var displayProductModel = displayProductCarouselUserControl.DataContext as DisplayProductModel;
                    if (displayProductModel != null) displayProductModel.AddButtonVisibility = Visibility.Collapsed;
                }
            }
        }

        private void AnimateCarouselRotation()
        {
            _carouselDirection = _nextCarouselDirection;
            var timeSpan = TimeSpan.FromSeconds(0.0);
            var timeSpan2 = TimeSpan.FromSeconds(_forceRotation ? 0 : 0);
            var timeSpan3 = timeSpan + timeSpan2;
            var timeSpan4 = TimeSpan.FromSeconds(0.0);
            var timeSpan5 = timeSpan3 + timeSpan4;
            var timeSpan6 = TimeSpan.FromSeconds(0.4);
            var timeSpan7 = timeSpan5 + timeSpan6;
            var timeSpan8 = timeSpan5 + TimeSpan.FromMilliseconds(timeSpan6.TotalMilliseconds / 2.0);
            var timeSpan9 = timeSpan7;
            _forceRotation = false;
            var carouselRotationStoryboard = _carouselRotationStoryboard;
            if (carouselRotationStoryboard != null) carouselRotationStoryboard.Stop();
            _carouselRotationStoryboard = new Storyboard();
            _carouselRotationStoryboard.Duration = timeSpan9;
            _carouselRotationStoryboard.Completed += carousel_animation_storyBoard_Completed;
            if (IsCarouselProperlyInitialized)
                for (var i = 0; i < _carouselPositionCount; i++)
                    if ((_carouselDirection == CarouselDirection.Left && i > 0) ||
                        (_carouselDirection == CarouselDirection.Right && i < _carouselPositionCount - 1))
                    {
                        var carouselPosition = _carouselPositions[i];
                        var num = i + (_carouselDirection == CarouselDirection.Left ? -1 : 1);
                        var carouselPosition2 = _carouselPositions[num];
                        var displayProductCarouselUserControl = _displayProductUserControlList[i];
                        var translateTransform = new TranslateTransform();
                        var scaleTransform = new ScaleTransform();
                        displayProductCarouselUserControl.RenderTransform = new TransformGroup
                        {
                            Children = { scaleTransform, translateTransform }
                        };
                        var flag = carouselPosition2 == _carouselPositions.CenterCarouselPosition;
                        var flag2 = carouselPosition == _carouselPositions.CenterCarouselPosition;
                        var width = carouselPosition.Width;
                        var num2 = width / carouselPosition.Width;
                        var num3 = carouselPosition2.Width / width;
                        var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = num2
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
                            Value = num2
                        });
                        doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num3
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames, displayProductCarouselUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
                        var height = carouselPosition.Height;
                        var num4 = height / carouselPosition.Height;
                        var num5 = carouselPosition2.Height / height;
                        var doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
                        doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                            Value = num4
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
                            Value = num4
                        });
                        doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = num5
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, displayProductCarouselUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames2);
                        var num6 = carouselPosition2.HorizontalCenter - carouselPosition.HorizontalCenter;
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
                            Value = num6,
                            EasingFunction = new CubicEase
                            {
                                EasingMode = EasingMode.EaseIn
                            }
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, displayProductCarouselUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames3);
                        var num7 = carouselPosition2.VerticalCenter - carouselPosition.VerticalCenter;
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
                            Value = num7
                        });
                        Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, displayProductCarouselUserControl);
                        Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4,
                            new PropertyPath(
                                "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
                        _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames4);
                        if (flag2)
                        {
                            var doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 0.5
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5,
                                new PropertyPath("ShaderOpacity"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames5);
                            var doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 10.0
                            });
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 10.0
                            });
                            doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 0.0
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6,
                                new PropertyPath("ShadowDepth"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames6);
                            var doubleAnimationUsingKeyFrames7 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 0.75
                            });
                            doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.75
                            });
                            doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 0.0
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames7, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames7,
                                new PropertyPath("ShadowOpacity"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames7);
                            var colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = _imageBorderColor
                            });
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = _imageBorderColor
                            });
                            colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = _imageBorderColor
                            });
                            Storyboard.SetTarget(colorAnimationUsingKeyFrames, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames,
                                new PropertyPath("ImageBorderColor"));
                            _carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames);
                            var thicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames();
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = new Thickness(3.0)
                            });
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = new Thickness(3.0)
                            });
                            thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = new Thickness(0.0)
                            });
                            Storyboard.SetTarget(thicknessAnimationUsingKeyFrames, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames,
                                new PropertyPath("ImageBorderThickness"));
                            _carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames);
                        }
                        else if (flag)
                        {
                            var doubleAnimationUsingKeyFrames8 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 0.5
                            });
                            doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.5
                            });
                            doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 0.0
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames8, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames8,
                                new PropertyPath("ShaderOpacity"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames8);
                            var doubleAnimationUsingKeyFrames9 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 10.0
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames9, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames9,
                                new PropertyPath("ShadowDepth"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames9);
                            var doubleAnimationUsingKeyFrames10 = new DoubleAnimationUsingKeyFrames();
                            doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = 0.0
                            });
                            doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = 0.75
                            });
                            Storyboard.SetTarget(doubleAnimationUsingKeyFrames10, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames10,
                                new PropertyPath("ShadowOpacity"));
                            _carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames10);
                            var colorAnimationUsingKeyFrames2 = new ColorAnimationUsingKeyFrames();
                            colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = _imageBorderColor
                            });
                            colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = _imageBorderColor
                            });
                            colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = _imageBorderColor
                            });
                            Storyboard.SetTarget(colorAnimationUsingKeyFrames2, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames2,
                                new PropertyPath("ImageBorderColor"));
                            _carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames2);
                            var thicknessAnimationUsingKeyFrames2 = new ThicknessAnimationUsingKeyFrames();
                            thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                                Value = new Thickness(0.0)
                            });
                            thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                                Value = new Thickness(0.0)
                            });
                            thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
                            {
                                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                                Value = new Thickness(3.0)
                            });
                            Storyboard.SetTarget(thicknessAnimationUsingKeyFrames2, displayProductCarouselUserControl);
                            Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames2,
                                new PropertyPath("ImageBorderThickness"));
                            _carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames2);
                        }

                        var int32AnimationUsingKeyFrames = new Int32AnimationUsingKeyFrames();
                        var zindex = Panel.GetZIndex(displayProductCarouselUserControl);
                        var num8 = _carouselDirection == CarouselDirection.Left ? i - 1 : i + 1;
                        int num9;
                        if (num8 < _carouselPositions.CenterCarouselPositionIndex)
                            num9 = num8;
                        else if (num8 > _carouselPositions.CenterCarouselPositionIndex)
                            num9 = _carouselPositions.Count - num8;
                        else
                            num9 = _carouselPositions.Count;
                        int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan8),
                            Value = num9
                        });
                        int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                            Value = zindex
                        });
                        Storyboard.SetTarget(int32AnimationUsingKeyFrames, displayProductCarouselUserControl);
                        Storyboard.SetTargetProperty(int32AnimationUsingKeyFrames, new PropertyPath("(Panel.ZIndex)"));
                        _carouselRotationStoryboard.Children.Add(int32AnimationUsingKeyFrames);
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
                            new PropertyPath("(local:RedboxProductCarouselUserControl.IsRotating)"));
                        _carouselRotationStoryboard.Children.Add(booleanAnimationUsingKeyFrames);
                    }

            _carouselRotationStoryboard.Begin();
        }

        private void carousel_animation_storyBoard_Completed(object sender, EventArgs e)
        {
            var carouselTestModel = CarouselTestModel;
            if (carouselTestModel != null && carouselTestModel.IsAnimationOn)
            {
                var carouselRotationStoryboard = _carouselRotationStoryboard;
                if (carouselRotationStoryboard != null) carouselRotationStoryboard.Stop();
                _carouselRotationStoryboard = null;
                RotateCarouselPositions();
                IsRotating = false;
                StartTimer();
            }
        }

        private void StartTimer()
        {
            var carouselTestModel = CarouselTestModel;
            if (carouselTestModel != null && carouselTestModel.IsAnimationOn && !_timerStarted)
            {
                _timerStarted = true;
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timerStarted = false;
        }

        private void ConfigureTimer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Send);
            _timer.Tick += _timer_Tick;
            var timer = _timer;
            var carouselTestModel = CarouselTestModel;
            timer.Interval = TimeSpan.FromSeconds(carouselTestModel != null ? carouselTestModel.RotationDelay : 2.0);
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            StopTimer();
            AnimateCarouselRotation();
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
                foreach (var displayProductCarouselUserControl in _displayProductUserControlList)
                {
                    var num2 = _displayProductUserControlList.IndexOf(displayProductCarouselUserControl);
                    displayProductCarouselUserControl.RenderTransform = null;
                    displayProductCarouselUserControl.DataContext = _rotatingCarouselProducts[num2];
                    displayProductCarouselUserControl.ShaderOpacity = 0.5;
                    if (num2 < _carouselPositions.CenterCarouselPositionIndex)
                    {
                        Panel.SetZIndex(displayProductCarouselUserControl, num2);
                    }
                    else if (num2 > _carouselPositions.CenterCarouselPositionIndex)
                    {
                        Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count - num2);
                    }
                    else
                    {
                        Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count);
                        displayProductCarouselUserControl.ShaderOpacity = 0.0;
                        displayProductCarouselUserControl.ShadowDepth = 10.0;
                        displayProductCarouselUserControl.ShadowOpacity = 0.75;
                        displayProductCarouselUserControl.ImageBorderThickness = new Thickness(3.0);
                        displayProductCarouselUserControl.ImageBorderColor = _imageBorderColor;
                    }
                }

                var carouselTestModel = CarouselTestModel;
                if (carouselTestModel == null) return;
                carouselTestModel.ProcessCarouselRotation(
                    _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex], null);
            }
        }

        private void InitializeCarouselPositions()
        {
            InitializeCarouselImageSizes();
            _carouselPositions.Clear();
            var num = _carouselPositionCount / 2;
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
            var carouselTestModel = CarouselTestModel;
            _isDarkMode = carouselTestModel != null && carouselTestModel.IsDarkMode;
            _carouselImageSmallSize = new Point(244.0, 366.0);
            _carouselImageLargeSize = new Point(284.0, 426.0);
            _carouselPositionCount = 9;
            _carouselImagePadding = -58;
            _verticalCenterOfCenterImage = 181.0;
            _horizontalCenterOfCenterImage = 512.0;
            _imageBorderColor = _isDarkMode ? Colors.White : Color.FromRgb(155, 153, 156);
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
                        var carouselTestModel = CarouselTestModel;
                        if (carouselTestModel != null)
                            carouselTestModel.ProcessDisplayProductModelSelected(displayProductModel, e.Parameter);
                    }
                    else if (displayProductUserControl.Tag != null && CarouselTestModel != null)
                    {
                        _nextCarouselDirection = (CarouselDirection)displayProductUserControl.Tag;
                        _forceRotation = true;
                        CarouselTestModel.IsAnimationOn = true;
                        StopTimer();
                        AnimateCarouselRotation();
                    }
                }
            }

            HandleWPFHit();
        }

        private DisplayProductCarouselUserControl GetDisplayProductUserControl(object source)
        {
            var frameworkElement = source as FrameworkElement;
            while (frameworkElement != null && !(frameworkElement is DisplayProductCarouselUserControl))
                frameworkElement = frameworkElement.Parent as FrameworkElement;
            return frameworkElement as DisplayProductCarouselUserControl;
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