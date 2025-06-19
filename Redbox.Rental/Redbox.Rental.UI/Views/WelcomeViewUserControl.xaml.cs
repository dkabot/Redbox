using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    public partial class WelcomeViewUserControl : UserControl
    {
        private readonly List<UserControl> _controlList = new List<UserControl>();

        private int _nextLocation = 1;

        public WelcomeViewUserControl()
        {
            InitializeComponent();
        }

        private void OnAnimationFinish(object sender, EventArgs e)
        {
            _nextLocation = (_nextLocation + 1) % _controlList.Count();
            grid1.Children.Clear();
            grid1.Children.Add(grid2.Children[0]);
            grid2.Children.Clear();
            grid2.Children.Add(_controlList[_nextLocation]);
            Dispatcher.Invoke(delegate { sb.Begin(); });
        }

        private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var welcomeViewModel = DataContext as WelcomeViewModel;
            if (welcomeViewModel == null) return;
            welcomeViewModel.ProcessNavigateHelp();
        }

        private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NagivateStartView(true);
        }

        private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NagivateStartView(false, true);
        }

        private void GeneralClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NagivateStartView();
        }

        private void NagivateStartView(bool inSpanish = false, bool inAda = false)
        {
            var welcomeViewModel = DataContext as WelcomeViewModel;
            if (welcomeViewModel == null) return;
            welcomeViewModel.ProcessNavigateStart(inSpanish, inAda);
        }
    }
}