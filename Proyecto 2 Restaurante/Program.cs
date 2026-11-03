using System;
using System.Collections.Generic;
using System.Text;

namespace AsignacionMesas
{
    // ===================== ESTRUCTURAS DE DATOS =====================

    class Nodo<T>
    {
        public T Dato;
        public Nodo<T> Siguiente;
        public Nodo(T dato) { Dato = dato; }
    }

    // Lista enlazada simple
    class ListaEnlazada<T> where T : class
    {
        private Nodo<T> cabeza;
        public int Cantidad { get; private set; }

        public void Agregar(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (cabeza == null) cabeza = nuevo;
            else
            {
                var actual = cabeza;
                while (actual.Siguiente != null) actual = actual.Siguiente;
                actual.Siguiente = nuevo;
            }
            Cantidad++;
        }

        public T Buscar(Predicate<T> condicion)
        {
            for (var n = cabeza; n != null; n = n.Siguiente)
                if (condicion(n.Dato)) return n.Dato;
            return null;
        }

        public bool Eliminar(Predicate<T> condicion)
        {
            Nodo<T> anterior = null;
            for (var n = cabeza; n != null; anterior = n, n = n.Siguiente)
            {
                if (!condicion(n.Dato)) continue;
                if (anterior == null) cabeza = n.Siguiente;
                else anterior.Siguiente = n.Siguiente;
                Cantidad--;
                return true;
            }
            return false;
        }

        public IEnumerable<T> Recorrer()
        {
            for (var n = cabeza; n != null; n = n.Siguiente)
                yield return n.Dato;
        }
    }

    // Cola (FIFO) implementada con nodos
    class Cola<T> where T : class
    {
        private Nodo<T> frente, final;
        public int Cantidad { get; private set; }

        public T Frente => frente?.Dato;

        public void Encolar(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (final == null) frente = final = nuevo;
            else { final.Siguiente = nuevo; final = nuevo; }
            Cantidad++;
        }

        public T Desencolar()
        {
            if (frente == null) return null;
            T dato = frente.Dato;
            frente = frente.Siguiente;
            if (frente == null) final = null;
            Cantidad--;
            return dato;
        }

        // Elimina un elemento de cualquier posición (para cancelaciones)
        public bool Eliminar(Predicate<T> condicion)
        {
            Nodo<T> anterior = null;
            for (var n = frente; n != null; anterior = n, n = n.Siguiente)
            {
                if (!condicion(n.Dato)) continue;
                if (anterior == null) frente = n.Siguiente;
                else anterior.Siguiente = n.Siguiente;
                if (n == final) final = anterior;
                Cantidad--;
                return true;
            }
            return false;
        }

        public bool Contiene(Predicate<T> condicion)
        {
            for (var n = frente; n != null; n = n.Siguiente)
                if (condicion(n.Dato)) return true;
            return false;
        }

        public IEnumerable<T> Recorrer()
        {
            for (var n = frente; n != null; n = n.Siguiente)
                yield return n.Dato;
        }
    }

    // ===================== MODELOS =====================

    enum EstadoCliente { Registrado, EnEspera, Sentado }

    class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Personas { get; set; }      // 0 = aún no registrado
        public EstadoCliente Estado { get; set; } = EstadoCliente.Registrado;
        public int MesaAsignada { get; set; }  // 0 = sin mesa

