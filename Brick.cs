using System.Drawing;

namespace Elliot_Arkanoid
{
    internal class Block
    {
        //Any block in game has
        Size size; //Width and height as elementary dimensions
        Color color; //Color as elementary property
        Point position; //Position which determines its place in world

        //Properties allow selective access to block's internals.
        //We can move block to certain position, but cannot change its size or color.
        public Size BlockSize
        {
            get { return size; }
        }
        public Color BlockColor
        {
            get { return color; }
        }
        public Point BlockPosition
        {
            get { return position; }
            set { position = value; }
        }

        //Constructor is called to spawn object
        public Block(Size blockSize, Color blockColor, Point blockPosition)
        {
            size = blockSize;
            color = blockColor;
            position = blockPosition;
        }

        //Property which returns structure in order to simplify calculating interactions
        public Rectangle BlockRectangle
        {
            get { return new Rectangle(position, size); }
        }
    }
}
