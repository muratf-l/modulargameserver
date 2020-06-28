//using System;
//using System.Linq;
//using System.Threading;
//using Reflect.Game.Ludo.Engine.BoardTemplates;
//using Reflect.Game.Ludo.Engine.Logger;

//namespace Reflect.Game.Ludo.Engine
//{
//    public class Board
//    {
//        private readonly int _numOfActivePlayers;

//        private readonly TimeSpan _timeout = new TimeSpan(0, 0, 0, 15);

//        public readonly BoardTemplate BoardTemplate;

//        private int _currentPlayerTurnNo = -1;

//        private Timer _timer;

//        public GameFinished BoardOnGameFinished;

//        public BoardPlayerMoving BoardOnMoving;

//        public BoardPlayerMovingList BoardOnMovingList;

//        public BoardPieceFinished BoardOnPieceFinished;

//        public BoardTurnChange BoardOnTurnChanged;

//        public int BotAiCount = 0;

//        public int[] DiceRolls;

//        public Player[] Players;

//        public Square[] Squares;

//        public Board() : this(4)
//        {
//        }

//        public Board(int numOfPlayers)
//        {
//            _numOfActivePlayers = numOfPlayers;

//            BoardTemplate = new BoardTemplate1();
//        }

//        public int CurrentTurnPlayerNo
//        {
//            get => _currentPlayerTurnNo;

//            private set
//            {
//                _currentPlayerTurnNo = value;

//                TurnPlay();

//                LogService.WriteDebug(this,
//                    $"New Turn player:{_currentPlayerTurnNo} dice:{string.Join(",", DiceRolls)}");

//                Players[_currentPlayerTurnNo].Play(DiceRolls);

//                BoardOnTurnChanged?.Invoke(_currentPlayerTurnNo, DiceRolls);

//                if (_timer == null)
//                    _timer = new Timer(TurnTimeout, null, (int) _timeout.TotalMilliseconds, Timeout.Infinite);

//                _timer.Change((int) _timeout.TotalMilliseconds, Timeout.Infinite);
//            }
//        }

//        public void Init()
//        {
//            GeneratePlayers();

//            GenerateSquares();
//        }

//        public void Start()
//        {
//            CurrentTurnPlayerNo = Dice.RollBy(0, _numOfActivePlayers);
//        }

//        #region "Moving"

//        private void OnPlayerMoving(Move move)
//        {
//            Move(move);
//        }

//        public void Move(Move move)
//        {
//            if (move.PlayerNo != CurrentTurnPlayerNo)
//                return;

//            var player = Players[CurrentTurnPlayerNo];

//            var piece = player.Pieces.First(x => x.Index == move.PieceIndex);

//            if (move.TurnChecker)
//            {
//                var isInBoard = player.Pieces.Where(x => x.IsInBoard).ToList();

//                ////henuz tahtaya inmemis ve zar 6 gelmemis
//                if (!isInBoard.Any() && move.Step != 6)
//                {
//                    TurnNext();
//                    return;
//                }
//            }

//            ////henuz tahtaya inmemis ve zar 6 gelmis
//            if (!piece.IsInBoard && move.Step == 6)
//            {
//                piece.MoveInBoard();
//                move.Step = 0;
//            }

//            if (piece.PositionBefore > -1)
//            {
//                var squareOld = Squares[piece.PositionBefore];

//                squareOld.RemovePiece(piece);
//            }

//            piece.StepAdd(move.Step);

//            var square = Squares[piece.Position];

//            square.AddPiece(piece);

//            LogService.WriteDebug(this,
//                $"Moving step:{move.Step} {piece}");

//            if (piece.IsFinished)
//                BoardOnPieceFinished?.Invoke(piece);

//            BoardOnMoving?.Invoke(piece);

//            // carpisma olursa turn degismez
//            if (move.TurnChecker)
//            {
//                if (CollisionDetect(square, piece))
//                {
//                    if (GameFinishedDetect() == false) TurnRepeat();
//                }
//                else
//                {
//                    if (GameFinishedDetect() == false) TurnNext();
//                }
//            }

//            //PrintBoard();
//        }

//        #endregion

//        #region "Game Controls"

//        private bool CollisionDetect(Square square, Piece piece)
//        {
//            if (piece.IsLapCompleted || piece.IsFinished) return false;

//            if (square.IsProtected) return false;

//            var colliderList = square.Pieces.Where(x => x.PlayerNo != piece.PlayerNo).ToList();

//            if (colliderList.Any())
//            {
//                foreach (var cooliderPiece in colliderList)
//                {
//                    cooliderPiece.MoveOutBoard();
//                    square.Pieces.Remove(cooliderPiece);
//                }

//                BoardOnMovingList?.Invoke(colliderList);
//                return true;
//            }

