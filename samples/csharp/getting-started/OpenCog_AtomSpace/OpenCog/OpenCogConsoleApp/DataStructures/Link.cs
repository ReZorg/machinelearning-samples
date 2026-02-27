using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// A Link connects multiple Atoms (nodes and/or other links) together.
    /// Links are the edges of the hypergraph and can reference any number of atoms.
    /// Examples: InheritanceLink(Dog, Animal), EvaluationLink(owns, ListLink(John, Car))
    /// </summary>
    public class Link : Atom
    {
        /// <summary>
        /// The outgoing set: the atoms this link points to.
        /// </summary>
        public List<Atom> Outgoing { get; }

        /// <summary>
        /// The arity of this link (number of outgoing atoms).
        /// </summary>
        public int Arity => Outgoing.Count;

        public Link(string linkType, params Atom[] outgoing)
        {
            AtomType = linkType;
            Outgoing = outgoing?.ToList() ?? new List<Atom>();
        }

        public Link(string linkType, IEnumerable<Atom> outgoing)
        {
            AtomType = linkType;
            Outgoing = outgoing?.ToList() ?? new List<Atom>();
        }

        public override string ToShortString()
        {
            var children = string.Join(" ", Outgoing.Select(a => a.ToShortString()));
            return $"({AtomType} {children})";
        }

        public override string ToString()
        {
            var children = string.Join("\n  ", Outgoing.Select(a => a.ToShortString()));
            return $"({AtomType}\n  {children}\n) {TruthValue}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Link other)
            {
                if (AtomType != other.AtomType || Arity != other.Arity)
                    return false;

                for (int i = 0; i < Arity; i++)
                {
                    if (!Outgoing[i].Equals(other.Outgoing[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = AtomType.GetHashCode();
            foreach (var atom in Outgoing)
            {
                hash = HashCode.Combine(hash, atom.GetHashCode());
            }
            return hash;
        }
    }

    /// <summary>
    /// Common link types used in OpenCog.
    /// </summary>
    public static class LinkTypes
    {
        /// <summary>
        /// InheritanceLink represents "is-a" relationships (e.g., Dog is-a Animal).
        /// </summary>
        public const string Inheritance = "InheritanceLink";

        /// <summary>
        /// EvaluationLink connects a predicate to its arguments.
        /// </summary>
        public const string Evaluation = "EvaluationLink";

        /// <summary>
        /// ListLink is an ordered list of atoms.
        /// </summary>
        public const string List = "ListLink";

        /// <summary>
        /// SetLink is an unordered set of atoms.
        /// </summary>
        public const string Set = "SetLink";

        /// <summary>
        /// ImplicationLink represents logical implication (A => B).
        /// </summary>
        public const string Implication = "ImplicationLink";

        /// <summary>
        /// AndLink represents logical conjunction.
        /// </summary>
        public const string And = "AndLink";

        /// <summary>
        /// OrLink represents logical disjunction.
        /// </summary>
        public const string Or = "OrLink";

        /// <summary>
        /// NotLink represents logical negation.
        /// </summary>
        public const string Not = "NotLink";

        /// <summary>
        /// SimilarityLink represents similarity between concepts.
        /// </summary>
        public const string Similarity = "SimilarityLink";

        /// <summary>
        /// MemberLink represents set membership.
        /// </summary>
        public const string Member = "MemberLink";

        /// <summary>
        /// BindLink is used for pattern matching queries.
        /// </summary>
        public const string Bind = "BindLink";

        /// <summary>
        /// ExecutionOutputLink represents the output of executing a schema.
        /// </summary>
        public const string ExecutionOutput = "ExecutionOutputLink";
    }
}
