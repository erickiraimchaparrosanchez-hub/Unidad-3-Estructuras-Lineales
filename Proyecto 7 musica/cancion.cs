using System;

namespace Proyecto7_Musica
{
    public class Cancion
    {
        public string Titulo { get; set; }
        public string Artista { get; set; }

        public Cancion(string titulo, string artista)
        {
            Titulo = titulo;
            Artista = artista;
        }

        public override string ToString()
        {
            return $"{Titulo} - {Artista}";
        }
    }
}
