using System;
using System.Text;

namespace Proyecto9_Procesos
{
    enum EstadoProceso { NUEVO, LISTO, EJECUTANDO, BLOQUEADO, TERMINADO }

    class Proceso
    {
        public int PID { get; }
        public string Nombre { get; }
        public int Prioridad { get; }
        public EstadoProceso Estado { get; set; }
        public int TiempoEjecucion { get; }
        public int TiempoRestante { get; set; }

        public Proceso(int pid, string nombre, int prioridad, int tiempo)
        {
            PID = pid;
            Nombre = nombre;
            Prioridad = prioridad;
            TiempoEjecucion = tiempo;
            TiempoRestante = tiempo;
            Estado = EstadoProceso.NUEVO;
        }

        public string Codigo => $"P{PID:D2}";

        public override string ToString() =>
            $"{Codigo} | {Nombre,-12} | Prioridad: {Prioridad} | {Estado,-10} | Tiempo: {TiempoEjecucion} (restante {TiempoRestante})";
    }

    class Nodo
    {
        public Proceso Dato;
        public Nodo Siguiente;
        public Nodo(Proceso p) { Dato = p; }
    }

    // LISTA ENLAZADA: tabla general de procesos
    class ListaEnlazada
    {
        private Nodo cabeza;

        public void Agregar(Proceso p)
        {
            var nuevo = new Nodo(p);
            if (cabeza == null) { cabeza = nuevo; return; }
            var actual = cabeza;
            while (actual.Siguiente != null) actual = actual.Siguiente;
            actual.Siguiente = nuevo;
        }

        public Proceso Buscar(int pid)
        {
            for (var a = cabeza; a != null; a = a.Siguiente)
                if (a.Dato.PID == pid) return a.Dato;
            return null;
        }

