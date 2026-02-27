using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCog_AtomSpace.DataStructures
{
    /// <summary>
    /// Represents an inference rule that can be applied in the AtomSpace.
    /// </summary>
    public class InferenceRule
    {
        public string Name { get; }
        public string Description { get; }
        public Func<AtomSpace, IEnumerable<Link>> Apply { get; }

        public InferenceRule(string name, string description, Func<AtomSpace, IEnumerable<Link>> apply)
        {
            Name = name;
            Description = description;
            Apply = apply;
        }
    }

    /// <summary>
    /// Simple inference engine implementing basic reasoning rules.
    /// Inspired by OpenCog's PLN (Probabilistic Logic Networks).
    /// </summary>
    public class InferenceEngine
    {
        private readonly AtomSpace _atomSpace;
        private readonly List<InferenceRule> _rules = new List<InferenceRule>();

        public InferenceEngine(AtomSpace atomSpace)
        {
            _atomSpace = atomSpace;
            RegisterDefaultRules();
        }

        /// <summary>
        /// Register the default inference rules.
        /// </summary>
        private void RegisterDefaultRules()
        {
            // Deduction Rule: If A inherits from B, and B inherits from C, then A inherits from C
            _rules.Add(new InferenceRule(
                "DeductionRule",
                "If A inherits from B, and B inherits from C, then A inherits from C",
                DeductionRule
            ));

            // Inversion Rule: Swap inheritance direction with adjusted truth value
            _rules.Add(new InferenceRule(
                "InversionRule",
                "Generate inverse inheritance links with adjusted truth values",
                InversionRule
            ));

            // Modus Ponens: If A implies B, and A is true, then B is true
            _rules.Add(new InferenceRule(
                "ModusPonens",
                "If A implies B, and A is true, then B is true",
                ModusPonensRule
            ));
        }

        /// <summary>
        /// Add a custom inference rule.
        /// </summary>
        public void AddRule(InferenceRule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// Run all inference rules once and return newly created links.
        /// </summary>
        public List<Link> RunInference()
        {
            var newLinks = new List<Link>();

            foreach (var rule in _rules)
            {
                var inferred = rule.Apply(_atomSpace);
                newLinks.AddRange(inferred);
            }

            return newLinks;
        }

        /// <summary>
        /// Run inference rules repeatedly until no new links are created.
        /// </summary>
        public List<Link> RunInferenceToFixpoint(int maxIterations = 100)
        {
            var allNewLinks = new List<Link>();
            int iteration = 0;

            while (iteration < maxIterations)
            {
                var newLinks = RunInference();
                if (newLinks.Count == 0)
                    break;

                allNewLinks.AddRange(newLinks);
                iteration++;
            }

            return allNewLinks;
        }

        /// <summary>
        /// Deduction rule implementation.
        /// If (Inheritance A B) and (Inheritance B C), infer (Inheritance A C)
        /// </summary>
        private IEnumerable<Link> DeductionRule(AtomSpace space)
        {
            var inheritanceLinks = space.GetAtomsByType(LinkTypes.Inheritance)
                .OfType<Link>()
                .Where(l => l.Arity == 2)
                .ToList();

            var newLinks = new List<Link>();

            foreach (var link1 in inheritanceLinks)
            {
                var a = link1.Outgoing[0];
                var b = link1.Outgoing[1];

                foreach (var link2 in inheritanceLinks)
                {
                    if (link2.Outgoing[0].Equals(b))
                    {
                        var c = link2.Outgoing[1];

                        // Check if we already have this link
                        bool exists = inheritanceLinks.Any(l => 
                            l.Outgoing[0].Equals(a) && l.Outgoing[1].Equals(c));

                        if (!exists && !a.Equals(c))
                        {
                            // Calculate deduced truth value
                            var tv = CalculateDeductionTruthValue(link1.TruthValue, link2.TruthValue);
                            var newLink = space.AddLink(LinkTypes.Inheritance, tv, a, c);
                            newLinks.Add(newLink);
                        }
                    }
                }
            }

            return newLinks;
        }

        /// <summary>
        /// Inversion rule implementation.
        /// If (Inheritance A B) with high confidence, infer weak (Similarity A B)
        /// </summary>
        private IEnumerable<Link> InversionRule(AtomSpace space)
        {
            var newLinks = new List<Link>();

            foreach (var link in space.GetAtomsByType(LinkTypes.Inheritance).OfType<Link>())
            {
                if (link.Arity == 2 && link.TruthValue.Confidence > 0.5)
                {
                    var a = link.Outgoing[0];
                    var b = link.Outgoing[1];

                    // Check if similarity already exists
                    var similarityExists = space.GetAtomsByType(LinkTypes.Similarity)
                        .OfType<Link>()
                        .Any(l => l.Arity == 2 && 
                            ((l.Outgoing[0].Equals(a) && l.Outgoing[1].Equals(b)) ||
                             (l.Outgoing[0].Equals(b) && l.Outgoing[1].Equals(a))));

                    if (!similarityExists)
                    {
                        // Similarity has lower strength than inheritance
                        var tv = new TruthValue(
                            link.TruthValue.Strength * 0.5,
                            link.TruthValue.Confidence * 0.7
                        );
                        var newLink = space.AddLink(LinkTypes.Similarity, tv, a, b);
                        newLinks.Add(newLink);
                    }
                }
            }

            return newLinks;
        }

        /// <summary>
        /// Modus Ponens rule implementation.
        /// If (Implication A B) and A has high truth value, infer B with appropriate truth value
        /// </summary>
        private IEnumerable<Link> ModusPonensRule(AtomSpace space)
        {
            var newLinks = new List<Link>();

            foreach (var impl in space.GetAtomsByType(LinkTypes.Implication).OfType<Link>())
            {
                if (impl.Arity == 2)
                {
                    var antecedent = impl.Outgoing[0];
                    var consequent = impl.Outgoing[1];

                    // Check if antecedent is "true" (high strength)
                    if (antecedent.TruthValue.Strength > 0.5 && 
                        antecedent.TruthValue.Confidence > 0.5)
                    {
                        // Apply modus ponens
                        var tv = CalculateModusPonensTruthValue(
                            antecedent.TruthValue, 
                            impl.TruthValue
                        );

                        // Update consequent truth value if it would increase
                        if (tv.Strength > consequent.TruthValue.Strength)
                        {
                            consequent.TruthValue = tv;
                        }
                    }
                }
            }

            return newLinks;
        }

        /// <summary>
        /// Calculate truth value for deduction using simple formula.
        /// </summary>
        private TruthValue CalculateDeductionTruthValue(TruthValue tv1, TruthValue tv2)
        {
            // Simple deduction formula: strength = s1 * s2
            // confidence = min(c1, c2) * k where k < 1
            double strength = tv1.Strength * tv2.Strength;
            double confidence = Math.Min(tv1.Confidence, tv2.Confidence) * 0.8;
            return new TruthValue(strength, confidence);
        }

        /// <summary>
        /// Calculate truth value for modus ponens.
        /// </summary>
        private TruthValue CalculateModusPonensTruthValue(TruthValue antecedentTv, TruthValue implicationTv)
        {
            double strength = antecedentTv.Strength * implicationTv.Strength;
            double confidence = Math.Min(antecedentTv.Confidence, implicationTv.Confidence) * 0.9;
            return new TruthValue(strength, confidence);
        }
    }
}
