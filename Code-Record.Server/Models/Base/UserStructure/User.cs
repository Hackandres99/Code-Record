using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.UserStructure;

public partial class User : CommonCreation
{
    public string Email { get; set; } = null!;

    public string? Username { get; set; }

    public string AccountPass { get; set; } = null!;

    public RolOptions Rol { get; set; } = RolOptions.Common;

    public SubscriptionOptions Subscription { get; set; } = SubscriptionOptions.Free;
}
