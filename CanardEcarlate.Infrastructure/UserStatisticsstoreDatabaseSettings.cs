﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanardEcarlate.Infrastructure
{
    public class UserStatisticsstoreDatabaseSettings : IUserStatisticsstoreDatabaseSettings
    {
        public string UserStatisticsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IUserStatisticsstoreDatabaseSettings
    {
        string UserStatisticsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
