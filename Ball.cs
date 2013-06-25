using System;
using System.Drawing;

namespace Elliot_Arkanoid
{
    //Направление движения шара
    enum MoveDirection { LeftUp, RightUp, RightDown, LeftDown };

    internal class Ball : Block
    {
        Boolean isMoving = false;
        MoveDirection ballMoveDirection = MoveDirection.RightUp;
        public Boolean IsMoving
        {
            get { return isMoving; }
            set { isMoving = value; }
        }
        public MoveDirection BallMoveDirection
        {
            get { return ballMoveDirection; }
            set { ballMoveDirection = value; }
        }
        public Ball(UInt16 ballRadius, Color ballColor, Point ballPosition):
            base(new Size(ballRadius * 2, ballRadius * 2), ballColor, ballPosition) { }
    }
}
