using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Timer = System.Timers.Timer;

namespace Redbox.Rental.UI.Controls
{
    public class RentalUserControl : UserControl
    {
        private int _isRunning;

        public RentalUserControl()
        {
            var service = ServiceLocator.Instance.GetService<IApplicationState>();
            IsABEOn = service != null && service.IsABEOn;
            IsDesign = DesignerProperties.GetIsInDesignMode(this);
        }

        private static Timer DelayTimer { get; set; }

        public bool IsDesign { get; set; }

        public bool IsABEOn { get; set; }

        public IActor Actor { get; set; }

        protected bool IsMultiClickBlocked
        {
            get => DelayTimer != null;
            set => SetDelayTimer(value);
        }

        public event WPFHitHandler OnWPFHit;

        public void HandleWPFHit()
        {
            if (Actor != null)
            {
                var onWPFHit = OnWPFHit;
                if (onWPFHit != null) onWPFHit(Actor);
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IRenderingService>();
                if (service != null) service.ActiveScene.PrcoessWPFHit();
            }

            PostHealthActivity();
        }

        protected void PostHealthActivity()
        {
            var service = ServiceLocator.Instance.GetService<ITouchScreenHealth>();
            if (service != null) service.PostActivity();
            var service2 = ServiceLocator.Instance.GetService<IViewHealth>();
            if (service2 == null) return;
            service2.PostActivity("Button Press");
        }

        protected void SetDelayTimer(bool setTimer)
        {
            if (setTimer)
            {
                DelayTimer = new Timer(500.0);
                DelayTimer.Elapsed += delegate { DisposeDelayTimer(); };
                DelayTimer.Start();
                return;
            }

            DisposeDelayTimer();
        }

        protected void DisposeDelayTimer()
        {
            if (DelayTimer != null)
            {
                DelayTimer.Stop();
                DelayTimer.Dispose();
                DelayTimer = null;
            }
        }

        protected void Log(string message, string prefix = "")
        {
            LogHelper.Instance.Log(string.Format("{0}WPF View {1}: {2}", prefix, GetType().Name, message));
        }

        protected static IEnumerable<T> FindLogicalChildren<T>(DependencyObject dependencyObj)
            where T : DependencyObject
        {
            if (dependencyObj != null)
            {
                if (dependencyObj is T) yield return dependencyObj as T;
                foreach (var dependencyObject in
                         LogicalTreeHelper.GetChildren(dependencyObj).OfType<DependencyObject>())
                {
                    foreach (var t in FindLogicalChildren<T>(dependencyObject)) yield return t;
                    IEnumerator<T> enumerator2 = null;
                }

                IEnumerator<DependencyObject> enumerator = null;
            }

            yield break;
        }

        protected static DependencyProperty CreateDependencyProperty<TOwner>(string propertyName, Type propertyType,
            object defaultValue = null, bool affectsRender = true)
        {
            return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner),
                new FrameworkPropertyMetadata(defaultValue)
                {
                    AffectsRender = affectsRender
                });
        }

        protected void RunCommandOnce(Action action)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                LogHelper.Instance.Log("RunCommandOnce - Skipping Click...");
                return;
            }

            if (action != null) action();
        }

        protected void CompleteRunOnce()
        {
            _isRunning = 0;
        }
    }
}