using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab03.Domain
{
    public interface IPriceCalculator
    {
        int CalculateComplexBookingPrice();
    }

    public interface ILogger
    {
        void LogMessage(string message);
    }

    public interface IPaymentGateway
    {
        void CapturePayment(decimal amount);
    }

    // fake repository - works like a database but MUCH simpler.
    // in-memory database
    public class BookingRepository
    {
        List<Booking> bookings = new List<Booking>();

        public void AddBooking(Booking bookingToAdd)
        {
            // no-deduplication
            bookings.Add(bookingToAdd);
        }

        public void UpdateBooking(Booking bookingToUpdate)
        {
            var existingBooking = GetByReference(bookingToUpdate.Reference);

            // do changes
        }

        public Booking GetByReference(string reference)
        {
            return bookings.FirstOrDefault(s => s.Reference == reference);
        }
    }

    public class BookingService
    {
        private readonly IPaymentGateway paymentGateway;
        private readonly IPriceCalculator priceCalculator;
        private readonly BookingRepository repository;
        public BookingService(BookingRepository repository, IPaymentGateway paymentGateway, IPriceCalculator priceCalculator)
        {
            this.repository = repository; // managed dependency
            this.paymentGateway = paymentGateway; // unmanaged dependency
            this.priceCalculator = priceCalculator; // unmanaged dependency
        }

        public void CreateBooking(string reference)
        {
            repository.AddBooking(new Booking { Reference = reference });
        }

        public void AlsoConfirmsBooking(string bookingReference)
        {
            var existingBooking = repository.GetByReference(bookingReference);

            existingBooking.Confirm();

            repository.UpdateBooking(existingBooking);
        }
        public void ConfirmBooking(string bookingReference)
        {
            var existingBooking = repository.GetByReference(bookingReference);

            // orchestration
            try
            {
                this.paymentGateway.CapturePayment(5);
                // do some things with the booking
                existingBooking.Confirm();
            }
            catch (Exception)
            {
                // tell someone
            }

            repository.UpdateBooking(existingBooking);
        }
    }

    public class Booking
    {
        public DateTime ConfirmedAt {get; private set; }

        public enum BookingStatus
        {
            Draft,
            Reserved,
            Confirmed
        }

        // encapsulation
        // protecting the invariants
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ConfirmedAt = DateTime.Now;
        }

        public BookingStatus Status { get; private set; }
        public string Reference { get; set; }
    }
}