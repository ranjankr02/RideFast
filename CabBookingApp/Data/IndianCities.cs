using Microsoft.AspNetCore.Mvc.Rendering;

namespace CabBookingApp.Data;

/// <summary>
/// Reference list of Indian cities used to populate the pickup/drop selects.
/// Grouped by state so the dropdowns can render &lt;optgroup&gt;s.
/// </summary>
public static class IndianCities
{
    public static readonly IReadOnlyList<(string State, string[] Cities)> ByState = new (string, string[])[]
    {
        ("Andaman & Nicobar Islands", new[] { "Port Blair" }),
        ("Andhra Pradesh", new[] { "Amaravati", "Anantapur", "Chittoor", "Eluru", "Guntur", "Kadapa", "Kakinada", "Kurnool", "Machilipatnam", "Nellore", "Ongole", "Rajahmundry", "Srikakulam", "Tirupati", "Vijayawada", "Visakhapatnam", "Vizianagaram" }),
        ("Arunachal Pradesh", new[] { "Itanagar", "Naharlagun", "Pasighat", "Tawang", "Ziro" }),
        ("Assam", new[] { "Bongaigaon", "Dhubri", "Dibrugarh", "Diphu", "Goalpara", "Guwahati", "Jorhat", "Karimganj", "Nagaon", "North Lakhimpur", "Silchar", "Sivasagar", "Tezpur", "Tinsukia" }),
        ("Bihar", new[] { "Ara", "Aurangabad", "Begusarai", "Bettiah", "Bhagalpur", "Bihar Sharif", "Buxar", "Chhapra", "Darbhanga", "Gaya", "Gopalganj", "Hajipur", "Jamui", "Katihar", "Kishanganj", "Madhubani", "Motihari", "Munger", "Muzaffarpur", "Nawada", "Patna", "Purnia", "Saharsa", "Samastipur", "Sasaram", "Sitamarhi", "Siwan" }),
        ("Chandigarh", new[] { "Chandigarh" }),
        ("Chhattisgarh", new[] { "Ambikapur", "Bhilai", "Bilaspur", "Durg", "Jagdalpur", "Korba", "Raigarh", "Raipur", "Rajnandgaon" }),
        ("Dadra & Nagar Haveli and Daman & Diu", new[] { "Daman", "Diu", "Silvassa" }),
        ("Delhi NCR", new[] { "Delhi", "Faridabad", "Ghaziabad", "Greater Noida", "Gurugram", "New Delhi", "Noida" }),
        ("Goa", new[] { "Madgaon", "Mapusa", "Panaji", "Ponda", "Vasco da Gama" }),
        ("Gujarat", new[] { "Ahmedabad", "Anand", "Bharuch", "Bhavnagar", "Bhuj", "Gandhidham", "Gandhinagar", "Jamnagar", "Junagadh", "Mehsana", "Morbi", "Nadiad", "Navsari", "Palanpur", "Porbandar", "Rajkot", "Surat", "Vadodara", "Valsad", "Vapi", "Veraval" }),
        ("Haryana", new[] { "Ambala", "Bahadurgarh", "Bhiwani", "Hisar", "Jind", "Kaithal", "Karnal", "Kurukshetra", "Panipat", "Rewari", "Rohtak", "Sirsa", "Sonipat", "Yamunanagar" }),
        ("Himachal Pradesh", new[] { "Bilaspur", "Chamba", "Dharamshala", "Kullu", "Manali", "Mandi", "Palampur", "Shimla", "Solan", "Una" }),
        ("Jammu & Kashmir", new[] { "Anantnag", "Baramulla", "Jammu", "Katra", "Srinagar", "Udhampur" }),
        ("Jharkhand", new[] { "Bokaro Steel City", "Chaibasa", "Deoghar", "Dhanbad", "Dumka", "Giridih", "Hazaribagh", "Jamshedpur", "Ramgarh", "Ranchi" }),
        ("Karnataka", new[] { "Ballari", "Belagavi", "Bengaluru", "Bidar", "Chikkamagaluru", "Chitradurga", "Davanagere", "Dharwad", "Gadag", "Hassan", "Hubballi", "Kalaburagi", "Kolar", "Mandya", "Mangaluru", "Mysuru", "Raichur", "Shivamogga", "Tumakuru", "Udupi", "Vijayapura" }),
        ("Kerala", new[] { "Alappuzha", "Kannur", "Kasaragod", "Kochi", "Kollam", "Kottayam", "Kozhikode", "Malappuram", "Palakkad", "Pathanamthitta", "Thalassery", "Thiruvananthapuram", "Thrissur" }),
        ("Ladakh", new[] { "Kargil", "Leh" }),
        ("Lakshadweep", new[] { "Kavaratti" }),
        ("Madhya Pradesh", new[] { "Bhopal", "Burhanpur", "Chhindwara", "Dewas", "Gwalior", "Indore", "Jabalpur", "Katni", "Khandwa", "Morena", "Murwara", "Ratlam", "Rewa", "Sagar", "Satna", "Shivpuri", "Singrauli", "Ujjain", "Vidisha" }),
        ("Maharashtra", new[] { "Ahmednagar", "Akola", "Amravati", "Aurangabad", "Bhiwandi", "Chandrapur", "Dhule", "Jalgaon", "Kalyan", "Kolhapur", "Latur", "Mumbai", "Nagpur", "Nanded", "Nashik", "Navi Mumbai", "Panvel", "Parbhani", "Pune", "Sangli", "Satara", "Solapur", "Thane", "Vasai-Virar", "Wardha" }),
        ("Manipur", new[] { "Churachandpur", "Imphal", "Thoubal" }),
        ("Meghalaya", new[] { "Jowai", "Shillong", "Tura" }),
        ("Mizoram", new[] { "Aizawl", "Champhai", "Lunglei" }),
        ("Nagaland", new[] { "Dimapur", "Kohima", "Mokokchung" }),
        ("Odisha", new[] { "Balasore", "Baripada", "Berhampur", "Bhadrak", "Bhubaneswar", "Cuttack", "Jeypore", "Puri", "Rourkela", "Sambalpur" }),
        ("Puducherry", new[] { "Karaikal", "Puducherry", "Yanam" }),
        ("Punjab", new[] { "Amritsar", "Barnala", "Bathinda", "Firozpur", "Hoshiarpur", "Jalandhar", "Kapurthala", "Ludhiana", "Moga", "Mohali", "Pathankot", "Patiala", "Phagwara", "Sangrur" }),
        ("Rajasthan", new[] { "Ajmer", "Alwar", "Banswara", "Barmer", "Beawar", "Bharatpur", "Bhilwara", "Bikaner", "Bundi", "Chittorgarh", "Churu", "Dausa", "Hanumangarh", "Jaipur", "Jaisalmer", "Jhunjhunu", "Jodhpur", "Kota", "Mount Abu", "Pali", "Sikar", "Sri Ganganagar", "Udaipur" }),
        ("Sikkim", new[] { "Gangtok", "Gyalshing", "Namchi" }),
        ("Tamil Nadu", new[] { "Chennai", "Coimbatore", "Cuddalore", "Dindigul", "Erode", "Hosur", "Kanchipuram", "Kanyakumari", "Karur", "Kumbakonam", "Madurai", "Nagercoil", "Ooty", "Salem", "Thanjavur", "Thoothukudi", "Tiruchirappalli", "Tirunelveli", "Tiruppur", "Vellore" }),
        ("Telangana", new[] { "Adilabad", "Hyderabad", "Karimnagar", "Khammam", "Mahbubnagar", "Nalgonda", "Nizamabad", "Ramagundam", "Secunderabad", "Siddipet", "Suryapet", "Warangal" }),
        ("Tripura", new[] { "Agartala", "Dharmanagar", "Udaipur" }),
        ("Uttar Pradesh", new[] { "Agra", "Aligarh", "Ayodhya", "Azamgarh", "Bareilly", "Basti", "Bijnor", "Bulandshahr", "Deoria", "Etawah", "Faizabad", "Farrukhabad", "Firozabad", "Ghazipur", "Gonda", "Gorakhpur", "Hapur", "Jaunpur", "Jhansi", "Kanpur", "Lucknow", "Mathura", "Mau", "Meerut", "Mirzapur", "Moradabad", "Muzaffarnagar", "Prayagraj", "Raebareli", "Rampur", "Saharanpur", "Shahjahanpur", "Sitapur", "Sultanpur", "Varanasi" }),
        ("Uttarakhand", new[] { "Dehradun", "Haldwani", "Haridwar", "Kashipur", "Nainital", "Rishikesh", "Roorkee", "Rudrapur" }),
        ("West Bengal", new[] { "Asansol", "Bardhaman", "Berhampore", "Darjeeling", "Durgapur", "Haldia", "Howrah", "Jalpaiguri", "Kharagpur", "Kolkata", "Malda", "Siliguri" }),
    };

