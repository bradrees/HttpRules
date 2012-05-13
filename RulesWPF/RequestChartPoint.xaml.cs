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

    public class DomainMapPoint : DependencyObject
    {
        private static readonly DependencyProperty _date = DependencyProperty.Register("Date", typeof(DateTime),
                                                                                       typeof(DomainMapPoint));

        private static readonly DependencyProperty _maxConcurrent = DependencyProperty.Register("ResponseCode",
                                                                                                typeof(int),
                                                                                                typeof(
                                                                                                    DomainMapPoint));

        private static readonly DependencyProperty _length = DependencyProperty.Register("Length", typeof(long),
                                                                                        typeof(DomainMapPoint));

        private static readonly DependencyProperty _domain = DependencyProperty.Register("Domain", typeof(string),
                                                                                       typeof(DomainMapPoint));

        public DomainMapPoint(int responseCode, long length, string domain)
        {
            this.ResponseCode = responseCode;
            this.Length = length;
            this.Domain = domain;
            this.Date = DateTime.Now;
        }

        public DateTime Date
        {
            get { return (DateTime)GetValue(_date); }
            set { SetValue(_date, value); }
        }

        public int ResponseCode
        {
            get { return (int)GetValue(_maxConcurrent); }
            set { SetValue(_maxConcurrent, value); }
        }

        public string ResponseCodeString
        {
            get { return GetValue(_maxConcurrent).ToString(); }
        }

        public long Length
        {
            get { return (long)GetValue(_length); }
            set { SetValue(_length, value); }
        }

        public string LengthString
        {
            get { return GetValue(_length).ToString(); }
        }

        public string Domain
        {
            get { return (string)GetValue(_domain); }
            set { SetValue(_domain, value); }
        }
    }
}