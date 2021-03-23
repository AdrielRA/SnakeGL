using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGL.Utils
{
    class Size
    {
        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; set; }
        public float Height { get; set; }
    }
}
