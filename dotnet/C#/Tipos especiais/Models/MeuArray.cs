using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class MeuArray<T>
    {
        private static int capacidade = 10;
        private int contador = 0;
        private T[] array = new T[capacidade];

        public void AdicionarElemento(T elemento)
        {
            if ((contador) < capacidade)
            {
                array[contador] = elemento;
                contador++;
            }            
        }

        public T this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                array[index] = value;
            }
        }
    }
}