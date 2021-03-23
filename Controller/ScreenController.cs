using GameGL.Utils;
using System.Collections.Generic;
using System.Linq;

namespace GameGL.Controllers
{
    class ScreenController
    {
        static public ScreenController instance;

        public Size Size { get; } // Tamanho da tela
        public float Ratio { get; } // Proporção da tela
        public int Grid { get; } // Quantidade de divisões/quadrados tela

        public List<Coordinate> Free { get; set; }
        public Color clearColor { get; set; } = new Color(0, 0, 0, 1);

        public ScreenController(Size size, int grid)
        {
            instance = this;
            Size = size;
            Ratio = size.Width / size.Height;
            Grid = grid;
            clear();
        }

        public void clear()
        {
            Free = new List<Coordinate>();

            for (int i = 0; i < Grid * 2; i++)
                for (int j = 0; j < Grid / Ratio * 2; j++)
                    Free.Add(new Coordinate(i, j));
        }

        public void setPosition(Coordinate pos, bool filled) {
            Coordinate coord = Free.FirstOrDefault(f => f.X == pos.X && f.Y == pos.Y);
            if (filled)
            {
                if (coord != null) Free.Remove(coord);
            }            
            else if (coord == null) Free.Add(pos);
        }
    }
}
