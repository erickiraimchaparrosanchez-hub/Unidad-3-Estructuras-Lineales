using Proyecto6_tareas;
using System;

namespace Proyecto6_Tareas
{
    internal class Program
    {
        static ListaDoble lista = new ListaDoble();

        static void Main(string[] args)
        {
            int opcion;

            do
            {
                Console.WriteLine("\n===== GESTOR DE TAREAS =====");
                Console.WriteLine("1. Agregar tarea");
                Console.WriteLine("2. Buscar tarea");
                Console.WriteLine("3. Marcar tarea como completada");
                Console.WriteLine("4. Eliminar tarea");
                Console.WriteLine("5. Mostrar tareas (inicio a fin)");
                Console.WriteLine("6. Mostrar tareas (fin a inicio)");
                Console.WriteLine("0. Salir");
                Console.Write("Elige una opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                    opcion = -1;

                switch (opcion)
                {
                    case 1: Agregar(); break;
                    case 2: Buscar(); break;
                    case 3: Completar(); break;
                    case 4: Eliminar(); break;
                    case 5: lista.Mostrar(); break;
                    case 6: lista.MostrarInverso(); break;
                    case 0: Console.WriteLine("Saliendo..."); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }

            } while (opcion != 0);
        }

        static int LeerId()
        {
            int id;
            Console.Write("ID de la tarea: ");
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.Write("ID no válido, intenta de nuevo: ");
            }
            return id;
        }

        static void Agregar()
        {
            int id = LeerId();

            Console.Write("Descripción: ");
            string descripcion = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                Console.WriteLine("La descripción no puede estar vacía.");
                return;
            }

            if (!lista.insertar(new Tarea(id, descripcion)))
                Console.WriteLine("Ya existe una tarea con ese ID.");
            
            else
                Console.WriteLine("Tarea agregada.");
        }

        static void Buscar()
        {
            Tarea tarea = lista.Buscar(LeerId());

            if (tarea != null)
                Console.WriteLine(tarea);
            else
                Console.WriteLine("Tarea no encontrada.");
        }

        static void Completar()
        {
            if (lista.MarcarCompletada(LeerId()))
                Console.WriteLine("Tarea marcada como completada.");
            else
                Console.WriteLine("Tarea no encontrada.");
        }

        static void Eliminar()
        {
            if (lista.Eliminar(LeerId()))
                Console.WriteLine("Tarea eliminada.");
            else
                Console.WriteLine("Tarea no encontrada.");
        }
    }

    internal class Tarea
    {
        public int id { get; private set; }
        public string Descripcion { get; private set; }
        public bool Completada { get; set; }

        public Tarea(int id, string descripcion)
        {
            this.id = id;
            Descripcion = descripcion;
        }

        public override string ToString()
        {
            string estado = Completada ? "Completada" : "Pendiente";
            return $"ID: {id} - {Descripcion} ({estado})";
        }
    }
}
