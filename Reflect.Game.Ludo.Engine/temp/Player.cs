//namespace Reflect.Game.Ludo.Engine
//{
//    public class Player
//    {
//        public PlayerMoving OnMoving;

//        public PlayerTurnFinish OnTurn;

//        public int PlayerNo { get; set; }

//        public int PositionStart { get; set; }

//        public Piece[] Pieces { get; set; }

//        public virtual void Play(int[] diceRolls)
//        {
//            //var isPlayed = false;

//            //var diceTemp = diceRolls.ToList();
//            //var pieceTemp = Pieces.ToList();

//            //foreach (var dice in diceTemp)
//            //{
//            //    var isInBoard = pieceTemp.Where(x => x.IsInBoard).ToList();

//            //    if (!isInBoard.Any() && dice == 6)
//            //    {
//            //        var piece = pieceTemp.First();

//            //        //piece.MoveInBoard();

//            //        OnMoving?.Invoke(new Move
//            //        {
//            //            PieceIndex = piece.Index,
//            //            PlayerNo = PlayerNo,
//            //            Step = dice,
//            //            TurnChecker = false
//            //        });

//            //        isPlayed = true;
//            //        continue;
//            //    }

//            //    if (isInBoard.Count == 1)
//            //    {
//            //        var piece = isInBoard.First();

//            //        OnMoving?.Invoke(new Move
//            //        {
//            //            PieceIndex = piece.Index,
//            //            PlayerNo = PlayerNo,
//            //            Step = dice,
//            //            TurnChecker = false
//            //        });

//            //        isPlayed = true;
//            //    }
//            //}

//            //if (isPlayed) OnTurn?.Invoke();
//        }
//    }
//}