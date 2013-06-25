using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Elliot_Arkanoid
{
    internal class Field
    {
        Size size; //Размер игрового поля
        Form form; //Родительская форма
        BufferedGraphicsContext bufGraphicsCtx = BufferedGraphicsManager.Current;
        BufferedGraphics bufGraphics;
        Graphics graphics;
        List<Block> blocksList = new List<Block>(); //Блоки
        Caret strikingCaret;
        Ball strikingBall;
        Point topLeftBound, bottomRightBound; //Пределы игрового пространства
        Byte moveStep = 10, //Шаг движения шара
            caretStep = 70; //Шаг движения каретки
        Random randGen = new Random(); //Генератор псевдослучайных чисел
        Timer gameTimer = new Timer();

        public void RegisterBlocksAndBounds()
        {
            Byte blockWidth = 150, //Ширина блока
                blockHeight = 30, //Высота блока
                horizonalMargin = 5,
                verticalMargin = 5,
                blocksFillingPercentage = 20; //Процент пространства для блоков
            UInt16 horizontalAmout = Convert.ToUInt16(size.Width / (blockWidth + horizonalMargin)),
                verticalAmount = Convert.ToUInt16(
                    size.Height / (blockHeight + verticalMargin) * blocksFillingPercentage / 100
                ); //Количество блоков на поле
            Byte leftMarginForCentering = //Отступ для центрирования
                Convert.ToByte((size.Width - (blockWidth + horizonalMargin) * horizontalAmout) / 2 - horizonalMargin);
            for(UInt16 i = 0; i < horizontalAmout; i++)
                for(UInt16 j = 0; j < verticalAmount; j++)
                    blocksList.Add( //Добавление блока
                        new Block(new Size(blockWidth, blockHeight),
                            Color.FromArgb(randGen.Next(250), randGen.Next(250), randGen.Next(250)),
                            new Point(leftMarginForCentering + horizonalMargin + i * (blockWidth + horizonalMargin),
                                verticalMargin + j * (blockHeight + verticalMargin))));
            //Пределы пространства
            topLeftBound = new Point(leftMarginForCentering, verticalMargin);
            bottomRightBound = new Point(size.Width - leftMarginForCentering - horizonalMargin, size.Height - verticalMargin - 35);
        }

        public void RegisterCaretAndBall()
        {
            UInt16 caretWidth = 200,
                caretHeight = 5,
                ballRadius = 10;
            Point startingPoint = new Point((size.Width - caretWidth) / 2, bottomRightBound.Y - caretHeight );
            strikingCaret = new Caret(new Size(caretWidth, caretHeight), Color.Green, startingPoint);
            strikingBall = new Ball(ballRadius, Color.Black, new Point((size.Width - ballRadius * 2) / 2, startingPoint.Y - ballRadius * 2));
        }

        public Field(Size fieldSize, Form parentForm)
        {
            //Инициализация
            size = fieldSize;
            form = parentForm;
            //Буферизованная графика
            bufGraphicsCtx.MaximumBuffer = fieldSize;
            bufGraphics = bufGraphicsCtx.Allocate(parentForm.CreateGraphics(), parentForm.ClientRectangle);
            graphics = bufGraphics.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            parentForm.Paint += RedrawField; //Перерисовка мира
            parentForm.KeyDown += KeyPressed; //Событие KeyDown на движение каретки
            RegisterBlocksAndBounds(); //Блоки и пределы
            RegisterCaretAndBall(); //Шар и каретка
            strikingBall.IsMoving = true;
            gameTimer.Interval = 25;
            gameTimer.Tick += GameFrame;
            gameTimer.Start(); //Начало игры
        }

        public void KeyPressed(object sender, KeyEventArgs keyEvent)
        {
            switch (keyEvent.KeyCode)
            {
                case Keys.Left:
                    if (strikingCaret.BlockPosition.X - caretStep <= topLeftBound.X) break;
                    strikingCaret.BlockPosition = new Point(strikingCaret.BlockPosition.X - caretStep, strikingCaret.BlockPosition.Y);
                    form.Invalidate();
                break;
                case Keys.Right:
                if (strikingCaret.BlockPosition.X + strikingCaret.BlockSize.Width + caretStep >= bottomRightBound.X) break;
                strikingCaret.BlockPosition = new Point(strikingCaret.BlockPosition.X + caretStep, strikingCaret.BlockPosition.Y);
                form.Invalidate();
                break;
            }
        }

        public void CheckBallIntersection()
        {
            for(UInt16 i = 0; i < blocksList.Count; i++) //Проверка на столкновение
                if (strikingBall.BlockRectangle.IntersectsWith(blocksList[i].BlockRectangle))
                {
                    blocksList.RemoveAt(i); //Удаление блока
                    strikingBall.BallMoveDirection = //Случайное отражение шара
                        (randGen.Next(100) % 2 == 0) ? MoveDirection.LeftDown : MoveDirection.RightDown;
                    return;
                }
            //Проверка шара на столкновение с границей
            if (strikingBall.BlockPosition.X + strikingBall.BlockSize.Width >= bottomRightBound.X ||
                    strikingBall.BlockPosition.X <= topLeftBound.X)
            {
                switch (strikingBall.BallMoveDirection)
                {
                    case MoveDirection.RightUp: strikingBall.BallMoveDirection = MoveDirection.LeftUp; break;
                    case MoveDirection.LeftUp: strikingBall.BallMoveDirection = MoveDirection.RightUp; break;
                    case MoveDirection.LeftDown: strikingBall.BallMoveDirection = MoveDirection.RightDown; break;
                    case MoveDirection.RightDown: strikingBall.BallMoveDirection = MoveDirection.LeftDown; break;
                }
                return;
            }
            if (strikingBall.BlockPosition.Y <= topLeftBound.Y)
            {
                strikingBall.BallMoveDirection = 
                        (randGen.Next(100) % 2 == 0) ? MoveDirection.LeftDown : MoveDirection.RightDown;
            }
            //Кареткой
            if (strikingBall.BlockRectangle.IntersectsWith(strikingCaret.BlockRectangle))
            {
                //Отражения шара от каретки в зависимости от ударяемой стороны
                if(strikingBall.BlockPosition.X + strikingBall.BlockSize.Width / 2 >= 
                    strikingCaret.BlockPosition.X + strikingCaret.BlockSize.Width / 2)
                    strikingBall.BallMoveDirection = MoveDirection.RightUp;
                else strikingBall.BallMoveDirection = MoveDirection.LeftUp;
                return;
            }
            //И дном поля
            if (strikingBall.BlockPosition.Y - strikingBall.BlockSize.Height >= bottomRightBound.Y)
            {
                gameTimer.Stop();
                MessageBox.Show("You lose");
                Application.Exit();
            }
        }
        
        public void MoveBall()
        {
            if (!strikingBall.IsMoving) return;
            switch (strikingBall.BallMoveDirection)
            {
                case MoveDirection.RightUp:
                    strikingBall.BlockPosition =
                        new Point(strikingBall.BlockPosition.X + moveStep, strikingBall.BlockPosition.Y - moveStep);
                break;
                case MoveDirection.RightDown:
                strikingBall.BlockPosition =
                    new Point(strikingBall.BlockPosition.X + moveStep, strikingBall.BlockPosition.Y + moveStep);
                break;
                case MoveDirection.LeftUp:
                strikingBall.BlockPosition =
                    new Point(strikingBall.BlockPosition.X - moveStep, strikingBall.BlockPosition.Y - moveStep);
                break;
                case MoveDirection.LeftDown:
                strikingBall.BlockPosition =
                    new Point(strikingBall.BlockPosition.X - moveStep, strikingBall.BlockPosition.Y + moveStep);
                break;
            }
        }

        public void GameFrame(object sender, EventArgs e)
        {
            if(blocksList.Count == 0)
            {
                gameTimer.Stop();
                MessageBox.Show("You win");
                Application.Exit();
            }
            
            bufGraphics.Graphics.Clear(Color.White); //Очистка поля
            foreach (Block b in blocksList)
                DrawBlock(b, b.BlockColor); //Отображение блоков
            DrawBlock(strikingCaret, Color.Black); //Каретка
            graphics.DrawArc(Pens.Black, strikingBall.BlockRectangle, 0, 360); //Шар
            CheckBallIntersection(); //Проверка на столкновения
            MoveBall();
            form.Invalidate(); //Перерисовка формы
        }

        public void DrawBlock(Block block, Color color)
        {
            graphics.DrawRectangle(Pens.White, block.BlockRectangle);
            graphics.FillRectangle(new SolidBrush(color), block.BlockRectangle);
        }

        public void RedrawField(object sender, PaintEventArgs eventArgs)
        {
            bufGraphics.Render(eventArgs.Graphics);
        }
    }
}
