using Tao.OpenGl;
using Tao.FreeGlut;
using System;
using GameGL.Utils;
using GameGL.Controllers;
using GameGL.Observers;
using GameGL.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        public static int fps = 60; // define a taxa de quadros por segundo
        private static int vel = 20; // define a velocidade da cobrinha
        static bool pause = false; // define se o jogo esta ou não pausado
        static int animationTimeout = 2; // define o tempo da animação inicial
        static Stopwatch sw = new Stopwatch(); // controla o timer da animação

        static KeyboardSubject keyboardSubject; // ouvinte do teclado

        static Snake snake; // cobrinha
        static public Coordinate food; // comida
        static Random random; // utilizado para gerar posição aleatória da comida

        static void timer(int val) // temporizador
        {
            Glut.glutPostRedisplay();
            Glut.glutTimerFunc(1000 / fps, timer, 0);
        }

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            //Hide Console
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
            Glut.glutReshapeFunc(OnReshape); // Trata a responsividade
            Glut.glutMouseFunc(OnMouseClick); // Trata o clique do mouse
            Glut.glutMainLoop();
        }

        static void start()
        {
            // Definições da MATRIZ (0 a 1)
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, 1, 0, 1);            
            Gl.glClearColor(0, 0, 0, 1);

            // Intancia novo player
            snake = new Snake(new Coordinate((int)(screen.Grid / 2 * screen.Ratio), screen.Grid / 2), vel, new Coordinate(1, 0), new Color(0, 0.8f, 0.8f, 1), new Color(1, 1, 1, 1));
            
            keyboardSubject = new KeyboardSubject(); // instancia observador do teclado
            keyboardSubject.Attach(snake);  // sinaliza que a cobrinha vai ouvir o teclado

            random = new Random();
            food = ScreenController.instance.Free[random.Next(0, ScreenController.instance.Free.Count)];
            ScreenController.instance.setPosition(food, true);

            sw.Start();

            Gl.glPushMatrix();

        }

        static void OnDisplay()
        {
            if (sw.ElapsedMilliseconds - animationTimeout * 1000 < 0) animation(); // se for animação é animação           
            else
            {
                if (sw.IsRunning) { // ao terminar animação para animação
                    Gl.glPopMatrix();
                    sw.Stop();
                }
                if (!pause) game(); // se não pausado, joga
                else {
                    if (isEnd) end(); // se morreu, acabou
                    else menu(); // se não, você esta no menu
                }
            }

            Glut.glutSwapBuffers();
        }

        static float scale = 1;
        static void animation()  // momento animado!
        {
            Gl.glTranslatef(-(scale -1) / 2, 0, 0);
            Gl.glScaled(scale, scale, 1);

            scale += 0.00005f;
            Tools.drawBG("textures/bg.jpg");
            Tools.drawImg("textures/logo.png", new Coordinate((int)(screen.Grid / 2 * screen.Ratio) -2, screen.Grid / 2 - 2), 4);

            Tools.drawImg("textures/name.png", new Coordinate((int)(screen.Grid / 2 * screen.Ratio) - 4, screen.Grid / 2 - 10), 8);

        }

        static void game() // jogo bacana
        {
            Tools.drawBG("textures/bg.jpg"); // mostra textura no plano de fundo
            snake.move();
            snake.render();

            Tools.drawRect(food, 1f, new Color(0.8f,0.1f,0.1f,1)); // desenha comida

            if (snake.checkFood(food)) // faz verificação da comida
            {
                food = ScreenController.instance.Free[random.Next(0, ScreenController.instance.Free.Count)];
                ScreenController.instance.setPosition(food, true);
            }
        }

        /* MENU */
        static int selectMenu = 1;
        static bool moveMenu = true;
        static void menu()
        {
            if (moveMenu)
            {
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
                Gl.glPushMatrix();
                Gl.glTranslatef(0, selectMenu * 1f / ScreenController.instance.Grid, 0);
                Tools.drawTriangle(new Coordinate(15, 9), 1, new Color(0, 1, 0, 1));

                Gl.glPopMatrix();

                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2f(0.37f, 0.6f);
                Gl.glVertex2f(0.55f, 0.6f);
                Gl.glEnd();

                Tools.output(0.42f, 0.65f, 1, 1, 1, Glut.GLUT_BITMAP_HELVETICA_18, "MENU");
                Tools.output(0.42f, 0.515f, 1, 1, 1, Glut.GLUT_BITMAP_HELVETICA_12, "Reiniciar");
                Tools.output(0.42f, 0.415f, 1, 1, 1, Glut.GLUT_BITMAP_HELVETICA_12, "Continuar");
                moveMenu = false;
            }
        }

        static bool endRender = false, isEnd = false;
        public static void OnEnd() => pause = endRender = isEnd = true;

        /* MORREU */
        static async void end()  // tela de fim
        {
            if (endRender)
            {
                endRender = false;
                Tools.drawBG("textures/bg-end.jpg");

                // Escreve na tela
                Tools.output(0.55f, 0.65f, 1, 1, 1, Glut.GLUT_BITMAP_TIMES_ROMAN_24, "MOOOOREEEEUUUU"); 
                Tools.output(0.55f, 0.515f, 1, 1, 1, Glut.GLUT_BITMAP_HELVETICA_18, "mizeravi");

                snake.reset();

                await Task.Delay(3000);

                pause = isEnd = false;
            }
        }

        static void OnKeyboard(byte key, int x, int y)
        {
            switch (key)
            {
                case 13:
                    if (pause)
                    {
                        switch (selectMenu)
                        {
                            case 1: // reinicia
                                keyboardSubject.OnKeyboard(114);
                                break;
                            default: break;
                        }
                        pause = !pause;
                    }
                        
                    break;
                case 27: // Apertou ESC
                    Glut.glutLeaveMainLoop(); // Fechar o jogo
                    break;
                case 32: // Apertou Espaço
                    pause = true; // Pausar/despausar o jogo
                    moveMenu = !isEnd && true;
                    break;
                default: // apertou outra tecla
                    if(!pause) keyboardSubject.OnKeyboard(key);
                    break;
            }
        }

        static void OnArrowkey(int key, int x, int y) { 
            if (!pause) keyboardSubject.OnKeyboard(key, true);
            else if (key == Glut.GLUT_KEY_UP || key == Glut.GLUT_KEY_DOWN)
            {
                selectMenu *= -1;
                moveMenu = true;
            }

        }

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

        static void OnMouseClick(int button, int state, int x, int y)
        {
            if (button == Glut.GLUT_LEFT_BUTTON && state == Glut.GLUT_DOWN)
            {
                pause = true;
                moveMenu = true;
            }
        }

    }
}
