namespace Reflect.Game.Ludo.Engine.Logic
{
    public class Piece
    {
        private int _lap;

        public Piece(int playerNo, int index, int playerStartSquare, int playerFinishSquareCount)
        {
            PlayerNo = playerNo;

            PlayerStartSquare = playerStartSquare;

            PlayerFinishSquare = playerFinishSquareCount;

            Index = index;

            IsInBoard = false;

            IsLapCompleted = false;

            Position = -1;
        }

        public int PlayerNo { get; protected set; }

        public int PlayerStartSquare { get; protected set; }

        public int PlayerFinishSquare { get; protected set; }

        public int Index { get; protected set; }

        public int Position { get; protected set; }

        public int PositionBefore { get; protected set; }

        public bool IsInBoard { get; protected set; }

        public bool IsLapCompleted { get; protected set; }

        public bool IsFinished { get; protected set; }

        public void StepAdd(int step)
        {
            //if (IsFinished) return;

            //PositionBefore = Position;

            //Position += step;

            //if (!IsLapCompleted && Position >= BoardTotalSquare)
            //{
            //    _lap++;
            //    Position -= BoardTotalSquare;
            //}

            //if (!IsLapCompleted && _lap > 0 && Position >= PlayerStartSquare)
            //{
            //    _lap = 0;
            //    IsLapCompleted = true;
            //    Position -= PlayerStartSquare;
            //    return;
            //}

            //if (IsLapCompleted && !IsFinished)
            //{
            //    if (Position == PlayerFinishSquare)
            //    {
            //        IsFinished = true;
            //        Position = PlayerFinishSquare;
            //    }
            //    else if (Position > PlayerFinishSquare)
            //    {
            //        Position -= step;
            //    }
            //}
        }

        public void MoveInBoard()
        {
            if (IsLapCompleted || IsFinished) return;

            PositionBefore = -1;

            Position = PlayerStartSquare;

            IsLapCompleted = false;

            IsFinished = false;

            IsInBoard = true;

            _lap = 0;
        }

        public void MoveOutBoard()
        {
            if (IsLapCompleted || IsFinished) return;

            PositionBefore = Position;

            Position = -1;

            IsLapCompleted = false;

            IsFinished = false;

            IsInBoard = false;

            _lap = 0;
        }

        public override string ToString()
        {
            return $@"player:{PlayerNo} index:{Index} position:{Position} old position:{PositionBefore}";
        }
    }
}