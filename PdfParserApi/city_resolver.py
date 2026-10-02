# Master dictionary of Indian Districts, Major Cities, and PIN Prefix Mappings
import re
from typing import Optional

# Comprehensive 3-digit PIN prefix mapping for India
PIN_PREFIX_MAP = {
    # Delhi
    "110": "New Delhi",
    # Haryana
    "121": "Faridabad", "122": "Gurugram", "123": "Rewari", "124": "Rohtak", "125": "Hisar",
    "126": "Jind", "127": "Bhiwani", "131": "Sonipat", "132": "Panipat", "133": "Ambala",
    "134": "Panchkula", "135": "Yamunanagar", "136": "Kurukshetra",
    # Punjab & Chandigarh
    "140": "Mohali", "141": "Ludhiana", "142": "Moga", "143": "Amritsar", "144": "Jalandhar",
    "145": "Pathankot", "146": "Hoshiarpur", "147": "Patiala", "148": "Sangrur", "151": "Bathinda",
    "152": "Firozpur", "160": "Chandigarh",
    # Himachal Pradesh
    "171": "Shimla", "172": "Solan", "173": "Solan", "174": "Bilaspur", "175": "Mandi",
    "176": "Kangra", "177": "Hamirpur",
    # Jammu & Kashmir & Ladakh
    "180": "Jammu", "181": "Samba", "182": "Udhampur", "184": "Kathua", "185": "Rajouri",
    "190": "Srinagar", "191": "Budgam", "192": "Anantnag", "193": "Baramulla", "194": "Leh",
    # Uttar Pradesh & Uttarakhand
    "201": "Noida", "202": "Aligarh", "203": "Bulandshahr", "204": "Hathras", "205": "Mainpuri",
    "206": "Etawah", "207": "Etah", "208": "Kanpur", "209": "Unnao", "210": "Banda",
    "211": "Prayagraj", "212": "Kaushambi", "221": "Varanasi", "222": "Jaunpur", "223": "Azamgarh",
    "224": "Ayodhya", "225": "Barabanki", "226": "Lucknow", "227": "Amethi", "228": "Sultanpur",
    "229": "Raebareli", "230": "Pratapgarh", "231": "Mirzapur", "232": "Chandauli", "233": "Ghazipur",
    "241": "Hardoi", "242": "Shahjahanpur", "243": "Bareilly", "244": "Moradabad", "245": "Hapur",
    "246": "Bijnor", "247": "Saharanpur", "248": "Dehradun", "249": "Haridwar", "250": "Meerut",
    "251": "Muzaffarnagar", "261": "Sitapur", "262": "Lakhimpur Kheri", "263": "Nainital",
    "271": "Gonda", "272": "Basti", "273": "Gorakhpur", "274": "Deoria", "281": "Mathura",
    "282": "Agra", "283": "Firozabad", "284": "Jhansi", "285": "Jalaun",
    # Rajasthan
    "301": "Alwar", "302": "Jaipur", "303": "Jaipur", "304": "Tonk", "305": "Ajmer",
    "311": "Bhilwara", "312": "Chittorgarh", "313": "Udaipur", "314": "Dungarpur", "321": "Bharatpur",
    "322": "Sawai Madhopur", "324": "Kota", "325": "Baran", "326": "Jhalawar", "331": "Churu",
    "332": "Sikar", "333": "Jhunjhunu", "334": "Bikaner", "335": "Sri Ganganagar", "341": "Nagaur",
    "342": "Jodhpur", "343": "Jalore", "344": "Barmer", "345": "Jaisalmer",
    # Gujarat & Daman/Diu/Dadra
    "360": "Rajkot", "361": "Jamnagar", "362": "Junagadh", "363": "Surendranagar", "364": "Bhavnagar",
    "365": "Amreli", "370": "Kutch", "380": "Ahmedabad", "382": "Gandhinagar", "383": "Sabarkantha",
    "384": "Mehsana", "385": "Banaskantha", "387": "Kheda", "388": "Anand", "389": "Panchmahal",
    "390": "Vadodara", "391": "Vadodara", "392": "Bharuch", "393": "Bharuch", "394": "Surat",
    "395": "Surat", "396": "Valsad",
    # Maharashtra & Goa
    "400": "Mumbai", "401": "Thane", "402": "Raigad", "403": "Goa", "410": "Pune", "411": "Pune",
    "412": "Pune", "413": "Solapur", "414": "Ahmednagar", "415": "Satara", "416": "Kolhapur",
    "421": "Kalyan", "422": "Nashik", "423": "Malegaon", "424": "Dhule", "425": "Jalgaon",
    "431": "Chhatrapati Sambhajinagar", "440": "Nagpur", "441": "Nagpur", "442": "Wardha",
    "443": "Buldhana", "444": "Amravati", "445": "Yavatmal",
    # Madhya Pradesh & Chhattisgarh
    "450": "Khandwa", "451": "Khargone", "452": "Indore", "453": "Indore", "454": "Dhar",
    "455": "Dewas", "456": "Ujjain", "457": "Ratlam", "458": "Mandsaur", "460": "Betul",
    "461": "Hoshangabad", "462": "Bhopal", "464": "Vidisha", "465": "Shajapur", "466": "Sehore",
    "470": "Sagar", "471": "Chhatarpur", "472": "Tikamgarh", "473": "Guna", "474": "Gwalior",
    "475": "Gwalior", "476": "Morena", "477": "Bhind", "480": "Chhindwara", "481": "Balaghat",
    "482": "Jabalpur", "483": "Jabalpur", "484": "Shahdol", "485": "Satna", "486": "Rewa",
    "490": "Bhilai", "491": "Durg", "492": "Raipur", "493": "Raipur", "494": "Jagdalpur",
    "495": "Bilaspur", "496": "Raigarh", "497": "Ambikapur",
    # Andhra Pradesh & Telangana
    "500": "Hyderabad", "501": "Rangareddy", "502": "Sangareddy", "503": "Nizamabad", "504": "Adilabad",
    "505": "Karimnagar", "506": "Warangal", "507": "Khammam", "508": "Nalgonda", "509": "Mahbubnagar",
    "515": "Anantapur", "516": "Kadapa", "517": "Tirupati", "518": "Kurnool", "520": "Vijayawada",
    "521": "Krishna", "522": "Guntur", "523": "Prakasam", "524": "Nellore", "530": "Visakhapatnam",
    "531": "Visakhapatnam", "532": "Srikakulam", "533": "East Godavari", "534": "West Godavari",
    "535": "Vizianagaram",
    # Karnataka
    "560": "Bengaluru", "561": "Bengaluru Rural", "562": "Bengaluru Rural", "563": "Kolar",
    "570": "Mysuru", "571": "Mandya", "572": "Tumakuru", "573": "Hassan", "574": "Dakshina Kannada",
    "575": "Mangaluru", "576": "Udupi", "577": "Shivamogga", "580": "Hubballi-Dharwad",
    "581": "Uttara Kannada", "582": "Gadag", "583": "Ballari", "584": "Raichur", "585": "Kalaburagi",
    "586": "Vijayapura", "587": "Bagalkote", "590": "Belagavi", "591": "Belagavi",
    # Tamil Nadu, Puducherry & Kerala
    "600": "Chennai", "601": "Tiruvallur", "602": "Kanchipuram", "603": "Chengalpattu", "604": "Viluppuram",
    "605": "Puducherry", "606": "Tiruvannamalai", "607": "Cuddalore", "608": "Cuddalore", "609": "Mayiladuthurai",
    "610": "Tiruvarur", "611": "Nagapattinam", "612": "Thanjavur", "613": "Thanjavur", "614": "Pudukkottai",
    "620": "Tiruchirappalli", "621": "Perambalur", "622": "Pudukkottai", "624": "Dindigul", "625": "Madurai",
    "626": "Virudhunagar", "627": "Tirunelveli", "628": "Thoothukudi", "629": "Kanyakumari", "630": "Sivaganga",
    "631": "Ranipet", "632": "Vellore", "635": "Krishnagiri", "636": "Salem", "637": "Namakkal",
    "638": "Erode", "639": "Karur", "641": "Coimbatore", "642": "Tiruppur", "643": "Nilgiris",
    "670": "Kannur", "671": "Kasaragod", "673": "Kozhikode", "676": "Malappuram", "678": "Palakkad",
    "679": "Palakkad", "680": "Thrissur", "682": "Kochi", "683": "Ernakulam", "685": "Idukki",
    "686": "Kottayam", "688": "Alappuzha", "689": "Pathanamthitta", "690": "Alappuzha", "691": "Kollam",
    "695": "Thiruvananthapuram",
    # West Bengal, Odisha & North-East
    "700": "Kolkata", "711": "Howrah", "712": "Hooghly", "713": "Asansol", "721": "Midnapore",
    "722": "Bankura", "723": "Purulia", "731": "Birbhum", "732": "Malda", "733": "Uttar Dinajpur",
    "734": "Siliguri", "735": "Jalpaiguri", "736": "Cooch Behar", "741": "Nadia", "742": "Murshidabad",
    "743": "North 24 Parganas", "744": "Port Blair", "751": "Bhubaneswar", "752": "Puri", "753": "Cuttack", "754": "Paradip",
    "755": "Jajpur", "756": "Baleswar", "757": "Mayurbhanj", "758": "Kendujhar", "759": "Dhenkanal",
    "760": "Ganjam", "761": "Ganjam", "764": "Koraput", "765": "Rayagada", "766": "Kalahandi",
    "767": "Balangir", "768": "Sambalpur", "769": "Rourkela", "770": "Sundargarh",
    "781": "Guwahati", "782": "Nagaon", "783": "Goalpara", "784": "Tezpur", "785": "Jorhat",
    "786": "Dibrugarh", "787": "Lakhimpur", "788": "Silchar", "790": "Tawang", "791": "Itanagar",
    "792": "Pasighat", "793": "Shillong", "795": "Imphal", "796": "Aizawl", "797": "Kohima",
    "798": "Dimapur", "799": "Agartala",
    # Bihar & Jharkhand
    "800": "Patna", "801": "Patna", "802": "Bhojpur", "803": "Nalanda", "804": "Jehanabad",
    "805": "Nawada", "811": "Munger", "812": "Bhagalpur", "813": "Banka", "814": "Deoghar",
    "815": "Giridih", "816": "Dumka", "821": "Rohtas", "823": "Gaya", "824": "Aurangabad",
    "825": "Hazaribagh", "826": "Dhanbad", "827": "Bokaro", "828": "Dhanbad", "829": "Ramgarh",
    "831": "Jamshedpur", "832": "Jamshedpur", "833": "Chaibasa", "834": "Ranchi", "835": "Ranchi",
    "841": "Saran", "842": "Muzaffarpur", "843": "Sitamarhi", "844": "Vaishali", "845": "East Champaran",
    "846": "Darbhanga", "847": "Madhubani", "848": "Samastipur", "851": "Begusarai", "852": "Saharsa",
    "853": "Khagaria", "854": "Purnia", "855": "Kishanganj",
}

