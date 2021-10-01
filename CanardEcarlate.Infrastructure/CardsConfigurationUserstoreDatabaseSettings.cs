﻿namespace CanardEcarlate.Infrastructure
{
    public class CardsConfigurationUserStoreDatabaseSettings : ICardsConfigurationUserStoreDatabaseSettings
    {
        public string CardsConfigurationUsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ICardsConfigurationUserStoreDatabaseSettings
    {
        string CardsConfigurationUsersCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}