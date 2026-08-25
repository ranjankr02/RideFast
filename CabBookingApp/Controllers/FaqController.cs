using CabBookingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CabBookingApp.Controllers;

public class FaqController : Controller
{
    private static readonly List<FaqItem> _faqs = new()
    {
        // ── Booking Process ──────────────────────────────────────────────
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "How do I book a cab with RideFast?",
            Answer   = "Booking is simple! Use the Quick Booking widget on our Home page — select your pickup location, destination, vehicle type, and travel date & time, then click 'Book Now'. You'll be taken to the booking form where you fill in your name and mobile number to confirm the ride.",
            IsPopular = true
        },
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "Can I book a cab in advance?",
            Answer   = "Yes! RideFast supports advance bookings. You can schedule a ride days or even weeks ahead by selecting a future date and time in the booking form. This is especially useful for airport trips, outstation travel, and early-morning pickups.",
            IsPopular = true
        },
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "What details are required to make a booking?",
            Answer   = "You need to provide: your full name, a 10-digit mobile number, pickup location (Source), drop location (Destination), preferred vehicle type, travel date & time, and the agreed booking amount. All fields are mandatory to ensure a smooth ride.",
            IsPopular = false
        },
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "Can I book a cab for someone else?",
            Answer   = "Absolutely. You can enter the name and mobile number of the passenger who will be taking the ride. Our driver will contact that number for coordination. Just make sure the contact number belongs to the actual passenger.",
            IsPopular = false
        },
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "How will I receive booking confirmation?",
            Answer   = "Once a booking is saved, it appears instantly in the 'My Bookings' section with full trip details — booking ID, route, vehicle, date & time, and amount. Our team will also reach out via call or WhatsApp on the provided mobile number to confirm.",
            IsPopular = false
        },
        new() {
            Category = "Booking Process", CategoryIcon = "bi-calendar-check",
            Question = "Can I modify a booking after it is confirmed?",
            Answer   = "Yes. Go to 'My Bookings', find your booking, and click the Edit button. You can update the travel date, time, pickup/drop location, or vehicle type as long as the trip hasn't started. Please inform us at least 2 hours before departure for smooth rescheduling.",
            IsPopular = false
        },

        // ── Pricing & Payment ─────────────────────────────────────────────
        new() {
            Category = "Pricing & Payment", CategoryIcon = "bi-currency-rupee",
            Question = "How is the cab fare calculated?",
            Answer   = "Our fares are distance-based and vary by vehicle type. The Routes page shows indicative price ranges for all major routes from Patna. For custom routes, our team provides a quote based on distance, fuel rates, and vehicle selected. There are no surge charges — fares are fixed.",
            IsPopular = true
        },
        new() {
            Category = "Pricing & Payment", CategoryIcon = "bi-currency-rupee",
            Question = "Are there any hidden charges?",
            Answer   = "No hidden charges — ever. The booking amount you confirm covers the base ride. Toll charges, parking fees, and state taxes (for outstation trips) may apply separately and are communicated upfront before your trip begins.",
            IsPopular = true
        },
        new() {
            Category = "Pricing & Payment", CategoryIcon = "bi-currency-rupee",
            Question = "What payment methods are accepted?",
            Answer   = "We accept Cash, UPI (Google Pay, PhonePe, Paytm), NEFT/Bank Transfer, and major Credit/Debit cards. Payment can be made to the driver at trip end or via online transfer for advance bookings. We do not charge any transaction fees.",
            IsPopular = false
        },
        new() {
            Category = "Pricing & Payment", CategoryIcon = "bi-currency-rupee",
            Question = "Can I get a receipt or invoice for my trip?",
            Answer   = "Yes. A trip summary with booking ID, route details, vehicle type, date, and amount is available in 'My Bookings' at any time. For a formal GST invoice, please request one while booking or call our support team — we'll send it to your email within 24 hours.",
            IsPopular = false
        },
        new() {
            Category = "Pricing & Payment", CategoryIcon = "bi-currency-rupee",
            Question = "Do you offer discounts or promo codes?",
            Answer   = "Yes! We run regular offers — check the offer banner at the top of every page for active promo codes. New users get their first ride free with code FIRST100. Weekend deals, referral bonuses, and group discounts are also available.",
            IsPopular = false
        },

        // ── Vehicles & Drivers ────────────────────────────────────────────
        new() {
            Category = "Vehicles & Drivers", CategoryIcon = "bi-car-front-fill",
            Question = "What vehicle types does RideFast offer?",
            Answer   = "We offer four vehicle categories: Hatchback (4 seats, budget-friendly), Sedan (4 seats, comfortable & stylish), SUV (6 seats, spacious & premium), and Tempo Traveller (12 seats, ideal for group travel). You can select your preferred vehicle during booking.",
            IsPopular = true
        },
        new() {
            Category = "Vehicles & Drivers", CategoryIcon = "bi-car-front-fill",
            Question = "Are RideFast drivers verified and trained?",
            Answer   = "Yes. Every driver undergoes police verification, driving licence validation, and a background check before joining RideFast. Drivers also receive training in road safety, customer etiquette, and emergency response. You can rest assured your driver is fully qualified.",
            IsPopular = false
        },
        new() {
            Category = "Vehicles & Drivers", CategoryIcon = "bi-car-front-fill",
            Question = "Can I request a specific driver?",
            Answer   = "While we cannot always guarantee a specific driver due to availability, repeat customers can request a preferred driver by mentioning their name during booking. We do our best to accommodate the request, subject to the driver's schedule.",
            IsPopular = false
        },
        new() {
            Category = "Vehicles & Drivers", CategoryIcon = "bi-car-front-fill",
            Question = "What is the luggage policy?",
            Answer   = "Each vehicle type has a standard luggage allowance: Hatchback – 1 large bag; Sedan – 2 large bags; SUV – 3 large bags; Tempo Traveller – luggage per passenger. For extra luggage or oversized items, please inform us at the time of booking so we can arrange a suitable vehicle.",
            IsPopular = false
        },
        new() {
            Category = "Vehicles & Drivers", CategoryIcon = "bi-car-front-fill",
            Question = "Are the vehicles air-conditioned?",
            Answer   = "Yes, all RideFast vehicles are fully air-conditioned and regularly serviced. Vehicles are cleaned and sanitized after every trip to ensure a hygienic and comfortable experience for every passenger.",
            IsPopular = false
        },

        // ── Cancellation & Refunds ────────────────────────────────────────
        new() {
            Category = "Cancellation & Refunds", CategoryIcon = "bi-x-circle",
            Question = "How do I cancel a booking?",
            Answer   = "To cancel a booking, go to 'My Bookings', find the trip you wish to cancel, and click the Delete button. You can also call our support number at +91 98765 43210 to cancel over the phone. Please try to cancel at least 4 hours before the scheduled departure.",
            IsPopular = true
        },
        new() {
            Category = "Cancellation & Refunds", CategoryIcon = "bi-x-circle",
            Question = "What is the cancellation policy?",
            Answer   = "Cancellations made more than 24 hours before departure are free of charge. Cancellations between 4–24 hours attract a 10% cancellation fee. Cancellations within 4 hours of departure attract a 25% fee. No-shows (driver dispatched but passenger absent) are charged 50% of the fare.",
            IsPopular = false
        },
        new() {
            Category = "Cancellation & Refunds", CategoryIcon = "bi-x-circle",
            Question = "How long does a refund take?",
            Answer   = "Refunds for eligible cancellations are processed within 3–5 working days. For UPI/card payments, the refund appears in your account within 5–7 business days depending on your bank. Cash payments are refunded via UPI or bank transfer to the number on file.",
            IsPopular = false
        },

        // ── Safety ────────────────────────────────────────────────────────
        new() {
            Category = "Safety", CategoryIcon = "bi-shield-check",
            Question = "What safety measures does RideFast follow?",
            Answer   = "Safety is our top priority. All drivers are background-verified, vehicles are GPS-tracked, and every booking is logged with passenger contact details. Our team monitors active trips and is available 24/7 for emergencies. We also conduct regular vehicle safety inspections.",
            IsPopular = true
        },
        new() {
            Category = "Safety", CategoryIcon = "bi-shield-check",
            Question = "What should I do if I feel unsafe during a ride?",
            Answer   = "Call our emergency support number (+91 98765 43210) immediately or ask the driver to stop at the nearest police station or public place. Share your live location with a trusted contact. After the trip, report the incident to us — we take all safety concerns very seriously and will act promptly.",
            IsPopular = false
        },
        new() {
            Category = "Safety", CategoryIcon = "bi-shield-check",
            Question = "Are vehicles sanitized between trips?",
            Answer   = "Yes. All vehicles are cleaned and sanitized with hospital-grade disinfectant between rides. Drivers are required to maintain hand hygiene and the vehicle interiors are wiped down after every trip, including door handles, seats, and windows.",
            IsPopular = false
        },

        // ── Customer Support ──────────────────────────────────────────────
        new() {
            Category = "Customer Support", CategoryIcon = "bi-headset",
            Question = "How can I contact RideFast customer support?",
            Answer   = "You can reach us via phone at +91 98765 43210, email at support@ridefast.in, or WhatsApp. Our support team is available 24 hours a day, 7 days a week. For non-urgent queries, use the contact details in the footer — we typically respond within 30 minutes.",
            IsPopular = true
        },
        new() {
            Category = "Customer Support", CategoryIcon = "bi-headset",
            Question = "What should I do if my driver doesn't show up?",
            Answer   = "First, try calling the driver's number shared during booking confirmation. If unreachable after 5 minutes, call our support line at +91 98765 43210. We will arrange an alternate driver or provide a full refund if we cannot fulfill the booking in time.",
            IsPopular = false
        },
        new() {
            Category = "Customer Support", CategoryIcon = "bi-headset",
            Question = "Is RideFast available on weekends and public holidays?",
            Answer   = "Yes! RideFast operates 365 days a year including all weekends and public holidays. We understand that travel needs don't follow a calendar. Our booking desk and support team are staffed round the clock to assist you whenever you need a ride.",
            IsPopular = false
        },
    };

    public IActionResult Index(string? category = null, string? q = null)
    {
        var categories = _faqs.Select(f => f.Category).Distinct().ToList();

        var filtered = _faqs.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            filtered = filtered.Where(f => f.Category == category);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            filtered = filtered.Where(f =>
                f.Question.ToLower().Contains(term) ||
                f.Answer.ToLower().Contains(term));
        }

        ViewBag.Categories     = categories;
        ViewBag.SelectedCat    = category ?? "All";
        ViewBag.SearchQuery    = q ?? string.Empty;
        ViewBag.TotalCount     = filtered.Count();
        return View(filtered.ToList());
    }

    public static List<FaqItem> GetPopular(int count = 6) =>
        _faqs.Where(f => f.IsPopular).Take(count).ToList();
}
