// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using daBoot.Data;

namespace daBoot.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210906163612_ChangeRelationName")]
    partial class ChangeRelationName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("daBoot.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("ProfilePicURL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("daBoot.Models.Item", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Borrower")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Lender")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("daBoot.Models.Relation", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("TeamMemberId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "TeamMemberId");

                    b.HasIndex("TeamMemberId");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("daBoot.Models.Relation", b =>
                {
                    b.HasOne("daBoot.Models.Account", "TeamMember")
                        .WithMany("TeamMembers")
                        .HasForeignKey("TeamMemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("daBoot.Models.Account", "User")
                        .WithMany("OthersTeamMember")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TeamMember");

                    b.Navigation("User");
                });

            modelBuilder.Entity("daBoot.Models.Account", b =>
                {
                    b.Navigation("OthersTeamMember");

                    b.Navigation("TeamMembers");
                });
#pragma warning restore 612, 618
        }
    }
}
