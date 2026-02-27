using System;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// A Node represents a concept, entity, or value in the AtomSpace.
    /// Nodes are the leaf elements of the hypergraph.
    /// Examples: ConceptNode("Dog"), NumberNode(42), PredicateNode("owns")
    /// </summary>
    public class Node : Atom
    {
        /// <summary>
        /// The name/identifier of this node.
        /// </summary>
        public string Name { get; }

        public Node(string nodeType, string name)
        {
            AtomType = nodeType;
            Name = name;
        }

        public override string ToShortString() => $"({AtomType} \"{Name}\")";

        public override string ToString() => 
            $"({AtomType} \"{Name}\" {TruthValue})";

        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return AtomType == other.AtomType && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode() => HashCode.Combine(AtomType, Name);
    }

    /// <summary>
    /// Common node types used in OpenCog.
    /// </summary>
    public static class NodeTypes
    {
        /// <summary>
        /// A concept node represents a general concept (e.g., "Dog", "Animal").
        /// </summary>
        public const string Concept = "ConceptNode";

        /// <summary>
        /// A predicate node represents a predicate or property (e.g., "is_red", "can_fly").
        /// </summary>
        public const string Predicate = "PredicateNode";

        /// <summary>
        /// A variable node represents a variable for pattern matching.
        /// </summary>
        public const string Variable = "VariableNode";

        /// <summary>
        /// A number node represents a numeric value.
        /// </summary>
        public const string Number = "NumberNode";

        /// <summary>
        /// A schema node represents an executable procedure.
        /// </summary>
        public const string Schema = "SchemaNode";

        /// <summary>
        /// A grounded schema node represents a procedure implemented in code.
        /// </summary>
        public const string GroundedSchema = "GroundedSchemaNode";
    }
}
