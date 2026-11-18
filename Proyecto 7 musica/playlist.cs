using System;

namespace Proyecto7_Musica
{
    // Lista circular doblemente enlazada:
    // inicio.Anterior es la ultima cancion y ultima.Siguiente es inicio.
    public class Playlist
    {
        public string Nombre { get; private set; }
        public bool Reproduciendo { get; private set; }
        public int Cantidad { get; private set; }

        private Nodo inicio;
        private Nodo actual;

        private class Nodo
        {
            public Cancion Datos;
            public Nodo Anterior;
            public Nodo Siguiente;

            public Nodo(Cancion cancion)
            {
                Datos = cancion;
            }
        }

        public Playlist(string nombre)
        {
            Nombre = nombre;
        }

        public bool EstaVacia()
        {
            return inicio == null;
        }

        // Agrega al final de la playlist
        public void Agregar(Cancion cancion)
        {
            Nodo nuevo = new Nodo(cancion);

            if (inicio == null)
            {
                inicio = nuevo;
                nuevo.Siguiente = nuevo;
                nuevo.Anterior = nuevo;
            }
            else
            {
                Nodo ultimo = inicio.Anterior;

                ultimo.Siguiente = nuevo;
                nuevo.Anterior = ultimo;
                nuevo.Siguiente = inicio;
                inicio.Anterior = nuevo;
            }

            Cantidad++;
        }

        private Nodo BuscarNodo(string titulo)
        {
            if (inicio == null)
                return null;

            Nodo n = inicio;
            do
            {
                if (string.Equals(n.Datos.Titulo, titulo, StringComparison.OrdinalIgnoreCase))
                    return n;

                n = n.Siguiente;
            } while (n != inicio);

            return null;
        }

        public Cancion Buscar(string titulo)
        {
            Nodo n = BuscarNodo(titulo);
            return n == null ? null : n.Datos;
        }

        public bool Eliminar(string titulo)
        {
            Nodo n = BuscarNodo(titulo);
            if (n == null)
                return false;

            if (Cantidad == 1)
            {
                inicio = null;
                actual = null;
                Reproduciendo = false;
            }
            else
            {
                n.Anterior.Siguiente = n.Siguiente;
                n.Siguiente.Anterior = n.Anterior;

                if (n == inicio)
                    inicio = n.Siguiente;

                if (n == actual)
                    actual = n.Siguiente;
            }

            Cantidad--;
            return true;
        }

        // Reproduce la cancion actual (o la primera si aun no hay una). Tambien sirve para reanudar.
        public Cancion Reproducir()
        {
            if (inicio == null)
                return null;

            if (actual == null)
                actual = inicio;

            Reproduciendo = true;
            return actual.Datos;
        }

        // Pasa a la siguiente. Si estaba en la ultima, regresa a la primera.
        public Cancion Siguiente()
        {
            if (inicio == null)
                return null;

            actual = (actual == null) ? inicio : actual.Siguiente;
            Reproduciendo = true;
            return actual.Datos;
        }

        // Pasa a la anterior. Si estaba en la primera, va a la ultima.
        public Cancion Anterior()
        {
            if (inicio == null)
                return null;

            actual = (actual == null) ? inicio.Anterior : actual.Anterior;
            Reproduciendo = true;
            return actual.Datos;
        }

        public bool Pausar()
        {
            if (!Reproduciendo)
                return false;

            Reproduciendo = false;
            return true;
        }

        public void Mostrar()
        {
            if (inicio == null)
            {
                Console.WriteLine("La playlist esta vacia.");
                return;
            }

            Console.WriteLine($"--- Playlist: {Nombre} ({Cantidad} canciones) ---");

            Nodo n = inicio;
            int posicion = 1;
            do
            {
                string marca = "";
                if (n == actual)
                    marca = Reproduciendo ? "  <- sonando" : "  <- en pausa";

                Console.WriteLine($"{posicion}. {n.Datos}{marca}");

                n = n.Siguiente;
                posicion++;
            } while (n != inicio);
        }

        // Simula la reproduccion continua: al llegar a la ultima cancion vuelve a la primera.
        public void ReproduccionCircular(int canciones)
        {
            if (inicio == null)
            {
                Console.WriteLine("La playlist esta vacia.");
                return;
            }

            Cancion c = Reproducir();
            Console.WriteLine($"Sonando: {c}");

            for (int i = 1; i < canciones; i++)
            {
                c = Siguiente();
                Console.WriteLine($"Sonando: {c}");
            }
        }
    }
}
