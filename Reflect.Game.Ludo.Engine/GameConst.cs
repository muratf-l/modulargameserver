using System;

namespace Reflect.Game.Ludo.Engine
{
    public class GameConst
    {
        public static int BoardSquareCount = 52; // board nokta sayisi - base 0

        public static int[] PlayerSquare = { 1, 14, 27, 40 }; // oyuncu baslangic noktalari

        public static int[] StarSquare = { 9, 22, 34, 47 }; // oyuncu koruma noktalari

        public static int MaxPlayersCount = 4; // max oyuncu sayisi

        public static int PlayerPiecesPerPlayer = 4; // oyuncu pul sayisi

        public static int PlayerFinishSquareCount = 5; // oyuncu bitis yuruyusu

        public static int PlayerTimeout = (int)new TimeSpan(0, 0, 0, 30).TotalMilliseconds;
    }
}