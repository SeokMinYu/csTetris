using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_ysm
{
    class DiagramBlock
    {
        internal int X // x 좌표
        {
            get;
            private set;

        }
        internal int Y // y 좌표
        {
            get;
            private set;

        }
        internal int Turn //회전에 대해서 멤버로 정의
        {
            get;
            private set;
        }
        internal int BlockNum
        {
            get;
            private set;
        }
        internal DiagramBlock()
        {
            Reset();
        }
        internal void Reset() //시작좌표 지정
        {
            Random random = new Random();
            X = GameRule.S_X;
            Y = GameRule.S_Y;
            Turn = random.Next() % 4; //4개중에 하나를 랜덤
            BlockNum = random.Next() % 7; //나중에 지정 현재는 하나만 정의
        }
        internal void MoveLeft() //시작좌표 지정
        {
            X--;
        }
        internal void MoveRight() //시작좌표 지정
        {
            X++;
        }
        internal void MoveDown() //시작좌표 지정
        {
            Y++;
        }
        internal void MoveTurn() //회전 지정
        {
            Turn = (Turn + 1) % 4;
        }
    }
}
