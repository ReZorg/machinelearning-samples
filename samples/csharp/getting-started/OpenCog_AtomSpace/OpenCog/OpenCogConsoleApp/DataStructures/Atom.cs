using System;
using System.Collections.Generic;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// Base class for all Atoms in the AtomSpace.
    /// In OpenCog, an Atom is a fundamental unit of knowledge representation.
    /// Atoms can be either Nodes (representing concepts/entities) or Links (representing relationships).
    /// </summary>
    public abstract class Atom
    {
        /// <summary>
        /// Unique identifier for this atom.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The type of this atom (e.g., ConceptNode, InheritanceLink).
        /// </summary>
        public string AtomType { get; protected set; }

        /// <summary>
        /// Truth value representing confidence/strength of this knowledge.
        /// In OpenCog, this is used for probabilistic inference.
        /// </summary>
        public TruthValue TruthValue { get; set; } = new TruthValue();

        /// <summary>
        /// Attention value representing importance/relevance of this atom.
        /// Higher attention means the atom is more relevant in the current context.
        /// </summary>
        public AttentionValue AttentionValue { get; set; } = new AttentionValue();

        /// <summary>
        /// Additional metadata values associated with this atom.
        /// </summary>
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Timestamp when this atom was created.
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public abstract string ToShortString();
        public abstract override string ToString();
    }

    /// <summary>
    /// Represents the truth value of an Atom.
    /// In OpenCog's PLN (Probabilistic Logic Networks), truth values are used for reasoning.
    /// </summary>
    public class TruthValue
    {
        /// <summary>
        /// Strength: probability or degree of truth (0.0 to 1.0).
        /// </summary>
        public double Strength { get; set; } = 1.0;

        /// <summary>
        /// Confidence: certainty about the strength value (0.0 to 1.0).
        /// Higher confidence means more evidence supports this truth value.
        /// </summary>
        public double Confidence { get; set; } = 0.9;

        /// <summary>
        /// Count: number of observations supporting this truth value.
        /// </summary>
        public int Count { get; set; } = 1;

        public TruthValue() { }

        public TruthValue(double strength, double confidence)
        {
            Strength = Math.Max(0.0, Math.Min(1.0, strength));
            Confidence = Math.Max(0.0, Math.Min(1.0, confidence));
        }

        public override string ToString() => $"<{Strength:F2}, {Confidence:F2}>";
    }

    /// <summary>
    /// Represents the attention value of an Atom.
    /// In OpenCog's ECAN (Economic Attention Networks), attention values control focus.
    /// </summary>
    public class AttentionValue
    {
        /// <summary>
        /// Short-term importance (STI): how relevant is this atom right now.
        /// </summary>
        public int ShortTermImportance { get; set; } = 0;

        /// <summary>
        /// Long-term importance (LTI): how important is this atom generally.
        /// </summary>
        public int LongTermImportance { get; set; } = 0;

        /// <summary>
        /// Very long-term importance (VLTI): whether to persist this atom.
        /// </summary>
        public bool VeryLongTermImportance { get; set; } = false;

        public AttentionValue() { }

        public AttentionValue(int sti, int lti, bool vlti = false)
        {
            ShortTermImportance = sti;
            LongTermImportance = lti;
            VeryLongTermImportance = vlti;
        }

        public override string ToString() => $"[STI:{ShortTermImportance}, LTI:{LongTermImportance}]";
    }
}
