﻿using Tao.OpenGl;
using Tao.FreeGlut;
using System;
using GameGL.Utils;
using GameGL.Controllers;
using GameGL.Observers;
using GameGL.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameGL
{
    class Program
    {
        /* OCULTA O CONSOLE */
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        /* DECLARA VARIAVEIS */
        static ScreenController screen = new ScreenController(new Size(640, 320), 20);
        public static int fps = 60;
        private static int vel = 20;
        static bool pause = false;
        static KeyboardSubject keyboardSubject;
        static int animationTimeout = 5;
        static Stopwatch sw = new Stopwatch();

        static Snake snake;
        static public Coordinate food;
        static Random random;

        static void timer(int val)
        {
            Glut.glutPostRedisplay();
            Glut.glutTimerFunc(1000 / fps, timer, 0);
        }

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            // Hide Console
            ShowWindow(handle, SW_HIDE);

            Glut.glutInit();
            Glut.glutInitWindowSize((int)screen.Size.Width, (int)screen.Size.Height);
            Glut.glutInitDisplayMode(Glut.GLUT_SINGLE | Glut.GLUT_RGB);
            Glut.glutCreateWindow("Game");
            Glut.glutTimerFunc(1000 / fps, timer, 0);
            start();
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutKeyboardFunc(OnKeyboard);
            Glut.glutSpecialFunc(OnArrowkey);
            Glut.glutReshapeFunc(OnReshape); // Macumba que mantem a proporção da tela
            Glut.glutMainLoop();

            
        }

        static void start()
        {
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, 1, 0, 1);
            screen.clearColor = new Color(0, 0, 0, 1);
            Gl.glClearColor(0, 0, 0, 1);

            snake = new Snake(new Coordinate((int)(screen.Grid / 2 * screen.Ratio), screen.Grid / 2), vel, new Coordinate(1, 0), new Color(0, 0.8f, 0.8f, 1), new Color(1, 1, 1, 1));
            
            keyboardSubject = new KeyboardSubject();
            keyboardSubject.Attach(snake);     

            
            random = new Random();
            food = ScreenController.instance.Free[random.Next(0, ScreenController.instance.Free.Count)];
            ScreenController.instance.setPosition(food, true);

            sw.Start();

            Gl.glPushMatrix();

        }

        static void OnDisplay()
        {
            if (sw.ElapsedMilliseconds - animationTimeout * 1000 < 0) animation();            
            else
            {
                if (sw.IsRunning) {
                    Gl.glPopMatrix();
                    sw.Stop();
                }
                if (!pause) game();
            }

            Glut.glutSwapBuffers();
        }

        static float scale = 1;

        static void animation()
        {
            Gl.glTranslatef(-(scale -1) / 2, 0, 0);
            Gl.glScaled(scale, scale, 1);

            scale += 0.00005f;
            Tools.drawBG();
            Tools.drawImg("textures/logo.png", new Coordinate((int)(screen.Grid / 2 * screen.Ratio) -2, screen.Grid / 2 - 2), 4);

            Tools.drawImg("textures/name.png", new Coordinate((int)(screen.Grid / 2 * screen.Ratio) - 4, screen.Grid / 2 - 10), 8);
            Tools.drawCicle(new Coordinate(5, 5), 1f, new Color(0.8f, 0.1f, 0.1f, 1));
        }

        static void game()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
            snake.move();
            snake.render();

            Tools.drawRect(food, 1f, new Color(0.8f,0.1f,0.1f,1));

            if (snake.checkFood(food))
            {
                food = ScreenController.instance.Free[random.Next(0, ScreenController.instance.Free.Count)];
                ScreenController.instance.setPosition(food, true);
            }
        }
        
        static void OnKeyboard(byte key, int x, int y)
        {
            switch (key)
            {
                case 27: // Apertou ESC
                    Glut.glutLeaveMainLoop(); // Fechar o jogo
                    break;
                case 32: // Apertou Espaço
                    pause = !pause; // Pausar/despausar o jogo
                    break;
                default: // apertou outra tecla
                    if(!pause) keyboardSubject.OnKeyboard(key);
                    break;
            }
        }

        static void OnArrowkey(int key, int x, int y) { if (!pause) keyboardSubject.OnKeyboard(key, true); }

        static void OnReshape(int width, int height)
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);

            var ratioX = width / screen.Size.Width;
            var ratioY = height / screen.Size.Height;
            var ratio = ratioX < ratioY ? ratioX : ratioY;
            
            var viewWidth = (int)(screen.Size.Width * ratio);
            var viewHeight = (int)(screen.Size.Height * ratio);
            
            var viewX = (int)((width - screen.Size.Width * ratio) / 2);
            var viewY = (int)((height - screen.Size.Height * ratio) / 2);
            
            Gl.glViewport(viewX, viewY, viewWidth, viewHeight);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

    }
}