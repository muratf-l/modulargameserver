using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Library.Interfaces;
using Reflect.GameServer.Library.Messages;

namespace Reflect.GameServer.Library
{
    [Serializable]
    public abstract class Game<TP> : BaseGame where TP : BasePlayer, new()
    {
        private int _gameCapacity;

        private string _gameGuid;

        private BaseGameHost _host;

        private int _playerInstanceId;

        private ConcurrentDictionary<string, TP> _players = new ConcurrentDictionary<string, TP>();

        protected Game()
        {
            _playerInstanceId = 0;
        }

        public override string GameId =>
            _gameGuid;

        public override int PlayerCount =>
            _players.Count;

        public override int GameCapacity =>
            _gameCapacity;

        public override bool IsEmpty()
        {
            var count = _players.Values.Count(x => x.Status == PlayerStatus.Offline);

            return count >= _players.Count;
        }

        public override bool IsFull()
        {
            return _players.Count >= _gameCapacity;
        }

        public override bool IsPlayerInGame(string playerId)
        {
            return _players.ContainsKey(playerId);
        }

        public TP GetPlayer(string userId)
        {
            _players.TryGetValue(userId, out var pl);

            return pl;
        }

        public TP GetPlayer(int index)
        {
            if (index > _players.Count)
                return null;

            var pl = _players.Values.ElementAt(index);

            return pl;
        }

        public override void AttemptSetup(BaseGameHost host, string gameGuid, int capacity)
        {
            if (ReferenceEquals(_host, null))
            {
                _gameGuid = gameGuid;

                _host = host;

                _gameCapacity = capacity;

                AddPlayer = delegate (BasePlayer p)
                {
                    if (IsPlayerInGame(p.User.Id)) return;

                    var temp = (TP)p;
                    temp.Status = PlayerStatus.Online;

                    if (_players.TryAdd(p.User.Id, temp))
                        PlayerJoined(temp);
                };

                RemovePlayer = delegate (BasePlayer p)
                {
                    if (_players.TryGetValue(p.User.Id, out var temp))
                    {
                        temp.Status = PlayerStatus.Offline;
                        PlayerLeft(temp);
                    }

                    return _players.Count;
                };

                DisconnectEverybody = delegate
                {
                    foreach (var local in _players.Values) local.Disconnect();
                };

                GetPlayers = delegate
                {
                    var list = new List<BasePlayer>();

                    foreach (var local in _players.Values) list.Add(local);

                    return list;
                };

                GotMessage = delegate (BasePlayer player, MessageGame message)
                {
                    if (_players.TryGetValue(player.User.Id, out var temp))
                        PlayerMessage(temp, message);
                };

                AllowUserJoin = player => PlayerAllowed((TP)player);

                CreatePlayer = delegate (CreatePlayerArgs args)
                {
                    var local = Activator.CreateInstance<TP>();
                    local.Game = this;
                    local.Id = Interlocked.Increment(ref _playerInstanceId);
                    local.Connection = args.Connection;
                    local.GameId = GameId;
                    local.User = args.User;
                    local.Status = PlayerStatus.Online;
                    return local;
                };
            }
        }

        public void Broadcast(IMessage message)
        {
            foreach (var local in _players.Values)
            {
                if (local.Status == PlayerStatus.Online)
                    local.Connection.Send(message);
            }
        }

        public virtual void PlayerJoined(TP player)
        {
        }

        public virtual void PlayerLeft(TP player)
        {
        }

        public virtual void PlayerMessage(TP player, MessageGame message)
        {
        }

        public virtual bool PlayerAllowed(TP player)
        {
            return true;
        }

        public virtual Timer AddTimer(Action callback, int interval)
        {
            throw new NotImplementedException();
        }

        public override void DoDisconnect(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        public virtual Timer ScheduleCallback(Action callback, int dueTime)
        {
            throw new NotImplementedException();
        }
    }
}