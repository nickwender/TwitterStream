using FluentMigrator;
using System;

namespace Setup.Migrators
{
    [Migration(202206221920)]
    public class InitialMigration : Migration
    {
        public override void Up()
        {
            // Table to store tweets
            Create.Table("Tweets")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("TwitterTweetId").AsString(Int32.MaxValue).NotNullable()
                .WithColumn("Text").AsString(Int32.MaxValue).NotNullable();

            // Table to store hashtags.
            Create.Table("Hashtags")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Hashtag").AsString(Int32.MaxValue).NotNullable();

            // Table to store tweet <> hashtag associations.
            Create.Table("TweetHashtags")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("HashtagId").AsInt32()
                .WithColumn("TweetId").AsInt32();
        }

        public override void Down()
        {
        }
    }
}
