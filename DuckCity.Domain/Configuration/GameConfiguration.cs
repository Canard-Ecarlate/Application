﻿namespace DuckCity.Domain.Configuration
{
    public class GameConfiguration
    {
        public bool IsPrivate { get; set; }
        public string? Code { get; set; }
        public CardsConfiguration? CardsConfiguration { get; set; }
        public NbPlayersConfiguration? NbPlayersConfiguration { get; set; }
        public bool IsPlaying { get; set; }
    }
}
