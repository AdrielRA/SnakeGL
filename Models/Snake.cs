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
        private List<Coordinate> body, moves;
        private Coordinate Start { get; }
        private Coordinate Direction { get; set; }
        public Coordinate StartDir { get; }
        private int BodySize { get; set; }
        private int Velocity { get; set; }
        public int StartVel { get; }
        private int TakedTimeout { get; set; }
        private Color[] Colors { get; }

        Stopwatch sw = new Stopwatch();

        public Snake(Coordinate start, int velocity, Coordinate direction, Color headColor, Color bodyColor)
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


        public void Update(ISubject subject)
        {
            int key = (subject as KeyboardSubject).key;
            bool special = (subject as KeyboardSubject).special;

            if (special)
            {
                switch (key)
                {
                    case 100:
                        //Console.WriteLine("Pra esquerda");
                        if ((moves.Count == 0 && Direction.X < 1) || (moves.Count > 0 && moves[moves.Count - 1].X < 1)) moves.Add(new Coordinate(-1, 0));
                        break;
                    case 101:
                        //Console.WriteLine("Pra cima");
                        if ((moves.Count == 0 && Direction.Y > -1) || (moves.Count > 0 && moves[moves.Count - 1].Y > -1)) moves.Add(new Coordinate(0, 1));                        
                        break;
                    case 102:
                        //Console.WriteLine("Pra direita");
                        if ((moves.Count == 0 && Direction.X > -1) || (moves.Count > 0 && moves[moves.Count - 1].X > -1)) moves.Add(new Coordinate(1, 0));
                        break;
                    case 103:
                        //Console.WriteLine("Pra baixo");
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
                        Console.WriteLine("Pressionou:" + (char)key);
                        break;
                }
            }
        }

        public void render()
        {
            foreach (var pos in body.Select((p, i) => new { val = p, index = i }))
                if(pos.index > 0) Tools.drawRect(pos.val, 1f, getColor(pos.index));
            
            if(TakedTimeout > 0) Tools.drawRect(body[0], 1f, new Color(1f, 0f, 0.7f, 1));                     
            else Tools.drawRect(body[0], 1f, Colors[0]);
        }

        private float colorRate = -0.001f;
        private Color lastColor;

        private Color getColor(int index)
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

        public void move ()
        {
            if (sw.ElapsedMilliseconds - (1000 / (float)Velocity) < 0) return;
            else sw.Restart();

            if (TakedTimeout > 0) TakedTimeout--;
            if (moves.Count > 0)
            {
                Direction = moves[0];
                moves.RemoveAt(0);
            }

            Coordinate last = body[0];
            Coordinate next = borderCheck(new Coordinate(last.X + Direction.X, last.Y + Direction.Y));

            if (body.FirstOrDefault(p => p.X == next.X && p.Y == next.Y) != null) onCollide();
            else
            {
                handleBody("add", next);
                while (body.Count > BodySize) handleBody("rem");
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

        private Coordinate borderCheck(Coordinate next)
        {
            int nextX = next.X < 0 ? (int)(ScreenController.instance.Grid * ScreenController.instance.Ratio) - 1 : next.X > (int)(ScreenController.instance.Grid * ScreenController.instance.Ratio) - 1 ? 0 : next.X;
            int nextY = next.Y < 0 ? ScreenController.instance.Grid - 1 : next.Y > ScreenController.instance.Grid - 1 ? 0 : next.Y;

            return new Coordinate(nextX, nextY);
        }

        private void reset()
        {
            ScreenController.instance.clear();
            body = new List<Coordinate>();
            handleBody("add", Start);
            Velocity = StartVel;
            Direction = StartDir;
            BodySize = 5;
        }

        private void onCollide()
        {
            Console.WriteLine("Colidiu");
            reset();            
        }

        public bool checkFood(Coordinate food)
        {
            bool taked = body[0].X == food.X && body[0].Y == food.Y;

            if (taked) {
                TakedTimeout = 5;
                BodySize += 2; }
            return taked;
        }

    }
}
