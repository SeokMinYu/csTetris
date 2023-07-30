using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris_ysm
{
    // 벽돌을 움직일 수 있는 영역
    class Board
    {
        internal static Board GameBoard //정적인 단일체객체
        {
            get;
            private set;
        }
        static Board()
        {
            GameBoard = new Board();
        }

        internal int cnt // 카운트용
        {
            get;
            private set;

        }

        // x,y 로된 2차원 공간
        int[,] board = new int[GameRule.B_X, GameRule.B_Y];
        // x,y 가 비어있는지 아닌지 접근가능하게 함
        internal int this[int x, int y]
        {
            get
            {
                return board[x, y];
            }
        }
        // 벽돌이 쌓여있을때 벽돌이 움직일수 있는지 판단
        //b_n : 블록넘버 , t_n : 회전넘버
        internal bool MoveEnable(int b_n, int t_n, int x, int y)
        {
            // 4 * 4의 공간을 확인
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    if (BlockValue.b_vals[b_n, t_n, xx, yy] != 0)
                    {
                        // 양쪽에 벽돌이 있는지 
                        if (board[x + xx, y + yy] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        // 벽돌을 다 내리고 쌓을 수 있는 기능
        internal void Store(int b_n, int t_n, int x, int y)
        {
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    if ((x + xx) >= 0 && (x + xx) < GameRule.B_X && (y + yy) >= 0 && (y + yy) < GameRule.B_Y)
                    {
                        //보드의 값과 블록밸류의 값을 합한다. => 블록을 쌓는 효과
                        board[x + xx, y + yy] += BlockValue.b_vals[b_n, t_n, xx, yy];
                    }
                }
            }
            CheckLines(y + 3); //  벽돌꽉찬라인 밑에서부터제거
        }

        //  벽돌꽉찬라인제거
        private void CheckLines(int y)
        {
            int yy = 0;
            cnt = 0;
            for (yy = 0; yy < 4; yy++)
            {
                //밑에서 부터 확인 
                if (y - yy < GameRule.B_Y)
                {
                    if (CheckLine(y - yy))
                    {
                        cnt++;
                        ClearLine(y - yy);
                        y++; //같은라인확인
                    }
                }
            } 
        }
        //한라인이 전부 꽉 차있는지
        private bool CheckLine(int y)
        {
            for (int xx = 0; xx < GameRule.B_X; xx++)
            {
                if(board[xx, y] == 0) //꽉차있지 않음
                {
                    return false;
                }
            }
            return true;
        }

        // 게임이 끝나면 보드의 블록들을 지워준다
        internal void ClearBoard()
        {
            for(int xx = 0; xx < GameRule.B_X; xx++)
            {
                for (int yy = 0; yy < GameRule.B_Y; yy++)
                {
                    board[xx, yy] = 0;
                }
            }
        }

        // 라인지우기 지워지면 
        private void ClearLine(int y)
        {
            // 밑에서부터 올라갈수 있게
            for(; y > 0; y--)
            {
                // 윗줄의 x칸을 밑에줄에 복사, 그래야 내려온것처럼 보임
                 for(int xx = 0; xx < GameRule.B_X; xx++)
                {
                    //위의 블록들이 한칸씩 내려옴
                    board[xx, y] = board[xx, y - 1];
                }
            }
        }
    }
}
