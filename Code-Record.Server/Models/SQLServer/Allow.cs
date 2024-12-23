using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer;

public partial class Allow: Base.Allow
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ResourceId { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
