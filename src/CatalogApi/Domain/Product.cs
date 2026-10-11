namespace CatalogApi.Domain
{
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; } 
        public int CategoryId { get; private set; }


        public Category? Category { get; private set; }
        private Product() { }
        public Product(string name, string? description, int categoryId)
        {
            Update(name, description, categoryId);
        }

        public void Update(string name, string? description, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre es requerido.", nameof(name));
            if (categoryId <= 0)
                throw new ArgumentException("El Id de la categoría es requerido.", nameof(categoryId));
            Name = name.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            CategoryId = categoryId;
        }
    }
}