# Master list of all ~788 Indian Districts and prominent commercial/industrial hubs
# Arranged from longest to shortest to prevent prefix-collisions
INDIAN_CITIES_RAW = [
    # Metros & Twin Cities
    "Chhatrapati Sambhajinagar", "Pimpri Chinchwad", "North 24 Parganas", "South 24 Parganas",
    "North 24 Paraganas", "South 24 Paraganas", "Greater Noida", "Navi Mumbai", "New Delhi",
    "Sri Ganganagar", "Hubballi-Dharwad", "Uttara Kannada", "Dakshina Kannada", "Bengaluru Urban",
    "Bengaluru Rural", "Ranga Reddy", "Rangareddy", "Rangareddi", "Chengalpattu", "Puducherry",
    "Pondicherry", "Ahmedabad", "Bengaluru", "Bangalore", "Hyderabad", "Secunderabad", "Kolkata",
    "Calcutta", "Chennai", "Madras", "Mumbai", "Bombay", "Pune", "Poona", "Jaipur", "Lucknow",
    "Kanpur", "Nagpur", "Indore", "Thane", "Bhopal", "Visakhapatnam", "Patna", "Vadodara",
    "Ghaziabad", "Ludhiana", "Agra", "Nashik", "Faridabad", "Meerut", "Rajkot", "Varanasi",
    "Srinagar", "Aurangabad", "Dhanbad", "Amritsar", "Prayagraj", "Allahabad", "Ranchi",
    "Howrah", "Coimbatore", "Jabalpur", "Gwalior", "Vijayawada", "Jodhpur", "Madurai", "Raipur",
    "Kota", "Guwahati", "Chandigarh", "Solapur", "Bareilly", "Moradabad", "Mysuru", "Mysore",
    "Gurugram", "Gurgaon", "Aligarh", "Jalandhar", "Bhubaneswar", "Salem", "Warangal",
    "Thiruvananthapuram", "Trivandrum", "Saharanpur", "Guntur", "Amravati", "Bikaner", "Noida",
    "Jamshedpur", "Bhilai", "Cuttack", "Firozabad", "Kochi", "Cochin", "Bhavnagar", "Dehradun",
    "Durgapur", "Asansol", "Bardhaman", "Burdwan", "Rourkela", "Nanded", "Kolhapur", "Ajmer",
    "Akola", "Jamnagar", "Ujjain", "Siliguri", "Jhansi", "Jammu", "Mangaluru", "Mangalore",
    "Erode", "Belagavi", "Belgaum", "Tirunelveli", "Malegaon", "Gaya", "Jalgaon", "Udaipur",
    "Kozhikode", "Calicut", "Kurnool", "Rajahmundry", "Bokaro", "Bellary", "Ballari", "Patiala",
    "Agartala", "Bhagalpur", "Muzaffarnagar", "Latur", "Dhule", "Rohtak", "Korba", "Bhilwara",
    "Muzaffarpur", "Ahmednagar", "Mathura", "Kollam", "Bilaspur", "Shahjahanpur", "Satara",
    "Bijapur", "Vijayapura", "Rampur", "Shivamogga", "Shimoga", "Chandrapur", "Junagadh",
    "Thrissur", "Trichur", "Alwar", "Kakinada", "Nizamabad", "Panipat", "Darbhanga", "Aizawl",
    "Dewas", "Karnal", "Bathinda", "Jalna", "Sonipat", "Sagar", "Durg", "Imphal", "Ratlam",
    "Hapur", "Arrah", "Karimnagar", "Anantapur", "Etawah", "Bharatpur", "Begusarai", "Sikar",
    "Thoothukudi", "Tuticorin", "Rewa", "Mirzapur", "Raichur", "Pali", "Haridwar", "Vizianagaram",
    "Katihar", "Nagercoil", "Thanjavur", "Bulandshahr", "Sambhal", "Yamunanagar", "Kharagpur",
    "Dindigul", "Gandhinagar", "Malda", "Ongole", "Deoghar", "Haldia", "Khandwa", "Chittoor",
    "Morena", "Anand", "Bhind", "Bhiwani", "Navsari", "Puri", "Jorhat", "Ishapore", "Dibrugarh",
    "Tezpur", "Silchar", "Shillong", "Kohima", "Dimapur", "Gangtok", "Itanagar", "Port Blair",
    "Daman", "Diu", "Silvassa", "Kavaratti", "Leh", "Kargil", "Paradip", "Ankola", "Jagdalpur",
    "Palghar", "Cuddalore", "Mohali", "Panvel", "Ratnagiri", "Sindhudurg", "Sangli", "Udhampur",
    "Pasighat", "Tinsukia",

    # Comprehensive Districts of India
    "Alluri Sitharama Raju", "Anakapalli", "Annamayya", "Bapatla", "Dr. B.R. Ambedkar Konaseema",
    "Konaseema", "East Godavari", "West Godavari", "Eluru", "Nandyal", "NTR", "Palnadu",
    "Parvathipuram Manyam", "Prakasam", "Srikakulam", "Sri Sathya Sai", "Tirupati", "Kadapa",
    "Anjaw", "Changlang", "Kamle", "Kra Daadi", "Kurung Kumey", "Leparada", "Lohit", "Longding",
    "Lower Dibang Valley", "Lower Siang", "Lower Subansiri", "Namsai", "Pakke Kessang",
    "Papum Pare", "Shi Yomi", "Siang", "Tawang", "Tirap", "Upper Dibang Valley", "Upper Siang",
    "Upper Subansiri", "West Kameng", "West Siang", "Baksa", "Barpeta", "Biswanath", "Bongaigaon",
    "Cachar", "Charaideo", "Chirang", "Darrang", "Dhemaji", "Dhubri", "Dima Hasao", "Goalpara",
    "Golaghat", "Hailakandi", "Hojai", "Kamrup Metropolitan", "Kamrup", "Karbi Anglong", "Karimganj",
    "Kokrajhar", "Lakhimpur", "Majuli", "Morigaon", "Nagaon", "Nalbari", "Sivasagar", "Sonitpur",
    "South Salmara-Mankachar", "Udalguri", "West Karbi Anglong", "Araria", "Arwal", "Banka",
    "Bhojpur", "Buxar", "East Champaran", "West Champaran", "Gopalganj", "Jamui", "Jehanabad",
    "Kaimur", "Khagaria", "Kishanganj", "Lakhisarai", "Madhepura", "Madhubani", "Munger", "Nalanda",
    "Nawada", "Purnia", "Rohtas", "Saharsa", "Samastipur", "Saran", "Sheikhpura", "Sheohar",
    "Sitamarhi", "Siwan", "Supaul", "Vaishali", "Balod", "Baloda Bazar", "Balrampur", "Bastar",
    "Bemetara", "Dantewada", "Dhamtari", "Gariaband", "Gaurela Pendra Marwahi", "Janjgir Champa",
    "Jashpur", "Kabirdham", "Kanker", "Khairagarh", "Kondagaon", "Koriya", "Mahasamund",
    "Manendragarh", "Mohla Manpur", "Mungeli", "Narayanpur", "Raigarh", "Rajnandgaon", "Sakti",
    "Sarangarh Bilaigarh", "Sukma", "Surajpur", "Surguja", "Ambikapur", "North Goa", "South Goa",
    "Panaji", "Margao", "Mapusa", "Ponda", "Amreli", "Aravalli", "Banaskantha", "Palanpur",
    "Bharuch", "Botad", "Chhota Udaipur", "Dahod", "Dang", "Devbhumi Dwarka", "Gir Somnath",
    "Veraval", "Kheda", "Kutch", "Bhuj", "Mahisagar", "Mehsana", "Morbi", "Narmada", "Panchmahal",
    "Patan", "Porbandar", "Sabarkantha", "Surendranagar", "Tapi", "Vyara", "Valsad", "Ambala",
    "Charkhi Dadri", "Fatehabad", "Jhajjar", "Jind", "Kaithal", "Kurukshetra", "Mahendragarh",
    "Nuh", "Mewat", "Palwal", "Panchkula", "Rewari", "Sirsa", "Chamba", "Hamirpur", "Kangra",
    "Dharamshala", "Kinnaur", "Kullu", "Lahaul and Spiti", "Mandi", "Shimla", "Sirmaur",
    "Solan", "Una", "Anantnag", "Bandipora", "Baramulla", "Budgam", "Doda", "Ganderbal",
    "Kathua", "Kishtwar", "Kulgam", "Kupwara", "Poonch", "Pulwama", "Rajouri", "Ramban",
    "Reasi", "Samba", "Shopian", "Chatra", "Dumka", "East Singhbhum", "West Singhbhum", "Garhwa",
    "Giridih", "Godda", "Gumla", "Hazaribagh", "Jamtara", "Khunti", "Koderma", "Latehar",
    "Lohardaga", "Pakur", "Palamu", "Ramgarh", "Sahebganj", "Seraikela Kharsawan", "Simdega",
    "Bagalkote", "Bidar", "Chamarajanagara", "Chikkaballapura", "Chikkamagaluru", "Chitradurga",
    "Davanagere", "Dharwad", "Gadag", "Hassan", "Haveri", "Kalaburagi", "Gulbarga", "Kodagu",
    "Madikeri", "Kolar", "Koppal", "Mandya", "Ramanagara", "Tumakuru", "Tumkur", "Udupi",
    "Karwar", "Vijayanagara", "Yadgir", "Alappuzha", "Alleppey", "Ernakulam", "Idukki", "Kannur",
    "Kasaragod", "Kottayam", "Malappuram", "Palakkad", "Pathanamthitta", "Wayanad", "Agar Malwa",
    "Alirajpur", "Anuppur", "Ashoknagar", "Balaghat", "Barwani", "Betul", "Burhanpur", "Chhatarpur",
    "Chhindwara", "Damoh", "Datia", "Dhar", "Dindori", "Guna", "Harda", "Hoshangabad",
    "Narmadapuram", "Jhabua", "Katni", "Khargone", "Mandla", "Mandsaur", "Narsinghpur", "Neemuch",
    "Niwari", "Panna", "Raisen", "Rajgarh", "Satna", "Sehore", "Seoni", "Shahdol", "Shajapur",
    "Sheopur", "Shivpuri", "Sidhi", "Singrauli", "Tikamgarh", "Umaria", "Vidisha", "Maihar",
    "Mauganj", "Pandhurna", "Ahilyanagar", "Beed", "Bhandara", "Buldhana", "Gadchiroli", "Gondia",
    "Hingoli", "Nandurbar", "Osmanabad", "Dharashiv", "Parbhani", "Wardha", "Washim", "Yavatmal",
    "Bishnupur", "Chandel", "Churachandpur", "Imphal East", "Imphal West", "Jiribam", "Kakching",
    "Kamjong", "Kangpokpi", "Noney", "Pherzawl", "Senapati", "Tamenglong", "Tengnoupal", "Ukhrul",
    "East Garo Hills", "East Jaintia Hills", "East Khasi Hills", "North Garo Hills", "Ri Bhoi",
    "South Garo Hills", "South West Garo Hills", "South West Khasi Hills", "West Garo Hills",
    "West Jaintia Hills", "West Khasi Hills", "Champhai", "Hnahthial", "Khawzawl", "Kolasib",
    "Lawngtlai", "Lunglei", "Mamit", "Saiha", "Saitual", "Serchhip", "Chumoukedima", "Kiphire",
    "Longleng", "Mokokchung", "Mon", "Niuland", "Noklak", "Peren", "Phek", "Shamator",
    "Tseminyu", "Tuensang", "Wokha", "Zunheboto", "Angul", "Balangir", "Balasore", "Baleswar",
    "Bargarh", "Bhadrak", "Boudh", "Deogarh", "Dhenkanal", "Gajapati", "Ganjam", "Berhampur",
    "Jagatsinghpur", "Jajpur", "Jharsuguda", "Kalahandi", "Kandhamal", "Kendrapara", "Kendujhar",
    "Keonjhar", "Khordha", "Koraput", "Malkangiri", "Mayurbhanj", "Baripada", "Nabarangpur",
    "Nayagarh", "Nuapada", "Rayagada", "Sambalpur", "Subarnapur", "Sonepur", "Sundargarh",
    "Barnala", "Faridkot", "Fatehgarh Sahib", "Fazilka", "Ferozepur", "Gurdaspur", "Hoshiarpur",
    "Kapurthala", "Malerkotla", "Mansa", "Moga", "SAS Nagar", "Muktsar", "Pathankot", "Rupnagar",
    "Ropar", "Sangrur", "SBS Nagar", "Nawanshahr", "Tarn Taran", "Anupgarh", "Balotra", "Banswara",
    "Baran", "Barmer", "Beawar", "Bundi", "Chittorgarh", "Churu", "Dausa", "Deeg", "Dholpur",
    "Didwana Kuchaman", "Dudu", "Dungarpur", "Ganganagar", "Gangapur City", "Hanumangarh",
    "Jaisalmer", "Jalore", "Jhalawar", "Jhunjhunu", "Karauli", "Kekri", "Khairthal Tijara",
    "Kotputli Behror", "Nagaur", "Neem Ka Thana", "Phalodi", "Pratapgarh", "Rajsamand", "Salumbar",
    "Sanchore", "Sawai Madhopur", "Shahpura", "Sirohi", "Tonk", "Gyalshing", "Mangan", "Namchi",
    "Pakyong", "Soreng", "Ariyalur", "Dharmapuri", "Kallakurichi", "Kanchipuram", "Kanyakumari",
    "Karur", "Krishnagiri", "Mayiladuthurai", "Nagapattinam", "Namakkal", "Nilgiris", "Ooty",
    "Perambalur", "Pudukkottai", "Ramanathapuram", "Ranipet", "Sivaganga", "Tenkasi", "Theni",
    "Tiruchirappalli", "Trichy", "Tirupathur", "Tiruppur", "Tiruvallur", "Tiruvannamalai",
    "Tiruvarur", "Vellore", "Viluppuram", "Virudhunagar", "Adilabad", "Bhadradri Kothagudem",
    "Jagtial", "Jangaon", "Jayashankar Bhupalpally", "Jogulamba Gadwal", "Kamareddy", "Khammam",
    "Kumuram Bheem Asifabad", "Mahabubabad", "Mahbubnagar", "Mancherial", "Medak",
    "Medchal Malkajgiri", "Mulugu", "Nagarkurnool", "Nalgonda", "Narayanpet", "Nirmal",
    "Peddapalli", "Rajanna Sircilla", "Sangareddy", "Siddipet", "Suryapet", "Vikarabad",
    "Wanaparthy", "Hanamkonda", "Yadadri Bhuvanagiri", "Dhalai", "Gomati", "Khowai", "North Tripura",
    "Sepahijala", "South Tripura", "Unakoti", "West Tripura", "Ambedkar Nagar", "Amethi",
    "Amroha", "Auraiya", "Ayodhya", "Azamgarh", "Baghpat", "Bahraich", "Ballia", "Banda",
    "Barabanki", "Basti", "Bhadohi", "Bijnor", "Budaun", "Chandauli", "Chitrakoot", "Deoria",
    "Etah", "Farrukhabad", "Fatehpur", "Gautam Buddha Nagar", "Ghazipur", "Gonda", "Hamirpur",
    "Hardoi", "Hathras", "Jalaun", "Orai", "Jaunpur", "Kannauj", "Kanpur Dehat", "Kanpur Nagar",
    "Kasganj", "Kaushambi", "Kheri", "Lakhimpur", "Kushinagar", "Lalitpur", "Maharajganj",
    "Mahoba", "Mainpuri", "Mau", "Pilibhit", "Pratapgarh", "Raebareli", "Sant Kabir Nagar",
    "Shamli", "Shravasti", "Siddharthnagar", "Sitapur", "Sonbhadra", "Sultanpur", "Unnao",
    "Almora", "Bageshwar", "Chamoli", "Champawat", "Nainital", "Pauri Garhwal", "Pithoragarh",
    "Rudraprayag", "Tehri Garhwal", "Udham Singh Nagar", "Uttarkashi", "Roorkee", "Haldwani",
    "Rishikesh", "Alipurduar", "Bankura", "Birbhum", "Cooch Behar", "Dakshin Dinajpur",
    "Darjeeling", "Hooghly", "Jalpaiguri", "Jhargram", "Kalimpong", "Murshidabad", "Nadia",
    "Paschim Bardhaman", "Paschim Medinipur", "Purba Bardhaman", "Purba Medinipur", "Purulia",
    "Uttar Dinajpur", "Central Delhi", "East Delhi", "North Delhi", "North East Delhi",
    "North West Delhi", "South Delhi", "South East Delhi", "South West Delhi", "West Delhi",
    "Shahdara", "Karaikal", "Mahe", "Yanam", "Andaman and Nicobar", "Andaman & Nicobar",
    "Andaman", "Nicobar", "Lakshadweep"
]

