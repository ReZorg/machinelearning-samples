using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// A pattern matching result containing variable bindings.
    /// </summary>
    public class PatternMatch
    {
        /// <summary>
        /// Variable bindings: maps variable names to matched atoms.
        /// </summary>
        public Dictionary<string, Atom> Bindings { get; } = new Dictionary<string, Atom>();

        /// <summary>
        /// The matched atom.
        /// </summary>
        public Atom MatchedAtom { get; set; }

        public PatternMatch() { }

        public PatternMatch(Dictionary<string, Atom> bindings, Atom matchedAtom = null)
        {
            Bindings = new Dictionary<string, Atom>(bindings);
            MatchedAtom = matchedAtom;
        }

        public override string ToString()
        {
            var bindingsStr = string.Join(", ", Bindings.Select(kv => $"${kv.Key} = {kv.Value.ToShortString()}"));
            return $"{{ {bindingsStr} }}";
        }
    }

    /// <summary>
    /// Pattern matcher for querying the AtomSpace.
    /// Supports variable matching using VariableNode atoms.
    /// </summary>
    public class PatternMatcher
    {
        private readonly AtomSpace _atomSpace;

        public PatternMatcher(AtomSpace atomSpace)
        {
            _atomSpace = atomSpace;
        }

        /// <summary>
        /// Find all atoms matching a pattern with variable nodes.
        /// Variables are denoted by VariableNode atoms.
        /// </summary>
        public IEnumerable<PatternMatch> Match(Atom pattern)
        {
            if (pattern is Node node)
            {
                return MatchNode(node);
            }
            else if (pattern is Link link)
            {
                return MatchLink(link);
            }
            return Enumerable.Empty<PatternMatch>();
        }

        private IEnumerable<PatternMatch> MatchNode(Node pattern)
        {
            // If it's a variable, match any node
            if (pattern.AtomType == NodeTypes.Variable)
            {
                foreach (var atom in _atomSpace.GetAllAtoms())
                {
                    var match = new PatternMatch();
                    match.Bindings[pattern.Name] = atom;
                    match.MatchedAtom = atom;
                    yield return match;
                }
            }
            else
            {
                // Match exact node
                var existing = _atomSpace.GetNode(pattern.AtomType, pattern.Name);
                if (existing != null)
                {
                    var match = new PatternMatch();
                    match.MatchedAtom = existing;
                    yield return match;
                }
            }
        }

        private IEnumerable<PatternMatch> MatchLink(Link pattern)
        {
            // Get all links of the same type
            foreach (var atom in _atomSpace.GetAtomsByType(pattern.AtomType))
            {
                if (atom is Link link && link.Arity == pattern.Arity)
                {
                    var bindings = new Dictionary<string, Atom>();
                    bool matches = true;

                    for (int i = 0; i < pattern.Arity && matches; i++)
                    {
                        var patternChild = pattern.Outgoing[i];
                        var linkChild = link.Outgoing[i];

                        if (patternChild is Node pn && pn.AtomType == NodeTypes.Variable)
                        {
                            // Variable node - bind it
                            if (bindings.ContainsKey(pn.Name))
                            {
                                // Already bound - must match same atom
                                if (!bindings[pn.Name].Equals(linkChild))
                                    matches = false;
                            }
                            else
                            {
                                bindings[pn.Name] = linkChild;
                            }
                        }
                        else if (patternChild is Node node)
                        {
                            // Concrete node - must match exactly
                            if (!node.Equals(linkChild))
                                matches = false;
                        }
                        else if (patternChild is Link childPattern && linkChild is Link childLink)
                        {
                            // Nested link - recursive match
                            var nestedMatches = MatchLinkRecursive(childPattern, childLink, bindings);
                            if (nestedMatches == null)
                                matches = false;
                            else
                                bindings = nestedMatches;
                        }
                        else
                        {
                            matches = false;
                        }
                    }

                    if (matches)
                    {
                        yield return new PatternMatch(bindings, link);
                    }
                }
            }
        }

        private Dictionary<string, Atom> MatchLinkRecursive(Link pattern, Link link, Dictionary<string, Atom> existingBindings)
        {
            if (pattern.AtomType != link.AtomType || pattern.Arity != link.Arity)
                return null;

            var bindings = new Dictionary<string, Atom>(existingBindings);

            for (int i = 0; i < pattern.Arity; i++)
            {
                var patternChild = pattern.Outgoing[i];
                var linkChild = link.Outgoing[i];

                if (patternChild is Node pn && pn.AtomType == NodeTypes.Variable)
                {
                    if (bindings.ContainsKey(pn.Name))
                    {
                        if (!bindings[pn.Name].Equals(linkChild))
                            return null;
                    }
                    else
                    {
                        bindings[pn.Name] = linkChild;
                    }
                }
                else if (patternChild is Node node)
                {
                    if (!node.Equals(linkChild))
                        return null;
                }
                else if (patternChild is Link childPattern && linkChild is Link childLink)
                {
                    var nestedBindings = MatchLinkRecursive(childPattern, childLink, bindings);
                    if (nestedBindings == null)
                        return null;
                    bindings = nestedBindings;
                }
                else
                {
                    return null;
                }
            }

            return bindings;
        }

        /// <summary>
        /// Find all atoms that inherit from a given concept.
        /// </summary>
        public IEnumerable<Node> FindSubtypes(Node concept)
        {
            foreach (var link in _atomSpace.GetIncoming(concept))
            {
                if (link.AtomType == LinkTypes.Inheritance && link.Arity == 2)
                {
                    if (link.Outgoing[1].Equals(concept) && link.Outgoing[0] is Node subtype)
                    {
                        yield return subtype;
                    }
                }
            }
        }

        /// <summary>
        /// Find all atoms that a given concept inherits from.
        /// </summary>
        public IEnumerable<Node> FindSupertypes(Node concept)
        {
            foreach (var link in _atomSpace.GetIncoming(concept))
            {
                if (link.AtomType == LinkTypes.Inheritance && link.Arity == 2)
                {
                    if (link.Outgoing[0].Equals(concept) && link.Outgoing[1] is Node supertype)
                    {
                        yield return supertype;
                    }
                }
            }
        }

        /// <summary>
        /// Find all predicates that apply to a given concept.
        /// </summary>
        public IEnumerable<(Node predicate, List<Atom> arguments)> FindPredicates(Node concept)
        {
            foreach (var link in _atomSpace.GetIncoming(concept))
            {
                if (link.AtomType == LinkTypes.List)
                {
                    foreach (var evalLink in _atomSpace.GetIncoming(link))
                    {
                        if (evalLink.AtomType == LinkTypes.Evaluation && 
                            evalLink.Arity == 2 && 
                            evalLink.Outgoing[0] is Node predicate)
                        {
                            var listLink = evalLink.Outgoing[1] as Link;
                            yield return (predicate, listLink?.Outgoing ?? new List<Atom>());
                        }
                    }
                }
            }
        }
    }
}
