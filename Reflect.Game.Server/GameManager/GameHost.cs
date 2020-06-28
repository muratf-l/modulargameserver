using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Reflect.GameServer.CodeManager;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Data.Models.Helpers;
using Reflect.GameServer.Data.Models.Services;
using Reflect.GameServer.Library;
using Reflect.GameServer.Library.Helpers;
using Reflect.GameServer.Library.Interfaces;
using Reflect.GameServer.Library.Logging;
using Reflect.GameServer.Library.Messages;
using Reflect.GameServer.Library.SocketLibrary;

namespace Reflect.GameServer.GameManager
{
    public class GameHost : BaseGameHost
    {
        private readonly ConcurrentDictionary<string, BaseGame>
            _runningGames = new ConcurrentDictionary<string, BaseGame>();

        private readonly TimeSpan _updateTimerTimeOut = new TimeSpan(0, 0, 0, 30);

        private IGameServer _gameServer;

        private Type _gameType;

        private Timer _updateTimer;

        private IUserService _userService;

        public bool IsRunning { get; private set; }

        #region "Game"

        private void GameFindOrCreate(BasePlayer player, Message message)
        {
            BaseGame game = null;

            foreach (var gameValue in _runningGames.Values)
                if (!gameValue.IsFull() && !gameValue.IsPlayerInGame(player.User.Id))
                {
                    game = gameValue;
                    break;
                }

            if (game == null)
            {
                var playerCount = message.Data["player"].ToObject<int>();

                game = GameCreate(playerCount);
            }

            var gamePlayer = game.CreatePlayer(new CreatePlayerArgs(player));

            if (!game.AllowUserJoin(gamePlayer))
            {
                player.Send(new MessagePlayer
                {
                    Action = MessageAction.Error,
                    Body = new MessagePlayerBody { Code = HttpStatusCode.NotAcceptable }
                });

                return;
            }

            game.AddPlayer(gamePlayer);

            player.GameId = game.GameId;

            if (game.IsFull())
                game.GameStarted();
        }

        private void GameLeft(BasePlayer player, IMessage message)
        {
            if (string.IsNullOrEmpty(player.GameId))
                return;

            var game = GameGet(player.GameId);

            game?.RemovePlayer(player);

            player.GameId = string.Empty;

            player.Send(new MessagePlayer
            {
                Action = MessageAction.GameLeave,
                Body = new MessagePlayerBody { Code = HttpStatusCode.OK, Data = game?.GameId }
            });
        }

        private void GameData(BasePlayer player, MessageGame message)
        {
            if (string.IsNullOrEmpty(player.GameId))
                return;

            var game = GameGet(player.GameId);

            game?.GotMessage(player, message);
        }

        private BaseGame GameGet(string extendedGameId)
        {
            _runningGames.TryGetValue(extendedGameId, out var game);

            return game;
        }

        private BaseGame GameCreate(int capacity)
        {
            BaseGame game = null;

            try
            {
                var gameId = Utils.GetGuidId();

                game = ExternalCodeManager.Default.RunExternalCode(gameId,
                    () => (BaseGame)Activator.CreateInstance(_gameType), TimeSpan.FromSeconds(20.0),
                    out _);

                game.AttemptSetup(this, gameId, capacity);
                game.GameInit();

                _runningGames.TryAdd(gameId, game);
            }

            catch (Exception exception)
            {
                LogService.WriteDebug(exception.Message);
            }

            return game;
        }

        #endregion

        #region "Class Init"

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Setup(Type game, int port, ConnectionMode mode)
        {
            _gameType = game;

            _gameServer = new GameServer(IPAddress.Any, port) { Host = this };

            _userService = Program.ServiceProvider.GetService<IUserService>();
        }

        public void Start()
        {
            _gameServer.TryStart();

            IsRunning = true;

            _updateTimer = new Timer(UpdateScheduleLoop, null, (int)_updateTimerTimeOut.TotalMilliseconds,
                Timeout.Infinite);
        }

        public void Stop()
        {
            _gameServer.TryStop();

            IsRunning = false;
        }

        #endregion

        #region "User Login Or Register"

        private async void MailRegister(BasePlayer player, Message message)
        {
            var token = new Ref<UserInfo>();

            var result = await _userService.AddUserFromEmail(message.Data, token);

            if (result != HttpStatusCode.OK)
            {
                player.Send(new MessagePlayer
                {
                    Action = MessageAction.Error,
                    Body = new MessagePlayerBody { Code = result }
                });

                return;
            }

            player.User = token.Value;

            player.Send(new MessagePlayer
            {
                Action = MessageAction.UserInfo,
                Body = new MessagePlayerBody { Code = HttpStatusCode.OK, Data = player.User }
            });
        }