        public bool Eliminar(int pid)
        {
            Nodo anterior = null, actual = cabeza;
            while (actual != null)
            {
                if (actual.Dato.PID == pid)
                {
                    if (anterior == null) cabeza = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public void Mostrar()
        {
            if (cabeza == null) { Console.WriteLine("No hay procesos registrados."); return; }
            for (var a = cabeza; a != null; a = a.Siguiente)
                Console.WriteLine(a.Dato);
        }
    }

    // COLA (enlazada): procesos BLOQUEADOS
    class Cola
    {
        private Nodo frente, final;

        public void Encolar(Proceso p)
        {
            var nuevo = new Nodo(p);
            if (final == null) { frente = final = nuevo; }
            else { final.Siguiente = nuevo; final = nuevo; }
        }

        public Proceso Desencolar()
        {
            if (frente == null) return null;
            var p = frente.Dato;
            frente = frente.Siguiente;
            if (frente == null) final = null;
            return p;
        }

        public bool Eliminar(int pid)
        {
            Nodo anterior = null, actual = frente;
            while (actual != null)
            {
                if (actual.Dato.PID == pid)
                {
                    if (anterior == null) frente = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    if (actual == final) final = anterior;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public string Texto()
        {
            if (frente == null) return "(vacía)";
            string s = "";
            for (var a = frente; a != null; a = a.Siguiente)
                s += a.Dato.Codigo + (a.Siguiente != null ? " → " : "");
            return s;
        }
    }

    // COLA CIRCULAR (arreglo): procesos LISTOS, planificación Round Robin
    class ColaCircular
    {
        private readonly Proceso[] datos;
        private int frente, cantidad;

        public ColaCircular(int capacidad) { datos = new Proceso[capacidad]; }

        public bool EstaVacia => cantidad == 0;
        public bool EstaLlena => cantidad == datos.Length;

        public bool Encolar(Proceso p)
        {
            if (EstaLlena) return false;
            datos[(frente + cantidad) % datos.Length] = p;
            cantidad++;
            return true;
        }

        public Proceso Desencolar()
        {
            if (EstaVacia) return null;
            var p = datos[frente];
            datos[frente] = null;
            frente = (frente + 1) % datos.Length;
            cantidad--;
            return p;
        }

        public bool Eliminar(int pid)
        {
            bool hallado = false;
            int n = cantidad;
            for (int i = 0; i < n; i++)
            {
                var p = Desencolar();
                if (p.PID == pid) hallado = true;
                else Encolar(p);
            }
            return hallado;
        }

        public string Texto()
        {
            if (EstaVacia) return "(vacía)";
            string s = "";
            for (int i = 0; i < cantidad; i++)
                s += datos[(frente + i) % datos.Length].Codigo + (i < cantidad - 1 ? " → " : "");
            return s;
        }
    }

    class GestorProcesos
    {
        private readonly ListaEnlazada tabla = new ListaEnlazada();
        private readonly ColaCircular listos = new ColaCircular(10);
        private readonly Cola bloqueados = new Cola();
        private int siguientePid = 1;
        public int Quantum { get; set; } = 3;

        public void Crear(string nombre, int prioridad, int tiempo)
        {
            if (listos.EstaLlena)
            {
                Console.WriteLine("La cola de listos está llena (máx. 10).");
                return;
            }
            var p = new Proceso(siguientePid++, nombre, prioridad, tiempo);
            tabla.Agregar(p);                 // NUEVO
            p.Estado = EstadoProceso.LISTO;   // admitido
            listos.Encolar(p);
            Console.WriteLine($"Proceso {p.Codigo} creado (NUEVO) y admitido a la cola de listos (LISTO).");
        }

        public void Eliminar(int pid)
        {
            var p = tabla.Buscar(pid);
            if (p == null) { Console.WriteLine("Proceso no encontrado."); return; }
            listos.Eliminar(pid);
            bloqueados.Eliminar(pid);
            tabla.Eliminar(pid);
            Console.WriteLine($"Proceso {p.Codigo} eliminado.");
        }

        public void EjecutarSiguiente()
        {
            var p = listos.Desencolar();
            if (p == null) { Console.WriteLine("No hay procesos listos para ejecutar."); return; }

            p.Estado = EstadoProceso.EJECUTANDO;
            int uso = Math.Min(Quantum, p.TiempoRestante);
            p.TiempoRestante -= uso;
            Console.WriteLine($"Ejecutando {p.Codigo} ({p.Nombre}) durante {uso} unidad(es)...");

            if (p.TiempoRestante == 0)
            {
                p.Estado = EstadoProceso.TERMINADO;
                Console.WriteLine($"{p.Codigo} ha TERMINADO.");
            }
            else
            {
                p.Estado = EstadoProceso.LISTO;
                listos.Encolar(p);
                Console.WriteLine($"{p.Codigo} vuelve a la cola de listos (restante {p.TiempoRestante}).");
            }
        }

        public void Bloquear(int pid)
        {
            var p = tabla.Buscar(pid);
            if (p == null) { Console.WriteLine("Proceso no encontrado."); return; }
            if (p.Estado != EstadoProceso.LISTO)
            {
                Console.WriteLine($"Solo se puede bloquear un proceso LISTO (estado actual: {p.Estado}).");
                return;
            }
            listos.Eliminar(pid);
            p.Estado = EstadoProceso.BLOQUEADO;
            bloqueados.Encolar(p);
            Console.WriteLine($"{p.Codigo} ahora está BLOQUEADO.");
        }

        public void Desbloquear(int pid)
        {
            var p = tabla.Buscar(pid);
            if (p == null) { Console.WriteLine("Proceso no encontrado."); return; }
            if (p.Estado != EstadoProceso.BLOQUEADO)
            {
                Console.WriteLine($"El proceso no está BLOQUEADO (estado actual: {p.Estado}).");
                return;
            }
            if (listos.EstaLlena) { Console.WriteLine("La cola de listos está llena."); return; }
            bloqueados.Eliminar(pid);
            p.Estado = EstadoProceso.LISTO;
            listos.Encolar(p);
            Console.WriteLine($"{p.Codigo} desbloqueado y enviado a la cola de listos.");
        }

        public void MostrarProcesos() => tabla.Mostrar();

        public void Buscar(int pid)
        {
            var p = tabla.Buscar(pid);
            Console.WriteLine(p == null ? "Proceso no encontrado." : p.ToString());
        }

        public void MostrarColas()
        {
            Console.WriteLine("COLA DE LISTOS (circular): " + listos.Texto());
            Console.WriteLine("COLA DE BLOQUEADOS:        " + bloqueados.Texto());
            Console.WriteLine("Quantum actual: " + Quantum);
        }

        public void EjecutarTodos()
        {
            if (listos.EstaVacia) { Console.WriteLine("No hay procesos listos."); return; }
            while (!listos.EstaVacia) EjecutarSiguiente();
        }
    }

    class Program
    {
        static int LeerEntero(string mensaje)
        {
            int valor;
            Console.Write(mensaje);
            while (!int.TryParse(Console.ReadLine(), out valor) || valor < 0)
                Console.Write("Valor inválido. " + mensaje);
            return valor;
        }

        static void AdministrarCola(GestorProcesos gestor)
        {
            int op;
            do
            {
                Console.WriteLine("\n--- ADMINISTRAR COLA DE PROCESOS ---");
                Console.WriteLine("1. Ver colas");
                Console.WriteLine("2. Ejecutar todos los listos (Round Robin)");
                Console.WriteLine("3. Cambiar quantum");
                Console.WriteLine("0. Volver");
                op = LeerEntero("Opción: ");
                switch (op)
                {
                    case 1: gestor.MostrarColas(); break;
                    case 2: gestor.EjecutarTodos(); break;
                    case 3:
                        int q = LeerEntero("Nuevo quantum: ");
                        if (q > 0) gestor.Quantum = q;
                        else Console.WriteLine("El quantum debe ser mayor que 0.");
                        break;
                }
            } while (op != 0);
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var gestor = new GestorProcesos();
            int opcion;

            do
            {
                Console.WriteLine("\n===== CONTROL DE PROCESOS =====");
                Console.WriteLine("1. Crear proceso");
                Console.WriteLine("2. Eliminar proceso");
                Console.WriteLine("3. Ejecutar proceso");
                Console.WriteLine("4. Bloquear proceso");
                Console.WriteLine("5. Desbloquear proceso");
                Console.WriteLine("6. Mostrar procesos");
                Console.WriteLine("7. Buscar proceso");
                Console.WriteLine("8. Administrar cola de procesos");
                Console.WriteLine("0. Salir");
                opcion = LeerEntero("Opción: ");

                switch (opcion)
                {
                    case 1:
                        Console.Write("Nombre: ");
                        string nombre = Console.ReadLine();
                        int prioridad = LeerEntero("Prioridad: ");
                        int tiempo = LeerEntero("Tiempo de ejecución: ");
                        if (tiempo == 0) Console.WriteLine("El tiempo debe ser mayor que 0.");
                        else gestor.Crear(nombre, prioridad, tiempo);
                        break;
                    case 2: gestor.Eliminar(LeerEntero("PID a eliminar: ")); break;
                    case 3: gestor.EjecutarSiguiente(); break;
                    case 4: gestor.Bloquear(LeerEntero("PID a bloquear: ")); break;
                    case 5: gestor.Desbloquear(LeerEntero("PID a desbloquear: ")); break;
                    case 6: gestor.MostrarProcesos(); break;
                    case 7: gestor.Buscar(LeerEntero("PID a buscar: ")); break;
                    case 8: AdministrarCola(gestor); break;
                    case 0: Console.WriteLine("Saliendo..."); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            } while (opcion != 0);
        }
    }
}