# De-duplicate preserving sorted order (longest first)
INDIAN_CITIES = sorted(list(dict.fromkeys(INDIAN_CITIES_RAW)), key=lambda x: len(x), reverse=True)


def resolve_city_from_text(text: Optional[str]) -> Optional[str]:
    """
    Resolves the standard Indian city name from an address or location text.
    1. Special check: Any Delhi district -> 'New Delhi'
    2. 6-digit Indian PIN code lookup (e.g. '632013' -> 'Vellore', '492001' -> 'Raipur')
    3. Exact word boundary match against master gazetteer of Indian districts/cities
    """
    if not text:
        return None

    cleaned = str(text).strip()
    if not cleaned:
        return None

    # Unmask asterisks if present
    cleaned_no_star = re.sub(r"[*]+", " ", cleaned).strip()

    # Special check: Any Delhi district (e.g. 'South West delhi', 'North Delhi', 'New Delhi') -> 'New Delhi'
    if re.search(r"\bdelhi\b", cleaned_no_star, re.IGNORECASE):
        return "New Delhi"

    # Tier 1: 6-digit Indian PIN code extraction
    pin_match = re.search(r"\b([1-9][0-9]{5})\b", cleaned)
    if pin_match:
        pin = pin_match.group(1)
        prefix_3 = pin[:3]
        if prefix_3 in PIN_PREFIX_MAP:
            return PIN_PREFIX_MAP[prefix_3]

    # Tier 2: Match known city names in the text
    for city in INDIAN_CITIES:
        pattern = r"\b" + re.escape(city) + r"\b"
        if re.search(pattern, cleaned_no_star, re.IGNORECASE):
            # Standardize aliases
            low = city.lower()
            if low in ["raanchi"]:
                return "Ranchi"
            if low in ["rangareddi", "ranga reddy"]:
                return "Rangareddy"
            if "24 paraganas" in low:
                return "North 24 Parganas" if "north" in low else "South 24 Parganas"
            if low in ["bangalore"]:
                return "Bengaluru"
            if low in ["bombay"]:
                return "Mumbai"
            if low in ["calcutta"]:
                return "Kolkata"
            if low in ["madras"]:
                return "Chennai"
            if low in ["mysore"]:
                return "Mysuru"
            if low in ["poona"]:
                return "Pune"
            if low in ["trivandrum"]:
                return "Thiruvananthapuram"
            if low in ["cochin"]:
                return "Kochi"
            if low in ["calicut"]:
                return "Kozhikode"
            if low in ["burdwan"]:
                return "Bardhaman"
            if low in ["belgaum"]:
                return "Belagavi"
            if low in ["bellary"]:
                return "Ballari"
            if low in ["gurgaon"]:
                return "Gurugram"
            if "andaman" in low or "nicobar" in low:
                return "Port Blair"
            if low in ["lakshadweep"]:
                return "Kavaratti"
            return city

    return None


