using System;
using Microsoft.Extensions.Internal;
using NodaTime;

namespace Lab02.Domain
{
    public class User : Party
    {
        public int Age { get; }

        public User(int age)
        {
            this.Age = age;
        }
    }

    public class Price
    {
        public Money Amount { get; set; }
        public Percent Vat {get;set;}

        public Price(Money amount, Percent vat)
        {
            this.Amount = amount;
            this.Vat = vat;
        }
    }

    public class Percent
    {
        public decimal Value { get; }
        private Percent(decimal percent)
        {
            if (percent < 0 || percent > 1) throw new ArgumentOutOfRangeException(nameof(percent),
                   "Percent values should be between 0 and 1.");

            this.Value = percent;
        }

        public static Percent FromDecimal(decimal d)
        {
            return new Percent(d);
        }

        public static implicit operator decimal(Percent p) => p.Value;
        public static implicit operator Percent(decimal d) => new Percent(d);
    }

    public class Party
    {
        public List<Booking> RecentBookings { get; } = new List<Booking>();

        public void AddBooking(Booking bookingToAdd)
        {
            if (RecentBookings.Count() > 1)
                RecentBookings.RemoveAt(0);

            RecentBookings.Add(bookingToAdd);
        }
    }

    public class Money
    {
        public decimal Amount { get; }
        public Currency Currency { get; }

        private Money(Currency currency, decimal amount)
        {
            this.Currency = currency;
            this.Amount = amount;
        }

        public static Money Create(Currency currency, decimal amount)
        {
            return new Money(currency, amount);
        }

        public static implicit operator decimal(Money m) => m.Amount;
    }

    public class Location { 
        public LocalTime OpeningTime {get; }

        public Location(LocalTime openingTime)
        {
            this.OpeningTime = openingTime;
        }
    }

    public class Currency
    {
        public string Value {get;}
        public Currency(string currencyCode)
        {
            if (currencyCode == null) throw new ArgumentNullException(nameof(currencyCode));

            if (currencyCode.Length != 3)
                throw new ArgumentException(nameof(currencyCode));
        }

        public static implicit operator string(Currency c) => c.Value;
        public static implicit operator Currency(string c) => new Currency(c);
    }

    public class Company : Party
    {
        public decimal DiscountPercent { get; }

        public Company(Percent discount)
        {
            this.DiscountPercent = discount;
        }
    }

    public class Clock : ISystemClock
    {
        private DateTime currentTimeUtc;
        public Clock(DateTime currentTimeUtc)
        {
            this.currentTimeUtc = currentTimeUtc;
        }

        public DateTimeOffset UtcNow => currentTimeUtc;
    }

    public class Booking
    {
        public Party BookingParty { get; }
        public Location Location {get; }
        public bool IsCancelled { get; private set; }
        public DateTime StartTime { get; }
        ISystemClock systemClock;
        private Money basicPrice;
        private Percent vatRate;

        public static Booking Create(DateTime startTime, int durationMinutes, Money price, Percent vatRate, Party bookingParty, Location location, ISystemClock systemClock)
        {
            if (durationMinutes > 60) throw new InvalidOperationException("Bookings must not exceed 60 minutes.");
            if (new LocalTime(startTime.Hour, startTime.Minute) < location.OpeningTime) throw new InvalidOperationException("Bookings must not exceed 60 minutes.");
            return new Booking(startTime, price, vatRate, bookingParty, location, systemClock);
        }

        private Booking(DateTime startTime, Money price, Percent vatRate, Party bookingParty, Location location, ISystemClock systemClock)
        {
            this.StartTime = startTime;
            this.BookingParty = bookingParty;
            this.basicPrice = price;
            this.vatRate = vatRate;
            this.Location = location;
            this.systemClock = systemClock;

            bookingParty.AddBooking(this);
        }

        public TimeSpan Duration { get; }

        public decimal GetPrice() => this.BookingParty switch
        {
            User u when u.Age >= 70 => PensionersPrice,
            User u when u.Age < 12 => ChildPrice,
            Company c => CompanyPrice,
            _ => RegularPrice
        };

        private decimal BasicPriceIncVat => basicPrice + basicPrice * vatRate;
        private decimal PensionersPrice => 0;
        private decimal ChildPrice => BasicPriceIncVat * Percent.FromDecimal(0.5m);
        private decimal CompanyPrice => basicPrice - (basicPrice * ((Company)this.BookingParty).DiscountPercent);
        private decimal RegularPrice => BasicPriceIncVat;

        public void Cancel()
        {
            var timeRemainingUntilBookingTime = systemClock.UtcNow - StartTime;
            if (timeRemainingUntilBookingTime.TotalMinutes < 60)
                throw new InvalidOperationException("Booking may not be cancelled.");

            this.IsCancelled = true;
        }
    }
}