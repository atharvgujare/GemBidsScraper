using GemBidScraper.Models;
using Microsoft.EntityFrameworkCore;

namespace GemBidScraper.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bid> Bids { get; set; }
        public DbSet<GeMBidExtract> GeMBidExtracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // Bid
            // -----------------------------
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.Property(x => x.EMDAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.EPBGPercentage)
                      .HasPrecision(5, 2);
            });

            // -----------------------------
            // GeMBidExtract
            // -----------------------------
            modelBuilder.Entity<GeMBidExtract>(entity =>
            {
                entity.ToTable("gembidextracts");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                 .ValueGeneratedOnAdd();

                // ----------------------------------------------------
                // Decimal Precision Mappings
                // ----------------------------------------------------
                entity.Property(x => x.EstimatedBidValue).HasPrecision(18, 2);
                entity.Property(x => x.EmdAmount).HasPrecision(18, 2);
                entity.Property(x => x.EPBGPercentage).HasPrecision(18, 2);
                entity.Property(x => x.PurchasePreferencePercentage).HasPrecision(18, 2);
                entity.Property(x => x.MaximumPurchasePreferencePercentage).HasPrecision(18, 2);

                // ----------------------------------------------------
                // String Column Configurations (Prevent Truncation Errors)
                // ----------------------------------------------------

                // Bounded Fields (Indexed/Key Fields - Max index size safe)
                entity.Property(x => x.BidNumber).HasMaxLength(100);
                entity.Property(x => x.CategoryKey).HasMaxLength(250);
                entity.Property(x => x.CategorySubKey).HasMaxLength(250);
                entity.Property(x => x.Ministry).HasMaxLength(450);
                entity.Property(x => x.DepartmentName).HasMaxLength(450);
                entity.Property(x => x.OrganisationName).HasMaxLength(450);

                // Extended Single-Line / Medium Fields
                entity.Property(x => x.CardMinistry).HasMaxLength(450);
                entity.Property(x => x.CardDepartment).HasMaxLength(450);
                entity.Property(x => x.CardItemName).HasColumnType("nvarchar(max)");
                entity.Property(x => x.OfficeName).HasMaxLength(1000);
                entity.Property(x => x.TypeOfBid).HasMaxLength(1000);
                entity.Property(x => x.EvaluationMethod).HasMaxLength(1000);
                entity.Property(x => x.RAQualificationRule).HasMaxLength(1000);
                entity.Property(x => x.AdvisoryBank).HasMaxLength(500);

                // Large / High-Volume Text Fields -> nvarchar(max)
                entity.Property(x => x.JsonData).HasColumnType("nvarchar(max)");
                entity.Property(x => x.TechnicalSpecificationJson).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ItemCategory).HasColumnType("nvarchar(max)");
                entity.Property(x => x.PrimaryProductCategory).HasColumnType("nvarchar(max)");
                entity.Property(x => x.BOQTitle).HasColumnType("nvarchar(max)");
                entity.Property(x => x.DocumentRequiredFromSeller).HasColumnType("nvarchar(max)");
                entity.Property(x => x.GeMARPTSSearchedStrings).HasColumnType("nvarchar(max)");
                entity.Property(x => x.GeMARPTSSearchedResults).HasColumnType("nvarchar(max)");
                entity.Property(x => x.RelevantCategoriesSelectedForNotification).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ArbitrationClause).HasColumnType("nvarchar(max)");
                entity.Property(x => x.MediationClause).HasColumnType("nvarchar(max)");
                entity.Property(x => x.WarrantyText).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ServiceRequirement).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ServiceInclusions).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ConsigneeAddress).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ConsigneeQuantity).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ConsigneeName).HasColumnType("nvarchar(max)");
                entity.Property(x => x.ContactDetailsOfGrievanceRedressal).HasColumnType("nvarchar(max)");
                entity.Property(x => x.InspectionAgency).HasColumnType("nvarchar(max)");
                entity.Property(x => x.InspectionType).HasColumnType("nvarchar(max)");
                entity.Property(x => x.PaymentTimelines).HasColumnType("nvarchar(max)");
                entity.Property(x => x.SimilarCategory).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Specification).HasColumnType("nvarchar(max)");
                entity.Property(x => x.SpecificationDocument).HasColumnType("nvarchar(max)");
                entity.Property(x => x.SpecificationParameterName).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Values).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Material).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Surface).HasColumnType("nvarchar(max)");
                entity.Property(x => x.PhysicalCharacteristics).HasColumnType("nvarchar(max)");
                entity.Property(x => x.BOQDetailDocument).HasColumnType("nvarchar(max)");
                entity.Property(x => x.BuyerSpecificationDocument).HasColumnType("nvarchar(max)");
                entity.Property(x => x.PdfUrl).HasColumnType("nvarchar(max)");

                // ----------------------------------------------------
                // Indexes
                // ----------------------------------------------------
                entity.HasIndex(x => x.BidNumber).IsUnique();
                entity.HasIndex(x => x.CreatedOn);
                entity.HasIndex(x => x.BidEndDateTime);
                entity.HasIndex(x => x.Ministry);
                entity.HasIndex(x => x.DepartmentName);
                entity.HasIndex(x => x.OrganisationName);
                entity.HasIndex(x => x.CategoryKey);
                entity.HasIndex(x => x.CategorySubKey);
                entity.HasIndex(x => x.EmdAmount);
                entity.HasIndex(x => new { x.BidEndDateTime, x.CreatedOn });
            });
        }
    }
}