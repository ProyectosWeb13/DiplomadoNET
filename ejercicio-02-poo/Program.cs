// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Enum de Prioridades (Requisito Técnico)
public enum Prioridad
{
    Baja,
    Media,
    Alta,
    Critica
}

// 2. Interfaz IExportable (Requisito 3)
public interface IExportable
{
    string Exportar();
}

// Clase auxiliar para manejar la información de las categorías
public class Categoria
{
    public string Nombre { get; set; }
    public string Color { get; set; }
    public string Descripcion { get; set; }

    public Categoria(string nombre, string color, string descripcion)
    {
        Nombre = nombre;
        Color = color;
        Descripcion = descripcion;
    }
}

// 3. Clase Base Tarea que implementa IExportable (Requisito 2 y 3)
public class Tarea : IExportable
{
    // Contador estático para simular el autoincremental de la BD
    private static int contadorId = 1;

    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public Prioridad Prioridad { get; set; }
    public string Categoria { get; set; }
    public bool Completada { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Propiedad discriminadora para facilitar la serialización en JSON
    public string TipoTarea { get; set; } = "Simple";

    // Constructor por defecto necesario para la deserialización de JSON
    public Tarea() { }

    public Tarea(string titulo, string descripcion, Prioridad prioridad, string categoria)
    {
        Id = contadorId++;
        Titulo = titulo;
        Descripcion = descripcion;
        Prioridad = prioridad;
        Categoria = categoria;
        Completada = false;
        FechaCreacion = DateTime.Now;
    }

    // Permite ajustar el contador de IDs al cargar desde JSON para evitar duplicados
    public static void ActualizarContadorId(int ultimoId)
    {
        if (ultimoId >= contadorId)
        {
            contadorId = ultimoId + 1;
        }
    }

    // Método virtual para permitir sobreescritura (Polimorfismo)
    public virtual void MostrarInfo()
    {
        string estado = Completada ? "[x] Completada" : "[ ] Pendiente";
        Console.WriteLine($"ID: {Id} | Titulo: {Titulo} | Categoria: {Categoria} | Prioridad: {Prioridad} | Estado: {estado}");
        Console.WriteLine($"   Descripcion: {Descripcion}");
        Console.WriteLine($"   Fecha Creacion: {FechaCreacion.ToShortDateString()}");
    }

    // Implementación del método de la interfaz IExportable
    public string Exportar()
    {
        return $"{Id}|{Titulo}|{Prioridad}|{Completada}";
    }
}

// 4. Clase Derivada TareaConVencimiento (Hereda de Tarea)
public class TareaConVencimiento : Tarea
{
    public DateTime FechaVencimiento { get; set; }

    // Propiedad calculada en tiempo real según sugerencia de la guía
    [JsonIgnore]
    public int DiasRestantes
    {
        get
        {
            TimeSpan diferencia = FechaVencimiento.Date - DateTime.Now.Date;
            return diferencia.Days;
        }
    }

    // Constructor por defecto para JSON
    public TareaConVencimiento()
    {
        TipoTarea = "ConVencimiento";
    }

    // Constructor que llama a base() cumpliendo el requisito técnico
    public TareaConVencimiento(string titulo, string descripcion, Prioridad prioridad, string categoria, DateTime fechaVencimiento)
        : base(titulo, descripcion, prioridad, categoria)
    {
        FechaVencimiento = fechaVencimiento;
        TipoTarea = "ConVencimiento";
    }

    // Sobreescritura del método para mostrar días restantes (Polimorfismo)
    public override void MostrarInfo()
    {
        base.MostrarInfo();
        string estadoVencimiento = DiasRestantes < 0 ? "VENCIDA" : $"{DiasRestantes} dias restantes";
        Console.WriteLine($"   Fecha Vencimiento: {FechaVencimiento.ToShortDateString()} ({estadoVencimiento})");
    }
}

// 5. Clase GestorTareas (Requisito 4)
public class GestorTareas
{
    private List<Tarea> tareas = new List<Tarea>();

    public void Agregar(Tarea tarea)
    {
        tareas.Add(tarea);
    }

    public void Completar(int id)
    {
        var tarea = tareas.FirstOrDefault(t => t.Id == id);
        if (tarea != null)
        {
            tarea.Completada = true;
            Console.WriteLine("Tarea marcada como completada con exito.");
        }
        else
        {
            Console.WriteLine("No se encontro ninguna tarea con ese ID.");
        }
    }

