using System;
using System.Collections.Generic;
using System.Linq;
using OpenCog_AtomSpace.DataStructures;

namespace OpenCog_AtomSpace
{
    /// <summary>
    /// OpenCog AtomSpace Sample Application
    /// 
    /// This sample demonstrates OpenCog-style knowledge representation and reasoning in C#.
    /// OpenCog is an open-source AGI (Artificial General Intelligence) framework that uses
    /// a hypergraph-based knowledge store called AtomSpace.
    /// 
    /// This implementation shows:
    /// 1. Creating nodes and links to represent knowledge
    /// 2. Pattern matching to query the knowledge base
    /// 3. Simple inference/reasoning based on PLN (Probabilistic Logic Networks)
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("======================================================");
            Console.WriteLine("OpenCog AtomSpace Sample - Knowledge Representation");
            Console.WriteLine("======================================================");
            Console.WriteLine();

            // Create a new AtomSpace
            var atomSpace = new AtomSpace("AnimalKnowledge");

            Console.WriteLine("STEP 1: Building the Knowledge Base");
            Console.WriteLine("------------------------------------");
            BuildKnowledgeBase(atomSpace);
            atomSpace.PrintAtoms();
            Console.WriteLine();

            Console.WriteLine("STEP 2: Pattern Matching and Queries");
            Console.WriteLine("-------------------------------------");
            DemonstratePatternMatching(atomSpace);
            Console.WriteLine();

            Console.WriteLine("STEP 3: Inference and Reasoning");
            Console.WriteLine("--------------------------------");
            DemonstrateInference(atomSpace);
            Console.WriteLine();

            Console.WriteLine("STEP 4: Building and Querying a Social Network");
            Console.WriteLine("-----------------------------------------------");
            DemonstrateSocialNetwork();
            Console.WriteLine();