//            return false;
//        }

//        private bool GameFinishedDetect()
//        {
//            foreach (var player in Players)
//            {
//                var isFinishCount = player.Pieces.Where(x => x.IsFinished).ToList().Count;

//                if (isFinishCount == BoardTemplate.PlayerFinishSquareCount)
//                {
//                    BoardOnGameFinished?.Invoke(player.PlayerNo);
//                    return true;
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region "Turns"

//        private void TurnPlay()
//        {
//            DiceRolls = new int[3];

//            DiceRolls[0] = Dice.Roll();

//            if (DiceRolls[0] == 6)
//                DiceRolls[1] = Dice.Roll();

//            repeat:
//            if (DiceRolls[1] == 6)
//                DiceRolls[2] = Dice.Roll();

//            if (DiceRolls[2] == 6) goto repeat;

//            DiceRolls = DiceRolls.Where(c => c > 0).ToArray();

//            //LogService.WriteDebug(this, $"player:{CurrentTurnPlayerNo} dice:{string.Join(",", DiceRolls)}");
//        }

//        private void TurnNext()
//        {
//            var turn = CurrentTurnPlayerNo + 1;

//            if (turn >= _numOfActivePlayers)
//                turn = 0;

//            CurrentTurnPlayerNo = turn;
//        }

//        private void TurnRepeat()
//        {
//            TurnPlay();

//            Players[CurrentTurnPlayerNo].Play(DiceRolls);

//            BoardOnTurnChanged?.Invoke(CurrentTurnPlayerNo, DiceRolls);
//        }

//        private void OnTurn()
//        {
//            TurnNext();
//        }

//        private void TurnTimeout(object state)
//        {
//            TurnNext();
//        }

//        #endregion

//        #region "Generator"

//        private void PrintBoard()
//        {
//            LogService.WriteDebug(this, "----------------------------------------------");

//            foreach (var player in Players)
//            {
//                LogService.WriteDebug(this, $"player:{player.PlayerNo} start:{player.PositionStart}");

//                foreach (var piece in player.Pieces)
//                    LogService.WriteDebug(this,
//                        $"p:{player.PlayerNo} in:{piece.IsInBoard} index:{piece.Index} pos:{piece.Position}  pos old:{piece.PositionBefore}");
//            }

//            LogService.WriteDebug(this, "----------------------------------------------");
//        }

//        private void GenerateSquares()
//        {
//            Squares = new Square[BoardTemplate.BoardSquareCount];

//            for (var side = 0; side < BoardTemplate.BoardSquareCount; side++) Squares[side] = new Square(side);

//            foreach (var starSquare in BoardTemplate.StarSquare) Squares[starSquare] = new SquareStar(starSquare);

//            for (var playerId = 0; playerId < BoardTemplate.PlayerSquare.Length; playerId++)
//            {
//                var positionId = BoardTemplate.PlayerSquare[playerId];

//                Squares[positionId] = new SquarePlayer(positionId) {PlayerNo = playerId};
//            }
//        }

//        private void GeneratePlayers()
//        {
//            Players = new Player[_numOfActivePlayers];

//            var playerCount = _numOfActivePlayers;

//            if (BotAiCount > 0)
//                playerCount -= BotAiCount;

//            for (var playerNo = 0; playerNo < playerCount; playerNo++)
//                Players[playerNo] = CreatePlayer(playerNo, false);

//            if (BotAiCount > 0)
//                for (var i = playerCount; i < playerCount + BotAiCount; i++)
//                    Players[i] = CreatePlayer(i, true);
//        }

//        private Player CreatePlayer(int playerNo, bool isBot)
//        {
//            var positionId = BoardTemplate.PlayerSquare[playerNo];

//            Player player = null;

//            if (isBot)
//                player = new PlayerAi
//                {
//                    PlayerNo = playerNo,
//                    PositionStart = positionId,
//                    Pieces = new Piece[BoardTemplate.PlayerPiecesPerPlayer],
//                    OnMoving = OnPlayerMoving,
//                    OnTurn = OnTurn
//                };
//            else
//                player = new Player
//                {
//                    PlayerNo = playerNo,
//                    PositionStart = positionId,
//                    Pieces = new Piece[BoardTemplate.PlayerPiecesPerPlayer],
//                    OnMoving = OnPlayerMoving,
//                    OnTurn = OnTurn
//                };

//            for (var pIndex = 0; pIndex < BoardTemplate.PlayerPiecesPerPlayer; pIndex++)
//                player.Pieces[pIndex] = new Piece(playerNo, pIndex, positionId,
//                    BoardTemplate.PlayerFinishSquareCount,
//                    BoardTemplate.BoardSquareCount);

//            return player;
//        }

//        #endregion
//    }
//}