        public override string ToString() =>
            $"#{Id} {Nombre} | Personas: {(Personas == 0 ? "-" : Personas.ToString())} | {Estado}" +
            (MesaAsignada > 0 ? $" (Mesa {MesaAsignada})" : "");
    }

    class Mesa
    {
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public Cliente Cliente { get; set; }
        public bool Ocupada => Cliente != null;
    }

    // ===================== PROGRAMA =====================

    class Program
    {
        static ListaEnlazada<Cliente> clientes = new ListaEnlazada<Cliente>();
        static ListaEnlazada<Mesa> mesas = new ListaEnlazada<Mesa>();
        static Cola<Cliente> listaEspera = new Cola<Cliente>();
        static int siguienteId = 1;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            CargarMesas();

            int opcion;
            do
            {
                MostrarMenu();
                opcion = LeerEntero("Opción: ", 0, 10);
                Console.WriteLine();
                switch (opcion)
                {
                    case 1: RegistrarCliente(); break;
                    case 2: RegistrarPersonas(); break;
                    case 3: AgregarAListaEspera(); break;
                    case 4: AsignarMesa(); break;
                    case 5: CancelarReservacion(); break;
                    case 6: BuscarCliente(); break;
                    case 7: EliminarCliente(); break;
                    case 8: MostrarListaEspera(); break;
                    case 9: MostrarMesas(false); break;
                    case 10: MostrarMesas(true); break;
                }
                if (opcion != 0)
                {
                    Console.WriteLine("\nPresiona una tecla para continuar...");
                    Console.ReadKey(true);
                }
            } while (opcion != 0);

            Console.WriteLine("¡Hasta luego!");
        }

        static void CargarMesas()
        {
            int[] capacidades = { 2, 2, 2, 4, 4, 4, 4, 6, 6, 8 };
            for (int i = 0; i < capacidades.Length; i++)
                mesas.Agregar(new Mesa { Numero = i + 1, Capacidad = capacidades[i] });
        }

        static void MostrarMenu()
        {
            Console.Clear();
            Console.WriteLine("===== ASIGNACIÓN DE MESAS - RESTAURANTE =====");
            Console.WriteLine(" 1. Registrar cliente");
            Console.WriteLine(" 2. Registrar número de personas");
            Console.WriteLine(" 3. Agregar cliente a lista de espera");
            Console.WriteLine(" 4. Asignar mesa");
            Console.WriteLine(" 5. Cancelar reservación");
            Console.WriteLine(" 6. Buscar cliente");
            Console.WriteLine(" 7. Eliminar cliente");
            Console.WriteLine(" 8. Mostrar lista de espera");
            Console.WriteLine(" 9. Mostrar mesas disponibles");
            Console.WriteLine("10. Mostrar mesas ocupadas");
            Console.WriteLine(" 0. Salir");
            Console.WriteLine("=============================================");
        }

        // 1. Registrar cliente
        static void RegistrarCliente()
        {
            string nombre = LeerTexto("Nombre del cliente: ");
            var c = new Cliente { Id = siguienteId++, Nombre = nombre };
            clientes.Agregar(c);
            Console.WriteLine($"Cliente registrado con ID {c.Id}.");
        }

        // 2. Registrar número de personas
        static void RegistrarPersonas()
        {
            var c = PedirCliente();
            if (c == null) return;
            if (c.Estado != EstadoCliente.Registrado)
            {
                Console.WriteLine("Solo se puede cambiar el número de personas de un cliente sin espera ni mesa.");
                return;
            }
            c.Personas = LeerEntero("Número de personas (1-8): ", 1, 8);
            Console.WriteLine("Número de personas registrado.");
        }

        // 3. Agregar a lista de espera (Cola)
        static void AgregarAListaEspera()
        {
            var c = PedirCliente();
            if (c == null) return;
            if (c.Personas == 0)
            {
                Console.WriteLine("Primero registra el número de personas (opción 2).");
                return;
            }
            if (c.Estado != EstadoCliente.Registrado)
            {
                Console.WriteLine($"El cliente ya está: {c.Estado}.");
                return;
            }
            listaEspera.Encolar(c);
            c.Estado = EstadoCliente.EnEspera;
            Console.WriteLine($"{c.Nombre} agregado a la lista de espera (posición {listaEspera.Cantidad}).");
        }

        // 4. Asignar mesa al primero de la cola
        static void AsignarMesa()
        {
            var c = listaEspera.Frente;
            if (c == null)
            {
                Console.WriteLine("La lista de espera está vacía.");
                return;
            }

            // Mesa libre más pequeña que alcance para el grupo
            Mesa mejor = null;
            foreach (var m in mesas.Recorrer())
                if (!m.Ocupada && m.Capacidad >= c.Personas &&
                    (mejor == null || m.Capacidad < mejor.Capacidad))
                    mejor = m;

            if (mejor == null)
            {
                Console.WriteLine($"No hay mesa disponible para {c.Personas} personas ({c.Nombre}).");
                return;
            }

            listaEspera.Desencolar();
            mejor.Cliente = c;
            c.Estado = EstadoCliente.Sentado;
            c.MesaAsignada = mejor.Numero;
            Console.WriteLine($"Mesa {mejor.Numero} (cap. {mejor.Capacidad}) asignada a {c.Nombre}.");
        }

        // 5. Cancelar reservación
        static void CancelarReservacion()
        {
            var c = PedirCliente();
            if (c == null) return;

            switch (c.Estado)
            {
                case EstadoCliente.EnEspera:
                    listaEspera.Eliminar(x => x.Id == c.Id);
                    break;
                case EstadoCliente.Sentado:
                    LiberarMesa(c);
                    break;
                default:
                    Console.WriteLine("El cliente no tiene una reservación activa.");
                    return;
            }
            c.Estado = EstadoCliente.Registrado;
            Console.WriteLine($"Reservación de {c.Nombre} cancelada.");
        }

        // 6. Buscar cliente por ID o nombre
        static void BuscarCliente()
        {
            string entrada = LeerTexto("ID o nombre a buscar: ");
            bool encontrado = false;

            if (int.TryParse(entrada, out int id))
            {
                var c = clientes.Buscar(x => x.Id == id);
                if (c != null) { Console.WriteLine(c); encontrado = true; }
            }
            else
            {
                foreach (var c in clientes.Recorrer())
                    if (c.Nombre.IndexOf(entrada, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine(c);
                        encontrado = true;
                    }
            }
            if (!encontrado) Console.WriteLine("No se encontró ningún cliente.");
        }

        // 7. Eliminar cliente del sistema
        static void EliminarCliente()
        {
            var c = PedirCliente();
            if (c == null) return;

            if (c.Estado == EstadoCliente.EnEspera) listaEspera.Eliminar(x => x.Id == c.Id);
            else if (c.Estado == EstadoCliente.Sentado) LiberarMesa(c);

            clientes.Eliminar(x => x.Id == c.Id);
            Console.WriteLine($"Cliente {c.Nombre} eliminado del sistema.");
        }

        // 8. Mostrar lista de espera
        static void MostrarListaEspera()
        {
            Console.WriteLine("--- LISTA DE ESPERA ---");
            if (listaEspera.Cantidad == 0) { Console.WriteLine("(vacía)"); return; }
            int pos = 1;
            foreach (var c in listaEspera.Recorrer())
                Console.WriteLine($"{pos++}. {c.Nombre} (ID {c.Id}) - {c.Personas} personas");
        }

        // 9 y 10. Mostrar mesas disponibles u ocupadas
        static void MostrarMesas(bool ocupadas)
        {
            Console.WriteLine(ocupadas ? "--- MESAS OCUPADAS ---" : "--- MESAS DISPONIBLES ---");
            bool hay = false;
            foreach (var m in mesas.Recorrer())
            {
                if (m.Ocupada != ocupadas) continue;
                hay = true;
                Console.WriteLine(ocupadas
                    ? $"Mesa {m.Numero} (cap. {m.Capacidad}) - {m.Cliente.Nombre}, {m.Cliente.Personas} personas"
                    : $"Mesa {m.Numero} (cap. {m.Capacidad})");
            }
            if (!hay) Console.WriteLine("(ninguna)");
        }

        static void LiberarMesa(Cliente c)
        {
            var mesa = mesas.Buscar(m => m.Cliente != null && m.Cliente.Id == c.Id);
            if (mesa != null) mesa.Cliente = null;
            c.MesaAsignada = 0;
        }

        static Cliente PedirCliente()
        {
            if (clientes.Cantidad == 0)
            {
                Console.WriteLine("No hay clientes registrados.");
                return null;
            }
            int id = LeerEntero("ID del cliente: ", 1, int.MaxValue);
            var c = clientes.Buscar(x => x.Id == id);
            if (c == null) Console.WriteLine("Cliente no encontrado.");
            return c;
        }

        static string LeerTexto(string mensaje)
        {
            string s;
            do
            {
                Console.Write(mensaje);
                s = Console.ReadLine()?.Trim();
            } while (string.IsNullOrEmpty(s));
            return s;
        }

        static int LeerEntero(string mensaje, int min, int max)
        {
            int n;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out n) && n >= min && n <= max) return n;
                Console.WriteLine($"Ingresa un número entre {min} y {max}.");
            }
        }
    }
}
