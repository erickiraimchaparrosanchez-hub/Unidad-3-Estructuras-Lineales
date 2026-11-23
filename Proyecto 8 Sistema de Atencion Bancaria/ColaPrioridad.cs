using System;
namespace Proyecto8_Banco
{
    public class ColaPrioridad
    {
        private Nodo frente;

        private class Nodo
        {
            public Cliente Datos;
            public Nodo Siguiente;

            public Nodo(Cliente cliente)
            {
                Datos = cliente;
            }
        }

        public void Insertar(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException("cliente");

            Nodo nuevoNodo = new Nodo(cliente);
            if (frente == null || cliente.Prioridad < frente.Datos.Prioridad)
            {
                nuevoNodo.Siguiente = frente;
                frente = nuevoNodo;
            }
            else
            {
                Nodo actual = frente;
                while (actual.Siguiente != null && actual.Siguiente.Datos.Prioridad <= cliente.Prioridad)
                {
                    actual = actual.Siguiente;
                }
                nuevoNodo.Siguiente = actual.Siguiente;
                actual.Siguiente = nuevoNodo;
            }
        }

        public bool EstaVacia()
        {
            return frente == null;
        }

        public Cliente Atender()
        {
            if (frente == null)
                return null;

            Cliente cliente = frente.Datos;
            frente = frente.Siguiente;

            return cliente;
        }
        
        public void Mostrar()
        {
            Nodo actual = frente;
            while (actual != null)
            {
                Console.WriteLine(actual.Datos);
                actual = actual.Siguiente;
            }
        }
    }
}