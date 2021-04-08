using GameGL.Controllers;
using GameGL.Observers;
using GameGL.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGL.Models
{
    class Snake : IObserver
    {
        private List<Coordinate> body, moves; // cordenadas do corpo da cobrinha // fila de movimentos no teclado
        private Coordinate Start { get; } // posição inicial no grid
        private Coordinate Direction { get; set; } // direção atual (direita, esquerda etc)
        public Coordinate StartDir { get; } // direção inicial (restart)
        private int BodySize { get; set; }  // tamanho máximo da cobrinha
        private int Velocity { get; set; } // velocidade atual
        public int StartVel { get; } // velocidade inicial
        private int TakedTimeout { get; set; } // timer animação ao pegar comida
        private Color[] Colors { get; } // cores da cabeça e do corpo da cobrinha

        Stopwatch sw = new Stopwatch(); // timer da animação ao pegar comida

        public Snake(Coordinate start, int velocity, Coordinate direction, Color headColor, Color bodyColor) // contrutor
        {
            body = new List<Coordinate>();
            moves = new List<Coordinate>();
            Start = start;
            handleBody("add", start);
            BodySize = 5;

            Velocity = StartVel = velocity;
            Direction = StartDir = direction;

            sw.Start();

            Colors = new Color[2];
            Colors[0] = headColor;
            Colors[1] = bodyColor;
        }

        public void Update(ISubject subject) // implementação do padrão observer
        {
            int key = (subject as KeyboardSubject).key;
            bool special = (subject as KeyboardSubject).special;

            if (special)
            {
                switch (key)
                {
                    case 100:
                        // Pra esquerda
                        if ((moves.Count == 0 && Direction.X < 1) || (moves.Count > 0 && moves[moves.Count - 1].X < 1)) moves.Add(new Coordinate(-1, 0));
                        break;
                    case 101:
                        // Pra cima
                        if ((moves.Count == 0 && Direction.Y > -1) || (moves.Count > 0 && moves[moves.Count - 1].Y > -1)) moves.Add(new Coordinate(0, 1));                        
                        break;
                    case 102:
                        // Pra direita
                        if ((moves.Count == 0 && Direction.X > -1) || (moves.Count > 0 && moves[moves.Count - 1].X > -1)) moves.Add(new Coordinate(1, 0));
                        break;
                    case 103:
                        // Pra baixo
                        if ((moves.Count == 0 && Direction.Y < 1) || (moves.Count > 0 && moves[moves.Count - 1].Y < 1)) moves.Add(new Coordinate(0, -1));
                        break;
                    default:                        
                        break;
                }
            }
            else {
                switch ((char)key)
                {
                    case 'r':
                        reset();
                        break;                       
                    default:
                        break;
                }
            }
        }

        public void render() // desenha cobrinha bem bonita
        {
            foreach (var pos in body.Select((p, i) => new { val = p, index = i }))
                if(pos.index > 0) Tools.drawRect(pos.val, 1f, getColor(pos.index)); // Função de desenho
            
            if(TakedTimeout > 0) Tools.drawRect(body[0], 1f, new Color(1f, 0f, 0.7f, 1));                     
            else Tools.drawRect(body[0], 1f, Colors[0]);
        }

        private float colorRate = -0.001f;
        private Color lastColor;

        private Color getColor(int index) // cria efeito degrade no corpo da cobrinha, para ser bem bonita
        {
            float R, G, B, A;
            if(index == 1)
            {
                lastColor = Colors[1];
                colorRate = -0.001f;
            }
            if (index %30 == 0) colorRate *= -1;

            R = lastColor.R + index % 30 * colorRate;
            G = lastColor.G + index % 30 * colorRate;
            B = lastColor.B + index % 30 * colorRate;
            A = lastColor.A;

            lastColor = new Color(R, G, B, A);

            return lastColor;
        }

        public void move () // trata os movimentos da cobrinha (regra de negocio)
        {
            if (sw.ElapsedMilliseconds - (1000 / (float)Velocity) < 0) return; // faz o controle de velocidade (independente do fps)
            else sw.Restart();

            if (TakedTimeout > 0) TakedTimeout--; // controle timer da animação da cobrinha ao pegar comida

            if (moves.Count > 0) // desempilha ultimos movimentos
            {
                Direction = moves[0];
                moves.RemoveAt(0);
            }

            Coordinate last = body[0]; // pega posição atual da cabeça da cobrinha
            // define nova coordenada da cabeça da cobrinha
            Coordinate next = borderCheck(new Coordinate(last.X + Direction.X, last.Y + Direction.Y));

            //verifica se a proxima posição já é ocupada
            if (body.FirstOrDefault(p => p.X == next.X && p.Y == next.Y) != null) onCollide(); // detecta colisão
            else // movimenta cobrinha de fato
            {
                handleBody("add", next);
                while (body.Count > BodySize) handleBody("rem"); // remove posições fora do limite do tamanho máximo
            }
        }

        private void handleBody(string op, Coordinate coordinate = null)
        {
            switch (op)
            {
                case "add":
                    if(coordinate != null)
                    {
                        body.Insert(0, coordinate);
                        ScreenController.instance.setPosition(coordinate, true);

                        if (ScreenController.instance.Free.Count == 0)
                        {
                            reset();
                        }
                    }
                    
                    break;
                case "rem":
                    Coordinate coord = body[body.Count - 1];
                    body.Remove(coord);
                    ScreenController.instance.setPosition(coord, false);
                    break;
                default:
                    break;
            }
        }

        private Coordinate borderCheck(Coordinate next) // se for borda, inverte coordenada no eixo respectivo
        {
            int nextX = next.X < 0 ? (int)(ScreenController.instance.Grid * ScreenController.instance.Ratio) - 1 : next.X > (int)(ScreenController.instance.Grid * ScreenController.instance.Ratio) - 1 ? 0 : next.X;
            int nextY = next.Y < 0 ? ScreenController.instance.Grid - 1 : next.Y > ScreenController.instance.Grid - 1 ? 0 : next.Y;

            return new Coordinate(nextX, nextY);
        }

        public void reset() // reiniciar jogo
        {
            ScreenController.instance.clear();
            body = new List<Coordinate>();
            handleBody("add", Start);
            Velocity = StartVel;
            Direction = StartDir;
            BodySize = 5;
        }

        private void onCollide() => Program.OnEnd(); // Chama animação de colisão
        
        public bool checkFood(Coordinate food) // verifica se pegou alguma comida
        {
            bool taked = body[0].X == food.X && body[0].Y == food.Y;

            if (taked) {
                TakedTimeout = 10;
                BodySize += 2;
                Velocity += (int)(Velocity * 0.05);
            }
            return taked;
        }


    }
}
