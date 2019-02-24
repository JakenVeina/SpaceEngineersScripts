using System;

namespace IngameScript
{
    public partial class Program
    {
        public interface IDateTimeProvider
        {
            DateTime Now { get; }
        }

        public partial class DateTimeProvider
            : IDateTimeProvider
        {
            public DateTime Now
                => DateTime.Now;
        }
    }
}
