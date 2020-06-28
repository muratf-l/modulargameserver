using System;
using System.Collections.Generic;
using Reflect.GameServer.Library.Interfaces;
using Reflect.GameServer.Library.Messages;

namespace Reflect.GameServer.Library
{
    public abstract class BaseGame : MarshalByRefObject
    {
        public Action<BasePlayer> AddPlayer;

        public Func<BasePlayer, bool> AllowUserJoin;

        public Func<CreatePlayerArgs, BasePlayer> CreatePlayer;

        public Action DisconnectEverybody;

        public Func<List<BasePlayer>> GetPlayers;

        public Func<BasePlayer, int> RemovePlayer; 

        public Action<BasePlayer, MessageGame> GotMessage;

        public abstract int PlayerCount { get; }

        public abstract string GameId { get; }

        public abstract int GameCapacity { get; }

        public abstract void AttemptSetup(BaseGameHost host, string gameGuid, int capacity);

        public abstract void DoDisconnect(BasePlayer player);

        public abstract bool IsEmpty();

        public abstract bool IsFull();

        public abstract bool IsPlayerInGame(string playerId);

        public virtual void GameClosed()
        {
        }

        public virtual void GameStarted()
        {
        }

        public virtual void GameInit()
        {
        }


    }
}