using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FileEncryptor.WPF.Views.Windows
{
    public partial class ProgressWindow
    {
        private IProgress<double> _progressInformation;

        private IProgress<string> _statusInformation;

        private IProgress<(double Percent, string Message)> _progressAndStatusInformation;

        #region Status : string - Статус операции

        /// <summary>
        /// Статус операции.
        /// </summary>
        public static readonly DependencyProperty StatusProperty
            = DependencyProperty.Register(
                nameof(Status),
                typeof(string),
                typeof(ProgressWindow),
                new PropertyMetadata(default(string)));

        /// <summary>
        /// Статус операции.
        /// </summary>
        [Description("Статус операции")]
        // [Category("")]
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        #endregion

        #region ProgressValue : double - Значение прогресса операции

        /// <summary>
        /// Значение прогресса операции.
        /// </summary>
        public static readonly DependencyProperty ProgressValueProperty
            = DependencyProperty.Register(
                nameof(ProgressValue),
                typeof(double),
                typeof(ProgressWindow),
                new PropertyMetadata(double.NaN, OnProgressValueChanged));

        private static void OnProgressValueChanged(DependencyObject d, 
            DependencyPropertyChangedEventArgs e)
        {
            var progressValue = (double)e.NewValue;
            ProgressBar progressView = ((ProgressWindow)d).ProgressView;
            progressView.Value = progressValue;
            progressView.IsIndeterminate = double.IsNaN(progressValue);
        }

        /// <summary>
        /// Значение прогресса операции.
        /// </summary>
        [Description("Значение прогресса операции")]
        // [Category("")]
        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        #endregion

        public IProgress<double> ProgressInformation => _progressInformation ??= 
            new Progress<double>(p => ProgressValue = p);

        public IProgress<string> StatusInformation => _statusInformation ??= 
            new Progress<string>(status => Status = status);

        public IProgress<(double Percent, string Message)> ProgressAndStatusInformation => 
            _progressAndStatusInformation ??= new Progress<(double Percent, string Message)>(
                p=> 
                {
                    ProgressValue = p.Percent;
                    Status = p.Message;
                });

        private CancellationTokenSource _cancellation;

        public CancellationToken CancellationToken
        {
            get 
            {
                if (_cancellation != null)
                {
                    return _cancellation.Token;
                }
                
                _cancellation = new CancellationTokenSource(); 
                CancelButton.IsEnabled = true;

                return _cancellation.Token;
            }
        }
        
        public ProgressWindow() => InitializeComponent();

        private void OnCancelButtonClick(object sender, RoutedEventArgs e) => _cancellation?.Cancel();
    }
}
