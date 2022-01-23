﻿using DuckCity.Domain.Cards;

namespace DuckCity.Domain.Rooms
{
    public class RoomConfiguration
    {
        public bool IsPrivate { get; set; }
        public  int NbPlayers { get; set; }
        public  List<NbEachCard> Cards { get; set; }

        public RoomConfiguration(bool isPrivate, int nbPlayers)
        {
            IsPrivate = isPrivate;
            NbPlayers = nbPlayers;
            Cards = new List<NbEachCard>
            {
                new()
                {
                    CardName = "Bomb",
                    Number = 1
                },
                new()
                {
                    CardName = "Green",
                    Number = NbPlayers
                },
                new()
                {
                    CardName = "Yellow",
                    Number = NbPlayers * 5 - (NbPlayers + 1)
                }
            };

        }
    }
}
