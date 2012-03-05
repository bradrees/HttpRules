using System;
using System.Windows;

namespace RulesWPF
{
    public class RequestChartPoint : DependencyObject
    {
        private static readonly DependencyProperty _date = DependencyProperty.Register("Date", typeof (DateTime),
                                                                                       typeof (RequestChartPoint));

        private static readonly DependencyProperty _maxConcurrent = DependencyProperty.Register("MaxConcurrent",
                                                                                                typeof (int),
                                                                                                typeof (
                                                                                                    RequestChartPoint));

        private static readonly DependencyProperty _value = DependencyProperty.Register("Value", typeof (int),
                                                                                        typeof (RequestChartPoint));

        public RequestChartPoint(int value, int maxConcurrent)
        {
            this.MaxConcurrent = maxConcurrent;
            this.Value = value;
            this.Date = DateTime.Now;
        }

        public DateTime Date
        {
            get { return (DateTime) GetValue(_date); }
            set { SetValue(_date, value); }
        }

        public int MaxConcurrent
        {
            get { return (int) GetValue(_maxConcurrent); }
            set { SetValue(_maxConcurrent, value); }
        }

        public int Value
        {
            get { return (int) GetValue(_value); }
            set { SetValue(_value, value); }
        }
    }
}