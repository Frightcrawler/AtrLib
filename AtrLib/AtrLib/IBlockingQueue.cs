using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtrLib
{
    interface IBlockingQueue<T>
    {
        void Push(T item);

        T Pop();
    }
}
