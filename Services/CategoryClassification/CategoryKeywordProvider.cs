using System.Collections.Generic;

namespace GemBidScraper.Services.CategoryClassification;

public static class CategoryKeywordProvider
{
    public static List<CategoryDefinition> Categories => new()
    {
        // =====================================================
        // IT & ELECTRONICS
        // =====================================================
        new CategoryDefinition
        {
            Key = "IT",
            DisplayName = "IT & Electronics",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "HARDWARE",
                    DisplayName = "Hardware & Peripherals",
                    Keywords = new List<string>
                    {
                        "computer", "desktop", "laptop", "notebook computer", "workstation computer", "personal laptop",
                        "high end desktop", "high end laptop", "entry and mid level desktop", "monitor", "keyboard", "mouse",
                        "keyboard and mouse combo", "cpu", "gpu", "graphics card", "processor", "motherboard", "mother board", "ram",
                        "ddr4", "ddr5", "memory module", "ssd", "hdd", "hard disk", "hard drive", "internal storage device",
                        "logic card", "ipad", "projector", "display", "led display", "video wall", "large format display",
                        "interactive panel", "ifpd", "smart board", "digital signage", "barcode", "barcode scanner", "scanner", "ocr",
                        "ocr based scanner", "webcam", "speaker", "headset", "power supply", "smps", "pen drive", "usb drive",
                        "external hard disk", "docking station", "rack server", "blade server", "thin client", "zero client",
                        "tower server", "hyperconverged infrastructure", "hci node", "all in one pc", "aio pc", "kvm switch",
                        "biometric device", "fingerprint scanner", "raspberry pi", "flight controller", "dashboard camera",
                        "digital multimeter", "add on cards", "pabx system", "intercom system", "nvme ssd", "sata ssd", "m.2 ssd",
                        "ecc ram", "nvdimm", "intel xeon", "amd epyc", "intel core i9", "intel core i7", "intel core i5",
                        "intel core i3", "amd ryzen", "nvidia rtx", "nvidia a100", "nvidia h100", "nvidia l40s", "quadro",
                        "pos terminal", "card reader", "smart card reader", "rfid reader", "magnetic stripe reader",
                        "biometric attendance system", "iris scanner", "face recognition terminal", "palm scanner", "document scanner",
                        "flatbed scanner", "sheetfed scanner", "handheld scanner", "portable monitor", "touchscreen monitor",
                        "curved monitor", "4k monitor", "oled display", "commercial display", "led wall", "video controller",
                        "matrix switcher", "hdmi extender", "kvm extender", "kvm over ip", "server rack", "rack enclosure", "pdu",
                        "power distribution unit", "ats", "automatic transfer switch", "surge protector", "soundbar",
                        "conference speakerphone", "video conferencing endpoint", "ptz video camera", "capture card", "frame grabber",
                        "microcontroller", "arduino", "esp32", "jetson nano", "single board computer", "fpga board", "logic analyzer",
                        "oscilloscope", "function generator", "eprom programmer", "ic tester", "soldering station",
                        "desoldering station", "cable tester", "fiber power meter", "otdr", "optical time domain reflectometer",
                        "epabx", "ip pbx", "voip phone", "sip phone", "desk phone", "dvd drive", "blu-ray drive",
                        "hard drive enclosure", "usb hub", "type-c hub", "thunderbolt dock", "stylus pen", "digitizer tablet",
                        "drawing tablet", "vr headset", "ar headset", "virtual reality goggles", "interactive audio visual",
                        "audio digital signal processor", "audio mixer"


                    }
                },

                new SubCategory
                {
                    Key = "OPERATING_SYSTEMS",
                    DisplayName = "Operating Systems & Virtualization",
                    Keywords = new List<string>
                    {
                        "operating system", "os", "windows 10", "windows 11", "windows server", "windows server 2019",
                        "windows server 2022", "linux server", "ubuntu server", "red hat enterprise linux", "red hat", "redhat",
                        "rhel", "centos", "rocky linux", "alma linux", "debian", "suse linux", "opensuse", "fedora", "arch linux",
                        "freebsd", "openbsd", "macos", "mac os", "unix", "solaris", "aix", "android os", "chromeos", "rtos", "vxworks",
                        "server os", "desktop os", "server migration", "os migration", "os upgrade", "server deployment",
                        "server administration", "system administration", "patch management", "endpoint management",
                        "active directory", "group policy", "domain controller", "hypervisor", "virtualization", "vmware",
                        "vmware esxi", "vmware vsphere", "hyper-v", "proxmox", "kvm virtualization", "xen", "xenserver",
                        "citrix hypervisor", "nutanix ahv"

                    }
                },

                new SubCategory
                {
                    Key = "WEB_MOBILE_APPS",
                    DisplayName = "Web & Mobile Applications",
                    Keywords = new List<string>
                    {
                        "web application", "mobile application", "mobile app", "website", "web portal", "portal", "e-commerce portal",
                        "web development", "mobile app development", "pwa", "progressive web app", "ios app", "android app",
                        "cross platform app", "web site development", "portal customization", "custom web app", "intranet portal",
                        "extranet portal", "micro-site", "landing page", "content portal", "citizen portal", "e-services portal",
                        "payment gateway integration", "rest api development"

                    }
                },

                new SubCategory
                {
                    Key = "PROGRAMMING_LANGUAGES",
                    DisplayName = "Programming Languages & Frameworks",
                    Keywords = new List<string>
                    {
                        "c#", "csharp", "c++", "c language", "java", "python", "javascript", "typescript", "php", "ruby", "swift",
                        "kotlin", "go", "golang", "rust", "r language", "scala", "perl", "dart", "haskell", "lua", "assembly", "bash",
                        "powershell", "shell script", "vb.net", "visual basic", "cobol", "fortran", "elixir", "erlang", "clojure",
                        "groovy", "zig", "node.js", "nodejs", "deno", "bun", "python3", "jdk", "jre", ".net framework", ".net core",
                        ".net 6", ".net 7", ".net 8", ".net 9", "react", "react native", "angular", "vue.js", "vuejs", "next.js",
                        "nuxt.js", "express.js", "nest.js", "django", "fastapi", "spring boot", "laravel", "symfony", "codeigniter",
                        "asp.net", "asp.net mvc", "blazor", "ruby on rails", "flutter", "ionic", "cordova", "xamarin", "maui",
                        "bootstrap", "tailwind css", "jquery", "svelte", "astro", "solidjs", "pytorch", "tensorflow", "keras",
                        "scikit-learn", "opencv", "pandas", "numpy", "spacy", "nltk", "huggingface", "transformers", "langchain",
                        "llama-index", "jax", "scipy", "xgboost"


                    }
                },

                new SubCategory
                {
                    Key = "DEV_TOOLS_DEVOPS",
                    DisplayName = "Developer Tools & DevOps",
                    Keywords = new List<string>
                    {
                        "software development kit", "sdk", "ide", "visual studio", "vscode", "intellij", "eclipse", "pycharm", "git",
                        "github", "gitlab", "bitbucket", "ci/cd", "jenkins", "github actions", "gitlab ci", "azure devops", "docker",
                        "kubernetes", "k8s", "containerization", "helm", "rancher", "podman", "terraform", "ansible", "puppet", "chef",
                        "vagrant", "maven", "gradle", "npm", "yarn", "pip", "nuget", "sonarqube", "artifactory", "nexus", "postman",
                        "swagger", "openapi"

                    }
                },

                new SubCategory
                {
                    Key = "DATABASE_DBMS",
                    DisplayName = "Databases & DBMS Systems",
                    Keywords = new List<string>
                    {
                        "rdbms", "database", "dbms", "relational database", "sql server", "ms sql", "microsoft sql server", "oracle",
                        "oracle database", "oracle 19c", "oracle 21c", "edb", "enterprisedb", "enterprise db", "enterprisedb postgres",
                        "postgresql", "postgres", "mysql", "mariadb", "sqlite", "ibm db2", "db2", "cockroachdb", "tidb", "amazon rds",
                        "azure sql", "cloud spanner", "nosql", "nosql database", "mongodb", "couchdb", "couchbase", "documentdb",
                        "ravendb", "rethinkdb", "orientdb", "firestore", "dynamodb", "redis", "redis cluster", "memcached",
                        "dragonflydb", "keydb", "aerospike", "vector database", "milvus", "pinecone", "qdrant", "weaviate", "chromadb",
                        "neo4j", "arangodb", "amazon neptune", "influxdb", "timescaledb", "questdb", "elasticsearch", "opensearch",
                        "solr", "clickhouse", "snowflake", "databricks", "apache hive", "amazon redshift", "google bigquery", "duckdb",
                        "trino", "presto"

                    }
                },

                new SubCategory
                {
                    Key = "EXCHANGE_SERVERS",
                    DisplayName = "Exchange Servers & Email Infrastructure",
                    Keywords = new List<string>
                    {
                        "microsoft exchange server", "exchange server", "exchange online", "microsoft 365", "office 365",
                        "email server", "mail server", "email migration", "exchange migration", "office 365 migration",
                        "microsoft 365 migration", "exchange server upgrade", "exchange server support", "exchange server migration",
                        "email infrastructure", "enterprise email", "email backup", "email archiving", "email disaster recovery",
                        "hybrid exchange", "exchange hybrid", "smtp server", "imap", "pop3", "mailbox migration", "email hosting"

                    }
                },

                new SubCategory
                {
                    Key = "MAIL_SOLUTIONS",
                    DisplayName = "Mail Solutions & Email Security",
                    Keywords = new List<string>
                    {
                        "business email", "enterprise email solutions", "corporate email", "email management",
                        "email security solutions", "secure email gateway", "email protection", "anti-spam", "anti-phishing",
                        "email filtering", "email recovery", "cloud email", "email continuity", "mail security", "secure mail",
                        "email compliance", "email data loss prevention", "microsoft 365 email", "google workspace",
                        "gmail for business"


                    }
                },

                new SubCategory
                {
                    Key = "FIREWALL_SECURITY",
                    DisplayName = "Firewall & Network Security",
                    Keywords = new List<string>
                    {
                        "firewall", "next generation firewall", "next-gen firewall", "ngfw", "network firewall", "enterprise firewall",
                        "hardware firewall", "software firewall", "cloud firewall", "firewall security", "firewall management",
                        "firewall migration", "firewall upgrade", "firewall replacement", "firewall configuration",
                        "firewall monitoring", "firewall support", "network security", "cybersecurity", "utm",
                        "unified threat management", "intrusion prevention system", "intrusion detection system", "ips", "ids", "vpn",
                        "ssl vpn", "site-to-site vpn", "zero trust", "network access control", "secure web gateway"

                    }
                },

                new SubCategory
                {
                    Key = "ENTERPRISE_SOFTWARE",
                    DisplayName = "Enterprise Software (ERP/CRM/BI)",
                    Keywords = new List<string>
                    {
                        "enterprise software", "business software", "custom software", "software development",
                        "application development", "enterprise applications", "business applications", "erp", "crm", "hrms", "scm",
                        "dms", "workflow automation", "digital transformation", "software integration", "api integration",
                        "application modernization", "legacy application modernization", "software migration", "application support",
                        "managed software services", "it software solutions", "software license", "enterprise it solutions",
                        "it infrastructure modernization", "it infrastructure upgrade", "saas", "office suite", "software maintenance",
                        "middleware", "bi tool", "business intelligence", "cms", "content management system",
                        "document management system", "lms", "learning management system", "human resource management system",
                        "gis software", "matlab", "digital e-learning software", "designing software", "global mapper",
                        "accessibility software", "digitization", "digitisation", "sap", "sap hana", "oracle ebs", "oracle fusion",
                        "microsoft dynamics", "salesforce", "zoho", "odoo", "tally", "tally prime", "workday", "servicenow", "zendesk",
                        "jira", "confluence", "sharepoint", "power bi", "tableau", "qlikview", "looker", "grafana", "kibana",
                        "autocad", "solidworks", "revit", "ansys", "arcgis", "qgis", "photoshop", "illustrator", "indesign",
                        "adobe premiere", "coreldraw", "maya", "3ds max", "blender", "unity", "unreal engine", "labview", "spss",
                        "stata", "minitab", "simulink", "scada software", "plc programming", "ocr software", "abbyy finefinder",
                        "pdf editor", "adobe acrobat"

                    }
                },

                new SubCategory
                {
                    Key = "NETWORKING",
                    DisplayName = "Networking Equipment & Communication Devices",
                    Keywords = new List<string>
                    {
                        "router", "enterprise router", "network router", "core router", "edge router", "branch router",
                        "wireless router", "sd-wan", "sd-wan router", "network infrastructure", "network deployment",
                        "network upgrade", "router configuration", "router management", "router support", "network monitoring",
                        "switch", "network switch", "l2 switch", "l3 switch", "wifi", "wireless", "wireless access point", "modem",
                        "access point", "network cable", "patch panel", "leased line", "internet connectivity", "bandwidth", "lan",
                        "wan", "wlan", "mpls", "internet gateway", "network gateway", "vpn router", "cisco router", "juniper router",
                        "fortinet router", "huawei router", "mikrotik router", "network architecture", "cat6", "fiber optic", "ofc",
                        "sfp module", "media converter", "fibre media converter", "patch cord", "crimping", "structured cabling",
                        "wi-fi controller", "dhcp server", "dns", "load balancer", "voip", "ip phone", "networking trainer",
                        "joint enclosure", "network attached storage device", "network card", "managed switch", "unmanaged switch",
                        "poe switch", "poe injector", "poe splitter", "cat5e", "cat6a", "cat7", "cat8", "single mode fiber",
                        "multi mode fiber", "armored fiber cable", "fiber patch cord", "splice tray", "fiber closure", "pigtail",
                        "sfp+", "qsfp", "qsfp28", "10gbe switch", "40gbe switch", "100gbe switch", "network interface card", "nic",
                        "hba card", "host bus adapter", "gateway", "cellular router", "4g router", "5g router", "ill",
                        "internet leased line", "ftth", "gpon", "epon", "onu", "olt", "wi-fi 6", "wi-fi 6e", "wi-fi 7", "mesh router",
                        "outdoor access point", "directional antenna", "omni antenna", "attenuator", "optical power meter",
                        "fusion splicer", "otdr cable", "network tap", "packet broker", "dns server", "radius server", "tacacs+",
                        "vlan", "mpls router", "core switch", "distribution switch", "access switch", "chassis switch",
                        "terminal server", "console server", "modem router", "vdsl modem", "walkie talkie", "walkie talkies",
                        "handheld transceiver", "pmr446", "radio set", "radio sets", "digital uhf hand-held radio", "transceiver"

                    }
                },

                new SubCategory
                {
                    Key = "STORAGE",
                    DisplayName = "Storage & Disaster Recovery",
                    Keywords = new List<string>
                    {
                        "nas", "san", "storage", "storage array", "backup", "backup solution", "data backup", "tape drive",
                        "cloud storage", "raid storage", "nvme", "iscsi", "fibre channel", "fc storage", "object storage",
                        "disaster recovery", "dr storage", "network attached storage", "storage area network", "lto tape",
                        "all-flash array", "hybrid storage", "unified storage", "das", "direct attached storage", "lto-7", "lto-8",
                        "lto-9", "tape library", "autoloader", "tape cartridge", "disk enclosure", "jbod", "raid controller",
                        "sas hard drive", "nearline sas", "sata hard drive", "enterprise ssd", "nvme-of", "s3 compatible storage",
                        "immutable storage", "air-gapped backup", "deduplication appliance", "replication", "storage snapshot",
                        "block storage", "file storage", "cold storage", "archive storage", "storage enclosure", "san switch",
                        "fc switch", "multipathing", "thin provisioning"

                    }
                },

                new SubCategory
                {
                    Key = "SECURITY",
                    DisplayName = "Cyber Security & Endpoint Protection",
                    Keywords = new List<string>
                    {
                        "antivirus", "endpoint", "endpoint protection", "cyber security", "intrusion detection",
                        "intrusion prevention", "penetration testing", "vulnerability assessment", "siem", "soc", "data encryption",
                        "ssl certificate", "vapt", "mfa", "multi factor authentication", "dlp", "data loss prevention", "iam",
                        "identity management", "edr", "xdr", "ddos", "waf", "web application firewall", "pam",
                        "privileged access management", "hsm", "hardware security module", "soar", "security orchestration",
                        "threat intelligence", "zerotrust", "ztna", "zero trust network access", "sase", "casb",
                        "cloud access security broker", "threat hunting", "incident response", "security audit", "iso 27001",
                        "cert-in", "cert-in security audit", "code audit", "static code analysis", "sast", "dast",
                        "container security", "microsegmentation", "honeypot", "deception technology", "syslog server", "log analyzer",
                        "email security", "spam filter", "anti phishing", "sandboxing", "edr agent", "xdr platform", "security token",
                        "fido2", "yubikey", "biometric authentication", "pki", "public key infrastructure", "antivirus software",
                        "kaspersky", "sophos", "mcafee", "symantec", "bitdefender", "crowdstrike", "trend micro", "crowdstrike falcon",
                        "dlp software", "backup software", "veeam", "commvault", "veritas", "netbackup"


                    }
                },

                new SubCategory
                {
                    Key = "CLOUD",
                    DisplayName = "Cloud Infrastructure & Hosting",
                    Keywords = new List<string>
                    {
                        "cloud", "cloud computing", "cloud management", "cloud orchestration", "azure", "aws", "google cloud",
                        "datacenter", "data center", "virtual machine", "hosting", "web hosting", "cloud migration", "cloud service",
                        "iaas", "paas", "vps", "virtual private server", "hybrid cloud", "private cloud", "colocation",
                        "amazon web services", "microsoft azure", "gcp", "oracle cloud", "ibm cloud", "meghraj", "nic cloud",
                        "bsnl cloud", "e2e networks", "yotta", "ctrls", "cloud infrastructure", "cloud backup", "cloud security",
                        "serverless", "aws lambda", "azure functions", "openstack", "cloudstack", "cdn", "content delivery network",
                        "cloud load balancer", "vps server", "virtual datacenter", "vdc", "disaster recovery as a service", "draas",
                        "backup as a service", "baas", "desktop as a service", "daas", "cloud monitoring"


                    }
                },

                new SubCategory
                {
                    Key = "CCTV",
                    DisplayName = "CCTV & Surveillance",
                    Keywords = new List<string>
                    {
                        "cctv", "camera", "ip camera", "dome camera", "bullet camera", "dvr", "nvr", "video surveillance",
                        "ptz camera", "thermal camera", "body worn camera", "video analytics", "anpr",
                        "automatic number plate recognition", "vms", "video management software", "surveillance monitor",
                        "static sentry surveillance", "x ray baggage inspection", "x-ray baggage", "xbis", "dslr", "camcorder",
                        "video camera", "binocular", "varifocal camera", "fisheye camera", "panoramic camera", "infrared camera",
                        "night vision camera", "explosion proof camera", "under vehicle surveillance", "uvss", "frs",
                        "facial recognition system", "crowd management system", "video wall controller", "joystick controller",
                        "cctv pole", "cctv enclosure", "surge protector for cctv", "ir illuminator", "bnc connector", "rg59 cable",
                        "rg6 cable", "video encoder", "video decoder", "hd-tvi camera", "hd-cvi camera", "ahd camera", "speed dome",
                        "solar powered cctv", "surveillance hard disk", "skywatch tower", "perimeter intrusion detection", "pids"


                    }
                },

                new SubCategory
                {
                    Key = "PRINTING",
                    DisplayName = "Printing & Consumables",
                    Keywords = new List<string>
                    {
                        "printer", "toner", "ink", "ink cartridge", "laser printer", "inkjet printer", "multifunction printer",
                        "multifunction machine", "mfp", "mfm", "photocopier", "scanner printer", "plotter", "3d printer",
                        "thermal printer", "pos printer", "dot matrix printer", "label printer", "drum unit", "fuser kit",
                        "fuser unit", "duplex printer", "printer head", "printer maint box", "dvd writer", "mono laser printer",
                        "color laser printer", "ink tank printer", "large format printer", "barcode printer", "rfid printer",
                        "id card printer", "passbook printer", "receipt printer", "line printer", "continuous stationary printer",
                        "3d printer filament", "pla filament", "abs filament", "resin printer", "printer ribbon", "printhead",
                        "toner cartridge", "refill ink", "waste toner box", "maintenance cartridge", "heavy duty printer",
                        "production printer", "copier paper", "laminator", "laminating film"


                    }
                },

                new SubCategory
                {
                    Key = "SERVICES",
                    DisplayName = "IT Support & Managed Services",
                    Keywords = new List<string>
                    {
                        "amc", "annual maintenance contract", "installation", "maintenance", "repair", "configuration", "it support",
                        "helpdesk", "system integration", "data migration", "it consultancy", "managed services", "fms", "it resource",
                        "sla based service", "hardware replacement", "debugging", "troubleshooting", "it maint", "it spares",
                        "software development services", "portal development", "ui/ux design", "cloud migration services",
                        "devops services", "database administration", "dba services", "network management", "soc management",
                        "managed security service provider", "mssp", "it audit", "vapt audit", "cert-in audit",
                        "data center colocation", "data entry services", "digitization service", "scanning and indexing",
                        "manpower for it", "it manpower", "software testing", "qa testing", "performance testing", "security testing",
                        "amc of cctv", "amc of ups", "amc of servers", "amc of computers", "onsite support", "remote support",
                        "software support", "it project", "hiring of agency for it"



                    }
                },

                new SubCategory
                {
                    Key = "DRONE",
                    DisplayName = "Drones & UAV",
                    Keywords = new List<string>
                    {
                        "drone", "unmanned aerial vehicle", "uav", "drone training simulator", "drone interceptor kit",
                        "surveillance drone", "gps module", "gps antenna", "quadcopter", "hexacopter", "octocopter",
                        "fixed wing drone", "hybrid drone", "agricultural drone", "mapping drone", "inspection drone",
                        "tethered drone", "anti-drone system", "drone jammer", "drone camera", "gimbal camera",
                        "lidar sensor for drone", "fpv goggles", "drone flight controller", "telemetry module", "drone battery",
                        "li-po battery", "drone propeller", "ground control station", "gcs"

                    }
                }
            }
        },

        // =====================================================
        // DEFENCE & MILITARY
        // =====================================================
        new CategoryDefinition
        {
            Key = "DEFENCE",
            DisplayName = "Defence & Military",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "WEAPONS_AMMO",
                    DisplayName = "Weapons, Ammunition & Ranges",
                    Keywords = new List<string>
                    {
                        "shooting range", "ammunition", "amn bunker", "weapon training chart", "pneumatic gun",
                        "ir make pneumatic gun", "lead shots", "cartridge", "arms issue register", "ordnance", "ord spares",
                        "ord stores", "small arms", "rifle", "pistol", "carbine", "mortar", "artillery", "grenade", "pyrotechnics",
                        "bullet proof jacket", "firing target", "target retriever system", "bullet trap"

                    }
                },

                new SubCategory
                {
                    Key = "VEHICLES_MILITARY",
                    DisplayName = "Military Vehicles & Shelters",
                    Keywords = new List<string>
                    {
                        "tatra 815", "swaraj mazda amb", "tata amb", "recovery vehicle", "all terrain tactical hauler",
                        "all terrain vehicle", "troop carrier", "mobile spares shelter", "field shelter", "office shelter",
                        "living shelter", "vehicle chassis", "lpta", "armoured vehicle", "bullet proof vehicle",
                        "mine protected vehicle", "infantry combat vehicle", "bvp", "als truck", "tactical vehicle",
                        "military trailer", "camouflaged shelter", "containerized shelter"

                    }
                },

                new SubCategory
                {
                    Key = "PROTECTIVE_GEAR",
                    DisplayName = "Tactical & Protective Gear",
                    Keywords = new List<string>
                    {
                        "bullet resistant helmet", "riot control helmet", "protective px", "tyre killer", "sonic defenders",
                        "hearing protection", "concertina coil", "barbed coil", "summer headset system", "leather helmet",
                        "poly carbonate shield", "expandable barricade", "personal safety kit", "ballistic helmet", "ballistic plate",
                        "tactical vest", "bulletproof vest", "body armor", "riot control suit", "anti riot shield", "lathi",
                        "spike strip", "spike barrier", "road blocker", "tactical boots", "camo suit", "ghillie suit",
                        "bullet resistant jacket", "bullet resistant jackets"

                    }
                },

                new SubCategory
                {
                    Key = "TRAINING_SIM",
                    DisplayName = "Training & Simulators",
                    Keywords = new List<string>
                    {
                        "driving training simulator", "map reading nursery", "training simulator",
                        "networking trainer educational kit", "fiber optics trainer kit", "small arms simulator",
                        "small arms training simulator", "sats", "flight simulator", "tank simulator", "virtual firing range",
                        "combat training simulator", "tactical simulator"


                    }
                },

                new SubCategory
                {
                    Key = "COMMUNICATIONS",
                    DisplayName = "Military Communications & Radar",
                    Keywords = new List<string>
                    {
                        "dmr tier", "radio trunking", "hf trans-receiver", "trans-receiver", "cellular antenna", "hand held gps",
                        "manpack radio", "vhf radio", "uhf radio", "tactical communication", "crypto unit", "scrambler", "radar",
                        "surveillance radar", "signal jammer", "rf jammer", "satellite terminal", "satcom", "antenna mast",
                        "telescopic mast", "field telephone"

                    }
                },

                new SubCategory
                {
                    Key = "NAVAL_MARINE",
                    DisplayName = "Naval, Marine & Boats",
                    Keywords = new List<string>
                    {
                        "dinghy", "sailing boat", "e c dinghy", "boat", "vessel", "naval spares", "ship refit", "marine equipment",
                        "life raft", "outboard motor"

                    }
                }
            }
        },

        // =====================================================
        // MEDICAL & HEALTHCARE
        // =====================================================
        new CategoryDefinition
        {
            Key = "MEDICAL",
            DisplayName = "Medical & Healthcare",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "PHARMA",
                    DisplayName = "Medicines & Pharmaceuticals",
                    Keywords = new List<string>
                    {
                        "tablet", "tab ", "syp ", "syrup", "injection", "inj ", "vial", "ampoule", "vaccine", "hepatitis b vaccine",
                        "anti tb drugs", "moxifloxacin", "atropine sulphate", "levofloxacin", "timolol maleate", "dexmedetomidine",
                        "terlipressin", "clindamycin", "amorolfine", "diethyl phenyl acetamide", "folic acid", "niacinamide", "sachet",
                        "medicines", "drugs and consumable", "pharma", "medicine", "capsule", "cap ", "ointment", "gel", "infusion",
                        "iv fluid", "paracetamol", "amoxicillin", "azithromycin", "pantoprazole", "ceftriaxone", "meropenem",
                        "insulin", "anesthetic", "antiseptic solution", "saline", "dextrose", "rickettsial vaccine",
                        "sodium valproate", "milk of magnesia", "liquid paraffin"

                    }
                },

                new SubCategory
                {
                    Key = "EQUIPMENT_DEVICES",
                    DisplayName = "Medical Equipment & Diagnostic Devices",
                    Keywords = new List<string>
                    {
                        "oxygen concentrator", "liquid medical oxygen", "cryogenic storage tank", "oxygen analyzer", "o2 analyzer",
                        "microscope", "ear drum dissector", "endoscopic ear instruments", "microscopic ear instruments",
                        "co2 incubator", "elisa", "glucometer", "uv-visible spectrophotometer", "environmental chamber",
                        "laboratory refrigerator", "sample gas probe", "chemical indicator", "knee replacement prosthesis",
                        "bone screw", "surgical drapes", "surgical hooks", "digital thermo hygrometer", "digital hygrometer",
                        "patient monitor", "icu ventilator", "defibrillator", "ecg machine", "usg machine", "ultrasound scanner",
                        "x-ray machine", "digital radiography", "ct scanner", "mri scanner", "pulse oximeter", "infusion pump",
                        "syringe pump", "autoclave", "blood pressure monitor", "sphygmomanometer", "dialysis machine",
                        "operation theater light", "ot table", "anesthesia workstation", "c-arm system", "medical centrifuge",
                        "centrifuge", "refrigerated centrifuge", "auto keratorefractometer", "auto ref keratometer", "keratometer",
                        "refractometer", "uterine balloon tamponade"

                    }
                },

                new SubCategory
                {
                    Key = "CONSUMABLES",
                    DisplayName = "Medical Consumables & Surgical Disposables",
                    Keywords = new List<string>
                    {
                        "biochemistry reagent kit", "o-ring kit", "surgical gloves", "bedsheet and pillow cover (medical use)",
                        "kick bucket", "steam sterilization", "aspen 6200", "diluent", "lyse", "reagent", "examination gloves",
                        "latex gloves", "nitrile gloves", "syringes", "hypodermic needle", "iv cannula", "blood collection tube",
                        "surgical mask", "n95 mask", "surgical gown", "gauze bandage", "cotton roll", "surgical suture", "catheter",
                        "foley catheter", "urine bag", "ppe kit medical", "face shield medical", "swab stick", "rapid test kit",
                        "knee brace", "orthopedic brace", "adhesive tape", "surgical adhesive tape"


                    }
                },

                new SubCategory
                {
                    Key = "DENTAL_EQUIPMENT",
                    DisplayName = "Dental Equipment & Consumables",
                    Keywords = new List<string>
                    {
                        "dental lab consumables", "dental ceramic", "dentin powder", "dental cement", "dental amalgam", "denture",
                        "dental composite", "dental impression material", "shade guide", "shade a2", "shade a1", "shade d3",
                        "dental casting", "dental matrix", "matrix band"

                    }
                },

                new SubCategory
                {
                    Key = "WASTE_MGMT",
                    DisplayName = "Medical / Bio Waste Management",
                    Keywords = new List<string>
                    {
                        "medical waste decomposition", "bio-medical waste", "biomedical waste", "foot operated pedal bin",
                        "waste gasification", "incinerator", "bio hazard bag", "sharp container", "needle cutter", "needle burner",
                        "autoclave waste", "shredder medical waste", "effluent treatment plant medical"

                    }
                }
            }
        },

        // =====================================================
        // AUTOMOBILE & VEHICLE SPARES
        // =====================================================
        new CategoryDefinition
        {
            Key = "AUTOMOBILE",
            DisplayName = "Automobile & Vehicle Spares",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "SPARES",
                    DisplayName = "Vehicle Spares & Engine Parts",
                    Keywords = new List<string>
                    {
                        "fuel pump", "fuel filter", "fuel injection pump", "injector nozzle", "injector assy", "spark plug",
                        "ignition coil", "carburetor", "air filter", "oil filter", "brake shoe", "brake pad", "brake disc",
                        "caliper assy", "clutch plate", "shock absorber", "radiator", "coolant pipe", "water pump", "starter motor",
                        "self starter", "wheel bearing", "wheel brg", "needle roller brg", "gasket", "piston ring",
                        "cylinder head bolt", "gear shifter cable", "wiper blade", "fan belt", "v belt", "chain sproket", "rotor assy",
                        "combi switch", "pressure plate", "release bearing", "flywheel", "crank shaft", "crankshaft seal",
                        "speedo cable", "hose pipe", "gear lever kit", "stabilizer link", "tie rod end", "dual mass flywheel",
                        "lower arm", "front stabilizer link", "u joint", "universal joint", "uj cross", "cover outer",
                        "engine alignment tool", "wheel alignment", "alternator", "turbocharger", "leaf spring", "steering rack",
                        "ball joint", "headlight assy", "tail light", "brake master cylinder", "clutch master cylinder",
                        "silencer pipe", "catalytic converter", "obd", "on-board diagnostics", "hand brake", "door lock"

                    }
                },

                new SubCategory
                {
                    Key = "TYRES",
                    DisplayName = "Tyres, Tubes & Wheels",
                    Keywords = new List<string>
                    {
                        "tyre", "tire", "tubes for pneumatic tyres", "wheel solid tyre", "solid cushion tyre", "roller tyre",
                        "radial tyre", "tubeless tyre", "alloy wheel", "wheel rim", "flap tyre", "retreading material",
                        "tyre inflator", "wheel balancer"

                    }
                },

                new SubCategory
                {
                    Key = "LUBRICANTS",
                    DisplayName = "Lubricants, Oils & Automotive Fluids",
                    Keywords = new List<string>
                    {
                        "hydraulic oil", "diesel engine oil", "gear lubricant", "crankcase oil", "lub oil", "engine oil", "lubricant",
                        "grease", "brake fluid", "coolant", "radiator coolant", "transmission fluid", "atf oil", "chassis grease",
                        "synthetic engine oil", "adblue", "def fluid", "diesel exhaust fluid", "oil 2t", "servo oil"

                    }
                },

                new SubCategory
                {
                    Key = "VEHICLES",
                    DisplayName = "Vehicles, Transport & Heavy Earthmovers",
                    Keywords = new List<string>
                    {
                        "bus hiring", "buses", "utility vehicle", "flatbed truck", "open body lcv truck", "refrigerator truck",
                        "cab & taxi", "goods transport service", "built up trucks", "crawler hydraulic excavator", "e bicycle",
                        "electric bicycle", "e-rickshaw", "electric vehicle", "ev charger", "backhoe loader", "jcb", "dump truck",
                        "tipper", "tractor", "forklift", "crane vehicle", "ambulance", "fire tender", "water tanker vehicle",
                        "garbage compactor vehicle", "sweeper vehicle", "vehicle hiring", "hiring of vehicle", "cab hiring",
                        "car hiring", "transport truck"

                    }
                }
            }
        },

        // =====================================================
        // ELECTRICAL & POWER
        // =====================================================
        new CategoryDefinition
        {
            Key = "ELECTRICAL",
            DisplayName = "Electrical & Power Equipment",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "CABLES",
                    DisplayName = "Cables, Conductor & Wiring",
                    Keywords = new List<string>
                    {
                        "electric cable", "xlpe cable", "pvc insulated cable", "power cable", "copper cable", "cable lugs",
                        "cable tie", "earthing rod", "wire rope", "steel wire rope", "braided copper flexible connector", "fuse wire",
                        "fuse link", "armoured cable", "unarmoured cable", "aluminum cable", "control cable", "instrumentation cable",
                        "submersible cable", "flexible wire", "house wire", "cable tray", "perforated cable tray", "ladder cable tray",
                        "cable gland", "heat shrinkable termination kit", "jointing kit"

                    }
                },

                new SubCategory
                {
                    Key = "POWER_EQUIPMENT",
                    DisplayName = "UPS, Generators, Transformers & Power Supplies",
                    Keywords = new List<string>
                    {
                        "ups", "online ups", "line interactive ups", "battery charger", "lead-acid", "voltage stabilizer",
                        "voltage corrector", "frequency converter", "dg set", "silent dg set", "generator", "acb",
                        "air circuit breaker", "mccb", "moulded case circuit breaker", "shunt power capacitor",
                        "surge protective device", "battery", "exide powersafe", "distribution transformer", "power transformer",
                        "servo stabilizer", "tubular battery", "smf battery", "smf vrla battery", "stationary lead acid",
                        "valve regulated", "lithium ion battery", "solar power plant", "solar panel", "solar inverter", "inverter",
                        "amf panel", "auto transfer switch panel", "isolation transformer", "dummy load"

                    }
                },

                new SubCategory
                {
                    Key = "LIGHTING",
                    DisplayName = "Lighting & Luminaires",
                    Keywords = new List<string>
                    {
                        "led flood light", "led bulb", "led luminaire", "street light", "tungsten halogen lamp",
                        "helipad elevated light", "decorative lightning pole", "solar lantern", "lamp fluorescent", "led tube light",
                        "bay light", "high bay led light", "panel light", "downlight", "flameproof light",
                        "aviation obstruction light", "stadium light", "high mast light", "solar street light", "led driver"

                    }
                },

                new SubCategory
                {
                    Key = "SWITCHGEAR",
                    DisplayName = "Switches, Switchgear & Panels",
                    Keywords = new List<string>
                    {
                        "rotary switch", "rotary selector switch", "foot operated switch", "plug and socket", "distribution board",
                        "choke", "relay module", "relay frame", "mcb", "miniature circuit breaker", "elcb", "rccb", "contactor",
                        "thermal overload relay", "vfd", "variable frequency drive", "soft starter", "lt panel", "ht panel",
                        "feeder pillar", "busbar", "busduct", "changeover switch", "industrial plug socket"

                    }
                },

                new SubCategory
                {
                    Key = "HVAC_AIR_CONDITIONING",
                    DisplayName = "HVAC, Air Conditioning & Ventilation",
                    Keywords = new List<string>
                    {
                        "air conditioner", "split air conditioner", "ac unit", "hvac", "window ac", "cassette ac", "chiller",
                        "cooling tower", "room heater", "convection heater", "blower", "ventilation fan", "exhaust fan",
                        "axial explosion proof blower", "positive pressure ventilation fan", "smoke exhauster", "air exhauster",
                        "air cooler"

                    }
                }
            }
        },

        // =====================================================
        // MECHANICAL & INDUSTRIAL
        // =====================================================
        new CategoryDefinition
        {
            Key = "MECHANICAL",
            DisplayName = "Mechanical & Industrial Equipment",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "BEARINGS_FASTENERS",
                    DisplayName = "Bearings, Fasteners & Hardware",
                    Keywords = new List<string>
                    {
                        "roller bearing", "cylindrical roller bearing", "fan bearing", "big end bearing", "bolt", "nut", "ms bolt",
                        "ht bolt", "washer", "lock washer", "rivet nut", "coach bolt", "backing ring", "support ring", "gland packing",
                        "o ring", "check nut", "screwdriver", "screwdriver kit", "wire stripper", "welding rod", "electrode welding",
                        "deep groove ball bearing", "tapered roller bearing", "spherical roller bearing", "pillow block bearing",
                        "stud bolt", "anchor bolt", "grub screw", "allen key", "spanner set", "torque wrench", "socket wrench set",
                        "welding machine", "mig welding"


                    }
                },

                new SubCategory
                {
                    Key = "MACHINE_TOOLS",
                    DisplayName = "Machine Tools & Workshop Machinery",
                    Keywords = new List<string>
                    {
                        "milling machine", "vmc machine", "hydraulic press brake", "bench grinder", "pedestal grinder", "chuck",
                        "collet chuck", "drill chuck", "twist drill", "reamer", "tap ", "grinder", "pneumatic grinder",
                        "hydraulic press", "lathe", "cnc lathe", "cnc turning center", "shaper machine", "radial drilling machine",
                        "band saw machine", "shearing machine", "surface grinder", "tool cutter grinder", "power hacksaw",
                        "cnc router", "jaw crusher", "crusher machine", "fuel dispensing", "power tools"

                    }
                },

                new SubCategory
                {
                    Key = "PUMPS_COMPRESSORS",
                    DisplayName = "Pumps, Compressors & Fluid Handling",
                    Keywords = new List<string>
                    {
                        "submersible motor pump", "submersible pumpsets", "submersible pump", "air compressor",
                        "reciprocating air compressor", "hydraulic cylinder", "coalescer cartridge", "separator cartridge",
                        "pressure gauge", "vacuum gauge", "pressure switch", "pressure transmitter", "digital pressure check device",
                        "centrifugal pump", "screw air compressor", "rotary vane pump", "dosing pump", "slurry pump",
                        "de-watering pump", "monoblock pump", "hydraulic power pack", "hydraulic hose", "hydraulic jack",
                        "hydraulic puller", "manual winch", "hydraulic mobile crane", "rough terrain crane"


                    }
                },

                new SubCategory
                {
                    Key = "VALVES_FITTINGS",
                    DisplayName = "Valves, Pipes & Industrial Fittings",
                    Keywords = new List<string>
                    {
                        "gate valve", "cs gate valve", "copper alloy gate valve", "gun metal gate valve", "ball valve",
                        "butterfly valve", "check valve", "non return valve", "nrv", "dome valve", "dome valve assembly",
                        "safety relief valve", "relief valve", "expansion valve", "solenoid valve", "flange", "pipe fitting"

                    }
                },

                new SubCategory
                {
                    Key = "SPARES_GENERIC",
                    DisplayName = "Industrial Spares & Machined Components",
                    Keywords = new List<string>
                    {
                        "spares", "mt spares", "spare parts", "repair kit", "repair and overhauling", "servicing", "amc of machines",
                        "shaft assembly", "sleeve", "diagnostics machine", "vibration acoustics", "gearbox", "worm gearbox",
                        "planetary gearbox", "coupling", "flexible coupling", "pulley", "conveyor belt", "chain drive",
                        "mechanical seal", "body machining", "investment casting", "yoke", "wedge", "hand wheel"

                    }
                }
            }
        },

        // =====================================================
        // CONSTRUCTION & CIVIL
        // =====================================================
        new CategoryDefinition
        {
            Key = "CONSTRUCTION",
            DisplayName = "Construction & Civil Works",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "CIVIL_WORKS",
                    DisplayName = "Civil Engineering, Infrastructure & Structures",
                    Keywords = new List<string>
                    {
                        "construction of", "excavation", "formation of pond", "compaction of surface", "stone pitching", "masonry",
                        "rr masonry", "borewell", "demolition", "flooring", "acoustic panelling", "acoustic doors",
                        "partition of wall", "observation window", "modular toilet", "portable toilet", "bathroom toilet cubicle",
                        "prefab bathroom", "road construction", "asphalt road", "concrete road", "paver block flooring",
                        "drainage work", "boundary wall construction", "waterproofing work", "roofing work", "false ceiling",
                        "tiling work", "trenching work", "porta cabin", "steel porta cabin", "shelter", "metal crash barrier",
                        "w beam"


                    }
                },

                new SubCategory
                {
                    Key = "BUILDING_MATERIALS",
                    DisplayName = "Building Materials & Structural Steel",
                    Keywords = new List<string>
                    {
                        "cement", "ordinary portland cement", "fly ash building bricks", "low cement castable", "puf panel",
                        "self adhesive tar sheet", "silicone sealant", "thinner", "araldite", "adhesive", "plywood", "steel angle",
                        "nails", "weld mesh", "aluminium door", "sliding door", "rolling shutter", "tmt bars", "steel rebar",
                        "structural steel", "ms channel", "ms beam", "ms pipe", "ms plate", "is 2062", "ismb", "steel tube", "is 1161",
                        "gi sheet", "color coated sheet", "bitumen", "ready mix concrete", "rmc", "sand", "aggregate", "granite slab",
                        "marble slab", "water proofing compound"

                    }
                },

                new SubCategory
                {
                    Key = "PLUMBING_WATER",
                    DisplayName = "Plumbing, Water Treatment & Purification",
                    Keywords = new List<string>
                    {
                        "potable water purification", "water purification system", "water purifier", "m2 pro water purifier",
                        "reverse osmosis", "ro plant", "de-ionization", "water management control system", "pvc casing pipe",
                        "hand pump", "deepwell hand pump", "gi pipe", "hdpe pipe", "upvc pipe", "cpvc pipe", "di pipe",
                        "ductile iron pipe", "water meter", "storage water tank", "sintex tank", "sewage treatment plant", "stp",
                        "water treatment plant", "wtp"

                    }
                }
            }
        },

        // =====================================================
        // FURNITURE
        // =====================================================
        new CategoryDefinition
        {
            Key = "FURNITURE",
            DisplayName = "Furniture & Interior Fixtures",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "OFFICE_FURNITURE",
                    DisplayName = "Office Furniture & Desks",
                    Keywords = new List<string>
                    {
                        "revolving chair", "office chair", "computer table", "meeting table", "centre table", "modular table", "sofa",
                        "cheque drop box", "steel filing cabinet", "steel almirah", "cabinet", "platform trolley", "executive chair",
                        "ergonomic chair", "conference table", "workstation desk", "office table", "reception table", "visitor chair",
                        "mesh chair", "credenza", "bookcase", "mobile pedestal", "metal drawer", "drawer unit"

                    }
                },

                new SubCategory
                {
                    Key = "STORAGE_RACKS",
                    DisplayName = "Storage Racks & Warehousing Shelves",
                    Keywords = new List<string>
                    {
                        "heavy duty storage rack", "metal shelving rack", "veg rack", "vegetable storage rack", "poly-pallets",
                        "plastic storage pallet", "kitchen shelf", "slotted angle rack", "pallet rack", "cantilever rack",
                        "compactor storage system", "mobile compactor", "steel rack", "display rack", "plastic crate",
                        "metal storage cabinet"

                    }
                },

                new SubCategory
                {
                    Key = "INSTITUTIONAL_FURNITURE",
                    DisplayName = "Institutional, Educational & Outdoor Furniture",
                    Keywords = new List<string>
                    {
                        "table supreme folding", "chair supreme baby", "bean bag chair", "open bar canopy", "teak furniture",
                        "flag post", "flag pole", "dual desk classroom", "school bench", "auditorium chair", "library table",
                        "hostel bed", "bunk bed", "dining table mess", "park bench", "garden chair", "podium", "lectern",
                        "desk and bench set"

                    }
                }
            }
        },

        // =====================================================
        // SPORTS & FITNESS
        // =====================================================
        new CategoryDefinition
        {
            Key = "SPORTS",
            DisplayName = "Sports & Fitness",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "SPORTS_EQUIPMENT",
                    DisplayName = "Sports Goods & Outdoor Gym Equipment",
                    Keywords = new List<string>
                    {
                        "basketball", "basketball court", "volleyball", "hockey", "personal kit hockey", "cricket trouser",
                        "field hockey ball", "sports and gym kit bag", "outdoor gym equipment", "air walker", "leg press",
                        "shoulder builder", "arm wheel", "hand rower", "lat pull down", "seesaw", "twister", "spring rider",
                        "merry go round", "football", "badminton net", "badminton racket", "table tennis table", "cricket bat",
                        "cricket ball", "cricket kit", "treadmill", "exercise bike", "dumbbell", "barbell",
                        "synthetic sports flooring", "gymnastic mat", "taekwondo", "kick pad", "ped-o-cycle"

                    }
                },

                new SubCategory
                {
                    Key = "SPORTS_APPAREL",
                    DisplayName = "Sports Wear, Medals & Trophies",
                    Keywords = new List<string>
                    {
                        "medal", "medal with box", "sports shirt", "t-shirt", "physical training shoes", "track suit", "sports jersey",
                        "sports shorts", "trophy", "shield sports", "sports socks", "pt shoes"

                    }
                }
            }
        },

        // =====================================================
        // STATIONERY & OFFICE SUPPLIES
        // =====================================================
        new CategoryDefinition
        {
            Key = "STATIONERY",
            DisplayName = "Stationery & Office Supplies",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "OFFICE_STATIONERY",
                    DisplayName = "Office Desk Supplies & Filing",
                    Keywords = new List<string>
                    {
                        "file/folder", "file cover", "register", "bills or bill books", "pen refill", "pilot pen", "glue stick",
                        "scissors", "stationery scissors", "box file", "printed register", "training stationery", "ballpen", "gel pen",
                        "marker pen", "highlighter", "stapler", "stapler pin", "paper clip", "punching machine", "sticky notes",
                        "notepad", "envelope", "laminated folder", "cobra file", "ring binder", "pin up notice board", "notice board",
                        "teaching chart", "charts & publications", "educational chart", "answer book", "answer sheet", "drawing board"

                    }
                },

                new SubCategory
                {
                    Key = "PRINTING_PAPER",
                    DisplayName = "Printing Paper & Media Supplies",
                    Keywords = new List<string>
                    {
                        "plain copier paper", "maplitho paper", "photography paper", "tracing paper", "printing services",
                        "paper-based printing", "self adhesive flags", "a4 copier paper", "a3 paper", "fs paper", "bond paper",
                        "thermal paper roll", "plotter paper roll", "carbon paper", "lamination pouch", "writing and printing paper",
                        "writing paper"

                    }
                }
            }
        },

        // =====================================================
        // TEXTILE, UNIFORMS & FOOTWEAR
        // =====================================================
        new CategoryDefinition
        {
            Key = "TEXTILE",
            DisplayName = "Textile, Uniforms & Footwear",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "UNIFORMS",
                    DisplayName = "Uniforms & Tailored Apparel",
                    Keywords = new List<string>
                    {
                        "tunic terrywool", "stitching and tailoring", "beret cap", "handloom cotton", "lapel pin", "khaki uniform",
                        "dungaree", "overall suit", "boiler suit", "apron", "blazer", "raincoat", "high visibility jacket",
                        "reflective jacket", "ceremonial uniform", "peak cap", "lanyard", "epaulettes", "water proof cover",
                        "bag water proof", "canvas cover", "tarpaulin", "fire resistant action overall"

                    }
                },

                new SubCategory
                {
                    Key = "BEDDING_LINEN",
                    DisplayName = "Bedding, Towels & Linens",
                    Keywords = new List<string>
                    {
                        "bed sheet", "bedsheet", "pillow cover", "bedding set", "handloom cotton bed sheet", "blanket",
                        "woollen blanket", "quilt", "mattress", "foam mattress", "coir mattress", "bath towel", "hand towel",
                        "curtains", "mosquito net", "table cloth", "cotton waste", "cotton yarn waste", "hessian"

                    }
                },

                new SubCategory
                {
                    Key = "FOOTWEAR",
                    DisplayName = "Safety Footwear, Boots & Shoes",
                    Keywords = new List<string>
                    {
                        "shoes leather oxford", "safety footwear", "esd slippers", "safety shoes", "gumboots", "rubber boots",
                        "jungle boots", "combat boots", "derby shoes", "canvas shoes", "sports shoes footwear"

                    }
                }
            }
        },

        // =====================================================
        // FOOD, RATIONS & CATERING
        // =====================================================
        new CategoryDefinition
        {
            Key = "FOOD",
            DisplayName = "Food, Rations & Catering",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "PROVISIONS",
                    DisplayName = "Dry Provisions, Grocery & Fresh Rations",
                    Keywords = new List<string>
                    {
                        "cheese spread", "cheese slice", "cheese cube", "ham and bacon", "chicken sausage", "chicken curried",
                        "processed cheese", "vegetable fresh", "lady finger", "pumpkin", "cucumber", "brinjal", "tomato ripe",
                        "amaranthus", "bitter gourd", "biscuit", "cashew nut", "pickle", "corn flour", "tomato sauce", "vinegar",
                        "papad", "horlicks", "jam & marmalade", "jam and marmalade", "tea (ctc)", "spices and condiments", "coriander",
                        "condiments", "officer ration", "rice", "wheat flour", "atta", "pulses", "dal", "mustard oil", "sunflower oil",
                        "refined oil", "sugar", "salt", "milk powder", "butter", "egg fresh", "fish fresh", "mutton fresh",
                        "chicken fresh", "fruit fresh", "apple", "banana fresh", "pet food", "dog food", "cat food"

                    }
                },

                new SubCategory
                {
                    Key = "CATERING_SERVICES",
                    DisplayName = "Catering Services & Mess Utensils",
                    Keywords = new List<string>
                    {
                        "canteen service", "catering service", "catering services", "bain marie", "dining bowl", "spoon",
                        "hiring of catering", "mess service", "cooked food service", "stainless steel plate", "thali", "water glass",
                        "cooking vessel", "commercial gas stove", "deep freezer", "commercial refrigerator", "dishwashing machine",
                        "bottle washing machine", "fruit and vegetable washing machine"

                    }
                }
            }
        },

        // =====================================================
        // FIRE & SAFETY
        // =====================================================
        new CategoryDefinition
        {
            Key = "FIRE_SAFETY",
            DisplayName = "Fire Fighting & Personal Protective Equipment",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "FIRE_FIGHTING",
                    DisplayName = "Fire Fighting & Extinguishing Systems",
                    Keywords = new List<string>
                    {
                        "fire extinguisher", "portable fire extinguisher", "wheeled fire extinguisher", "fire suppression system",
                        "master stream foam nozzle", "co2 cylinder", "abc fire extinguisher", "co2 fire extinguisher",
                        "foam fire extinguisher", "fire hose pipe", "fire hydrant valve", "fire alarm system", "smoke detector",
                        "heat detector", "fire sprinkler", "fire pump", "fire suit", "fire proximity suit"

                    }
                },

                new SubCategory
                {
                    Key = "PPE",
                    DisplayName = "Personal Safety & Protective Equipment",
                    Keywords = new List<string>
                    {
                        "safety helmet", "industrial safety helmet", "industrial safety glove", "gauntlet", "live working gloves",
                        "insulating mats", "heat resistant gloves", "electrical safety devices", "esd apron", "safety goggles",
                        "ear defender", "ear plug", "dust mask", "respirator", "fall arrest harness", "safety belt", "life jacket",
                        "chemical protective suit", "wind sock", "safety and emergency"

                    }
                }
            }
        },

        // =====================================================
        // HOUSEKEEPING & CLEANING
        // =====================================================
        new CategoryDefinition
        {
            Key = "HOUSEKEEPING",
            DisplayName = "Housekeeping & Facility Hygiene",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "CLEANING_CONSUMABLES",
                    DisplayName = "Cleaning Consumables & Reagents",
                    Keywords = new List<string>
                    {
                        "harpic", "toilet cleaner", "floor cleaner", "dettol", "hand wash", "sodium hypochlorite",
                        "carpet and upholstery shampoo", "electrostatic liquid cleaner", "mouse trap", "hdpe bucket", "broom",
                        "air freshener", "domestic wiper", "phenyl", "bleaching powder", "liquid soap", "hand sanitizer",
                        "garbage bag", "mop cloth", "microfiber cloth", "scrubber", "dustbin", "wheelie bin", "bath soap",
                        "laundry soap", "shampoo", "hair oil"

                    }
                },

                new SubCategory
                {
                    Key = "HOUSEKEEPING_SERVICES",
                    DisplayName = "Housekeeping & Sanitation Services",
                    Keywords = new List<string>
                    {
                        "cleaning, sanitation and disinfection", "housekeeping consumables", "upholstery cleaning", "general cleaning",
                        "housekeeping services", "facility cleaning", "pest control service", "pest and animal control",
                        "rodent control service", "facade cleaning", "septic tank cleaning", "waste collection service",
                        "drain cleaning"

                    }
                }
            }
        },

        // =====================================================
        // LABORATORY & SCIENTIFIC
        // =====================================================
        new CategoryDefinition
        {
            Key = "LABORATORY",
            DisplayName = "Laboratory & Scientific Instruments",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "LAB_EQUIPMENT",
                    DisplayName = "Scientific Instruments & Lab Ware",
                    Keywords = new List<string>
                    {
                        "spectrophotometer", "lcr meter", "magnetic stirring bar", "buffer solution", "cocktail shaker",
                        "scintillation cocktail solution", "laboratory glassware", "topographical model", "analytical balance",
                        "precision balance", "ph meter", "conductivity meter", "magnetic stirrer", "hot plate", "water bath",
                        "muffle furnace", "fume hood", "laminar air flow", "pipette", "beaker", "flask", "viscometer", "visgage",
                        "high temperature furnace", "tubular furnace", "compression testing machine", "clamp meter", "true rms",
                        "filter integrity", "electrostatic detection apparatus", "bottle for chemical laboratory", "dna extraction"


                    }
                }
            }
        },

        // =====================================================
        // CHEMICALS & INDUSTRIAL SUPPLIES
        // =====================================================
        new CategoryDefinition
        {
            Key = "CHEMICALS",
            DisplayName = "Chemicals & Industrial Solvents",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "INDUSTRIAL_CHEMICALS",
                    DisplayName = "Industrial Chemicals, Adhesives & Paints",
                    Keywords = new List<string>
                    {
                        "precipitated barium carbonate", "rust converter", "cyanoacrylate adhesive", "threadlocking adhesive",
                        "thermal silicon adhesive", "paint rfu", "primer", "hydrochloric acid", "sulfuric acid", "caustic soda",
                        "alum", "polyaluminium chloride", "enamel paint", "epoxy paint", "emulsion paint", "thinner solvent",
                        "distilled water", "smoke grey paint", "potassium nitrate", "nitroguanidine", "picrite", "marine fuel",
                        "furnace oil", "diesel fuel", "aviation fuel"


                    }
                }
            }
        },

        // =====================================================
        // MANPOWER & OUTSOURCED SERVICES
        // =====================================================
        new CategoryDefinition
        {
            Key = "SERVICES_GENERAL",
            DisplayName = "Manpower & Outsourced Services",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "MANPOWER",
                    DisplayName = "Manpower Outsourcing & Security Services",
                    Keywords = new List<string>
                    {
                        "manpower outsourcing", "facility management services", "security services", "esm security services",
                        "dgr empanelled", "unskilled manpower", "semi skilled manpower", "skilled manpower", "highly skilled manpower",
                        "data entry operator outsourcing", "driver outsourcing", "cook outsourcing", "gardener outsourcing",
                        "security guard", "security manpower service", "manpower hiring"

                    }
                },

                new SubCategory
                {
                    Key = "LOGISTICS_WAREHOUSING",
                    DisplayName = "Logistics, Transportation & Warehousing Services",
                    Keywords = new List<string>
                    {
                        "warehousing service", "storage service", "logistics service", "godown service", "cargo service",
                        "freight service", "handling and transport", "hiring of transport"


                    }
                },

                new SubCategory
                {
                    Key = "PROFESSIONAL_SERVICES",
                    DisplayName = "Professional Consultancy & Misc Services",
                    Keywords = new List<string>
                    {
                        "photography services", "hiring of consultants", "asset valuation services", "calibration services",
                        "custom bid for services", "strain measurement", "videography services", "event management services",
                        "chartered accountant services", "legal consultancy", "third party inspection services",
                        "selection of laboratories", "testing laboratory", "binding and stitching", "creative agency",
                        "advertising agency", "training services", "training service", "skill development", "painting service",
                        "professional painting service", "short refit", "repair and overhauling service", "customized amc",
                        "horticulture service"

                    }
                }
            }
        },

        new CategoryDefinition
        {
            Key = "OTHER",
            DisplayName = "Other",
            SubCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Key = "OTHER",
                    DisplayName = "Other",
                    Keywords = new List<string>()
                }
            }
        }
    };
}