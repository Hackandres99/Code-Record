namespace Code_Record.Server.Models.Base;

public partial class Visit
{
	public string UserEmail { get; set; } = null!;

    public DateTime VisitDate { get; set; }
}
