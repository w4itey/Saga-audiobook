using System.Collections.Generic;

namespace Saga.Models
{
    public class Shelf
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public List<LibraryItem> Entities { get; set; } = new List<LibraryItem>();
    }
}