using Code_Record.Server.Models.SQLServer;
using Code_Record.Server.Models.SQLServer.SubjectStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using Microsoft.EntityFrameworkCore;

namespace Code_Record.Server.Contexts;

public partial class SQLServerContext : DbContext
{
    public SQLServerContext()
    {
    }

    public SQLServerContext(DbContextOptions<SQLServerContext> options)
        : base(options)
    {
    }

	public virtual DbSet<Allow> Allows { get; set; }

	public virtual DbSet<Answer> Anwsers { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.UseSqlServer("Server=host.docker.internal,1434; Database=code_record; User Id=CodeRecord; Password=coderecordpass; TrustServerCertificate=True;");

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Allow>(entity =>
		{
			entity.ToTable("allows", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.ResourceId).HasColumnName("resourceId");
			entity.Property(e => e.ResourceType)
				.HasMaxLength(50)
				.IsUnicode(false)
				.HasColumnName("resourceType");
			entity.Property(e => e.UpdateDate)
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UserId).HasColumnName("userId");

			entity.HasOne(d => d.User).WithMany(p => p.Allows)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_userId");
		});

		modelBuilder.Entity<Answer>(entity =>
		{
			entity.ToTable("anwsers", "code_record");

			entity.HasIndex(e => e.UserEmail, "user_email");

			entity.HasIndex(e => e.UserMention, "user_mention");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.IdComment).HasColumnName("id_comment");
			entity.Property(e => e.Message)
				.IsUnicode(false)
				.HasColumnName("message");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UserEmail)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_email");
			entity.Property(e => e.UserMention)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_mention");

			entity.HasOne(d => d.IdCommentNavigation).WithMany(p => p.Anwsers)
				.HasForeignKey(d => d.IdComment)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_anwsers_idComment");

			entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.AnwserUserEmailNavigations)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserEmail)
				.HasConstraintName("FK_anwsers_userEmail");

			entity.HasOne(d => d.UserMentionNavigation).WithMany(p => p.AnwserUserMentionNavigations)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserMention)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_anwsers_userMention");
		});

		modelBuilder.Entity<Comment>(entity =>
		{
			entity.ToTable("comments", "code_record");

			entity.HasIndex(e => e.UserEmail, "user_email");

			entity.HasIndex(e => e.UserMention, "user_mention");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.IdConversation)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id_conversation");
			entity.Property(e => e.Message)
				.IsUnicode(false)
				.HasColumnName("message");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UserEmail)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_email");
			entity.Property(e => e.UserMention)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_mention");

			entity.HasOne(d => d.IdConversationNavigation).WithMany(p => p.Comments)
				.HasForeignKey(d => d.IdConversation)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_comments_idConversation");

			entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.CommentUserEmailNavigations)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserEmail)
				.HasConstraintName("FK_comments_userEmail");

			entity.HasOne(d => d.UserMentionNavigation).WithMany(p => p.CommentUserMentionNavigations)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserMention)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_comments_userMention");
		});

		modelBuilder.Entity<Conversation>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PK_conversation");

			entity.ToTable("conversations", "code_record");

			entity.HasIndex(e => e.UserEmail, "user_email");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.IdVideo).HasColumnName("id_video");
			entity.Property(e => e.Message)
				.IsUnicode(false)
				.HasColumnName("message");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UserEmail)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_email");

			entity.HasOne(d => d.IdVideoNavigation).WithMany(p => p.Conversations)
				.HasForeignKey(d => d.IdVideo)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_conversations_idVideo");

			entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.Conversations)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserEmail)
				.HasConstraintName("FK_conversations_userEmail");
		});

		modelBuilder.Entity<Option>(entity =>
		{
			entity.ToTable("options", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.Anwser)
				.IsUnicode(false)
				.HasColumnName("anwser");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.IdQuestion).HasColumnName("id_question");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");

			entity.HasOne(d => d.IdQuestionNavigation).WithMany(p => p.Options)
				.HasForeignKey(d => d.IdQuestion)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_options_idQuestion");
		});

		modelBuilder.Entity<Question>(entity =>
		{
			entity.ToTable("questions", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.Anwser)
				.IsUnicode(false)
				.HasColumnName("anwser");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.IdTest).HasColumnName("id_test");
			entity.Property(e => e.Question1)
				.IsUnicode(false)
				.HasColumnName("question");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");

			entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.Questions)
				.HasForeignKey(d => d.IdTest)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_questions_idTest");
		});

		modelBuilder.Entity<Resource>(entity =>
		{
			entity.ToTable("resources", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.Description)
				.IsUnicode(false)
				.HasColumnName("description");
			entity.Property(e => e.IdSubject).HasColumnName("id_subject");
			entity.Property(e => e.ResourceContent)
				.IsUnicode(false)
				.HasColumnName("resource_content");
			entity.Property(e => e.Src)
				.IsUnicode(false)
				.HasColumnName("src");
			entity.Property(e => e.Tag)
				.HasMaxLength(20)
				.IsUnicode(false)
				.HasColumnName("tag");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UploadDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("upload_date");

			entity.HasOne(d => d.IdSubjectNavigation).WithMany(p => p.Resources)
				.HasForeignKey(d => d.IdSubject)
				.HasConstraintName("FK_resources_idSubject");
		});

		modelBuilder.Entity<Result>(entity =>
		{
			entity.ToTable("results", "code_record");

			entity.HasIndex(e => e.UserEmail, "user_email");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.IdTest).HasColumnName("id_test");
			entity.Property(e => e.Score).HasColumnName("score");
			entity.Property(e => e.UploadDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("upload_date");
			entity.Property(e => e.UserEmail)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_email");

			entity.HasOne(d => d.IdTestNavigation).WithMany(p => p.Results)
				.HasForeignKey(d => d.IdTest)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK_results_idTest");

			entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.Results)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserEmail)
				.HasConstraintName("FK_results_userEmail");
		});

		modelBuilder.Entity<Subject>(entity =>
		{
			entity.ToTable("subjects", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.Description)
				.IsUnicode(false)
				.HasColumnName("description");
			entity.Property(e => e.Image)
				.IsUnicode(false)
				.HasColumnName("image");
			entity.Property(e => e.Link)
				.IsUnicode(false)
				.HasColumnName("link");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.Tool)
				.HasMaxLength(15)
				.IsUnicode(false)
				.HasColumnName("tool");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
		});

		modelBuilder.Entity<Test>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PK_test");

			entity.ToTable("tests", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.Description)
				.IsUnicode(false)
				.HasColumnName("description");
			entity.Property(e => e.IdSubject).HasColumnName("id_subject");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");

			entity.HasOne(d => d.IdSubjectNavigation).WithMany(p => p.Tests)
				.HasForeignKey(d => d.IdSubject)
				.HasConstraintName("FK_tests_idSubject");
		});

		modelBuilder.Entity<Theme>(entity =>
		{
			entity.ToTable("themes", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.Description)
				.IsUnicode(false)
				.HasColumnName("description");
			entity.Property(e => e.IdSubject).HasColumnName("id_subject");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");

			entity.HasOne(d => d.IdSubjectNavigation).WithMany(p => p.Themes)
				.HasForeignKey(d => d.IdSubject)
				.HasConstraintName("FK_themes_idSubject");
		});

		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Id)
				.HasName("PK_users_1")
				.IsClustered(false);

			entity.ToTable("users", "code_record");

			entity.HasIndex(e => e.Email, "users$email")
				.IsUnique()
				.IsClustered();

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.AccountPass)
				.IsUnicode(false)
				.HasColumnName("account_pass");
			entity.Property(e => e.CreationDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("creation_date");
			entity.Property(e => e.Email)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("email");
			entity.Property(e => e.Rol).HasColumnName("rol");
			entity.Property(e => e.Subscription).HasColumnName("subscription");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.Username)
				.IsUnicode(false)
				.HasColumnName("username");
		});

		modelBuilder.Entity<Video>(entity =>
		{
			entity.ToTable("videos", "code_record");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.Duration).HasColumnName("duration");
			entity.Property(e => e.IdTheme).HasColumnName("id_theme");
			entity.Property(e => e.Src)
				.IsUnicode(false)
				.HasColumnName("src");
			entity.Property(e => e.Title)
				.IsUnicode(false)
				.HasColumnName("title");
			entity.Property(e => e.UpdateDate)
				.HasDefaultValueSql("(NULL)")
				.HasColumnType("datetime")
				.HasColumnName("update_date");
			entity.Property(e => e.UploadDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("upload_date");

			entity.HasOne(d => d.IdThemeNavigation).WithMany(p => p.Videos)
				.HasForeignKey(d => d.IdTheme)
				.HasConstraintName("FK_videos_idTheme");
		});

		modelBuilder.Entity<Visit>(entity =>
		{
			entity.ToTable("visits", "code_record");

			entity.HasIndex(e => e.UserEmail, "user_email");

			entity.Property(e => e.Id)
				.HasDefaultValueSql("(newsequentialid())")
				.HasColumnName("id");
			entity.Property(e => e.UserEmail)
				.HasMaxLength(60)
				.IsUnicode(false)
				.HasColumnName("user_email");
			entity.Property(e => e.VisitDate)
				.HasDefaultValueSql("(getdate())")
				.HasColumnType("datetime")
				.HasColumnName("visit_date");

			entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.Visits)
				.HasPrincipalKey(p => p.Email)
				.HasForeignKey(d => d.UserEmail)
				.HasConstraintName("FK_visits_userEmail");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
