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
        public int Grid { get; } // Quantidade de divisões/quadrados tela (fixo)

        public List<Coordinate> Free { get; set; } // Posições livres no grid ex.: {0,0} {15,1} {3,21}

        public ScreenController(Size size, int grid) // construtor
        {
            instance = this; // instancia global
            Size = size;
            Ratio = size.Width / size.Height; // calcula a proporção
            Grid = grid;
            clear();
        }

        public void clear() // libera todas as posições do grid
        {
            Free = new List<Coordinate>(); 

            for (int i = 0; i < Grid * 2; i++)
                for (int j = 0; j < Grid / Ratio * 2; j++)
                    Free.Add(new Coordinate(i, j));
        }

        public void setPosition(Coordinate pos, bool filled) // define se a posição esta ou não ocupada no grid
        { 
            Coordinate coord = Free.FirstOrDefault(f => f.X == pos.X && f.Y == pos.Y);
            if (filled)
            {
                if (coord != null) Free.Remove(coord);
            }            
            else if (coord == null) Free.Add(pos);
        }
    }
}
