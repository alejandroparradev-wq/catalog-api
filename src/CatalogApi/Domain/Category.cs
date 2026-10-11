namespace CatalogApi.Domain
{
	public class Category
	{
		public int Id { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public string? Description { get; private set; }
		public List<Product> Products { get; private set; } = new();

		private Category() { }
		public Category(string name, string? description)
		{
			Update(name, description);
		}

		public void Update(string name, string?	 description)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("El nombre es requerido.", nameof(name));
			Name = name.Trim();
			Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
		}
	}
}