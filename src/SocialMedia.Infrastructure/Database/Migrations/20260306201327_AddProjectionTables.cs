using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── PostProjections ──────────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE PostProjections (
                    PostId          INT             NOT NULL,
                    UserId          INT             NOT NULL,
                    UserName      NVARCHAR(200)   NOT NULL,
                    UserEmail     NVARCHAR(200)   NOT NULL,
                    UserAvatarUrl NVARCHAR(500)   NOT NULL DEFAULT '',
                    Content         NVARCHAR(1000)  NOT NULL,
                    ReactsCount  INT             NOT NULL DEFAULT 0,
                    CommentsCount   INT             NOT NULL DEFAULT 0,
                    IsDeleted       BIT             NOT NULL DEFAULT 0,
                    CreatedAt       DATETIME2       NOT NULL,
                    UpdatedAt       DATETIME2       NULL,
                    DeletedAt       DATETIME2       NULL,
                    CONSTRAINT PK_PostProjections PRIMARY KEY (PostId)
                );
                CREATE INDEX IX_PostProjections_UserId    ON PostProjections (UserId);
                CREATE INDEX IX_PostProjections_IsDeleted ON PostProjections (IsDeleted);
            ");

            // ── PostAttachmentProjections ────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE PostAttachmentProjections (
                    AttachmentId    INT             NOT NULL,
                    PostId          INT             NOT NULL,
                    Url             NVARCHAR(500)   NOT NULL,
                    AttachmentType  INT             NOT NULL,
                    CONSTRAINT PK_PostAttachmentProjections PRIMARY KEY (AttachmentId)
                );
                CREATE INDEX IX_PostAttachmentProjections_PostId ON PostAttachmentProjections (PostId);
            ");

            // ── CommentProjections ───────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE CommentProjections (
                    CommentId       INT             NOT NULL,
                    PostId          INT             NOT NULL,
                    ParentCommentId INT             NULL,
                    UserId          INT             NOT NULL,
                    UserName      NVARCHAR(200)   NOT NULL,
                    UserEmail     NVARCHAR(200)   NOT NULL,
                    UserAvatarUrl NVARCHAR(500)   NOT NULL DEFAULT '',
                    Content         NVARCHAR(500)   NOT NULL,
                    ReactsCount  INT             NOT NULL DEFAULT 0,
                    RepliesCount    INT             NOT NULL DEFAULT 0,
                    CreatedAt       DATETIME2       NOT NULL,
                    UpdatedAt       DATETIME2       NULL,
                    CONSTRAINT PK_CommentProjections PRIMARY KEY (CommentId)
                );
                CREATE INDEX IX_CommentProjections_PostId          ON CommentProjections (PostId);
                CREATE INDEX IX_CommentProjections_ParentCommentId ON CommentProjections (ParentCommentId);
            ");

            // ── PostReactProjections ─────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE PostReactProjections (
                    Id              INT             NOT NULL,
                    PostId          INT             NOT NULL,
                    UserId          INT             NOT NULL,
                    UserName        NVARCHAR(200)   NOT NULL,
                    UserEmail       NVARCHAR(200)   NOT NULL,
                    UserAvatarUrl   NVARCHAR(500)   NOT NULL DEFAULT '',
                    ReactType       INT             NOT NULL,
                    CreatedAt       DATETIME2       NOT NULL,
                    CONSTRAINT PK_PostReactProjections PRIMARY KEY (Id)
                );
                CREATE INDEX IX_PostReactProjections_PostId ON PostReactProjections (PostId);
            ");

            // ── CommentReactProjections ──────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE CommentReactProjections (
                    Id              INT             NOT NULL,
                    CommentId       INT             NOT NULL,
                    UserId          INT             NOT NULL,
                    UserName        NVARCHAR(200)   NOT NULL,
                    UserEmail       NVARCHAR(200)   NOT NULL,
                    UserAvatarUrl   NVARCHAR(500)   NOT NULL DEFAULT '',
                    ReactType       INT             NOT NULL,
                    CreatedAt       DATETIME2       NOT NULL,
                    CONSTRAINT PK_CommentReactProjections PRIMARY KEY (Id)
                );
                CREATE INDEX IX_CommentReactProjections_CommentId ON CommentReactProjections (CommentId);
            ");

            // ── UserFollowProjections ────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE UserFollowProjections (
                    FollowerId          INT             NOT NULL,
                    FollowingId         INT             NOT NULL,
                    FollowerName        NVARCHAR(200)   NOT NULL,
                    FollowerEmail       NVARCHAR(200)   NOT NULL,
                    FollowerAvatarUrl   NVARCHAR(500)   NOT NULL DEFAULT '',
                    FollowingName       NVARCHAR(200)   NOT NULL,
                    FollowingEmail      NVARCHAR(200)   NOT NULL,
                    FollowingAvatarUrl  NVARCHAR(500)   NOT NULL DEFAULT '',
                    CreatedAt           DATETIME2       NULL,
                    CONSTRAINT PK_UserFollowProjections PRIMARY KEY (FollowerId, FollowingId)
                );
                CREATE INDEX IX_UserFollowProjections_FollowerId  ON UserFollowProjections (FollowerId);
                CREATE INDEX IX_UserFollowProjections_FollowingId ON UserFollowProjections (FollowingId);
            ");

            // ── GroupProjections ─────────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE GroupProjections (
                    GroupId         UNIQUEIDENTIFIER    NOT NULL,
                    Name            NVARCHAR(200)       NULL,
                    Type            INT                 NOT NULL,
                    TotalMessages   INT                 NOT NULL DEFAULT 0,
                    CreatedAt       DATETIME2           NOT NULL,
                    CONSTRAINT PK_GroupProjections PRIMARY KEY (GroupId)
                );
            ");

            // ── GroupMemberProjections ───────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE GroupMemberProjections (
                    GroupId         UNIQUEIDENTIFIER    NOT NULL,
                    UserId          INT                 NOT NULL,
                    UserName        NVARCHAR(200)       NOT NULL,
                    UserEmail       NVARCHAR(200)       NOT NULL,
                    UserAvatarUrl   NVARCHAR(500)       NOT NULL DEFAULT '',
                    CONSTRAINT PK_GroupMemberProjections PRIMARY KEY (GroupId, UserId)
                );
                CREATE INDEX IX_GroupMemberProjections_GroupId ON GroupMemberProjections (GroupId);
            ");

            // ── MessageProjections ───────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE MessageProjections (
                    MessageId       INT                 NOT NULL,
                    GroupId         UNIQUEIDENTIFIER    NOT NULL,
                    FromId          INT                 NOT NULL,
                    SenderName      NVARCHAR(200)       NOT NULL,
                    SenderEmail     NVARCHAR(200)       NOT NULL,
                    SenderAvatarUrl NVARCHAR(500)       NOT NULL DEFAULT '',
                    Data            NVARCHAR(MAX)       NOT NULL,
                    CreatedAt       DATETIME2           NOT NULL,
                    CONSTRAINT PK_MessageProjections PRIMARY KEY (MessageId)
                );
                CREATE INDEX IX_MessageProjections_GroupId ON MessageProjections (GroupId);
            ");

            // ── MessageStatusProjections ─────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE MessageStatusProjections (
                    MessageId           INT                 NOT NULL,
                    ReceiverId          INT                 NOT NULL,
                    ReceiverName        NVARCHAR(200)       NOT NULL,
                    ReceiverEmail       NVARCHAR(200)       NOT NULL,
                    ReceiverAvatarUrl   NVARCHAR(500)       NOT NULL DEFAULT '',
                    StatusType          INT                 NOT NULL,
                    SentAt              DATETIME2           NULL,
                    DeliveredAt         DATETIME2           NULL,
                    SeenAt              DATETIME2           NULL,
                    CONSTRAINT PK_MessageStatusProjections PRIMARY KEY (MessageId, ReceiverId)
                );
                CREATE INDEX IX_MessageStatusProjections_MessageId ON MessageStatusProjections (MessageId);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS MessageStatusProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS MessageProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS GroupMemberProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS GroupProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS UserFollowProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS CommentReactProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS PostReactProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS CommentProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS PostAttachmentProjections;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS PostProjections;");
        }
    }
}
