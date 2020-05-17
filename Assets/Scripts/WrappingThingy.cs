using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class WrappingThingy
    {
        public Temperature temp;
        public readonly int i;
        public readonly int j;

        public WrappingThingy(Temperature temp, int i, int j)
        {
            this.temp = temp;
            this.i = i;
            this.j = j;
        }
    }
}
