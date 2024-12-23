namespace Code_Record.Server.Models.SQLServer;
public partial class Visit : Base.Visit
{
	public Guid Id { get; set; }

	public virtual User UserEmailNavigation { get; set; } = null!;
}
