using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Proyecto10_Pedidos
{
    
    public class Nodo<T>
    {
        public T Dato;
        public Nodo<T> Siguiente;
        public Nodo(T dato) { Dato = dato; }
    }

    public class ListaEnlazada<T> : IEnumerable<T> where T : class
    {
        private Nodo<T> cabeza;
        private Nodo<T> ultimo;
        public int Cantidad { get; private set; }

        public void AgregarAlFinal(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (cabeza == null) { cabeza = nuevo; ultimo = nuevo; }
            else { ultimo.Siguiente = nuevo; ultimo = nuevo; }
            Cantidad++;
        }

        public T Buscar(Func<T, bool> condicion)
        {
            var actual = cabeza;
            while (actual != null)
            {
                if (condicion(actual.Dato)) return actual.Dato;
                actual = actual.Siguiente;
            }
            return null;
        }

        public bool Eliminar(Func<T, bool> condicion)
        {
            Nodo<T> actual = cabeza, anterior = null;
            while (actual != null)
            {
                if (condicion(actual.Dato))
                {
                    if (anterior == null) cabeza = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    if (actual == ultimo) ultimo = anterior;
                    Cantidad--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var actual = cabeza;
            while (actual != null) { yield return actual.Dato; actual = actual.Siguiente; }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Cola<T> : IEnumerable<T> where T : class
    {
        private Nodo<T> frente;
        private Nodo<T> final;
        public int Cantidad { get; private set; }

        public void Encolar(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (final == null) { frente = nuevo; final = nuevo; }
            else { final.Siguiente = nuevo; final = nuevo; }
            Cantidad++;
        }

        public T Desencolar()
        {
            if (frente == null) return null;
            var dato = frente.Dato;
            frente = frente.Siguiente;
            if (frente == null) final = null;
            Cantidad--;
            return dato;
        }

        public T Frente() => frente == null ? null : frente.Dato;

        public bool Eliminar(Func<T, bool> condicion)
        {
            Nodo<T> actual = frente, anterior = null;
            while (actual != null)
            {
                if (condicion(actual.Dato))
                {
                    if (anterior == null) frente = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    if (actual == final) final = anterior;
                    Cantidad--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var actual = frente;
            while (actual != null) { yield return actual.Dato; actual = actual.Siguiente; }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class ColaPrioridad<T> : IEnumerable<T> where T : class
    {
        private class NodoP
        {
            public T Dato;
            public int Prioridad;
            public NodoP Siguiente;
            public NodoP(T dato, int prioridad) { Dato = dato; Prioridad = prioridad; }
        }

        private NodoP cabeza;
        public int Cantidad { get; private set; }

        public void Encolar(T dato, int prioridad)
        {
            var nuevo = new NodoP(dato, prioridad);
            if (cabeza == null || prioridad < cabeza.Prioridad)
            {
                nuevo.Siguiente = cabeza;
                cabeza = nuevo;
            }
            else
            {
                var actual = cabeza;
                while (actual.Siguiente != null && actual.Siguiente.Prioridad <= prioridad)
                    actual = actual.Siguiente;
                nuevo.Siguiente = actual.Siguiente;
                actual.Siguiente = nuevo;
            }
            Cantidad++;
        }

        public T Desencolar()
        {
            if (cabeza == null) return null;
            var dato = cabeza.Dato;
            cabeza = cabeza.Siguiente;
            Cantidad--;
            return dato;
        }

        public bool Eliminar(Func<T, bool> condicion)
        {
            NodoP actual = cabeza, anterior = null;
            while (actual != null)
            {
                if (condicion(actual.Dato))
                {
                    if (anterior == null) cabeza = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    Cantidad--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var actual = cabeza;
            while (actual != null) { yield return actual.Dato; actual = actual.Siguiente; }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Pila<T> : IEnumerable<T> where T : class
    {
        private Nodo<T> tope;
        public int Cantidad { get; private set; }

        public void Push(T dato)
        {
            var nuevo = new Nodo<T>(dato) { Siguiente = tope };
            tope = nuevo;
            Cantidad++;
        }

        public T Pop()
        {
            if (tope == null) return null;
            var dato = tope.Dato;
            tope = tope.Siguiente;
            Cantidad--;
            return dato;
        }

        public T Peek() => tope == null ? null : tope.Dato;

        public IEnumerator<T> GetEnumerator()
        {
            var actual = tope;
            while (actual != null) { yield return actual.Dato; actual = actual.Siguiente; }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public enum Prioridad { Urgente = 1, Alta = 2, Normal = 3 }
    public enum Estado { Pendiente, Procesado, Cancelado }

    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Existencias { get; set; }
        public string Categoria { get; set; } = "";

        public override string ToString() =>
            $"[{Id}] {Nombre,-20} {Precio,10:C2}  Existencias: {Existencias,4}  Categoría: {Categoria}";
    }

    public class ItemPedido
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class Pedido
    {
        public int Numero { get; set; }
        public string Cliente { get; set; } = "";
        public DateTime Fecha { get; set; }
        public ListaEnlazada<ItemPedido> Productos { get; } = new ListaEnlazada<ItemPedido>();
        public Prioridad Prioridad { get; set; }
        public Estado Estado { get; set; } = Estado.Pendiente;

        public decimal Total
        {
            get
            {
                decimal total = 0;
                foreach (var item in Productos) total += item.Subtotal;
                return total;
            }
        }

        public void Mostrar()
        {
            Console.WriteLine($"Pedido #{Numero} | Cliente: {Cliente} | Fecha: {Fecha:dd/MM/yyyy HH:mm} | " +
                              $"Prioridad: {Prioridad} | Estado: {Estado}");
            foreach (var item in Productos)
                Console.WriteLine($"     - {item.Cantidad} x {item.Nombre} ({item.PrecioUnitario:C2}) = {item.Subtotal:C2}");
            Console.WriteLine($"     Total: {Total:C2}");
        }
    }

    public static class Entrada
    {
        public static string Texto(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("  El valor no puede estar vacío.");
            }
        }

        public static int Entero(string mensaje, int minimo = int.MinValue, int maximo = int.MaxValue)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out int n) && n >= minimo && n <= maximo) return n;
                Console.WriteLine("  Valor inválido, intenta de nuevo.");
            }
        }

        public static decimal Decimal(string mensaje, decimal minimo = 0)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal d)
                    && d >= minimo) return d;
                Console.WriteLine("  Valor inválido, intenta de nuevo.");
            }
        }
    }


    public class Tienda
    {
        private readonly ListaEnlazada<Producto> productos = new ListaEnlazada<Producto>();  
        private readonly Cola<Pedido> pendientes = new Cola<Pedido>();                       
        private readonly ColaPrioridad<Pedido> urgentes = new ColaPrioridad<Pedido>();      
        private readonly Pila<string> historial = new Pila<string>();                         
        private readonly ListaEnlazada<Pedido> todosLosPedidos = new ListaEnlazada<Pedido>(); 
        private int siguienteNumero = 1;

        private const int StockBajo = 5;

        private void Registrar(string operacion) =>
            historial.Push($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {operacion}");

        public void CargarDatosEjemplo()
        {
            productos.AgregarAlFinal(new Producto { Id = 1, Nombre = "Mouse inalámbrico", Precio = 250m, Existencias = 20, Categoria = "Accesorios" });
            productos.AgregarAlFinal(new Producto { Id = 2, Nombre = "Teclado mecánico", Precio = 899m, Existencias = 10, Categoria = "Accesorios" });
            productos.AgregarAlFinal(new Producto { Id = 3, Nombre = "Monitor 24\"", Precio = 2999m, Existencias = 4, Categoria = "Pantallas" });
            productos.AgregarAlFinal(new Producto { Id = 4, Nombre = "Memoria USB 64GB", Precio = 120m, Existencias = 50, Categoria = "Almacenamiento" });
        }

        public void RegistrarProducto()
        {
            Console.WriteLine("\n--- Registrar producto ---");
            int id = Entrada.Entero("ID: ", 1);
            if (productos.Buscar(p => p.Id == id) != null)
            {
                Console.WriteLine("Ya existe un producto con ese ID.");
                return;
            }
            var producto = new Producto
            {
                Id = id,
                Nombre = Entrada.Texto("Nombre: "),
                Precio = Entrada.Decimal("Precio: ", 0.01m),
                Existencias = Entrada.Entero("Existencias: ", 0),
                Categoria = Entrada.Texto("Categoría: ")
            };
            productos.AgregarAlFinal(producto);
            Registrar($"Producto registrado: {producto.Nombre} (ID {producto.Id})");
            Console.WriteLine("Producto registrado correctamente.");
        }

        public void BuscarProducto()
        {
            Console.WriteLine("\n--- Buscar producto ---");
            Console.WriteLine("1. Por ID");
            Console.WriteLine("2. Por nombre");
            int opcion = Entrada.Entero("Opción: ", 1, 2);
            bool encontrado = false;

            if (opcion == 1)
            {
                int id = Entrada.Entero("ID: ");
                var p = productos.Buscar(x => x.Id == id);
                if (p != null) { Console.WriteLine(p); encontrado = true; }
            }
            else
            {
                string texto = Entrada.Texto("Nombre (o parte): ");
                foreach (var p in productos)
                {
                    if (p.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine(p);
                        encontrado = true;
                    }
                }
            }
            if (!encontrado) Console.WriteLine("No se encontró el producto.");
        }

        public void EliminarProducto()
        {
            Console.WriteLine("\n--- Eliminar producto ---");
            int id = Entrada.Entero("ID del producto: ");
            var producto = productos.Buscar(p => p.Id == id);
            if (producto == null) { Console.WriteLine("Producto no encontrado."); return; }

            if (ProductoEnPedidosPendientes(id))
            {
                Console.WriteLine("No se puede eliminar: el producto está en pedidos pendientes.");
                return;
            }
            productos.Eliminar(p => p.Id == id);
            Registrar($"Producto eliminado: {producto.Nombre} (ID {id})");
            Console.WriteLine("Producto eliminado.");
        }

        private bool ProductoEnPedidosPendientes(int idProducto)
        {
            foreach (var pedido in pendientes)
                if (pedido.Productos.Buscar(i => i.IdProducto == idProducto) != null) return true;
            foreach (var pedido in urgentes)
                if (pedido.Productos.Buscar(i => i.IdProducto == idProducto) != null) return true;
            return false;
        }

        public void RegistrarPedido()
        {
            Console.WriteLine("\n--- Registrar pedido ---");
            if (productos.Cantidad == 0) { Console.WriteLine("No hay productos registrados."); return; }

            var pedido = new Pedido
            {
                Cliente = Entrada.Texto("Cliente: "),
                Fecha = DateTime.Now
            };

            Console.WriteLine("Prioridad: 1. Urgente   2. Alta   3. Normal");
            pedido.Prioridad = (Prioridad)Entrada.Entero("Opción: ", 1, 3);

            Console.WriteLine("\nAgrega productos (ID 0 para terminar):");
            while (true)
            {
                int id = Entrada.Entero("ID del producto: ", 0);
                if (id == 0) break;

                var producto = productos.Buscar(p => p.Id == id);
                if (producto == null) { Console.WriteLine("  Producto no encontrado."); continue; }

                int cantidad = Entrada.Entero($"Cantidad (disponibles: {producto.Existencias}): ", 1);
                var existente = pedido.Productos.Buscar(i => i.IdProducto == id);
                int yaPedido = existente?.Cantidad ?? 0;

                if (yaPedido + cantidad > producto.Existencias)
                {
                    Console.WriteLine("  Existencias insuficientes.");
                    continue;
                }

                if (existente != null) existente.Cantidad += cantidad;
                else pedido.Productos.AgregarAlFinal(new ItemPedido
                {
                    IdProducto = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            if (pedido.Productos.Cantidad == 0)
            {
                Console.WriteLine("El pedido no tiene productos; no se registró.");
                return;
            }

            foreach (var item in pedido.Productos)
            {
                var producto = productos.Buscar(p => p.Id == item.IdProducto);
                producto.Existencias -= item.Cantidad;
            }

            pedido.Numero = siguienteNumero++;
            todosLosPedidos.AgregarAlFinal(pedido);

            if (pedido.Prioridad == Prioridad.Normal) pendientes.Encolar(pedido);
            else urgentes.Encolar(pedido, (int)pedido.Prioridad);

            Registrar($"Pedido #{pedido.Numero} registrado ({pedido.Cliente}, {pedido.Prioridad}, {pedido.Total:C2})");
            Console.WriteLine($"\nPedido #{pedido.Numero} registrado. Total: {pedido.Total:C2}");
        }
 
        public void CancelarPedido()
        {
            Console.WriteLine("\n--- Cancelar pedido ---");
            int numero = Entrada.Entero("Número de pedido: ");
            var pedido = todosLosPedidos.Buscar(p => p.Numero == numero);

            if (pedido == null) { Console.WriteLine("Pedido no encontrado."); return; }
            if (pedido.Estado != Estado.Pendiente)
            {
                Console.WriteLine($"No se puede cancelar: el pedido ya está {pedido.Estado}.");
                return;
            }

            if (!pendientes.Eliminar(p => p.Numero == numero))
                urgentes.Eliminar(p => p.Numero == numero);

            foreach (var item in pedido.Productos)
            {
                var producto = productos.Buscar(p => p.Id == item.IdProducto);
                if (producto != null) producto.Existencias += item.Cantidad;
            }

            pedido.Estado = Estado.Cancelado;
            Registrar($"Pedido #{numero} cancelado");
            Console.WriteLine("Pedido cancelado y existencias restauradas.");
        }

        public void ProcesarPedido()
        {
            Console.WriteLine("\n--- Procesar pedido ---");
            var pedido = pendientes.Desencolar();
            if (pedido == null)
            {
                Console.WriteLine("No hay pedidos normales pendientes.");
                if (urgentes.Cantidad > 0) Console.WriteLine("Hay pedidos urgentes; usa la opción 7.");
                return;
            }
            FinalizarPedido(pedido);
        }

        public void AtenderPedidoUrgente()
        {
            Console.WriteLine("\n--- Atender pedido urgente ---");
            var pedido = urgentes.Desencolar();
            if (pedido == null) { Console.WriteLine("No hay pedidos urgentes pendientes."); return; }
            FinalizarPedido(pedido);
        }

        private void FinalizarPedido(Pedido pedido)
        {
            pedido.Estado = Estado.Procesado;
            Registrar($"Pedido #{pedido.Numero} procesado ({pedido.Prioridad})");
            Console.WriteLine("Pedido procesado:");
            pedido.Mostrar();
        }

        public void MostrarPedidosPendientes()
        {
            Console.WriteLine("\n--- Pedidos pendientes ---");
            Console.WriteLine($"\nUrgentes / alta prioridad ({urgentes.Cantidad}):");
            if (urgentes.Cantidad == 0) Console.WriteLine("  (ninguno)");
            foreach (var p in urgentes) p.Mostrar();

            Console.WriteLine($"\nNormales ({pendientes.Cantidad}):");
            if (pendientes.Cantidad == 0) Console.WriteLine("  (ninguno)");
            foreach (var p in pendientes) p.Mostrar();
        }

        public void MostrarHistorial()
        {
            Console.WriteLine("\n--- Historial de operaciones ---");
            if (historial.Cantidad == 0) { Console.WriteLine("Sin operaciones registradas."); return; }
            foreach (var op in historial) Console.WriteLine("  " + op);
        }

        public void ConsultarInventario()
        {
            Console.WriteLine("\n--- Inventario ---");
            if (productos.Cantidad == 0) { Console.WriteLine("No hay productos."); return; }

            decimal valorTotal = 0;
            foreach (var p in productos)
            {
                string alerta = p.Existencias <= StockBajo ? "  <-- STOCK BAJO" : "";
                Console.WriteLine(p + alerta);
                valorTotal += p.Precio * p.Existencias;
            }
            Console.WriteLine($"\nProductos: {productos.Cantidad} | Valor del inventario: {valorTotal:C2}");
        }

        public void BuscarCliente()
        {
            Console.WriteLine("\n--- Buscar cliente ---");
            string texto = Entrada.Texto("Nombre del cliente (o parte): ");
            bool encontrado = false;
            foreach (var p in todosLosPedidos)
            {
                if (p.Cliente.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    p.Mostrar();
                    encontrado = true;
                }
            }
            if (!encontrado) Console.WriteLine("No hay pedidos de ese cliente.");
        }


        public void MostrarEstadisticas()
        {
            Console.WriteLine("\n--- Estadísticas ---");
            int pend = 0, proc = 0, canc = 0;
            decimal ventas = 0;
            var vendidos = new Dictionary<int, (string Nombre, int Cantidad)>();

            foreach (var p in todosLosPedidos)
            {
                switch (p.Estado)
                {
                    case Estado.Pendiente: pend++; break;
                    case Estado.Cancelado: canc++; break;
                    case Estado.Procesado:
                        proc++;
                        ventas += p.Total;
                        foreach (var item in p.Productos)
                        {
                            vendidos.TryGetValue(item.IdProducto, out var acum);
                            vendidos[item.IdProducto] = (item.Nombre, acum.Cantidad + item.Cantidad);
                        }
                        break;
                }
            }

            Console.WriteLine($"Pedidos registrados: {todosLosPedidos.Cantidad}");
            Console.WriteLine($"  Pendientes: {pend} | Procesados: {proc} | Cancelados: {canc}");
            Console.WriteLine($"Ventas totales (procesados): {ventas:C2}");
            if (proc > 0) Console.WriteLine($"Ticket promedio: {ventas / proc:C2}");

            string masVendido = "(sin ventas)";
            int max = 0;
            foreach (var kv in vendidos)
                if (kv.Value.Cantidad > max) { max = kv.Value.Cantidad; masVendido = $"{kv.Value.Nombre} ({max} unidades)"; }
            Console.WriteLine($"Producto más vendido: {masVendido}");

            Console.WriteLine("Productos con stock bajo:");
            bool hay = false;
            foreach (var p in productos)
                if (p.Existencias <= StockBajo) { Console.WriteLine($"  - {p.Nombre}: {p.Existencias}"); hay = true; }
            if (!hay) Console.WriteLine("  (ninguno)");
        }
    }
    class Program
    {
        static void Main()
        {
            var tienda = new Tienda();
            tienda.CargarDatosEjemplo(); 

            int opcion;
            do
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine("   TIENDA EN LÍNEA - GESTIÓN DE PEDIDOS");
                Console.WriteLine("==========================================");
                Console.WriteLine(" 1. Registrar producto");
                Console.WriteLine(" 2. Buscar producto");
                Console.WriteLine(" 3. Eliminar producto");
                Console.WriteLine(" 4. Registrar pedido");
                Console.WriteLine(" 5. Cancelar pedido");
                Console.WriteLine(" 6. Procesar pedido");
                Console.WriteLine(" 7. Atender pedido urgente");
                Console.WriteLine(" 8. Mostrar pedidos pendientes");
                Console.WriteLine(" 9. Mostrar historial");
                Console.WriteLine("10. Consultar inventario");
                Console.WriteLine("11. Buscar cliente");
                Console.WriteLine("12. Mostrar estadísticas");
                Console.WriteLine(" 0. Salir");
                opcion = Entrada.Entero("Opción: ", 0, 12);

                switch (opcion)
                {
                    case 1: tienda.RegistrarProducto(); break;
                    case 2: tienda.BuscarProducto(); break;
                    case 3: tienda.EliminarProducto(); break;
                    case 4: tienda.RegistrarPedido(); break;
                    case 5: tienda.CancelarPedido(); break;
                    case 6: tienda.ProcesarPedido(); break;
                    case 7: tienda.AtenderPedidoUrgente(); break;
                    case 8: tienda.MostrarPedidosPendientes(); break;
                    case 9: tienda.MostrarHistorial(); break;
                    case 10: tienda.ConsultarInventario(); break;
                    case 11: tienda.BuscarCliente(); break;
                    case 12: tienda.MostrarEstadisticas(); break;
                    case 0: Console.WriteLine("¡Hasta luego!"); break;
                }
            } while (opcion != 0);
        }
    }
}
