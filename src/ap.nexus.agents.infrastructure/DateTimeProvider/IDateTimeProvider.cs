﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.infrastructure.DateTimeProvider
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
