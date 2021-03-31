using GameGL.Controllers;
using GameGL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.FreeGlut;
using Tao.OpenGl;

namespace GameGL.Utils
{
    class Tools
    {
        public static void drawRect(Coordinate origem, float size, Color color)
        {
            Point o = new Point(origem.X * 1f / ScreenController.instance.Grid / ScreenController.instance.Ratio, origem.Y * 1f / ScreenController.instance.Grid);
            Size s = new Size(size * 1f / ScreenController.instance.Grid, size * 1f / ScreenController.instance.Grid);

            Gl.glColor4f(color.R, color.G, color.B, color.A);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex2f(o.X, o.Y);
            Gl.glVertex2f(o.X, o.Y + s.Height);
            Gl.glVertex2f(o.X + s.Width / ScreenController.instance.Ratio, o.Y + s.Height);
            Gl.glVertex2f(o.X + s.Width / ScreenController.instance.Ratio, o.Y);
            Gl.glEnd();
        }

        public static void drawTriangle(Coordinate origem, float size, Color color)
        {
            Point o = new Point(origem.X * 1f / ScreenController.instance.Grid / ScreenController.instance.Ratio, origem.Y * 1f / ScreenController.instance.Grid);
            Size s = new Size(size * 1f / ScreenController.instance.Grid, size * 1f / ScreenController.instance.Grid);

            Gl.glColor4f(color.R, color.G, color.B, color.A);
            Gl.glBegin(Gl.GL_TRIANGLES);
            Gl.glVertex2f(o.X, o.Y);
            Gl.glVertex2f(o.X + s.Width / ScreenController.instance.Ratio, o.Y + s.Height / 2);
            Gl.glVertex2f(o.X, o.Y + s.Height);
            Gl.glEnd();
        }
        
        public static void drawCicle(Coordinate origem, float size, Color color)
        {
            Point o = new Point(origem.X * 1f / ScreenController.instance.Grid / ScreenController.instance.Ratio, origem.Y * 1f / ScreenController.instance.Grid);
            
            float raio, x, y, pontos;
            raio = (size * 1f / ScreenController.instance.Grid) / 2;
            pontos = (2 * (float)Math.PI) / 32;

            Gl.glColor4f(color.R, color.G, color.B, color.A);
            Gl.glLineWidth(2);
            Gl.glBegin(Gl.GL_TRIANGLE_FAN);

            for (float angulo = 0; angulo <= 2 * Math.PI; angulo += pontos)
            {
                x = (float)(raio * Math.Cos(angulo) + o.X + raio);
                y = (float)(raio * Math.Sin(angulo) + o.Y + raio);

                Gl.glVertex2f(x, y);
            }

            Gl.glEnd();
        }

        public static void drawBG(string path)
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            uint texture = 0;
            Gl.glColor4f(1, 1, 1, 1);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture);

            var bmp = loadImageBitmap(path);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, (int)Gl.GL_RGB8,
                bmp.Width, bmp.Height, 0, Gl.GL_BGR_EXT,
                Gl.GL_UNSIGNED_BYTE, bmpData.Scan0);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 1); Gl.glVertex2f(0, 0);
            Gl.glTexCoord2f(0, 0); Gl.glVertex2f(0, 1);
            Gl.glTexCoord2f(1, 0); Gl.glVertex2f(1, 1);
            Gl.glTexCoord2f(1, 1); Gl.glVertex2f(1, 0);

            Gl.glEnd();

            Gl.glDisable(Gl.GL_TEXTURE_2D);
            bmp.UnlockBits(bmpData);
        }

        public static void drawImg(string file, Coordinate origem, float size)
        {
            Point o = new Point(origem.X * 1f / ScreenController.instance.Grid / ScreenController.instance.Ratio, origem.Y * 1f / ScreenController.instance.Grid);
            Size s = new Size(size * 1f / ScreenController.instance.Grid, size * 1f / ScreenController.instance.Grid);

            Gl.glEnable(Gl.GL_TEXTURE_2D);

            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

            uint texture = 0;
            Gl.glColor4f(1, 1, 1, 1);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture);

            var bmp = loadImageBitmap(file);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, (int)Gl.GL_RGBA8,
                bmp.Width, bmp.Height, 0, (int)Gl.GL_BGRA_EXT,
                Gl.GL_UNSIGNED_BYTE, bmpData.Scan0);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 1);  Gl.glVertex2f(o.X, o.Y);
            Gl.glTexCoord2f(0, 0); Gl.glVertex2f(o.X, o.Y + s.Height);
            Gl.glTexCoord2f(1, 0); Gl.glVertex2f(o.X + s.Width / ScreenController.instance.Ratio, o.Y + s.Height);
            Gl.glTexCoord2f(1, 1); Gl.glVertex2f(o.X + s.Width / ScreenController.instance.Ratio, o.Y);
            Gl.glEnd();

            Gl.glDisable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            bmp.UnlockBits(bmpData);
        }

        static Bitmap loadImageBitmap(string pathImg)
        {
            using (Stream s = File.OpenRead(pathImg))
                return (Bitmap)Image.FromStream(s);
        }

        public static void output(float x, float y, float r, float g, float b, IntPtr font, string str)
        {
            Gl.glColor3f(r, g, b);
            Gl.glRasterPos2f(x, y);
            int len, i;
            len = str.Length;
            for (i = 0; i < len; i++)
            {
                Glut.glutBitmapCharacter(font, str[i]);
            }
        }
    }
}