def resolve_location(consignee_address: Optional[str] = None,
                     consignee_name: Optional[str] = None,
                     event_premises: Optional[str] = None,
                     office_name: Optional[str] = None,
                     zipcode: Optional[str] = None,
                     beneficiary: Optional[str] = None,
                     atc_text: Optional[str] = None) -> Optional[str]:
    """
    Cascading location resolver checking consignee address/name, event/work premises, zip codes, beneficiary, office name, etc.
    """
    # 1. Consignee Address (Highest precision for physical goods)
    city = resolve_city_from_text(consignee_address)
    if city:
        return city

    # 2. Consignee Name (Only matches if it is an actual Indian district/city, e.g. masked Defence bids like '***********Uttara Kannada' or '***********Jorhat')
    city = resolve_city_from_text(consignee_name)
    if city:
        return city

    # 3. Event Premises / Place of Delivery (For services and events like 'OLD AGE HOME, ANKOLA')
    if event_premises:
        city = resolve_city_from_text(event_premises)
        if city:
            return city

    # 4. Direct Zip Code (For transport/service bids)
    if zipcode:
        city = resolve_city_from_text(zipcode)
        if city:
            return city

    # 5. Beneficiary (Present in ~95% of tenders with official address & PIN)
    if beneficiary:
        city = resolve_city_from_text(beneficiary)
        if city:
            return city

    # 6. Office Name (From Page 1)
    if office_name:
        city = resolve_city_from_text(office_name)
        if city:
            return city

    # 7. ATC text
    if atc_text:
        city = resolve_city_from_text(atc_text)
        if city:
            return city

    return None
