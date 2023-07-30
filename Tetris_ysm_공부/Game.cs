using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_ysm
{
    class Game
    {
        DiagramBlock now;

        //게임 보드 정의
        Board gboard = Board.GameBoard;

        internal Point NowPosition //현재위치
        {
            get
            {
                if (now == null)
                {
                    return new Point(0, 0);
                }
                return new Point(now.X, now.Y); // DiagramBlock의 X 와 Y 반환
            }
        }
        internal int BlockNum
        {
            get
            {
                return now.BlockNum;
            }
        }
        internal int Turn
        {
            get
            {
                return now.Turn;
            }
        }
        #region 
        // 단일체 표현, 분기점 생성
        internal static Game Singleton
        {
            get;
            private set;
        }
        //보드 공간에 x, y 값에 어떤 내용이 있는지 알기 위한 방법 
        internal int this[int x, int y]
        {
            get
            {
                return gboard[x, y];
            }
        }
        static Game()
        {
            Singleton = new Game();
        }
        // 게임을 시작하면서 도형을 만들고 출발
        Game()
        {
            now = new DiagramBlock();
        }
        #endregion
        internal bool MoveLeft()
        {
            // 4*4 도형이 움직일 수 있는지 없는지 판단
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    // 현재블록번호, 회전정보, x, y
                    if (BlockValue.b_vals[now.BlockNum, Turn, xx, yy] != 0) // 0이 아닌 벽돌일때
                    {
                        if (now.X + xx <= 0) // 벽돌이 있는데 0보다 작을때
                        {
                            return false;
                        }
                    }
                }
            }
            // 해당 보드공간에 움직일 수 있는지 없는지 판단
            if(gboard.MoveEnable(now.BlockNum, Turn, now.X - 1, now.Y))
            {
                now.MoveLeft();
                return true;
            }
            return false;
        }
        internal bool MoveRight()
        {
            // 4*4 도형이 움직일 수 있는지 없는지 판단
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    // 현재블록번호, 회전정보, x, y
                    if (BlockValue.b_vals[now.BlockNum, Turn, xx, yy] != 0) // 0이 아닌 벽돌일때
                    {
                        if (now.X + xx + 1 >= GameRule.B_X) // 11칸까지 판단, 인덱스가 0부터 시작이라서 1추가
                        {
                            return false;
                        }
                    }
                }
            }
            // 해당 보드공간에 움직일 수 있는지 없는지 판단
            if (gboard.MoveEnable(now.BlockNum, Turn, now.X + 1, now.Y))
            {
                now.MoveRight();
                return true;
            }
            return false;      
        }
        internal bool MoveDown()
        {
            // 4*4 도형이 움직일 수 있는지 없는지 판단
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    // 현재블록번호, 회전정보, x, y
                    if (BlockValue.b_vals[now.BlockNum, Turn, xx, yy] != 0) // 0이 아닌 벽돌일때
                    {
                        if (now.Y + yy + 1 >= GameRule.B_Y) // 11칸까지 판단, 인덱스가 0부터 시작이라서 1추가
                        {
                            gboard.Store(now.BlockNum, Turn, now.X, now.Y);
                            return false;
                        }
                    }
                }
            }
            // 해당 보드공간에 움직일 수 있는지 없는지 판단
            if (gboard.MoveEnable(now.BlockNum, Turn, now.X, now.Y + 1))
            {
                now.MoveDown();
                return true;
            }
            // 움직일 수 없으면 쌓기
            gboard.Store(now.BlockNum, Turn, now.X, now.Y);
            return false;

        }
        internal bool MoveTurn()
        {
            // 4*4 도형이 움직일 수 있는지 없는지 판단
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    // 현재블록번호, 회전정보, x, y
                    if (BlockValue.b_vals[now.BlockNum, (Turn + 1) % 4, xx, yy] != 0) // 0이 아닌 벽돌일때 다음회전에 대해서
                    {
                        // 현재 X축이 0보다 작지않고 X값과 Y 값보다 같거나 크면 false
                        if ((now.X + xx) < 0 || (now.X + xx) >= GameRule.B_X || (now.Y + yy) >= GameRule.B_Y)
                        {
                            return false;
                        }
                    }
                }
            }
            // 해당 보드공간에 움직일 수 있는지 없는지 판단
            if (gboard.MoveEnable(now.BlockNum, (Turn+1)%4, now.X, now.Y))
            {
                now.MoveTurn();
                return true;
            }
            return false;
        }
        // 벽돌이 밑에까지 오면 다음벽돌 선택
        internal bool Next()
        {
            now.Reset();
            return gboard.MoveEnable(now.BlockNum, Turn, now.X, now.Y);
        }

        internal void Restart()
        {
            gboard.ClearBoard();
        }
    }
}
