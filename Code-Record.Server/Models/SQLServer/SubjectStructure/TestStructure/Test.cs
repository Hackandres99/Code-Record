namespace Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

public partial class Test : Base.SubjectStructure.TestStructure.Test
{
	public Guid Id { get; set; }

	public Guid? IdSubject { get; set; }

	public virtual Subject? IdSubjectNavigation { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
