using System;
using System.Collections.Generic;

namespace Proyecto7_Musica
{
    internal class Program
    {
        static List<Playlist> playlists = new List<Playlist>();
        static Playlist activa = null;

        static void Main(string[] args)
        {
            int opcion;

            do
            {
                Console.WriteLine("\n===== REPRODUCTOR MUSICAL =====");
                Console.WriteLine(activa == null
                    ? "Playlist activa: (ninguna)"
                    : $"Playlist activa: {activa.Nombre}");
                Console.WriteLine("1.  Crear playlist");
                Console.WriteLine("2.  Seleccionar playlist");
                Console.WriteLine("3.  Agregar cancion");
                Console.WriteLine("4.  Eliminar cancion");
                Console.WriteLine("5.  Buscar cancion");
                Console.WriteLine("6.  Reproducir");
                Console.WriteLine("7.  Siguiente");
                Console.WriteLine("8.  Anterior");
                Console.WriteLine("9.  Pausar");
                Console.WriteLine("10. Mostrar playlist");
                Console.WriteLine("11. Reproduccion circular");
                Console.WriteLine("0.  Salir");
                Console.Write("Elige una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                    opcion = -1;

                switch (opcion)
                {
                    case 1: CrearPlaylist(); break;
                    case 2: SeleccionarPlaylist(); break;
                    case 3: AgregarCancion(); break;
                    case 4: EliminarCancion(); break;
                    case 5: BuscarCancion(); break;
                    case 6: Reproducir(); break;
                    case 7: Siguiente(); break;
                    case 8: Anterior(); break;
                    case 9: Pausar(); break;
                    case 10: Mostrar(); break;
                    case 11: ReproduccionCircular(); break;
                    case 0: Console.WriteLine("Saliendo..."); break;
                    default: Console.WriteLine("Opcion no valida."); break;
                }

            } while (opcion != 0);
        }

        static bool HayActiva()
        {
            if (activa == null)
            {
                Console.WriteLine("Primero crea o selecciona una playlist.");
                return false;
            }
            return true;
        }

        static string LeerTexto(string mensaje)
        {
            Console.Write(mensaje);
            return (Console.ReadLine() ?? "").Trim();
        }

        static void CrearPlaylist()
        {
            string nombre = LeerTexto("Nombre de la playlist: ");

            if (nombre == "")
            {
                Console.WriteLine("El nombre no puede estar vacio.");
                return;
            }

            foreach (Playlist p in playlists)
            {
                if (string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Ya existe una playlist con ese nombre.");
                    return;
                }
            }

            Playlist nueva = new Playlist(nombre);
            playlists.Add(nueva);
            activa = nueva;
            Console.WriteLine($"Playlist '{nombre}' creada y seleccionada.");
        }

        static void SeleccionarPlaylist()
        {
            if (playlists.Count == 0)
            {
                Console.WriteLine("No hay playlists creadas.");
                return;
            }

            for (int i = 0; i < playlists.Count; i++)
                Console.WriteLine($"{i + 1}. {playlists[i].Nombre} ({playlists[i].Cantidad} canciones)");

            int num;
            if (int.TryParse(LeerTexto("Numero de playlist: "), out num) && num >= 1 && num <= playlists.Count)
            {
                activa = playlists[num - 1];
                Console.WriteLine($"Playlist activa: {activa.Nombre}");
            }
            else
            {
                Console.WriteLine("Numero no valido.");
            }
        }

        static void AgregarCancion()
        {
            if (!HayActiva()) return;

            string titulo = LeerTexto("Titulo: ");
            string artista = LeerTexto("Artista: ");

            if (titulo == "" || artista == "")
            {
                Console.WriteLine("Titulo y artista no pueden estar vacios.");
                return;
            }

            if (activa.Buscar(titulo) != null)
            {
                Console.WriteLine("Esa cancion ya esta en la playlist.");
                return;
            }

            activa.Agregar(new Cancion(titulo, artista));
            Console.WriteLine("Cancion agregada.");
        }

        static void EliminarCancion()
        {
            if (!HayActiva()) return;

            string titulo = LeerTexto("Titulo de la cancion a eliminar: ");

            if (activa.Eliminar(titulo))
                Console.WriteLine("Cancion eliminada.");
            else
                Console.WriteLine("Cancion no encontrada.");
        }

        static void BuscarCancion()
        {
            if (!HayActiva()) return;

            Cancion c = activa.Buscar(LeerTexto("Titulo a buscar: "));

            if (c != null)
                Console.WriteLine($"Encontrada: {c}");
            else
                Console.WriteLine("Cancion no encontrada.");
        }

        static void Reproducir()
        {
            if (!HayActiva()) return;

            Cancion c = activa.Reproducir();

            if (c != null)
                Console.WriteLine($"Reproduciendo: {c}");
            else
                Console.WriteLine("La playlist esta vacia.");
        }

        static void Siguiente()
        {
            if (!HayActiva()) return;

            Cancion c = activa.Siguiente();

            if (c != null)
                Console.WriteLine($"Reproduciendo: {c}");
            else
                Console.WriteLine("La playlist esta vacia.");
        }

        static void Anterior()
        {
            if (!HayActiva()) return;

            Cancion c = activa.Anterior();

            if (c != null)
                Console.WriteLine($"Reproduciendo: {c}");
            else
                Console.WriteLine("La playlist esta vacia.");
        }

        static void Pausar()
        {
            if (!HayActiva()) return;

            if (activa.Pausar())
                Console.WriteLine("Reproduccion en pausa.");
            else
                Console.WriteLine("No hay nada reproduciendose.");
        }

        static void Mostrar()
        {
            if (!HayActiva()) return;
            activa.Mostrar();
        }

        static void ReproduccionCircular()
        {
            if (!HayActiva()) return;

            int cantidad;
            if (!int.TryParse(LeerTexto("Cuantas canciones quieres reproducir: "), out cantidad) || cantidad < 1)
            {
                Console.WriteLine("Cantidad no valida.");
                return;
            }

            activa.ReproduccionCircular(cantidad);
        }
    }
}