    public List<Tarea> ObtenerTodas()
    {
        return tareas;
    }

    public List<Tarea> ListarPorCategoria(string categoria)
    {
        return tareas.Where(t => t.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public List<Tarea> ListarPorPrioridad(Prioridad prioridad)
    {
        return tareas.Where(t => t.Prioridad == prioridad).ToList();
    }

    public List<Tarea> ObtenerVencidas()
    {
        return tareas
            .OfType<TareaConVencimiento>()
            .Where(t => DateTime.Compare(t.FechaVencimiento.Date, DateTime.Now.Date) < 0 && !t.Completada)
            .Cast<Tarea>()
            .ToList();
    }

    public void Eliminar(int id)
    {
        var tarea = tareas.FirstOrDefault(t => t.Id == id);
        if (tarea != null)
        {
            tareas.Remove(tarea);
            Console.WriteLine("Tarea eliminada correctamente.");
        }
        else
        {
            Console.WriteLine("No se encontro ninguna tarea con ese ID.");
        }
    }

    // Persistencia: Guardar en JSON (Requisito 6)
    public void GuardarEnJSON(string archivo)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            
            // Creamos una lista de objetos anonimos para que el JSON conserve las propiedades de la hija
            var listaParaGuardar = tareas.Select<Tarea, object>(t =>
            {
                if (t is TareaConVencimiento tv)
                {
                    return new
                    {
                        tv.Id,
                        tv.Titulo,
                        tv.Descripcion,
                        tv.Prioridad,
                        tv.Categoria,
                        tv.Completada,
                        tv.FechaCreacion,
                        tv.TipoTarea,
                        tv.FechaVencimiento
                    };
                }
                return new
                {
                    t.Id,
                    t.Titulo,
                    t.Descripcion,
                    t.Prioridad,
                    t.Categoria,
                    t.Completada,
                    t.FechaCreacion,
                    t.TipoTarea
                };
            }).ToList();

            string jsonString = JsonSerializer.Serialize(listaParaGuardar, options);
            File.WriteAllText(archivo, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar el archivo JSON: {ex.Message}");
        }
    }

    // Persistencia: Cargar de JSON con control de excepciones (Requisito 6)
    public List<Tarea> CargarDeJSON(string archivo)
    {
        tareas.Clear();

        try
        {
            if (!File.Exists(archivo))
            {
                return tareas;
            }

            string jsonString = File.ReadAllText(archivo);
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                int maxId = 0;
                foreach (JsonElement element in doc.RootElement.EnumerateArray())
                {
                    string tipo = element.GetProperty("TipoTarea").GetString();

                    if (tipo == "ConVencimiento")
                    {
                        TareaConVencimiento tv = JsonSerializer.Deserialize<TareaConVencimiento>(element.GetRawText());
                        tareas.Add(tv);
                        if (tv.Id > maxId) maxId = tv.Id;
                    }
                    else
                    {
                        Tarea t = JsonSerializer.Deserialize<Tarea>(element.GetRawText());
                        tareas.Add(t);
                        if (t.Id > maxId) maxId = t.Id;
                    }
                }
                Tarea.ActualizarContadorId(maxId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: No se pudieron cargar los datos ({ex.Message}). Se iniciara con una lista vacia.");
            tareas = new List<Tarea>();
        }

        return tareas;
    }
}

// 6. Programa Principal y Menú
class Program
{
    static GestorTareas gestor = new GestorTareas();
    const string ARCHIVO_DATOS = "tareas.json";

    static void Main(string[] args)
    {
        // Cargar datos guardados al iniciar el sistema
        gestor.CargarDeJSON(ARCHIVO_DATOS);

        string opcion;

        do
        {
            Console.Clear();
            Console.WriteLine("=== GESTOR DE TAREAS ===");
            Console.WriteLine("1. Agregar tarea");
            Console.WriteLine("2. Listar todas");
            Console.WriteLine("3. Listar por categoria");
            Console.WriteLine("4. Listar por prioridad");
            Console.WriteLine("5. Marcar como completada");
            Console.WriteLine("6. Mostrar tareas vencidas");
            Console.WriteLine("7. Eliminar tarea");
            Console.WriteLine("8. Exportar a JSON (Guardar)");
            Console.WriteLine("9. Salir");
            Console.Write("\nSeleccione una opcion: ");

            opcion = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (opcion)
                {
                    case "1":
                        MenuAgregarTarea();
                        break;
                    case "2":
                        MostrarListaPolimorfica(gestor.ObtenerTodas(), "TODAS LAS TAREAS");
                        break;
                    case "3":
                        MenuFiltrarCategoria();
                        break;
                    case "4":
                        MenuFiltrarPrioridad();
                        break;
                    case "5":
                        MenuCompletar();
                        break;
                    case "6":
                        MostrarListaPolimorfica(gestor.ObtenerVencidas(), "TAREAS VENCIDAS");
                        break;
                    case "7":
                        MenuEliminar();
                        break;
                    case "8":
                        gestor.GuardarEnJSON(ARCHIVO_DATOS);
                        Console.WriteLine("Datos exportados e guardados exitosamente en 'tareas.json'.");
                        break;
                    case "9":
                        // Guardar automaticamente al salir como lo exige el enunciado
                        gestor.GuardarEnJSON(ARCHIVO_DATOS);
                        Console.WriteLine("¡Datos guardados! Gracias por usar el sistema.");
                        break;
                    default:
                        Console.WriteLine("Opcion no valida.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error insesperado: {ex.Message}");
            }

            if (opcion != "9")
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }

        } while (opcion != "9");
    }

    // Demostración explícita de Polimorfismo recorriendo la lista e invocando MostrarInfo()
    static void MostrarListaPolimorfica(List<Tarea> lista, string titulo)
    {
        Console.WriteLine($"--- {titulo} ---");
        if (lista.Count == 0)
        {
            Console.WriteLine("No hay tareas registradas en esta seccion.");
            return;
        }

        foreach (Tarea t in lista)
        {
            t.MostrarInfo(); // Llama a Tarea.MostrarInfo() o TareaConVencimiento.MostrarInfo() segun el objeto real
            Console.WriteLine(new string('-', 50));
        }
    }

    static void MenuAgregarTarea()
    {
        Console.WriteLine("--- NUEVA TAREA ---");
        Console.Write("Titulo: ");
        string titulo = Console.ReadLine();

        Console.Write("Descripcion: ");
        string desc = Console.ReadLine();

        Console.Write("Categoria (ej: Estudio, Trabajo, Personal): ");
        string cat = Console.ReadLine();

        Console.WriteLine("Prioridad: 0. Baja | 1. Media | 2. Alta | 3. Critica");
        Console.Write("Opcion prioridad: ");
        int pIndex = int.Parse(Console.ReadLine());
        Prioridad prio = (Prioridad)pIndex;

        Console.Write("¿Tiene fecha de vencimiento? (s/n): ");
        string esVenc = Console.ReadLine()?.ToLower();

        if (esVenc == "s")
        {
            Console.Write("Ingrese fecha de vencimiento (yyyy-mm-dd): ");
            DateTime fecha = DateTime.Parse(Console.ReadLine());

            TareaConVencimiento tv = new TareaConVencimiento(titulo, desc, prio, cat, fecha);
            gestor.Agregar(tv);
        }
        else
        {
            Tarea t = new Tarea(titulo, desc, prio, cat);
            gestor.Agregar(t);
        }

        Console.WriteLine("\n¡Tarea agregada exitosamente!");
    }

    static void MenuFiltrarCategoria()
    {
        Console.Write("Ingrese la categoria a buscar: ");
        string cat = Console.ReadLine();
        var filtradas = gestor.ListarPorCategoria(cat);
        MostrarListaPolimorfica(filtradas, $"TAREAS CATEGORIA: {cat.ToUpper()}");
    }

    static void MenuFiltrarPrioridad()
    {
        Console.WriteLine("Prioridades: 0. Baja | 1. Media | 2. Alta | 3. Critica");
        Console.Write("Ingrese el numero de prioridad: ");
        int pIndex = int.Parse(Console.ReadLine());
        Prioridad prio = (Prioridad)pIndex;

        var filtradas = gestor.ListarPorPrioridad(prio);
        MostrarListaPolimorfica(filtradas, $"TAREAS PRIORIDAD: {prio}");
    }

    static void MenuCompletar()
    {
        Console.Write("Ingrese el ID de la tarea a completar: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            gestor.Completar(id);
        }
    }

    static void MenuEliminar()
    {
        Console.Write("Ingrese el ID de la tarea a eliminar: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            gestor.Eliminar(id);
        }
    }
}
