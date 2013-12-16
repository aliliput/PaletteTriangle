namespace PaletteTriangle.Models
{
    public class Selector
    {
        public Selector(string query, string property, string template)
        {
            this.Query = query;
            this.Property = property;
            this.Template = template;
        }

        public string Query { get; private set; }
        public string Property { get; private set; }
        public string Template { get; private set; }
    }
}
