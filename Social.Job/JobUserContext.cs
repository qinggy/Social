﻿using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Job
{
    public class JobUserContext : IUserContext
    {
        public int UserId => 0;

        public int? SiteId => null;
    }
}
