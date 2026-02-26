using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// The AtomSpace is the central knowledge store in OpenCog.
    /// It functions as a hypergraph database where knowledge is represented
    /// as Atoms (Nodes and Links) with associated truth and attention values.
    /// </summary>
    public class AtomSpace
    {
        private readonly Dictionary<Guid, Atom> _atoms = new Dictionary<Guid, Atom>();
        private readonly Dictionary<(string, string), Node> _nodeIndex = new Dictionary<(string, string), Node>();
        private readonly Dictionary<string, List<Atom>> _typeIndex = new Dictionary<string, List<Atom>>();
        private readonly Dictionary<Guid, List<Link>> _incomingIndex = new Dictionary<Guid, List<Link>>();

        /// <summary>
        /// Name of this AtomSpace instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Number of atoms in the AtomSpace.
        /// </summary>
        public int Count => _atoms.Count;

        public AtomSpace(string name = "default")
        {
            Name = name;
        }

        /// <summary>
        /// Add a node to the AtomSpace. If the node already exists, return the existing one.
        /// </summary>
        public Node AddNode(string nodeType, string name, TruthValue truthValue = null)
        {
            var key = (nodeType, name);
            if (_nodeIndex.TryGetValue(key, out var existingNode))
            {
                if (truthValue != null)
                    existingNode.TruthValue = truthValue;
                return existingNode;
            }

            var node = new Node(nodeType, name);
            if (truthValue != null)
                node.TruthValue = truthValue;

            _atoms[node.Id] = node;
            _nodeIndex[key] = node;
            AddToTypeIndex(node);

            return node;
        }

        /// <summary>
        /// Add a link to the AtomSpace. If an identical link exists, return the existing one.
        /// </summary>
        public Link AddLink(string linkType, TruthValue truthValue = null, params Atom[] outgoing)
        {
            // Check for existing identical link
            foreach (var atom in GetAtomsByType(linkType))
            {
                if (atom is Link existingLink && existingLink.Equals(new Link(linkType, outgoing)))
                {
                    if (truthValue != null)
                        existingLink.TruthValue = truthValue;
                    return existingLink;
                }
            }

            var link = new Link(linkType, outgoing);
            if (truthValue != null)
                link.TruthValue = truthValue;

            _atoms[link.Id] = link;
            AddToTypeIndex(link);

            // Update incoming index for each outgoing atom
            foreach (var target in outgoing)
            {
                if (!_incomingIndex.ContainsKey(target.Id))
                    _incomingIndex[target.Id] = new List<Link>();
                _incomingIndex[target.Id].Add(link);
            }

            return link;
        }

        /// <summary>
        /// Get a node by type and name.
        /// </summary>
        public Node GetNode(string nodeType, string name)
        {
            _nodeIndex.TryGetValue((nodeType, name), out var node);
            return node;
        }

        /// <summary>
        /// Get all atoms of a specific type.
        /// </summary>
        public IEnumerable<Atom> GetAtomsByType(string atomType)
        {
            if (_typeIndex.TryGetValue(atomType, out var atoms))
                return atoms;
            return Enumerable.Empty<Atom>();
        }

        /// <summary>
        /// Get all links that point to a specific atom (incoming set).
        /// </summary>
        public IEnumerable<Link> GetIncoming(Atom atom)
        {
            if (_incomingIndex.TryGetValue(atom.Id, out var links))
                return links;
            return Enumerable.Empty<Link>();
        }

        /// <summary>
        /// Get all atoms in the AtomSpace.
        /// </summary>
        public IEnumerable<Atom> GetAllAtoms() => _atoms.Values;

        /// <summary>
        /// Get all nodes in the AtomSpace.
        /// </summary>
        public IEnumerable<Node> GetAllNodes() => _atoms.Values.OfType<Node>();

        /// <summary>
        /// Get all links in the AtomSpace.
        /// </summary>
        public IEnumerable<Link> GetAllLinks() => _atoms.Values.OfType<Link>();

        /// <summary>
        /// Remove an atom from the AtomSpace.
        /// </summary>
        public bool RemoveAtom(Atom atom)
        {
            if (!_atoms.ContainsKey(atom.Id))
                return false;

            // Can't remove if there are incoming links
            if (_incomingIndex.TryGetValue(atom.Id, out var incoming) && incoming.Count > 0)
                return false;

            _atoms.Remove(atom.Id);

            if (atom is Node node)
            {
                _nodeIndex.Remove((node.AtomType, node.Name));
            }
            else if (atom is Link link)
            {
                foreach (var target in link.Outgoing)
                {
                    if (_incomingIndex.TryGetValue(target.Id, out var inList))
                        inList.Remove(link);
                }
            }

            RemoveFromTypeIndex(atom);
            return true;
        }

        /// <summary>
        /// Check if an atom exists in the AtomSpace.
        /// </summary>
        public bool Contains(Atom atom) => _atoms.ContainsKey(atom.Id);

        /// <summary>
        /// Clear all atoms from the AtomSpace.
        /// </summary>
        public void Clear()
        {
            _atoms.Clear();
            _nodeIndex.Clear();
            _typeIndex.Clear();
            _incomingIndex.Clear();
        }

        private void AddToTypeIndex(Atom atom)
        {
            if (!_typeIndex.ContainsKey(atom.AtomType))
                _typeIndex[atom.AtomType] = new List<Atom>();
            _typeIndex[atom.AtomType].Add(atom);
        }

        private void RemoveFromTypeIndex(Atom atom)
        {
            if (_typeIndex.TryGetValue(atom.AtomType, out var list))
                list.Remove(atom);
        }

        /// <summary>
        /// Print all atoms in the AtomSpace.
        /// </summary>
        public void PrintAtoms()
        {
            Console.WriteLine($"AtomSpace '{Name}' contains {Count} atoms:");
            Console.WriteLine(new string('-', 50));
            foreach (var atom in _atoms.Values)
            {
                Console.WriteLine(atom.ToShortString());
            }
            Console.WriteLine(new string('-', 50));
        }
    }
}
