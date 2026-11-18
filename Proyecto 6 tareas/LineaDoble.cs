using Proyecto6_Tareas;
using System;

namespace Proyecto6_tareas
{
    internal class ListaDoble
    {
        private Nodo inicio;

        private class Nodo
        {
            public Tarea Datos;
            public Nodo Anterior;
            public Nodo Siguiente;

            public Nodo(Tarea tarea)
            {
                Datos = tarea;
            }
        }

        public bool insertar(Tarea tarea)
        {
            if (tarea == null || Buscar(tarea.id) != null)
                return false;

            Nodo nuevo = new Nodo(tarea);

            if (inicio == null)
            {
                inicio = nuevo;
                return true;
            }

            Nodo actual = inicio;

            while (actual.Siguiente != null)
                actual = actual.Siguiente;

            actual.Siguiente = nuevo;
            nuevo.Anterior = actual;
            return true;
        }

        public Tarea Buscar(int id)
        {
            Nodo actual = inicio;

            while (actual != null)
            {
                if (actual.Datos.id == id)
                {
                    return actual.Datos;
                }

                actual = actual.Siguiente;

            }
            return null;

        }

        public void Mostrar()
        {
            Nodo actual = inicio;

            while (actual != null)
            {
                Console.WriteLine(actual.Datos);
                actual = actual.Siguiente;
            }
        }

        public void MostrarInverso()
        {
            Nodo actual = inicio;

            if (actual == null)
                return;

            while (actual.Siguiente != null)
                actual = actual.Siguiente;

            while (actual != null)
            {
                Console.WriteLine(actual.Datos);
                actual = actual.Anterior;
            }
        }

        public bool MarcarCompletada(int id)
        {
            Nodo actual = inicio;

            while (actual != null)
            {
                if (actual.Datos.id == id)
                {
                    actual.Datos.Completada = true;
                    return true;
                }

                actual = actual.Siguiente;
            }

            return false;
        }

        public bool Eliminar(int id)
        {
            Nodo actual = inicio;

            while (actual != null)
            {
                if (actual.Datos.id == id)
                {
                    if (actual.Anterior == null)
                        inicio = actual.Siguiente;
                    else
                        actual.Anterior.Siguiente = actual.Siguiente;

                    if (actual.Siguiente != null)
                        actual.Siguiente.Anterior = actual.Anterior;

                    return true;
                }

                actual = actual.Siguiente;
            }

            return false;
        }
    }
}