    /// <summary>Every city name, sorted alphabetically and de-duplicated.</summary>
    public static readonly IReadOnlyList<string> All =
        ByState.SelectMany(s => s.Cities)
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
               .ToList();

    private static readonly HashSet<string> Lookup =
        new(All, StringComparer.OrdinalIgnoreCase);

    public static bool IsKnown(string? city) =>
        !string.IsNullOrWhiteSpace(city) && Lookup.Contains(city.Trim());

    /// <summary>Returns the canonically-cased city name, or the trimmed input when unknown.</summary>
    public static string Canonical(string? city)
    {
        var trimmed = (city ?? string.Empty).Trim();
        return Lookup.TryGetValue(trimmed, out var match) ? match : trimmed;
    }

    /// <summary>
    /// Options for a city &lt;select&gt;, grouped by state. A <paramref name="selected"/> value that
    /// is not in the list (e.g. an older free-text booking) is kept as an "Other" option so
    /// editing a booking never silently drops its city.
    /// </summary>
    public static List<SelectListItem> SelectItems(string? selected = null)
    {
        var items = new List<SelectListItem>();

        foreach (var (state, cities) in ByState)
        {
            var group = new SelectListGroup { Name = state };
            foreach (var city in cities)
                items.Add(new SelectListItem(city, city) { Group = group });
        }

        var current = (selected ?? string.Empty).Trim();
        if (current.Length > 0 && !Lookup.Contains(current))
            items.Add(new SelectListItem(current, current) { Group = new SelectListGroup { Name = "Other" } });

        return items;
    }
}
