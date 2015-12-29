using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using TheWorld.Models;
using Microsoft.Data.Entity.SqlServer.Metadata;

namespace TheWorld.Migrations
{
    [DbContext(typeof(WorldContext))]
    partial class WorldContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Annotation("ProductVersion", "7.0.0-beta7-15540")
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerIdentityStrategy.IdentityColumn);

            modelBuilder.Entity("TheWorld.Models.Stop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Arrival");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name");

                    b.Property<int>("Order");

                    b.Property<int?>("TripId");

                    b.Key("Id");
                });

            modelBuilder.Entity("TheWorld.Models.Trip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Name");

                    b.Property<string>("UserName");

                    b.Key("Id");
                });

            modelBuilder.Entity("TheWorld.Models.Stop", b =>
                {
                    b.Reference("TheWorld.Models.Trip")
                        .InverseCollection()
                        .ForeignKey("TripId");
                });
        }
    }
}
