﻿using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure.ThemeStructure;

public partial class Theme : CommonCreation
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;
}