        private async void MailLogin(BasePlayer player, Message message)
        {
            var token = new Ref<UserInfo>();

            var result = await _userService.LoginFromEmail(message.Data, token);

            if (result != HttpStatusCode.OK)
            {
                player.Send(new MessagePlayer
                {
                    Action = MessageAction.Error,
                    Body = new MessagePlayerBody { Code = result }
                });
                return;
            }

            player.User = token.Value;

            await _userService.ChangeUserStatus(player.User.Id, UserOnlineStatus.Online);

            player.Send(new MessagePlayer
            {
                Action = MessageAction.UserInfo,
                Body = new MessagePlayerBody { Code = HttpStatusCode.OK, Data = player.User }
            });
        }

        private async void FacebookLogin(BasePlayer player, Message message)
        {
            var token = new Ref<UserInfo>();

            var result = await _userService.AddUserFromFacebook(message.Data, token);

            if (result != HttpStatusCode.OK)
            {
                player.Send(new MessagePlayer
                {
                    Action = MessageAction.Error,
                    Body = new MessagePlayerBody { Code = result }
                });
                return;
            }

            player.User = token.Value;

            await _userService.ChangeUserStatus(player.User.Id, UserOnlineStatus.Online);

            player.Send(new MessagePlayer
            {
                Action = MessageAction.UserInfo,
                Body = new MessagePlayerBody { Code = HttpStatusCode.OK, Data = player.User }
            });
        }

        #endregion

        #region "Events"

        private void UpdateScheduleLoop(object state)
        {
            var offRoom = 0;

            foreach (var gameValue in _runningGames.Values)
                if (gameValue.IsEmpty())
                {
                    offRoom++;

                    _runningGames.TryRemove(gameValue.GameId, out var temp);

                    temp.GameClosed();
                }

            LogService.WriteDebug($"UpdateScheduleLoop Check online game:{_runningGames.Count} closed game:{offRoom}");

            _updateTimer.Change((int)_updateTimerTimeOut.TotalMilliseconds, Timeout.Infinite);
        }

        public void RunEvent(GameEvent evt, BasePlayer player, IMessage message, Action action)
        {
            try
            {
                ExternalCodeManager.Default.RunExternalCode(player.GameId,
                    () => runEvent_(evt, player, message, action), null, out _);
            }
            catch (Exception e)
            {
                LogService.WriteDebug(e.Message);
            }
        }

        private int runEvent_(GameEvent evt, BasePlayer player, IMessage message, Action action)
        {
            switch (evt)
            {
                case GameEvent.SocketConnected:
                    player.Send(new MessagePlayer
                    {
                        Action = MessageAction.ConnectionOK
                    });
                    break;

                case GameEvent.SocketDisconnected:
                    if (player.User != null)
                        _userService.ChangeUserStatus(player.User.Id, UserOnlineStatus.Offline);

                    GameLeft(player, message);
                    break;

                case GameEvent.UserRegister:
                    if (message.TryCast<Message>(out var userRegisterMessage))
                    {

                        if (userRegisterMessage.Action == MessageAction.MailRegister)
                            MailRegister(player, userRegisterMessage);

                        else if (userRegisterMessage.Action == MessageAction.FacebookRegisterOrLogin)
                            FacebookLogin(player, userRegisterMessage);
                    }

                    break;

                case GameEvent.UserLogin:

                    if (message.TryCast<Message>(out var userLoginMessage))
                        MailLogin(player, userLoginMessage);

                    break;

                case GameEvent.GotMessage:
                    if (player.User == null)
                    {
                        player.Disconnect();
                        return 0;
                    }

                    break;

                case GameEvent.GameJoin:
                    if (player.User == null)
                    {
                        player.Disconnect();
                        return 0;
                    }

                    if (message.TryCast<Message>(out var gameJoinMessage))
                        GameFindOrCreate(player, gameJoinMessage);

                    break;

                case GameEvent.GameLeft:
                    if (player.User == null)
                    {
                        player.Disconnect();
                        return 0;
                    }

                    GameLeft(player, message);
                    break;

                case GameEvent.GameData:
                    if (player.User == null)
                    {
                        player.Disconnect();
                        return 0;
                    }

                    if (message.TryCast<MessageGame>(out var gameDataMessage))
                        if(!string.IsNullOrEmpty(gameDataMessage.GameId))
                            GameData(player, gameDataMessage);

                    break;
            }

            return 0;
        }

        #endregion
    }
}