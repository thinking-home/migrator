namespace ThinkingHome.Migrator.Framework
{
    public class SchemaQualifiedObjectName
    {
        public string Name { get; set; }

        public string Schema { get; set; }

        public bool SchemaIsEmpty => string.IsNullOrWhiteSpace(Schema);

        /// <summary>
        /// Implicit type conversion string to SchemaQualifiedObjectName
        /// </summary>
        public static implicit operator SchemaQualifiedObjectName(string name)
        {
            return new SchemaQualifiedObjectName { Name = name };
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";
        }
    }
}