            Console.WriteLine("======================================================");
            Console.WriteLine("OpenCog AtomSpace Sample Complete");
            Console.WriteLine("======================================================");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Build a knowledge base about animals using OpenCog-style representations.
        /// </summary>
        static void BuildKnowledgeBase(AtomSpace atomSpace)
        {
            Console.WriteLine("Creating concept nodes for animal taxonomy...");

            // Create concept nodes for animals
            var animal = atomSpace.AddNode(NodeTypes.Concept, "Animal", new TruthValue(1.0, 1.0));
            var mammal = atomSpace.AddNode(NodeTypes.Concept, "Mammal", new TruthValue(1.0, 1.0));
            var bird = atomSpace.AddNode(NodeTypes.Concept, "Bird", new TruthValue(1.0, 1.0));
            var dog = atomSpace.AddNode(NodeTypes.Concept, "Dog", new TruthValue(1.0, 1.0));
            var cat = atomSpace.AddNode(NodeTypes.Concept, "Cat", new TruthValue(1.0, 1.0));
            var eagle = atomSpace.AddNode(NodeTypes.Concept, "Eagle", new TruthValue(1.0, 1.0));
            var penguin = atomSpace.AddNode(NodeTypes.Concept, "Penguin", new TruthValue(1.0, 1.0));

            Console.WriteLine("Creating inheritance links (is-a relationships)...");

            // Create inheritance links: Mammal is-a Animal, Bird is-a Animal
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), mammal, animal);
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), bird, animal);

            // Dog is-a Mammal, Cat is-a Mammal
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), dog, mammal);
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), cat, mammal);

            // Eagle is-a Bird, Penguin is-a Bird
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), eagle, bird);
            atomSpace.AddLink(LinkTypes.Inheritance, new TruthValue(1.0, 0.99), penguin, bird);

            Console.WriteLine("Creating predicate nodes for properties...");

            // Create predicates for properties
            var canFly = atomSpace.AddNode(NodeTypes.Predicate, "can_fly");
            var hasWings = atomSpace.AddNode(NodeTypes.Predicate, "has_wings");
            var hasFur = atomSpace.AddNode(NodeTypes.Predicate, "has_fur");

            Console.WriteLine("Creating evaluation links (predicate applications)...");

            // Birds have wings (with high confidence)
            var birdList = new Link(LinkTypes.List, bird);
            atomSpace.AddLink(LinkTypes.Evaluation, new TruthValue(0.95, 0.9), hasWings, birdList);

            // Eagles can fly
            var eagleList = new Link(LinkTypes.List, eagle);
            atomSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 0.99), canFly, eagleList);

            // Penguins cannot fly (low strength, high confidence)
            var penguinList = new Link(LinkTypes.List, penguin);
            atomSpace.AddLink(LinkTypes.Evaluation, new TruthValue(0.0, 0.99), canFly, penguinList);

            // Mammals have fur (with some exceptions)
            var mammalList = new Link(LinkTypes.List, mammal);
            atomSpace.AddLink(LinkTypes.Evaluation, new TruthValue(0.85, 0.8), hasFur, mammalList);

            Console.WriteLine($"Knowledge base built with {atomSpace.Count} atoms.");
        }

        /// <summary>
        /// Demonstrate pattern matching capabilities.
        /// </summary>
        static void DemonstratePatternMatching(AtomSpace atomSpace)
        {
            var matcher = new PatternMatcher(atomSpace);

            Console.WriteLine("Query 1: Find all concepts that inherit from 'Mammal'");
            var mammal = atomSpace.GetNode(NodeTypes.Concept, "Mammal");
            var subtypes = matcher.FindSubtypes(mammal);
            foreach (var subtype in subtypes)
            {
                Console.WriteLine($"  - {subtype.Name} is a Mammal");
            }
            Console.WriteLine();

            Console.WriteLine("Query 2: Find all concepts that 'Dog' inherits from");
            var dog = atomSpace.GetNode(NodeTypes.Concept, "Dog");
            var supertypes = matcher.FindSupertypes(dog);
            foreach (var supertype in supertypes)
            {
                Console.WriteLine($"  - Dog is a {supertype.Name}");
            }
            Console.WriteLine();

            Console.WriteLine("Query 3: Pattern match - Find all InheritanceLinks with variable");
            // Create a pattern with a variable
            var varX = new Node(NodeTypes.Variable, "X");
            var animal = atomSpace.GetNode(NodeTypes.Concept, "Animal");
            var pattern = new Link(LinkTypes.Inheritance, varX, animal);

            var matches = matcher.Match(pattern);
            foreach (var match in matches)
            {
                Console.WriteLine($"  - Found: {match}");
            }
            Console.WriteLine();

            Console.WriteLine("Query 4: Find inheritance relationships between two variables");
            var varY = new Node(NodeTypes.Variable, "Y");
            var inheritancePattern = new Link(LinkTypes.Inheritance, varX, varY);

            var allInheritances = matcher.Match(inheritancePattern);
            foreach (var match in allInheritances)
            {
                var x = match.Bindings["X"];
                var y = match.Bindings["Y"];
                Console.WriteLine($"  - {(x as Node)?.Name ?? "?"} inherits from {(y as Node)?.Name ?? "?"}");
            }
        }

        /// <summary>
        /// Demonstrate inference and reasoning capabilities.
        /// </summary>
        static void DemonstrateInference(AtomSpace atomSpace)
        {
            Console.WriteLine("Before inference - Inheritance links:");
            foreach (var link in atomSpace.GetAtomsByType(LinkTypes.Inheritance).OfType<Link>())
            {
                var from = (link.Outgoing[0] as Node)?.Name;
                var to = (link.Outgoing[1] as Node)?.Name;
                Console.WriteLine($"  - {from} inherits from {to} {link.TruthValue}");
            }
            Console.WriteLine();

            // Run inference
            var engine = new InferenceEngine(atomSpace);
            Console.WriteLine("Running inference engine (Deduction Rule)...");
            var newLinks = engine.RunInferenceToFixpoint();

            Console.WriteLine($"Inference created {newLinks.Count} new links.");
            Console.WriteLine();

            Console.WriteLine("After inference - Inheritance links (including inferred):");
            foreach (var link in atomSpace.GetAtomsByType(LinkTypes.Inheritance).OfType<Link>())
            {
                var from = (link.Outgoing[0] as Node)?.Name;
                var to = (link.Outgoing[1] as Node)?.Name;
                Console.WriteLine($"  - {from} inherits from {to} {link.TruthValue}");
            }
            Console.WriteLine();

            // Show example: Dog is now known to be an Animal through transitive inference
            Console.WriteLine("Example: Through deduction, we now know:");
            Console.WriteLine("  - Dog -> Mammal -> Animal (Dog is an Animal)");
            Console.WriteLine("  - Cat -> Mammal -> Animal (Cat is an Animal)");
            Console.WriteLine("  - Eagle -> Bird -> Animal (Eagle is an Animal)");
            Console.WriteLine("  - Penguin -> Bird -> Animal (Penguin is an Animal)");
        }

        /// <summary>
        /// Demonstrate a social network knowledge representation.
        /// </summary>
        static void DemonstrateSocialNetwork()
        {
            var socialSpace = new AtomSpace("SocialNetwork");

            Console.WriteLine("Building a social network knowledge base...");

            // Create person nodes
            var alice = socialSpace.AddNode(NodeTypes.Concept, "Alice");
            var bob = socialSpace.AddNode(NodeTypes.Concept, "Bob");
            var charlie = socialSpace.AddNode(NodeTypes.Concept, "Charlie");
            var diana = socialSpace.AddNode(NodeTypes.Concept, "Diana");

            // Create predicates
            var knows = socialSpace.AddNode(NodeTypes.Predicate, "knows");
            var likes = socialSpace.AddNode(NodeTypes.Predicate, "likes");
            var worksAt = socialSpace.AddNode(NodeTypes.Predicate, "works_at");

            // Create company nodes
            var techCorp = socialSpace.AddNode(NodeTypes.Concept, "TechCorp");
            var dataInc = socialSpace.AddNode(NodeTypes.Concept, "DataInc");

            // Create relationships with properly named ListLinks for readability
            // Alice knows Bob
            var aliceBobList = new Link(LinkTypes.List, alice, bob);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 0.99), 
                knows, aliceBobList);

            // Bob knows Charlie
            var bobCharlieList = new Link(LinkTypes.List, bob, charlie);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 0.99),
                knows, bobCharlieList);

            // Alice likes Charlie
            var aliceCharlieList = new Link(LinkTypes.List, alice, charlie);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(0.8, 0.9),
                likes, aliceCharlieList);

            // Alice works at TechCorp
            var aliceTechCorpList = new Link(LinkTypes.List, alice, techCorp);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 1.0),
                worksAt, aliceTechCorpList);

            // Bob works at TechCorp
            var bobTechCorpList = new Link(LinkTypes.List, bob, techCorp);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 1.0),
                worksAt, bobTechCorpList);

            // Charlie works at DataInc
            var charlieDataIncList = new Link(LinkTypes.List, charlie, dataInc);
            socialSpace.AddLink(LinkTypes.Evaluation, new TruthValue(1.0, 1.0),
                worksAt, charlieDataIncList);

            Console.WriteLine($"Created social network with {socialSpace.Count} atoms.");
            Console.WriteLine();

            // Create implication rule: If person X knows Y, and Y knows Z, then X might know Z
            // This would be used for friend recommendations
            Console.WriteLine("Creating implication rules for reasoning...");

            var varX = new Node(NodeTypes.Variable, "X");
            var varY = new Node(NodeTypes.Variable, "Y");
            var varZ = new Node(NodeTypes.Variable, "Z");

            // Pattern for "X knows Y" - extracting list links for clarity
            var xyList = new Link(LinkTypes.List, varX, varY);
            var xKnowsY = new Link(LinkTypes.Evaluation, knows, xyList);

            // Pattern for "Y knows Z"  
            var yzList = new Link(LinkTypes.List, varY, varZ);
            var yKnowsZ = new Link(LinkTypes.Evaluation, knows, yzList);

            // Combined pattern: X knows Y AND Y knows Z
            var premise = new Link(LinkTypes.And, xKnowsY, yKnowsZ);

            // Conclusion: X might know Z
            var xzList = new Link(LinkTypes.List, varX, varZ);
            var xMightKnowZ = new Link(LinkTypes.Evaluation, knows, xzList);

            // Create the implication
            socialSpace.AddLink(LinkTypes.Implication, new TruthValue(0.7, 0.6), premise, xMightKnowZ);

            Console.WriteLine("Implication added: If X knows Y and Y knows Z, then X might know Z");
            Console.WriteLine();

            // Print the social network
            Console.WriteLine("Social Network Knowledge Base:");
            foreach (var atom in socialSpace.GetAtomsByType(LinkTypes.Evaluation).OfType<Link>())
            {
                var predNode = atom.Outgoing[0] as Node;
                var argsList = atom.Outgoing[1] as Link;
                if (predNode != null && argsList != null)
                {
                    var args = string.Join(", ", argsList.Outgoing.OfType<Node>().Select(n => n.Name));
                    Console.WriteLine($"  - {predNode.Name}({args}) {atom.TruthValue}");
                }
            }
        }
    }
}
